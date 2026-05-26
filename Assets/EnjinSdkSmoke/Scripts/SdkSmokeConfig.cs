using UnityEngine;

namespace EnjinSdkSmoke
{
    /// <summary>
    /// Runtime configuration for the Enjin Platform SDK smoke test.
    ///
    /// Create an instance via Assets ▸ Create ▸ Enjin ▸ SDK Smoke Config, then
    /// drag it onto the <see cref="SdkSmokeRunner"/> component in the scene.
    ///
    /// The created .asset file is gitignored (see Assets/EnjinSdkSmoke/.gitignore)
    /// because it holds an API token. Never commit a populated config.
    /// </summary>
    [CreateAssetMenu(fileName = "SdkSmokeConfig", menuName = "Enjin/SDK Smoke Config", order = 0)]
    public sealed class SdkSmokeConfig : ScriptableObject
    {
        [Header("Platform endpoint")]
        [Tooltip("Full GraphQL endpoint URL. Canary: https://platform.beta.enjin.io/graphql  Production: https://platform.enjin.io/graphql")]
        public string platformUrl = "https://platform.beta.enjin.io/graphql";

        [Tooltip("Personal access token generated in the Platform UI under your account settings. Treat as a secret.")]
        public string platformToken = "";

        [Header("Smoke test inputs")]
        [Tooltip("A known SS58 address with on-chain history. Used by the GetAccount check.")]
        public string accountAddress = "cxNE5bEPcdpfbsMfdLka11Jj1QH7gihFcc9uKqXKtepcQhkPS";

        [Tooltip("A recent block id (number). The GetBlock/GetBlocks checks query around this id. Bump it occasionally as canary advances.")]
        public int recentBlockId = 11240208;

        [Header("Behavior")]
        [Tooltip("Run the smoke test automatically when the scene starts. If false, call SdkSmokeRunner.RunSmoke() yourself (e.g. from a UI button).")]
        public bool runOnStart = true;
    }
}
