using UnityEngine;
using HappyHarvest.EnjinIntegration.Data;

namespace HappyHarvest.EnjinIntegration.Gameplay
{
    public class EnjinToken : InteractiveObject, IEnjinToken
    {
        public EnjinItem item;

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

            item.Collect();

            Destroy(gameObject);
        }

        public Item GetItem()
        {
            return item;
        }
        
        public void Melt(int amount)
        {
            item.Melt(amount);
        }

        public void Transfer(string toAddress, int amount)
        {
            item.Transfer(toAddress, amount);
        }
    }
}