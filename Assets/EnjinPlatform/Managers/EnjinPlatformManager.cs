using EnjinPlatform.Services;
using GraphQlClient.Core;
using System.Threading.Tasks;
using UnityEngine;

namespace EnjinPlatform.Controllers
{
    public class EnjinPlatformManager : MonoBehaviour
    {
        public GraphApi mGraphApiReference;
        
        void Awake()
        {
            EnjinPlatformService.Instance.AuthToken = PlayerPrefs.GetString("authToken");
            Debug.Log(EnjinPlatformService.Instance.AuthToken);
            
            if (!string.IsNullOrEmpty(EnjinPlatformService.Instance.AuthToken))
            {
                LoadManagedWalletAccount();
            }
            
        }
        
        async Task LoadManagedWalletAccount()
        {
            if (string.IsNullOrEmpty(EnjinPlatformService.Instance.ManagedWalletAccount))
            {
                await EnjinPlatformService.Instance.CreateManagedWalletAccount();
                await EnjinPlatformService.Instance.GetManagedWalletAccount();
            }
            
            if (EnjinPlatformService.Instance.ManagedWalletAccount != null)
            {
                Debug.Log("Managed Wallet Account loaded: " + EnjinPlatformService.Instance.ManagedWalletAccount);
            }
            else
            {
                Debug.Log("Failed to load Managed Wallet Account.");
            }
        }
    }
}