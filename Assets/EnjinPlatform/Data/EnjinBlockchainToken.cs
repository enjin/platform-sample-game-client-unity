using EnjinPlatform.Data;
using EnjinPlatform.Events;
using EnjinPlatform.Interfaces;
using EnjinPlatform.Services;
using HappyHarvest;
using UnityEngine;

public class EnjinBlockchainToken : InteractiveObject, IEnjinBlockchainToken
{
    public EnjinBlockchainItem item;
    [Tooltip("The rarity of the token. Higher numbers indicates the token is more rare and reduce the chance of being found.")]
    [Range(0f, 1.0f)]
    public float rarity;
    
    public float GetRarity { get => rarity; }
    
    public override void InteractedWith()
    {
        Debug.Log("Interacted with token");
        
        Collect();
    }
    
    public void Collect()
    {
        Debug.Log("Collecting token");
        Debug.Log(item.BlockchainCollectionId);
        Debug.Log(item.BlockchainTokenId);
        
        ItemCollectedEventData data = new ItemCollectedEventData
        {
            collectionId = item.BlockchainCollectionId.Value.ToString(),
            tokenId = item.BlockchainTokenId.Value.ToString(),
            amount = 1
        };
        
        Debug.Log(data.GetEncodedData());
        
        EnjinPlatformService.Instance.LogGameEvent(GameEventType.ITEM_COLLECTED, data.GetEncodedData());
        
        Destroy(gameObject);
    }

    public Item GetItem()
    {
        return item;
    }

    [System.Serializable]
    public class ItemCollectedEventData: ItemEventData
    {
        public int amount;
    }
    
    [System.Serializable]
    public class ItemDestroyedEventData: ItemCollectedEventData { }
    
    [System.Serializable]
    public class ItemTransferredEventData: ItemEventData
    {
        public int amount;
        public string recipient;
    }
    
    [System.Serializable]
    public class ItemEventData: GameEventData
    {
        public string collectionId;
        public string tokenId;
    }
}
