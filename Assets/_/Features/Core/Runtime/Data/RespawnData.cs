using UnityEngine;

namespace Core.Data.Runtime
{
    [CreateAssetMenu(fileName = "RespawnData", menuName = "Data/Core/Respawn", order = 0)]
    public class RespawnData : ScriptableObject
    {
        public float m_respawnTime;
        public float m_timeBeforeStartingRespawn;
        public float m_initialSpawnTime;
        public float m_timeReductionBonus;
        public float m_maxRotationAngle;
        public float m_rotationSpeedMultiplier;
    }
}