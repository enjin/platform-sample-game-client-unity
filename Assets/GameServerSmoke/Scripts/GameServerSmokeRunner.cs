using System;
using System.Collections;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HappyHarvest.EnjinIntegration.API;
using HappyHarvest.EnjinIntegration.Data;
using UnityEngine;
using UnityEngine.UI;

namespace GameServerSmoke
{
    /// <summary>
    /// Drives the real game-client REST layer (<see cref="EnjinApiService"/>)
    /// through the full sample-server flow:
    ///
    ///   health-check
    ///   register (== login)
    ///   get-tokens (baseline)
    ///   mint     -> get-tokens
    ///   melt     -> get-tokens
    ///   transfer -> get-tokens
    ///
    /// Use this as a wire-format / JsonUtility sanity check before clicking
    /// through the actual game UI. Everything that runs here is the same code
    /// the production menus call - we just script it instead of waiting for a
    /// player to push buttons.
    ///
    /// Created by the editor menu "Enjin > Open Game Server Smoke Scene". Drop
    /// a <see cref="GameServerSmokeConfig"/> on the inspector field, fill in
    /// the email/password, then press Play.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameServerSmokeRunner : MonoBehaviour
    {
        [Tooltip("ScriptableObject holding server host, player credentials, token ids, and amounts.")]
        public GameServerSmokeConfig config;

        [Tooltip("Optional EnjinManager prefab to instantiate so EnjinApiService.Instance is available. If left empty the runner will try to find an existing instance in the scene.")]
        public GameObject enjinManagerPrefab;

        [Tooltip("Optional UGUI Text element to mirror the log into. Leave empty to only use Debug.Log.")]
        public Text outputText;

        private readonly StringBuilder _buffer = new(8192);
        private bool _running;

        private void Start()
        {
            if (config == null)
            {
                LogLine("ERROR: GameServerSmokeRunner has no GameServerSmokeConfig assigned. " +
                        "Create one via Assets > Create > Enjin > Game Server Smoke Config " +
                        "and drag it onto this component.");
                return;
            }

            if (config.runOnStart)
            {
                StartCoroutine(RunSmokeCoroutine());
            }
        }

        /// <summary>Public entry-point safe to bind to a UI button (UnityEvent void()).</summary>
        public void RunSmoke()
        {
            if (_running)
            {
                LogLine("(smoke already running; ignoring re-entry)");
                return;
            }
            StartCoroutine(RunSmokeCoroutine());
        }

        private IEnumerator RunSmokeCoroutine()
        {
            _running = true;
            var task = RunSmokeAsync();
            while (!task.IsCompleted) yield return null;
            if (task.IsFaulted)
            {
                LogLine($"FATAL: {task.Exception?.GetBaseException().GetType().Name}: " +
                        task.Exception?.GetBaseException().Message);
            }
            _running = false;
        }

        private async Task RunSmokeAsync()
        {
            _buffer.Clear();
            FlushBuffer();

            LogLine("=== Game server REST smoke test (Unity client -> C# backend) ===");
            LogLine($"Server: {config.serverHost}");
            LogLine($"Email:  {config.email}");
            LogLine($"Mint:   token={config.mintTokenId} amount={config.mintAmount}");
            LogLine($"Melt:   token={config.mintTokenId} amount={config.meltAmount}");
            LogLine($"Xfer:   token={config.transferTokenId} amount={config.transferAmount} -> {config.transferRecipient}");
            LogLine($"Unity:  {Application.unityVersion}  Platform: {Application.platform}");
            LogLine("");

            if (string.IsNullOrWhiteSpace(config.email) || string.IsNullOrWhiteSpace(config.password))
            {
                LogLine("ERROR: email and password must be set on the GameServerSmokeConfig asset.");
                return;
            }

            var api = EnsureApiService();
            if (api == null)
            {
                LogLine("ERROR: could not locate or create EnjinApiService. Assign EnjinManager.prefab on the runner.");
                return;
            }

            // Force the configured host onto the (private) field so this scene
            // is self-contained even if the prefab points somewhere else.
            OverrideHost(api, config.serverHost);

            // EnjinApiService.Start() kicks off its own health check; give it a frame.
            await Task.Yield();

            // ---- 1. health check ----------------------------------------------
            LogLine("--- 1. health check ---");
            bool healthy;
            try { healthy = await api.PerformHealthCheck(); }
            catch (Exception ex) { LogLine($"  EXCEPTION: {ex.GetType().Name}: {ex.Message}"); return; }
            LogLine($"  OK: healthy={healthy}");
            LogLine("");
            if (!healthy)
            {
                LogLine("ABORT: server health check failed. Is the sample server running on " +
                        $"{config.serverHost}?");
                return;
            }

            // ---- 2. register / login ------------------------------------------
            LogLine("--- 2. register (doubles as login) ---");
            string jwt;
            try { jwt = await api.LoginUser(config.email, config.password); }
            catch (Exception ex) { LogLine($"  EXCEPTION: {ex.GetType().Name}: {ex.Message}"); return; }
            if (string.IsNullOrEmpty(jwt))
            {
                LogLine("ABORT: LoginUser returned no token. Check server logs.");
                return;
            }
            LogLine($"  OK: token={Mask(jwt)}");
            LogLine("");

            // ---- 3. baseline get-tokens ---------------------------------------
            await DumpWallet(api, jwt, "3. get-tokens (baseline)");

            // ---- 4. mint ------------------------------------------------------
            LogLine($"--- 4. mint {config.mintAmount} of token #{config.mintTokenId} ---");
            bool minted;
            try { minted = await api.MintToken(jwt, config.mintTokenId, config.mintAmount); }
            catch (Exception ex) { LogLine($"  EXCEPTION: {ex.GetType().Name}: {ex.Message}"); return; }
            LogLine($"  OK: success={minted}");
            LogLine("");
            if (!minted) { LogLine("ABORT: mint failed."); return; }
            await PauseBetweenSteps();
            await DumpWallet(api, jwt, "5. get-tokens (post-mint)");

            // ---- 6. melt ------------------------------------------------------
            LogLine($"--- 6. melt {config.meltAmount} of token #{config.mintTokenId} ---");
            bool melted;
            try { melted = await api.MeltToken(jwt, config.mintTokenId, config.meltAmount); }
            catch (Exception ex) { LogLine($"  EXCEPTION: {ex.GetType().Name}: {ex.Message}"); return; }
            LogLine($"  OK: success={melted}");
            LogLine("");
            if (!melted) { LogLine("ABORT: melt failed."); return; }
            await PauseBetweenSteps();
            await DumpWallet(api, jwt, "7. get-tokens (post-melt)");

            // ---- 8. transfer --------------------------------------------------
            LogLine($"--- 8. transfer {config.transferAmount} of token #{config.transferTokenId} " +
                    $"-> {config.transferRecipient} ---");
            bool transferred;
            try
            {
                transferred = await api.TransferToken(jwt, config.transferTokenId,
                    config.transferAmount, config.transferRecipient);
            }
            catch (Exception ex) { LogLine($"  EXCEPTION: {ex.GetType().Name}: {ex.Message}"); return; }
            LogLine($"  OK: success={transferred}");
            LogLine("");
            if (!transferred) { LogLine("ABORT: transfer failed."); return; }
            await PauseBetweenSteps();
            await DumpWallet(api, jwt, "9. get-tokens (post-transfer)");

            LogLine("=== Done. All steps completed. ===");
        }

        private async Task DumpWallet(EnjinApiService api, string jwt, string label)
        {
            LogLine($"--- {label} ---");
            PlatformModels.ManagedWalletAccount wallet;
            try { wallet = await api.GetManagedWalletTokens(jwt); }
            catch (Exception ex) { LogLine($"  EXCEPTION: {ex.GetType().Name}: {ex.Message}"); return; }
            if (wallet == null)
            {
                LogLine("  (null wallet returned)");
                LogLine("");
                return;
            }
            LogLine($"  address: {wallet.account?.address}");
            if (wallet.tokenAccounts == null || wallet.tokenAccounts.Length == 0)
            {
                LogLine("  tokenAccounts: (empty)");
            }
            else
            {
                foreach (var ta in wallet.tokenAccounts)
                {
                    var tid = ta.token?.tokenId ?? "?";
                    var cid = ta.token?.collection?.collectionId ?? "?";
                    LogLine($"  - tokenId={tid} balance={ta.balance} collection={cid}");
                }
            }
            LogLine("");
        }

        private async Task PauseBetweenSteps()
        {
            var sec = Mathf.Max(0f, config.pauseBetweenStepsSeconds);
            if (sec <= 0f) return;
            await Task.Delay(TimeSpan.FromSeconds(sec));
        }

        private EnjinApiService EnsureApiService()
        {
            if (EnjinApiService.Instance != null) return EnjinApiService.Instance;

            if (enjinManagerPrefab != null)
            {
                LogLine($"(instantiating EnjinManager prefab '{enjinManagerPrefab.name}')");
                Instantiate(enjinManagerPrefab);
                if (EnjinApiService.Instance != null) return EnjinApiService.Instance;
            }

            // Last-ditch: spawn a bare service so we at least have transport.
            LogLine("(no EnjinManager prefab assigned; spawning a bare EnjinApiService GameObject)");
            var go = new GameObject("EnjinApiService (smoke-fallback)");
            return go.AddComponent<EnjinApiService>();
        }

        private void OverrideHost(EnjinApiService api, string host)
        {
            // host is [SerializeField] private; reach in via reflection so we
            // don't have to mutate the production class to add a public setter.
            var field = typeof(EnjinApiService).GetField("host",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                LogLine("WARN: EnjinApiService has no 'host' field; cannot override.");
                return;
            }
            var existing = (string)field.GetValue(api);
            if (existing != host)
            {
                LogLine($"(overriding EnjinApiService.host '{existing}' -> '{host}')");
                field.SetValue(api, host);
            }
        }

        private void LogLine(string line)
        {
            Debug.Log("[GameServerSmoke] " + line);
            _buffer.AppendLine(line);
            FlushBuffer();
        }

        private void FlushBuffer()
        {
            if (outputText != null) outputText.text = _buffer.ToString();
        }

        private static string Mask(string s)
        {
            if (string.IsNullOrEmpty(s)) return "<empty>";
            return s.Length > 12 ? s.Substring(0, 6) + "..." + s.Substring(s.Length - 4) : "<short>";
        }
    }
}
