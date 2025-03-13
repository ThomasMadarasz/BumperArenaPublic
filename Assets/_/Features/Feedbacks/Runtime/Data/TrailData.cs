using UnityEngine;

namespace Feedbacks.Runtime
{
    [CreateAssetMenu(fileName = "NewTrailData", menuName = "Data/Feedback/TrailData")]
    public class TrailData : ScriptableObject
    {
        public float m_time;
    }
}