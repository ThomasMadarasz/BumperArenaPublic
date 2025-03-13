namespace Interfaces.Runtime
{
    public interface IFeedback
    {
        public int GetID();
        public void SetID(int id);
        public void UseRumble(bool useRumble);
    }
}