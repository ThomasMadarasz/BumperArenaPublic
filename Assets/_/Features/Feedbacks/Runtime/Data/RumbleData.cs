using UnityEngine;

namespace Feedbacks.Runtime
{
    [CreateAssetMenu(fileName = "NewRumbleData", menuName = "Data/Feedback/RumbleData")]
    public class RumbleData : ScriptableObject
    {
        public float m_highFrequency;
        public float m_lowFrequency;
        public float m_duration;
    }
}