namespace HappyHarvest.EnjinIntegration.Gameplay
{
    public interface IEnjinToken
    {
        public Item GetItem();
        
        public float GetRarity { get; }

        public void Melt(int amount);
        
        public void Transfer(string toAddress, int amount);
    }
}