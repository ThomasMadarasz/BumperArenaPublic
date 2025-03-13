using System;
using UnityEngine.Localization;
using UnityEngine.Video;

namespace UserInterface.Runtime
{
    [Serializable]
    public class HowToPlayData
    {
        public VideoClip m_clip;
        public LocalizedString[] m_explanation = new LocalizedString[1];
        public bool m_useTimerForChangeText;
        public float m_timeBeforeChangeText;
    }
}