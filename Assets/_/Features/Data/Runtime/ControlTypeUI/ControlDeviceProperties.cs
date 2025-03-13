using System;
using UnityEngine;
using UnityEngine.UI;

namespace Data.Runtime
{
    [Serializable]
    public class ControlDeviceProperties
    {
        public ControlTypeEnum _type;
        public Sprite _keyboardImage;
        public Sprite _playstationImage;
        public Sprite _xboxImage;
    }
}