using UnityEngine;

namespace Interfaces.Runtime
{
    public interface ITeamable
    {
        public void SetTeamID(int id);

        public int GetTeamID();

        public void SetPlayerID(int id);

        public int GetPlayerID();

        public int GetUniqueID();

        public void SetUniqueID(int id);

        public Transform GetTransform();

        public bool IsAlive();
        
        public void SetImmuneToCollisions(bool immune);

        public void SetSpeedMultiplier(float multiplier);

        public void SetAccelerationMultiplier(float multiplier);

        public void ResetSpeedMultipliers();

        public bool IsAnAI();

        public void OnScorePoint(int gameMideId);
    }
}