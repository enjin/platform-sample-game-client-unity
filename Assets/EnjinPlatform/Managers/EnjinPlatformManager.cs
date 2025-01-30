using EnjinPlatform.Data;
using EnjinPlatform.Services;
using GraphQlClient.Core;
using System.Threading.Tasks;
using UnityEngine;

namespace EnjinPlatform.Managers
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
                LoadManagedWalletAccountAsync();
            }
            
        }
        
        async void LoadManagedWalletAccountAsync()
        {
            await LoadManagedWalletAccount();
        }
        
        async Task LoadManagedWalletAccount()
        {
            Debug.Log(EnjinPlatformService.Instance.ManagedWalletAccount);
            
            if (EnjinPlatformService.Instance.ManagedWalletAccount == null)
            {
                await EnjinPlatformService.Instance.CreateManagedWalletAccount();
                await EnjinPlatformService.Instance.GetManagedWalletAccount();
            }
            
            if (EnjinPlatformService.Instance.ManagedWalletAccount != null)
            {
                Debug.Log("Managed Wallet Account loaded: " + EnjinPlatformService.Instance.ManagedWalletAccount.publicKey);
                if (EnjinPlatformService.Instance.ManagedWalletAccount.tokens != null)
                {
                    foreach (var tokenAccount in EnjinPlatformService.Instance.ManagedWalletAccount.tokens)
                    {
                        EnjinBlockchainToken currentToken = EnjinGameManager.Instance.GetToken(tokenAccount.token.collectionId, tokenAccount.token.tokenId);
                        if(currentToken != null)
                            Debug.Log($"Token: {currentToken.item.DisplayName} ({currentToken.item.BlockchainCollectionId}), Balance: {tokenAccount.balance}");
                    }
                }
                else
                {
                    Debug.Log("No tokens found in the managed wallet account.");
                }
                
                await EnjinWebsocketManager.instance.SubscribeToWalletAccountChannel(EnjinPlatformService.Instance.ManagedWalletAccount.publicKey);
            }
            else
            {
                Debug.Log("Failed to load Managed Wallet Account.");
            }
        }
    }
}