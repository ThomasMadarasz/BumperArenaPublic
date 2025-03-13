using UnityEngine;

namespace Data.Runtime
{
    [CreateAssetMenu(fileName = "NewControlTypeUIData", menuName = "Data/ControlTypeUIData")]
    public class ControlTypeUIData : ScriptableObject
    {
        public ControlDeviceProperties[] _deviceProperties;
    }
}