namespace Interfaces.Runtime
{
    public interface IBoostable
    {
        public void DisableBoost();

        public void EnableBoost();

        public void AddBoost(int boost);
        
        public void TryToBoost();

        public int GetAvailableBoostChargeCount();

        public bool CanReceiveBoost();
    }
}