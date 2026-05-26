# Enjin SDK UPM smoke test

This folder hosts a small, hand-written smoke test that exercises the **Enjin
Platform C# SDK** package (`io.enjin.platform-sdk`) inside Unity to verify
that the UPM package works end-to-end at runtime: GraphQL transport, auth,
Newtonsoft.Json reflection-based (de)serialization, BigInteger handling, and
async/await on the Unity main thread.

It mirrors the standalone smoke test at
`platform-csharp-sdk/tools/SdkSmoke/Program.cs`. Keep them in sync when adding
new checks.

## Requirements

- The SDK is referenced from `Packages/manifest.json` via a local tarball:

  ```json
  "io.enjin.platform-sdk": "file:../../platform-csharp-sdk/EnjinPlatformSdk-v3.0.0-upm.tar.gz"
  ```

  This expects the SDK repo to be checked out next to this one. Once an
  `upm/v3.0.0` tag is published to the SDK remote, switch to the git URL:

  ```json
  "io.enjin.platform-sdk": "https://github.com/enjin/platform-csharp-sdk.git#upm/v3.0.0"
  ```

- A canary (beta) platform API token. Generate one in the platform UI under
  your account settings.

## Running it

1. Open the project in Unity 6000.0.24f1 (or later). Wait for the Package
   Manager to import `io.enjin.platform-sdk`.
2. Menu: **Enjin ▸ Open SDK Smoke Scene**. This creates
   - `Assets/EnjinSdkSmoke/SdkSmokeConfig.asset` (gitignored — contains the
     token)
   - `Assets/EnjinSdkSmoke/SdkSmoke.unity` (gitignored)
   and opens the scene with a `SdkSmokeRunner` already wired to the config.
3. Select `SdkSmokeConfig.asset` in the Project window and fill in:
   - **Platform URL** — defaults to canary
   - **Platform Token** — paste your token
   - **Account Address** — defaults to a known canary address with history
   - **Recent Block Id** — bump this when canary advances
4. Press **Play**. Output is mirrored to the Unity Console (filter for
   `[SdkSmoke]`).

## What it checks

| # | Call                                          | Verifies                                              |
| - | --------------------------------------------- | ----------------------------------------------------- |
| 1 | `GetBlock(id)` with `Number` + `Hash`         | Transport + auth                                      |
| 2 | `GetAccount(address)` with `Balance`          | Auth + BigInteger round-trip                          |
| 3 | `GetBlocks(ids)`                              | List deserialization                                  |
| 4 | `GetBlock` with `Events { CollectionId }`     | Nullable BigInteger, nested objects                   |
| 5 | `GetBlock` with `Validator` + `Extrinsics`    | Complex nested types                                  |
| 6 | `CreateManagedWallet(externalId)` (mutation)  | Mutation path; non-destructive (idempotent on retry)  |

**Known canary issue:** checks #1, #4, #5 currently fail with a server-side
GraphQL error on canary. The checks are left in place so we see when canary
recovers. Checks #2, #3, #6 should all pass against a healthy canary.

## IL2CPP / standalone builds

The package ships `Runtime/link.xml` which preserves the entire
`Enjin.Platform.Sdk` assembly under IL2CPP code stripping. If you build a
standalone player and see Newtonsoft.Json deserialization failures, confirm:

- The link.xml is being honored (check the build log for "Preserving 'Enjin.Platform.Sdk'").
- `com.unity.nuget.newtonsoft-json` is in the manifest (transitively required).

## Files

- `Scripts/SdkSmokeConfig.cs` — `ScriptableObject` for endpoint + token + inputs.
- `Scripts/SdkSmokeRunner.cs` — `MonoBehaviour` that runs the 6 checks.
- `Scripts/EnjinSdkSmoke.asmdef` — runtime assembly, references the SDK DLL.
- `Editor/SdkSmokeSceneBuilder.cs` — `Enjin ▸ Open SDK Smoke Scene` menu item.
- `Editor/EnjinSdkSmoke.Editor.asmdef` — editor-only assembly.
- `.gitignore` — keeps the populated config + locally-generated scene out of git.
