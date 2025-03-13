using UnityEngine;

namespace PlayerController.Data.Runtime
{
    [CreateAssetMenu(fileName = "DecelerationData", menuName = "Data/Controller/PlayerDeceleration", order = 4)]
    public class PlayerDecelerationData : ScriptableObject
    {
        public DecelerationProfile m_normalProfile;
        public DecelerationProfile m_boostProfile;
    }

    [System.Serializable]
    public class DecelerationProfile
    {
        public float m_decelerationTime;
        public AnimationCurve m_curve;
    }
}