using System.Numerics;
using HappyHarvest;
namespace EnjinPlatform.Interfaces
{
    public interface IEnjinBlockchainToken
    {
        public void Collect();

        public Item GetItem();
        
        public float GetRarity { get; }
    }
}