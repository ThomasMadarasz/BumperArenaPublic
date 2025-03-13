using UnityEngine;
using Enum.Runtime;

namespace ScreenShakes.Runtime
{
    public class ScreenShakeParameters
    {
        public ScreenShakeType m_type;
        public float m_duration;
        public float m_amplitude;
        public float m_frequency;
        public float m_fov;

        public ScreenShakeParameters(ScreenShakeData data)
        {
            m_type = data.m_type;
            m_duration = data.m_duration;
            m_amplitude = data.m_amplitude;
            m_frequency = data.m_frequency;
            m_fov = data.m_fov;
        }
    }
}