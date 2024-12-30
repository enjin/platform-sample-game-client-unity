using EnjinPlatform.Interfaces;
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
        
        Destroy(gameObject);
    }

    public Item GetItem()
    {
        return item;
    }
}
