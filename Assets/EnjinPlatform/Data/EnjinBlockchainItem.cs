using EnjinPlatform.Interfaces;
using EnjinPlatform.Managers;
using EnjinPlatform.Events;
using EnjinPlatform.Services;
using EnjinPlatform.Data;
using HappyHarvest;
using UnityEngine;

namespace EnjinPlatform.Data
{
    [CreateAssetMenu(fileName = "EnjinBlockchainItem", menuName = "2D Farming/Items/Enjin Blockchain Item")]
    public class EnjinBlockchainItem : Item
    {
        [SerializeField]
        public SerializableBigInteger BlockchainCollectionId = new SerializableBigInteger(-1);
        [SerializeField]
        public SerializableBigInteger BlockchainTokenId = new SerializableBigInteger(-1);
        
        public EnjinPlatformItemService ItemService = new EnjinPlatformItemService();
        
        public EnjinBlockchainItem()
        {
            ItemService.BlockchainCollectionId = BlockchainCollectionId;
            ItemService.BlockchainTokenId = BlockchainTokenId;
        }
        
        public override bool CanUse(Vector3Int target)
        {
            return true;
        }

        public override bool Use(Vector3Int target)
        {
            return true;
        }
    }
}
