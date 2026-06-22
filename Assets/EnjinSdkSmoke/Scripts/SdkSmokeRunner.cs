using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enjin.Platform.Sdk;
using UnityEngine;
using UnityEngine.UI;

namespace EnjinSdkSmoke
{
    /// <summary>
    /// Runs a hand-written set of Enjin Platform SDK calls against a live
    /// platform endpoint to verify that the UPM package works end-to-end inside
    /// the Unity runtime (IL2CPP + reflection-based Newtonsoft.Json
    /// deserialization, BigInteger handling, async/await on the main thread,
    /// etc.).
    ///
    /// Mirrors tools/SdkSmoke/Program.cs in the SDK repo. Some GetBlock-shaped
    /// checks (#1, #4, #5) are known to fail on canary today due to a
    /// server-side issue; they are still executed so we can see when canary
    /// recovers.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SdkSmokeRunner : MonoBehaviour
    {
        [Tooltip("ScriptableObject holding the platform URL, token, and test inputs.")]
        public SdkSmokeConfig config;

        [Tooltip("Optional UGUI Text element to mirror the smoke-test log into. Leave empty to only use Debug.Log.")]
        public Text outputText;

        private readonly StringBuilder _buffer = new(4096);
        private CancellationTokenSource _cts;

        private void Start()
        {
            if (config == null)
            {
                LogLine("ERROR: SdkSmokeRunner has no SdkSmokeConfig assigned. " +
                        "Create one via Assets ▸ Create ▸ Enjin ▸ SDK Smoke Config " +
                        "and drag it onto this component.");
                return;
            }

            if (config.runOnStart)
            {
                _ = RunSmokeAsync();
            }
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        /// <summary>
        /// Public entry-point. Safe to call from a UI button via UnityEvent
        /// (signature: <c>void()</c>). Discards the returned Task.
        /// </summary>
        public void RunSmoke()
        {
            _ = RunSmokeAsync();
        }

        public async Task RunSmokeAsync()
        {
            if (config == null)
            {
                LogLine("ERROR: no config assigned.");
                return;
            }
            if (string.IsNullOrWhiteSpace(config.platformUrl) ||
                string.IsNullOrWhiteSpace(config.platformToken))
            {
                LogLine("ERROR: platformUrl and platformToken must be set on the SdkSmokeConfig asset.");
                return;
            }

            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var ct = _cts.Token;

            _buffer.Clear();
            FlushBuffer();

            LogLine("=== Enjin Platform v3 SDK smoke test (Unity UPM) ===");
            LogLine($"Endpoint: {config.platformUrl}");
            LogLine($"Token:    {Mask(config.platformToken)}");
            LogLine($"Account:  {config.accountAddress}");
            LogLine($"Block:    {config.recentBlockId}");
            LogLine($"Unity:    {Application.unityVersion}  Platform: {Application.platform}  IL2CPP: {IsIl2Cpp()}");
            LogLine("");

            using var client = TryCreateClient();
            if (client == null)
            {
                LogLine("=== Done ===");
                return;
            }

            try
            {
                await Check1_GetBlock(client, ct);
                await Check2_GetAccount(client, ct);
                await Check3_GetBlocks(client, ct);
                await Check4_GetBlockWithEvents(client, ct);
                await Check5_GetBlockWithValidatorAndExtrinsics(client, ct);
                await Check6_CreateManagedWallet(client, ct);
            }
            catch (OperationCanceledException)
            {
                LogLine("(cancelled)");
            }
            catch (Exception ex)
            {
                LogLine($"FATAL: {ex.GetType().Name}: {ex.Message}");
            }

            LogLine("");
            LogLine("=== Done ===");
        }

        private PlatformClient TryCreateClient()
        {
            try
            {
                if (!Uri.TryCreate(config.platformUrl, UriKind.Absolute, out var uri))
                {
                    LogLine($"FATAL: platformUrl is not a valid absolute URI: '{config.platformUrl}'");
                    return null;
                }
                var client = new PlatformClient(uri);
                client.Auth(config.platformToken);
                return client;
            }
            catch (Exception ex)
            {
                LogLine($"FATAL: {ex.GetType().Name} while constructing PlatformClient: {ex.Message}");
                if (ex.InnerException is not null)
                {
                    LogLine($"  inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                }
                return null;
            }
        }

        // ---- individual checks ------------------------------------------------

        private async Task Check1_GetBlock(PlatformClient client, CancellationToken ct)
        {
            await RunQuery(client, "1. GetBlock(id: recent) - transport sanity",
                new QueryQueryBuilder()
                    .WithGetBlock(
                        new BlockQueryBuilder().WithNumber().WithHash(),
                        Network.Canary, Chain.Matrix, id: config.recentBlockId),
                r => $"block #{r.Data?.GetBlock?.Number} hash={r.Data?.GetBlock?.Hash}",
                ct);
        }

        private async Task Check2_GetAccount(PlatformClient client, CancellationToken ct)
        {
            await RunQuery(client, "2. GetAccount(known address) - auth + BigInteger",
                new QueryQueryBuilder()
                    .WithGetAccount(
                        new AccountQueryBuilder().WithId().WithAddress().WithNonce().WithBalance(),
                        Network.Canary, Chain.Matrix, config.accountAddress),
                r =>
                {
                    var a = r.Data?.GetAccount;
                    return a is null
                        ? "account not found"
                        : $"id={a.Id} addr={a.Address} nonce={a.Nonce} balance={a.Balance} (type={a.Balance.GetType().Name})";
                },
                ct);
        }

        private async Task Check3_GetBlocks(PlatformClient client, CancellationToken ct)
        {
            var ids = new[] { config.recentBlockId, config.recentBlockId - 1, config.recentBlockId - 2 };
            await RunQuery(client, "3. GetBlocks(ids: [recent, recent-1, recent-2]) - list of blocks",
                new QueryQueryBuilder()
                    .WithGetBlocks(
                        new BlockQueryBuilder().WithNumber().WithHash(),
                        Network.Canary, Chain.Matrix, ids: ids),
                r =>
                {
                    var blocks = r.Data?.GetBlocks;
                    if (blocks is null) return "null";
                    return $"{blocks.Count} blocks: " +
                           string.Join(", ", blocks.Select(b => b?.Number?.ToString() ?? "?"));
                },
                ct);
        }

        private async Task Check4_GetBlockWithEvents(PlatformClient client, CancellationToken ct)
        {
            await RunQuery(client, "4. GetBlock with Events - Event type + nullable BigInteger",
                new QueryQueryBuilder()
                    .WithGetBlock(
                        new BlockQueryBuilder()
                            .WithNumber()
                            .WithEvents(new EventQueryBuilder().WithId().WithName().WithCollectionId()),
                        Network.Canary, Chain.Matrix, id: config.recentBlockId),
                r =>
                {
                    var evs = r.Data?.GetBlock?.Events;
                    if (evs is null) return "no events field";
                    var sample = string.Join(", ",
                        evs.Take(3).Select(e => $"{e?.Name}(coll={e?.CollectionId?.ToString() ?? "null"})"));
                    return $"{evs.Count} events, sample=[{sample}]";
                },
                ct);
        }

        private async Task Check5_GetBlockWithValidatorAndExtrinsics(PlatformClient client, CancellationToken ct)
        {
            await RunQuery(client, "5. GetBlock with Validator + Extrinsics - nested complex types",
                new QueryQueryBuilder()
                    .WithGetBlock(
                        new BlockQueryBuilder()
                            .WithNumber()
                            .WithValidator(new AccountQueryBuilder().WithAddress().WithBalance())
                            .WithExtrinsics(new ExtrinsicQueryBuilder().WithHash().WithSuccess().WithPallet().WithMethod()),
                        Network.Canary, Chain.Matrix, id: config.recentBlockId),
                r =>
                {
                    var b = r.Data?.GetBlock;
                    if (b is null) return "null";
                    return $"#{b.Number} validator={b.Validator?.Address} extrinsics={b.Extrinsics?.Count ?? 0}";
                },
                ct);
        }

        private async Task Check6_CreateManagedWallet(PlatformClient client, CancellationToken ct)
        {
            // External id must be unique per call but the SDK doesn't expose a
            // sanitizer; keep within 40 chars to match the server-side limit.
            string externalId = $"unity-smoke-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}";
            if (externalId.Length > 40) externalId = externalId.Substring(0, 40);

            await RunMutation(client, $"6. CreateManagedWallet(externalId={externalId}) - non-destructive mutation",
                new MutationQueryBuilder()
                    .WithCreateManagedWallet(externalId),
                r => $"created={r.Data?.CreateManagedWallet}",
                ct);
        }

        // ---- helpers ----------------------------------------------------------

        private async Task RunQuery(
            PlatformClient client,
            string label,
            QueryQueryBuilder builder,
            Func<QueryResponse, string> summarize,
            CancellationToken ct)
        {
            LogLine($"--- {label} ---");
            LogLine($"  REQ: {Truncate(builder.Build(), 240)}");
            try
            {
                ct.ThrowIfCancellationRequested();
                var resp = await client.SendQuery(builder);
                LogLine($"  HTTP {(int)resp.StatusCode}");
                var errs = resp.Result?.Errors;
                if (errs is { Count: > 0 })
                {
                    LogLine("  ERRORS:");
                    foreach (var e in errs) LogLine($"    - {e.Message}");
                }
                else
                {
                    LogLine($"  OK: {summarize(resp.Result)}");
                }
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                PrintEx(ex);
            }
            LogLine("");
        }

        private async Task RunMutation(
            PlatformClient client,
            string label,
            MutationQueryBuilder builder,
            Func<MutationResponse, string> summarize,
            CancellationToken ct)
        {
            LogLine($"--- {label} ---");
            LogLine($"  REQ: {Truncate(builder.Build(), 240)}");
            try
            {
                ct.ThrowIfCancellationRequested();
                var resp = await client.SendMutation(builder);
                LogLine($"  HTTP {(int)resp.StatusCode}");
                var errs = resp.Result?.Errors;
                if (errs is { Count: > 0 })
                {
                    LogLine("  ERRORS:");
                    foreach (var e in errs) LogLine($"    - {e.Message}");
                }
                else
                {
                    LogLine($"  OK: {summarize(resp.Result)}");
                }
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                PrintEx(ex);
            }
            LogLine("");
        }

        private void PrintEx(Exception ex)
        {
            LogLine($"  EXCEPTION: {ex.GetType().Name}: {ex.Message}");
            if (ex.InnerException is not null)
            {
                LogLine($"    inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
            }
        }

        private void LogLine(string line)
        {
            Debug.Log("[SdkSmoke] " + line);
            _buffer.AppendLine(line);
            FlushBuffer();
        }

        private void FlushBuffer()
        {
            if (outputText != null)
            {
                outputText.text = _buffer.ToString();
            }
        }

        private static string Truncate(string s, int n) =>
            string.IsNullOrEmpty(s) || s.Length <= n ? s : s.Substring(0, n) + "...";

        private static string Mask(string token)
        {
            if (string.IsNullOrEmpty(token)) return "<empty>";
            return token.Length > 8
                ? token.Substring(0, 4) + "..." + token.Substring(token.Length - 4)
                : "<short>";
        }

        private static bool IsIl2Cpp()
        {
#if ENABLE_IL2CPP
            return true;
#else
            return false;
#endif
        }
    }
}
