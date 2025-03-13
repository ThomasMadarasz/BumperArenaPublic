using UnityEngine;

namespace GameModes.Runtime
{

    [CreateAssetMenu(fileName = "CaptureTheFlagData", menuName = "Data/GameModes/CaptureTheFlag/CaptureTheFlag", order = 1)]
    public class CaptureTheFlagData : ScriptableObject
    {
        public float m_speedMultiplier = 1;
        public float m_accelerationMultiplier = 1;
    }
}