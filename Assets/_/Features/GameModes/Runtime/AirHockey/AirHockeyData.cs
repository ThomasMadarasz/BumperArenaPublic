using UnityEngine;

namespace GameModes.Runtime
{

    [CreateAssetMenu(fileName = "AirHockeyData", menuName = "Data/GameModes/AirHockey/AirHockey", order = 1)]
    public class AirHockeyData : ScriptableObject
    {
        public float m_respawnPuckTime;
    }
}