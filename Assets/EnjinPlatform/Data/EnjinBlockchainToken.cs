using System.Buffers.Text;
using System.Numerics;
using EnjinPlatform.Data;
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
        
        // Encode the item collection id, token id into a base 64 string.
        ItemCollectedEventData data = new ItemCollectedEventData
        {
            collectionId = item.BlockchainCollectionId.Value.ToString(),
            tokenId = item.BlockchainTokenId.Value.ToString(),
            amount = 1
        };
        
        string jsonString = JsonUtility.ToJson(data);
        byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(jsonString);
        string base64EventData = System.Convert.ToBase64String(dataBytes);
        
        Debug.Log(base64EventData);
        
        EnjinPlatformService.Instance.LogGameEvent(GameEventType.ITEM_COLLECTED, base64EventData);
        
        Destroy(gameObject);
    }

    public Item GetItem()
    {
        return item;
    }

    [System.Serializable]
    public class ItemCollectedEventData
    {
        public string collectionId;
        public string tokenId;
        public int amount;
    }
}
