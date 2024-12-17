using System.Threading.Tasks;
using EnjinPlatform.Controllers;
using GraphQlClient.Core;
using HappyHarvest;
using UnityEngine;
using UnityEngine.Networking;

namespace EnjinPlatform.Services
{
    public class EnjinPlatformService
    {
        private static EnjinPlatformService _instance;
        private static readonly object _lock = new object();

        private static GraphApi m_GraphApi;
        private string m_AuthToken = null;
        public string AuthToken
        {
            get { return m_AuthToken; }
            set { m_AuthToken = value; }
        }
        
        public User Player { get; private set; }
        public string ManagedWalletAccount { get; private set; }
        
        public event System.Action<bool> OnLoginSuccess;


        private EnjinPlatformService() { }

        public static EnjinPlatformService Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new EnjinPlatformService();
                        m_GraphApi = GameManager.Instance.GetComponent<EnjinPlatformManager>().mGraphApiReference;
                    }
                    return _instance;
                }
            }
        }
        
        public bool IsLoggedIn()
        {
            string pattern = @"^\d+\|[A-Za-z0-9]{48}$";
            
            Debug.Log("auth token " + m_AuthToken);
            
            return System.Text.RegularExpressions.Regex.IsMatch(m_AuthToken, pattern);
        }
        
        public async void RegisterAndLogin(string email, string password)
        {
            // Gets the needed query from the Api Reference
            GraphApi.Query login = m_GraphApi.GetQueryByName("RegisterAndLoginUser", GraphApi.Query.Type.Mutation);
            
            // Sets the arguments for the query
            login.SetArgs(new {email = email, password = password});
            
            m_GraphApi.SetAuthToken(null);
            
            // Sends the query to the server
            UnityWebRequest request = await m_GraphApi.Post(login);
            
            string response = request.downloadHandler.text;
            DataResponse<RegisterAndLoginUserResponse> dataResponse = JsonUtility.FromJson<DataResponse<RegisterAndLoginUserResponse>>(response);
            m_AuthToken = dataResponse.data.RegisterAndLoginUser;
            
            Debug.Log(m_AuthToken);

            if (m_AuthToken != "unauthorized")
            {
                m_GraphApi.SetAuthToken(m_AuthToken);
            
                PlayerPrefs.SetString("authToken", m_AuthToken);
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
            m_AuthToken = null;
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
            GraphApi.Query getPlayer = m_GraphApi.GetQueryByName("GetUser", GraphApi.Query.Type.Query);
            
            // Sends the query to the server
            UnityWebRequest request = await m_GraphApi.Post(getPlayer);
            
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
            GraphApi.Query createManagedWalletAccount = m_GraphApi.GetQueryByName("CreateManagedWalletAccount", GraphApi.Query.Type.Mutation);
            
            // Sends the query to the server
            UnityWebRequest request = await m_GraphApi.Post(createManagedWalletAccount);
            
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
            
            if (!string.IsNullOrEmpty(ManagedWalletAccount))
            {
                return ManagedWalletAccount;
            }
            
            // Gets the needed query from the Api Reference
            GraphApi.Query getManagedWallet = m_GraphApi.GetQueryByName("GetManagedWalletAccount", GraphApi.Query.Type.Query);
            
            // Sends the query to the server
            UnityWebRequest request = await m_GraphApi.Post(getManagedWallet);
            
            // Parses the response
            string response = request.downloadHandler.text;
            
            DataResponse<string> dataResponse = JsonUtility.FromJson<DataResponse<string>>(response);
            
            ManagedWalletAccount = dataResponse.data;
            
            Debug.Log("Managed Wallet Account " + ManagedWalletAccount);

            return dataResponse.data;
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
        public class User
        {
            public string id;
            public string uuid;
            public string email;
        }
    }
}