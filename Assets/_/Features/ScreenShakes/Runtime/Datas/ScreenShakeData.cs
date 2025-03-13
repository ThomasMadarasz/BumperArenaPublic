using UnityEngine;
using Enum.Runtime;
using Sirenix.OdinInspector;

namespace ScreenShakes.Runtime
{
    [CreateAssetMenu(fileName = "ScreenShakeData", menuName = "Data/ScreenShake/ScreenShakeData", order = 0)]
    public class ScreenShakeData : ScriptableObject
    {
        [EnumToggleButtons]
        public ScreenShakeType m_type = ScreenShakeType.WiggleNoise;

        public float m_duration = 1f;

        [HideIf("m_type", ScreenShakeType.Zoom), Tooltip("Wiggle base value is 40 and Impulse base value is 1")]
        public float m_amplitude = 0.2f;

        [HideIf("m_type", ScreenShakeType.Zoom), Tooltip("Wiggle base value is 0.2 and Impulse base value is 1")]
        public float m_frequency = 40f;

        [ShowIf("m_type", ScreenShakeType.Zoom)]
        public float m_fov = 36;
    }
}