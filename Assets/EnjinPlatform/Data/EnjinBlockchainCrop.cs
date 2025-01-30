using EnjinPlatform.Interfaces;
using EnjinPlatform.Managers;
using EnjinPlatform.Events;
using EnjinPlatform.Services;
using EnjinPlatform.Data;
using HappyHarvest;
using UnityEngine;

namespace EnjinPlatform.Data
{
    [CreateAssetMenu(fileName = "EnjinBlockchainCrop", menuName = "2D Farming/Items/Enjin Blockchain Crop")]
    public class EnjinBlockchainCrop : Crop
    {
        [SerializeField]
        public SerializableBigInteger BlockchainCollectionId = new SerializableBigInteger(-1);
        [SerializeField]
        public SerializableBigInteger BlockchainTokenId = new SerializableBigInteger(-1);
        
        public EnjinPlatformItemService ItemService = new EnjinPlatformItemService();
        
        public EnjinBlockchainCrop()
        {
            ItemService.BlockchainCollectionId = BlockchainCollectionId;
            ItemService.BlockchainTokenId = BlockchainTokenId;
        }
    }
}
