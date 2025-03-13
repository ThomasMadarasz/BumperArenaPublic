namespace Interfaces.Runtime
{
    public interface IPlayer
    {
        public void SetLocalPlayerID(int id, bool isAI);

        public void SetWorldOrientation(bool useWorld);

        public void SetUseRumble(bool useRumble);

        public bool IsAnAI();
    }
}