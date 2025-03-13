using System;
using UnityEngine;

namespace Startup.Runtime
{
    [Serializable]
    public class StartupImageData
    {
         public Sprite m_image;
         public float m_duration;
         public float m_fadeDuration;
        public GameObject m_gameObjectToEnable;
    }
}