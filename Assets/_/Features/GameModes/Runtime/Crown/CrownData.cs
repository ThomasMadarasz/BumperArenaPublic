using UnityEngine;

namespace GameModes.Runtime
{
    [CreateAssetMenu(fileName = "CrownData", menuName = "Data/GameModes/Crown/Crown", order = 1)]
    public class CrownData : ScriptableObject
    {
        public int m_scoreToReachToWinRound;
        public float m_speedMultiplier;
        public float m_accelerationMultiplier;
        public float m_scoreTimer;
        public float m_immunityTimer;
    }
}