using Archi.Runtime;
using Interfaces.Runtime;

namespace Feedbacks.Runtime
{
    public abstract class FeedbackBase : CBehaviour, IFeedback
    {
        protected int p_index;

        protected bool p_useRumble;

        public int GetID() => p_index;

        public void SetID(int id) => p_index = id;

        public void UseRumble(bool useRumble)=> p_useRumble = useRumble;
    }
}