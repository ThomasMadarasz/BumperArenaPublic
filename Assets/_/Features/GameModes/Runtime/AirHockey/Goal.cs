using UnityEngine;
using Archi.Runtime;
using Mirror;

namespace GameModes.Runtime
{
    [RequireComponent(typeof(Collider))]
    public class Goal : CBehaviour
    {
        #region Exposed

        [SerializeField] private AirHockeyManager _airHockeyManager;
        [SerializeField] private Goal _oppositeGoal;
        [SerializeField] private Puck _puck;
        [SerializeField] internal int i_teamId;
        [SerializeField] private bool _isRightGoal;

        #endregion


        #region Unity API

        [ServerCallback]
        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag("Puck")) return;
            if (!_puck.m_isActive) return;
            _airHockeyManager.Score(_oppositeGoal.GetTeamId());

            Vector3 rotation = _isRightGoal ? new Vector3(0, 180, 0) : Vector3.zero;

            _puck.Disappear(_puck.transform.position, rotation, true);
        }

        #endregion


        #region Main

        public int GetTeamId() => i_teamId;

        #endregion
    }
}