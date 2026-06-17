using System.Threading.Tasks;
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

            // Fire-and-forget: collecting talks to the sample server and we
            // don't want to block the gameplay tick on it. The destruction
            // below is intentional even if the mint fails; the visual token
            // has been "picked up" from the player's point of view. We still
            // observe the task so a faulted mint surfaces in the log instead of
            // being swallowed as an unobserved exception.
            _ = item.Collect().ContinueWith(
                t => Debug.LogError($"[EnjinToken] Collect of token #{item.tokenId} faulted: {t.Exception?.GetBaseException().Message}"),
                TaskContinuationOptions.OnlyOnFaulted);

            Destroy(gameObject);
        }

        public Item GetItem()
        {
            return item;
        }

        public void Melt(int amount)
        {
            // Fire-and-forget; UI refresh happens via EnjinManager.OnWalletUpdated.
            _ = item.Melt(amount);
        }

        public void Transfer(string toAddress, int amount)
        {
            // Fire-and-forget; UI refresh happens via EnjinManager.OnWalletUpdated.
            _ = item.Transfer(toAddress, amount);
        }
    }
}