using HappyHarvest.EnjinIntegration.Core;
using System.Threading.Tasks;
using UnityEngine;

namespace HappyHarvest.EnjinIntegration.Data
{
    [CreateAssetMenu(fileName = "EnjinItem", menuName = "2D Farming/Items/Enjin Item")]
    public class EnjinItem : Item
    {
        [SerializeField]
        public PlatformModels.SerializableBigInteger collectionId = new PlatformModels.SerializableBigInteger(-1);
        [SerializeField]
        public PlatformModels.SerializableBigInteger tokenId = new PlatformModels.SerializableBigInteger(-1);

        public override bool CanUse(Vector3Int target)
        {
            return true;
        }

        public override bool Use(Vector3Int target)
        {
            return true;
        }

        public async Task Transfer(string recipient, int amount = 1)
        {
            Debug.Log($"Transferring {amount} of token: #{tokenId}");
            await EnjinManager.Instance.TransferToken(tokenId, amount, recipient);
        }

        public async Task Melt(int amount = 1)
        {
            Debug.Log($"Melting {amount} of token: #{tokenId}");
            await EnjinManager.Instance.MeltToken(tokenId, amount);
        }

        public async Task Collect()
        {
            Debug.Log($"Collecting token: #{tokenId}");
            await EnjinManager.Instance.MintToken(tokenId, 1);
        }
    }
}
