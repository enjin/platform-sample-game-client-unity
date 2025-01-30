using EnjinPlatform.Data;
using EnjinPlatform.Events;
using UnityEngine;

namespace EnjinPlatform.Services
{
    public class EnjinPlatformItemService
    {
        public SerializableBigInteger BlockchainCollectionId { get; set; }
        public SerializableBigInteger BlockchainTokenId { get; set; }
        
        public void Transfer(string recipient, int amount = 1)
        {
            Debug.Log("Transferring " + amount + " of token");
            Debug.Log(BlockchainCollectionId);
            Debug.Log(BlockchainTokenId);
        
            ItemTransferredEventData data = new ItemTransferredEventData
            {
                collectionId = BlockchainCollectionId.Value.ToString(),
                tokenId = BlockchainTokenId.Value.ToString(),
                amount = amount,
                recipient = recipient,
                signingAccount = EnjinPlatformService.Instance.ManagedWalletAccount.publicKey
            };
        
            Debug.Log(data.GetEncodedData());
        
            EnjinPlatformService.Instance.LogGameEvent(GameEventType.ITEM_TRANSFERRED, data.GetEncodedData());
        }
    
        public void Melt(int amount = 1)
        {
            Debug.Log("Melting " + amount + " of token");
            Debug.Log(BlockchainCollectionId);
            Debug.Log(BlockchainTokenId);
        
            ItemMeltedEventData data = new ItemMeltedEventData
            {
                collectionId = BlockchainCollectionId.Value.ToString(),
                tokenId = BlockchainTokenId.Value.ToString(),
                amount = amount,
                signingAccount = EnjinPlatformService.Instance.ManagedWalletAccount.publicKey
            };
        
            Debug.Log(data.GetEncodedData());
        
            EnjinPlatformService.Instance.LogGameEvent(GameEventType.ITEM_MELTED, data.GetEncodedData());
        }
        
        public void Collect()
        {
            Debug.Log("Collecting token");
            Debug.Log(BlockchainCollectionId);
            Debug.Log(BlockchainTokenId);
        
            ItemCollectedEventData data = new ItemCollectedEventData
            {
                collectionId = BlockchainCollectionId.Value.ToString(),
                tokenId = BlockchainTokenId.Value.ToString(),
                amount = 1
            };
        
            Debug.Log(data.GetEncodedData());
        
            EnjinPlatformService.Instance.LogGameEvent(GameEventType.ITEM_COLLECTED, data.GetEncodedData());
        }
        
        [System.Serializable]
        public class ItemCollectedEventData: ItemEventData
        {
            public int amount;
        }

        [System.Serializable]
        public class ItemMeltedEventData : ItemCollectedEventData
        {
            public string signingAccount;
        }
    
        [System.Serializable]
        public class ItemTransferredEventData: ItemEventData
        {
            public int amount;
            public string recipient;
            public string signingAccount;
        }
    
        [System.Serializable]
        public class ItemEventData: GameEventData
        {
            public string collectionId;
            public string tokenId;
        }
    }
}