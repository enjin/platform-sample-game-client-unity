using System.Linq;
using EnjinPlatform.Data;
using EnjinPlatform.Interfaces;
using HappyHarvest;
using UnityEngine;
using Random = UnityEngine.Random;

namespace EnjinPlatform.Managers
{
    public class EnjinGameManager : MonoBehaviour
    {
        private static EnjinGameManager s_Instance;
        
        public float tokenRevealProbabilityThreshold = 0.5f;
        
        [SerializeField]
        public EnjinBlockchainToken[] mBlockchainTokens;
        
        
#if UNITY_EDITOR
        //As our manager run first, it will also be destroyed first when the app will be exiting, which lead to s_Instance
        //to become null and so will trigger another instantiate in edit mode (as we dynamically instantiate the Manager)
        //so this is set to true when destroyed, so we do not reinstantiate a new one
        private static bool s_IsQuitting = false;
#endif
        public static EnjinGameManager Instance 
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying || s_IsQuitting)
                    return null;
                
                if (s_Instance == null)
                {
                    //in editor, we can start any scene to test, so we are not sure the game manager will have been
                    //created by the first scene starting the game. So we load it manually. This check is useless in
                    //player build as the 1st scene will have created the GameManager so it will always exist.
                    Instantiate(Resources.Load<EnjinGameManager>("EnjinGameManager"));
                }
#endif
                return s_Instance;
            }
        }
        
        private void Awake()
        {
            s_Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
#if UNITY_EDITOR
        private void OnDestroy()
        {
            s_IsQuitting = true;
        }
#endif
        public EnjinBlockchainToken GetToken(SerializableBigInteger collectionId, SerializableBigInteger tokenId)
        {
            if (mBlockchainTokens == null || mBlockchainTokens.Length == 0)
                return null;

            // Find the token by tokenId and collectionId
            return mBlockchainTokens.FirstOrDefault(token =>
                token.item.BlockchainTokenId.Equals(tokenId) &&
                token.item.BlockchainCollectionId.Equals(collectionId));
        }
        
        public void RandomlyRevealToken(Vector3Int target)
        {
            bool shouldRevealToken = ShouldPerformAction(tokenRevealProbabilityThreshold);
            
            if (shouldRevealToken)
            {
                IEnjinBlockchainToken tokenToReveal = null;
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