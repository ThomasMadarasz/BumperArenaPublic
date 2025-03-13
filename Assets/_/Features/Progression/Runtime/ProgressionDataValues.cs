using UnityEngine;

namespace Progression.Runtime
{
    [CreateAssetMenu(fileName ="NewProgressionDataValues",menuName ="Data/Progression")]
    public class ProgressionDataValues : ScriptableObject
    {
        public int m_coinGainedOnRoundFinished;
    }
}