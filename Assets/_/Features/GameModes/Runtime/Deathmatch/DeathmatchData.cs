using UnityEngine;

namespace GameModes.Runtime
{
    [CreateAssetMenu(fileName = "DeathmatchData", menuName = "Data/GameModes/Deathmatch/Deathmatch", order = 1)]
    public class DeathmatchData : ScriptableObject
    {
        public float m_tagLengthFactor;
    }
}