using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using HappyHarvest.EnjinIntegration.Core;
using HappyHarvest.EnjinIntegration.Data;

namespace HappyHarvest.EnjinIntegration.API
{
    public class EnjinApiService : MonoBehaviour
    {
        #region Singleton Implementation

        public static EnjinApiService Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        #endregion

        [Header("Server Configuration")]
        [SerializeField] private string host;

        async void Start()
        {
            await PerformHealthCheck();
        }

        /// <summary>
        /// Performs a health check against the server
        /// </summary>
        /// <returns>A Task that resolves to 'true' if the health check is successful, otherwise 'false'.</returns>
        public async Task<bool> PerformHealthCheck()
        {
            var getHealthCheckRequest = CreateRequest(this.host + "/api/auth/health-check");

            try
            {
                await getHealthCheckRequest.SendWebRequest();

                if (getHealthCheckRequest.result == UnityWebRequest.Result.Success)
                {
                    var deserializedData = JsonUtility.FromJson<HealthCheckResponse>(getHealthCheckRequest.downloadHandler.text);
                    if (deserializedData?.status == "OK")
                    {
                        Debug.Log("Server connection successful (Health Check: OK).");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"An error occurred during health check: {ex.Message}");
            }

            Debug.LogError($"Server connection failed or returned an error: {getHealthCheckRequest.error}");
            return false;
        }
        public async Task<string> LoginUser(string email, string password)
        {
            var loginDataToPost = new LoginRequest() { email = email, password = password };
            var postRequest = CreateRequest(this.host + "/api/auth/register", RequestType.POST, loginDataToPost);

            try
            {
                await postRequest.SendWebRequest();

                if (postRequest.result == UnityWebRequest.Result.Success)
                {
                    var deserializedGetData = JsonUtility.FromJson<RegisterResponse>(postRequest.downloadHandler.text);
                    if (!string.IsNullOrEmpty(deserializedGetData?.token))
                    {
                        Debug.Log("Login successful. Token received:" + deserializedGetData.token);
                        return deserializedGetData.token;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"An error occurred during login: {ex.Message}");
            }

            Debug.LogError($"Login failed or returned an error: {postRequest.error}");
            return null;
        }

        public async Task<bool> MintToken(string userAuth, string tokenId, int amount)
        {
            var mintDataToPost = new MintRequest() { tokenId = tokenId, amount = amount };
            var mintTokenRequest = CreateRequest(this.host + "/api/token/mint", RequestType.POST, mintDataToPost);
            AttachHeader(mintTokenRequest, "authorization", "bearer " + userAuth);

            try
            {
                await mintTokenRequest.SendWebRequest();

                if (mintTokenRequest.result == UnityWebRequest.Result.Success)
                {
                    var mintedToken = JsonUtility.FromJson<BoolResponse>(mintTokenRequest.downloadHandler.text);
                    if (mintedToken.success)
                    {
                        Debug.Log("Successfully minted token #" + tokenId);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"An error occurred during minting of #{tokenId}: {ex.Message}");
            }

            Debug.LogError($"minting of #{tokenId} failed or returned an error: {mintTokenRequest.error}");
            return false;
        }

        public async Task<bool> MeltToken(string userAuth, string tokenId, int amount)
        {
            var meltDataToPost = new MeltRequest() { tokenId = tokenId, amount = amount };
            var meltTokenRequest = CreateRequest(this.host + "/api/token/melt", RequestType.POST, meltDataToPost);
            AttachHeader(meltTokenRequest, "authorization", "bearer " + userAuth);

            try
            {
                await meltTokenRequest.SendWebRequest();

                if (meltTokenRequest.result == UnityWebRequest.Result.Success)
                {
                    var meltedToken = JsonUtility.FromJson<BoolResponse>(meltTokenRequest.downloadHandler.text);
                    if (meltedToken.success)
                    {
                        Debug.Log("Successfully melted token #" + tokenId);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"An error occurred during melting of #{tokenId}: {ex.Message}");
            }

            Debug.LogError($"melting of #{tokenId} failed or returned an error: {meltTokenRequest.error}");
            return false;
        }

        public async Task<bool> TransferToken(string userAuth, string tokenId, int amount, string recipient)
        {
            var transferDataToPost = new TransferRequest() { tokenId = tokenId, amount = amount, recipient = recipient };
            var transferTokenRequest = CreateRequest(this.host + "/api/token/transfer", RequestType.POST, transferDataToPost);
            AttachHeader(transferTokenRequest, "authorization", "bearer " + userAuth);

            try
            {
                await transferTokenRequest.SendWebRequest();

                if (transferTokenRequest.result == UnityWebRequest.Result.Success)
                {
                    var transferredToken = JsonUtility.FromJson<BoolResponse>(transferTokenRequest.downloadHandler.text);
                    if (transferredToken.success)
                    {
                        Debug.Log("Successfully transferred token #" + tokenId);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"An error occurred during transfer of #{tokenId}: {ex.Message}");
            }

            Debug.LogError($"transfer of #{tokenId} failed or returned an error: {transferTokenRequest.error}");
            return false;
        }

        public UnityWebRequest CreateRequest(string path, RequestType type = RequestType.GET, object data = null)
        {
            var request = new UnityWebRequest(path, type.ToString());

            if (data != null)
            {
                var bodyRaw = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data));
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            }

            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            return request;
        }

        public void AttachHeader(UnityWebRequest request, string key, string value)
        {
            request.SetRequestHeader(key, value);
        }

        public async Task<PlatformModels.ManagedWalletAccount> GetManagedWalletTokens(string userAuth)
        {
            var getManagedWalletTokensRequest = CreateRequest(this.host + "/api/wallet/get-tokens");
            AttachHeader(getManagedWalletTokensRequest, "authorization", "bearer " + userAuth);

            try
            {
                await getManagedWalletTokensRequest.SendWebRequest();

                // Check for an unauthorized error
                if (getManagedWalletTokensRequest.responseCode == 401)
                {
                    Debug.LogWarning("Authorization failed (401). Forcing logout.");
                    EnjinManager.Instance.Logout();
                    return null;
                }

                if (getManagedWalletTokensRequest.result == UnityWebRequest.Result.Success)
                {
                    var ManagedWalletAccount = JsonUtility.FromJson<PlatformModels.ManagedWalletAccount>(getManagedWalletTokensRequest.downloadHandler.text);
                    return ManagedWalletAccount;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"An error occurred during GetManagedWalletTokens: {ex.Message}");
            }

            Debug.LogError($"GetManagedWalletTokens failed or returned an error: {getManagedWalletTokensRequest.error}");
            return null;
        }
    }
}