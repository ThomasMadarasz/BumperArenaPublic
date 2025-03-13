using Interfaces.Runtime;

namespace PlayerController.Parameters.Runtime
{
    public class CollisionParameters
    {
        public ITeamable m_ownerTeamable;
        public ITeamable m_otherTeamable;
        public IFeedback m_feedback;
        public float m_bumpTime;
    }
}