using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Threading.Tasks;
using HappyHarvest.EnjinIntegration.API;
using HappyHarvest.EnjinIntegration.Data;
using HappyHarvest.EnjinIntegration.Gameplay;

namespace HappyHarvest.EnjinIntegration.Core {
    public class EnjinManager : MonoBehaviour
    {
        public static EnjinManager Instance { get; private set; }

        // Private variable to store the user's authentication token.
        private string _authToken = null;

        public float tokenRevealProbabilityThreshold = 0.5f;
        [SerializeField]
        public EnjinToken[] mBlockchainTokens;
        public event System.Action<bool> OnLoginComplete;
        public event System.Action<bool> OnLogoutComplete;
        public event System.Action OnWalletUpdated;


        public PlatformModels.ManagedWalletAccount walletAccount;

        private EnjinManager() { }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            Instance.LoadTokenFromPlayerPrefs();
            GetManagedWalletTokens();
        }

        async public Task GetManagedWalletTokens()
        {
            if (IsLoggedIn())
            {
                PlatformModels.ManagedWalletAccount allWalletTokens = await EnjinApiService.Instance.GetManagedWalletTokens(_authToken);

                // Exit if the wallet data is null or contains no token accounts.
                if (allWalletTokens == null || allWalletTokens.tokenAccounts == null)
                {
                    Debug.LogWarning("No tokens found in the managed wallet or failed to fetch.");
                    // Ensure walletAccount is initialized to avoid null reference issues.
                    walletAccount = new PlatformModels.ManagedWalletAccount { account = allWalletTokens?.account, tokenAccounts = new PlatformModels.TokenAccount[0] };
                    return;
                }

                // Create a lookup set of in-game token identifiers for efficient filtering.
                // We convert the SerializableBigInteger IDs to strings for comparison.
                var inGameTokenIds = new HashSet<(string, string)>(
                    mBlockchainTokens.Select(token => (
                        token.item.collectionId.ToString(),
                        token.item.tokenId.ToString()
                    ))
                );

                // Filter the token accounts from the wallet. A token account is kept only if its
                // collectionId and tokenId match an entry in our in-game token set.
                PlatformModels.TokenAccount[] filteredTokenAccounts = allWalletTokens.tokenAccounts.Where(walletTokenAccount =>
                {
                    string collectionId = walletTokenAccount.token.collection.collectionId;
                    string tokenId = walletTokenAccount.token.tokenId;

                    // Check if the token from the wallet exists in our set of in-game tokens.
                    return inGameTokenIds.Contains((collectionId, tokenId));
                }).ToArray();

                // Construct the final walletAccount object with the filtered list of tokens.
                walletAccount = new PlatformModels.ManagedWalletAccount
                {
                    account = allWalletTokens.account,
                    tokenAccounts = filteredTokenAccounts
                };
            }
        }

        async public Task MintToken(string tokenId, int amount)
        {
            if (IsLoggedIn())
            {
                bool success = await EnjinApiService.Instance.MintToken(_authToken, tokenId, amount);
                if (success)
                {
                    OnWalletUpdated?.Invoke();
                }
            }
        }

        async public Task MeltToken(string tokenId, int amount)
        {
            if (IsLoggedIn())
            {
                bool success = await EnjinApiService.Instance.MeltToken(_authToken, tokenId, amount);
                if (success)
                {
                    OnWalletUpdated?.Invoke();
                }
            }
        }

        async public Task TransferToken(string tokenId, int amount, string recipient)
        {
            if (IsLoggedIn())
            {
                bool success = await EnjinApiService.Instance.TransferToken(_authToken, tokenId, amount, recipient);
                if (success)
                {
                    OnWalletUpdated?.Invoke();
                }
            }
        }

        /// <summary>
        /// Checks if the user is currently logged in by verifying the auth token.
        /// </summary>
        /// <returns>True if an auth token exists, otherwise false.</returns>
        public bool IsLoggedIn()
        {
            // For this example, we just check if the token is not null or empty.
            // In a real app, you might want to validate the token's format or expiry.
            bool loggedIn = !string.IsNullOrEmpty(_authToken);
            //Debug.Log($"IsLoggedIn check: {loggedIn}");
            return loggedIn;
        }

        /// <summary>
        /// Simulates a login attempt with an email and password.
        /// In a real app, this would make a web request to your backend.
        /// </summary>
        public async Task RegisterAndLogin(string email, string password)
        {
            Debug.Log($"Attempting to log in with email: {email}");
            string loginResponse = await EnjinApiService.Instance.LoginUser(email, password);
            if (string.IsNullOrEmpty(loginResponse))
            {
                Debug.Log("Login failed.");
                OnLoginComplete?.Invoke(false);
                return;
            }
            _authToken = loginResponse;
            SaveTokenToPlayerPrefs();
            Debug.Log("Login successful. Token generated.");
            OnLoginComplete?.Invoke(true);
        }

        /// <summary>
        /// Logs the user out by clearing the auth token and saved data.
        /// </summary>
        public void Logout()
        {
            Debug.Log("Logging out.");
            _authToken = null;
            PlayerPrefs.SetString("playerAuthToken", "");
            PlayerPrefs.Save();
            OnLogoutComplete?.Invoke(true);
        }

        /// <summary>
        /// Saves the current auth token to the device's local storage.
        /// </summary>
        private void SaveTokenToPlayerPrefs()
        {
            if (string.IsNullOrEmpty(_authToken)) return;
            PlayerPrefs.SetString("playerAuthToken", _authToken);
            PlayerPrefs.Save();
            Debug.Log("Token saved to PlayerPrefs.");
        }

        /// <summary>
        /// Loads the auth token from the device's local storage.
        /// </summary>
        private void LoadTokenFromPlayerPrefs()
        {
            _authToken = PlayerPrefs.GetString("playerAuthToken", null);
            if (!string.IsNullOrEmpty(_authToken))
            {
                Debug.Log("Token loaded from PlayerPrefs.");
            }
        }

        public EnjinToken GetToken(PlatformModels.SerializableBigInteger collectionId, PlatformModels.SerializableBigInteger tokenId)
        {
            if (mBlockchainTokens == null || mBlockchainTokens.Length == 0)
                return null;

            // Find the token by tokenId and collectionId
            return mBlockchainTokens.FirstOrDefault(token =>
                token.item.tokenId.Equals(tokenId) &&
                token.item.collectionId.Equals(collectionId));
        }

        public void RandomlyRevealToken(Vector3Int target)
        {
            bool shouldRevealToken = ShouldPerformAction(tokenRevealProbabilityThreshold);

            if (shouldRevealToken)
            {
                IEnjinToken tokenToReveal = null;
                do
                {
                    tokenToReveal = mBlockchainTokens[Random.Range(0, mBlockchainTokens.Length)];
                } while (!ShouldPerformAction(tokenToReveal.GetRarity));

                Item tokenItem = tokenToReveal.GetItem();
                Debug.Log($"Revealed token: {tokenItem.name}");

                Instantiate(tokenItem.VisualPrefab, target - new Vector3(-0.5f, -0.5f, 0), Quaternion.identity);
            }
        }

        public bool ShouldPerformAction(float probabilityThreshold = 0.5f)
        {
            // Generate a random value between 0 and 1
            float randomValue = Random.value;

            // Return true if the random value is higher than the threshold, otherwise false
            return randomValue > probabilityThreshold;
        }
    }
}