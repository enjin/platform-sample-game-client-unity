using System;
using UnityEngine;

namespace GameServerSmoke
{
    /// <summary>
    /// Runtime configuration for the game-server smoke test (the Unity client
    /// hitting our C# REST backend, not the SDK directly).
    ///
    /// Create an instance via Assets ▸ Create ▸ Enjin ▸ Game Server Smoke Config,
    /// then drag it onto the <see cref="GameServerSmokeRunner"/> component in
    /// the scene built by Enjin ▸ Open Game Server Smoke Scene.
    ///
    /// The created .asset file is gitignored (see Assets/GameServerSmoke/.gitignore)
    /// because it can hold a player password. Never commit a populated config.
    /// </summary>
    [CreateAssetMenu(fileName = "GameServerSmokeConfig", menuName = "Enjin/Game Server Smoke Config", order = 1)]
    public sealed class GameServerSmokeConfig : ScriptableObject
    {
        [Header("Server endpoint")]
        [Tooltip("Base URL of the sample game server, including scheme. Example: http://localhost:3000")]
        public string serverHost = "http://localhost:3000";

        [Header("Player credentials")]
        [Tooltip("Email used for /api/auth/register. The endpoint doubles as login: a brand-new email creates a player + managed wallet; an existing email returns its JWT.")]
        public string email = "";

        [Tooltip("Password used for /api/auth/register.")]
        public string password = "";

        [Header("Token IDs (must match server resource token config)")]
        [Tooltip("Token id to mint and melt. Defaults to Gold Coin (1).")]
        public string mintTokenId = "1";

        [Tooltip("Token id to transfer. Defaults to Gold Coin (1).")]
        public string transferTokenId = "1";

        [Header("Amounts")]
        [Tooltip("How many of mintTokenId to mint.")]
        public int mintAmount = 5;

        [Tooltip("How many of mintTokenId to melt after minting.")]
        public int meltAmount = 2;

        [Tooltip("How many of transferTokenId to transfer to the recipient.")]
        public int transferAmount = 1;

        [Header("Transfer recipient")]
        [Tooltip("SS58 address that receives the transfer. Defaults to the canary daemon wallet, which the server is happy to receive funds at.")]
        public string transferRecipient = "cxNE5bEPcdpfbsMfdLka11Jj1QH7gihFcc9uKqXKtepcQhkPS";

        [Header("Behavior")]
        [Tooltip("Run the smoke test automatically when the scene starts. If false, call GameServerSmokeRunner.RunSmoke() yourself (e.g. from a UI button).")]
        public bool runOnStart = true;

        [Tooltip("Seconds to wait between mutation requests and the follow-up get-tokens read. Mint/melt/transfer go through finalization on-chain, which takes ~10-20s. The server already waits internally, but a small cushion helps the read see fresh state.")]
        public float pauseBetweenStepsSeconds = 1.0f;
    }
}
