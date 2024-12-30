using EnjinPlatform.Interfaces;
using EnjinPlatform.Managers;
using UnityEngine;

namespace HappyHarvest
{
    [CreateAssetMenu(fileName = "EnjinBlockchainToken", menuName = "2D Farming/Items/Enjin Blockchain Token")]
    public class EnjinBlockchainItem : Item
    {
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
