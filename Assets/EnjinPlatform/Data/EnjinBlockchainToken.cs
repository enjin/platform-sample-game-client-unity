using EnjinPlatform.Data;
using EnjinPlatform.Events;
using EnjinPlatform.Interfaces;
using EnjinPlatform.Services;
using HappyHarvest;
using UnityEngine;

namespace EnjinPlatform.Data
{
    public class EnjinBlockchainToken : InteractiveObject, IEnjinBlockchainToken
    {
        public EnjinBlockchainItem item;

        [Tooltip(
            "The rarity of the token. Higher numbers indicates the token is more rare and reduce the chance of being found.")]
        [Range(0f, 1.0f)]
        public float rarity;

        public float GetRarity
        {
            get => rarity;
        }

        public override void InteractedWith()
        {
            Debug.Log("Interacted with token");

            item.ItemService.Collect();

            Destroy(gameObject);
        }

        public Item GetItem()
        {
            return item;
        }
        
        public void Melt(int amount)
        {
            item.ItemService.Melt(amount);
        }

        public void Transfer(string toAddress, int amount)
        {
            item.ItemService.Transfer(toAddress, amount);
        }
    }
}