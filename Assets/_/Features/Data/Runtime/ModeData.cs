using UnityEngine;

namespace Data.Runtime
{
    [CreateAssetMenu(fileName = "ModeData", menuName = "Data/ModeData")]
    public class ModeData : ScriptableObject
    {
        public GameModeData[] m_gameModesData;
    }

}