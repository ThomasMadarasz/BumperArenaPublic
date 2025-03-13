using UnityEngine;

namespace GameModes.Runtime
{
    [CreateAssetMenu(fileName = "RaceData", menuName = "Data/GameModes/Race/Race", order = 1)]
    public class RaceData : ScriptableObject
    {
        public int m_scoreToReachToWinRound;
    }
}