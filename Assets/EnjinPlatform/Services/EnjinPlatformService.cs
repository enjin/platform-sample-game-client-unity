using System.Threading.Tasks;
using EnjinPlatform.Managers;
using GraphQlClient.Core;
using HappyHarvest;
using UnityEngine;
using UnityEngine.Networking;

namespace EnjinPlatform.Services
{
    public class EnjinPlatformService
    {
        private static EnjinPlatformService _instance;
        private static readonly object Lock = new object();

        private static GraphApi _mGraphApi;
        private string _mAuthToken = null;
        public string AuthToken
        {
            get => _mAuthToken;
            set => _mAuthToken = value;
        }
        
        public User Player { get; private set; }
        public WalletAccount ManagedWalletAccount { get; private set; }
        
        public event System.Action<bool> OnLoginSuccess;


        private EnjinPlatformService() { }

        public static EnjinPlatformService Instance
        {
            get
            {
                lock (Lock)
                {
                    if (_instance == null)
                    {
                        _instance = new EnjinPlatformService();
                        _mGraphApi = GameManager.Instance.GetComponent<EnjinPlatformManager>().mGraphApiReference;
                    }
                    return _instance;
                }
            }
        }
        
        public bool IsLoggedIn()
        {
            string pattern = @"^\d+\|[A-Za-z0-9]{48}$";
            
            Debug.Log("auth token " + _mAuthToken);
            
            return System.Text.RegularExpressions.Regex.IsMatch(_mAuthToken, pattern);
        }
        
        public async void RegisterAndLogin(string email, string password)
        {
            // Gets the needed query from the Api Reference
            GraphApi.Query login = _mGraphApi.GetQueryByName("RegisterAndLoginUser", GraphApi.Query.Type.Mutation);
            
            // Sets the arguments for the query
            login.SetArgs(new {email = email, password = password});
            
            _mGraphApi.SetAuthToken(null);
            
            // Sends the query to the server
            UnityWebRequest request = await _mGraphApi.Post(login);
            
            string response = request.downloadHandler.text;
            DataResponse<RegisterAndLoginUserResponse> dataResponse = JsonUtility.FromJson<DataResponse<RegisterAndLoginUserResponse>>(response);
            _mAuthToken = dataResponse.data.RegisterAndLoginUser;
            
            Debug.Log(_mAuthToken);

            if (_mAuthToken != "unauthorized")
            {
                _mGraphApi.SetAuthToken(_mAuthToken);
            
                PlayerPrefs.SetString("authToken", _mAuthToken);
                PlayerPrefs.Save();

                OnLoginSuccess?.Invoke(true);
            }
            else
            {
                OnLoginSuccess?.Invoke(false);   
            }
        }
        
        public bool Logout()
        {
            _mAuthToken = null;
            PlayerPrefs.SetString("authToken", "");
            PlayerPrefs.Save();
            
            return true;
        }
        
        public async Task<User> GetPlayer()
        {
            if (!IsLoggedIn())
            {
                Debug.Log("User not logged in.");
                
                return null;
            }
            
            if (Player != null)
            {
                return Player;
            }
            
            // Gets the needed query from the Api Reference
            GraphApi.Query getPlayer = _mGraphApi.GetQueryByName("GetUser", GraphApi.Query.Type.Query);
            
            // Sends the query to the server
            UnityWebRequest request = await _mGraphApi.Post(getPlayer);
            
            // Parses the response
            string response = request.downloadHandler.text;
            
            Player = JsonUtility.FromJson<User>(response);

            return Player;
        }
        
        public async Task<bool> CreateManagedWalletAccount()
        {
            if (!IsLoggedIn())
            {
                Debug.Log("User not logged in.");
                
                return false;
            }
            
            // Gets the needed query from the Api Reference
            GraphApi.Query createManagedWalletAccount = _mGraphApi.GetQueryByName("CreateManagedWalletAccount", GraphApi.Query.Type.Mutation);
            
            // Sends the query to the server
            UnityWebRequest request = await _mGraphApi.Post(createManagedWalletAccount);
            
            // Parses the response
            string response = request.downloadHandler.text;
            
            DataResponse<bool> dataResponse = JsonUtility.FromJson<DataResponse<bool>>(response);

            return dataResponse.data;
        }
        
        public async Task<string> GetManagedWalletAccount()
        {
            if (!IsLoggedIn())
            {
                Debug.Log("User not logged in.");
                
                return null;
            }
            
            if (ManagedWalletAccount != null)
            {
                return ManagedWalletAccount.address;
            }
            
            // Gets the needed query from the Api Reference
            GraphApi.Query getManagedWallet = _mGraphApi.GetQueryByName("GetManagedWalletAccount", GraphApi.Query.Type.Query);
            
            // Sends the query to the server
            UnityWebRequest request = await _mGraphApi.Post(getManagedWallet);
            
            // Parses the response
            string response = request.downloadHandler.text;
            Debug.Log(response);
            
            DataResponse<ManagedWalletAccountResponse> dataResponse = JsonUtility.FromJson<DataResponse<ManagedWalletAccountResponse>>(response);
            
            ManagedWalletAccount = dataResponse.data.GetManagedWalletAccount;
            
            Debug.Log("Managed Wallet Account " + ManagedWalletAccount.address);

            return ManagedWalletAccount.address;
        }
        
        [System.Serializable]
        public class DataResponse<T>
        {
            public T data;
        }
        
        [System.Serializable]
        public class RegisterAndLoginUserResponse
        {
            public string RegisterAndLoginUser;
        }
        
        [System.Serializable]
        public class ManagedWalletAccountResponse
        {
            public WalletAccount GetManagedWalletAccount;
        }
        
        [System.Serializable]
        public class User
        {
            public string id;
            public string uuid;
            public string email;
        }
        
        [System.Serializable]
        public class WalletAccount
        {
            public string address;
            public Token[] tokens;
        }
        
        [System.Serializable]
        public class Token
        {
            public string collectionId;
            public string tokenId;
            public string name;
            public Attribute[] attributes;
        }
        
        [System.Serializable]
        public class Attribute
        {
            public string key;
            public string value;
        }
    }
}