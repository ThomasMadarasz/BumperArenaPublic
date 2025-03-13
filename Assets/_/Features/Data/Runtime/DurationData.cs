using UnityEngine;

namespace Data.Runtime
{
    [CreateAssetMenu(fileName = "NewDurationData", menuName = "Data/DurationData")]
    public class DurationData : ScriptableObject
    {
        public float m_duration;
        public float m_delay;
    }
}