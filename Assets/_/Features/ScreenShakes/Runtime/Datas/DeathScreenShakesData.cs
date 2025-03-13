using Sirenix.OdinInspector;
using UnityEngine;
using Enum.Runtime;
using System.Collections.Generic;

namespace ScreenShakes.Runtime
{
    [CreateAssetMenu(fileName = "DeathScreenShakesData", menuName = "Data/ScreenShake/DeathScreenShakesData", order = 1)]
    public class DeathScreenShakesData : SerializedScriptableObject
    {
        public Dictionary<DeathType, ScreenShakeData> m_deathDatas;
    }
}