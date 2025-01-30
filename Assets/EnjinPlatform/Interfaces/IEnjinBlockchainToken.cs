using System.Numerics;
using HappyHarvest;
namespace EnjinPlatform.Interfaces
{
    public interface IEnjinBlockchainToken
    {
        public Item GetItem();
        
        public float GetRarity { get; }

        public void Melt(int amount);
        
        public void Transfer(string toAddress, int amount);
    }
}