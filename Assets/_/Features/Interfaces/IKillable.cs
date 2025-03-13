using System;
using UnityEngine;
using Enum.Runtime;

namespace Interfaces.Runtime
{
    public interface IKillable
    {
        public event Action m_onPlayerDie;
        public event Action m_onPlayerRespawn;

        public void Kill(DeathType type);

        public void Respawn(Transform transform, Quaternion rot);

        public void Spawn(Transform transform, Quaternion rot);

        public void MoveTo(Transform tr);

        public ITeamable GetTeamable();

        public IFeedback GetFeedback();

        public bool PlayerUseRespawnAccelerationBonus();

        public float GetRespawnDirection(Transform t);

        public void UpdateColliderCollisionFor(Collider col, bool ignore);

        public void DisableInvincibility();

        public IPlayer GetPlayerPorperties();
    }
}