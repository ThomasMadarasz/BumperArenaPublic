using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Interfaces.Runtime;

namespace AI.Runtime
{
    public class CrownAI : GameModeAI
    {
        #region Exposed

        public Transform m_crown
        {
            get { return _crown; }
        }

        public Transform m_playerWithCrown
        {
            get { return _playerWithCrown; }
        }

        public Transform m_targetPlace
        {
            get { return _targetPlace; }
        }

        public bool m_hasCrown
        {
            get { return _hasCrown; }
        }

        #endregion


        #region Unity API

        void OnDrawGizmos()
        {
            if (!this.enabled || !m_isRoundStarted || !_isSetup) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(_targetPlace.position, 1);
        }

        private void Update()
        {
            if (!m_isRoundStarted || !_isSetup) return;
            GetPlayerWithCrown();
            UpdateTargetPlace();
        }

        #endregion


        #region Main

        protected override void SetupRound()
        {
            if (!this.enabled) return;
            base.SetupRound();
            Transform[] transforms = _currentGameModeManager.GetObjectivesTransforms();
            _crown = transforms[0];
            _targetPlace = new GameObject().transform;
            _otherPlayersTransforms = _enemyTeamables.Select(x => x.GetTransform()).ToList();
            _hasCrown = false;
            _playerWithCrown = null;
    }

        private void GetPlayerWithCrown()
        {
            List<int> list = _currentGameModeManager.GetPlayersWithObjective();
            if (list.Count == 0)
            {
                _targetPlace.transform.position = _crown.transform.position;
                return;
            }
            int enemyId = list[0];
            _playerWithCrown = _enemyTeamables.FirstOrDefault(x => x.GetPlayerID() == enemyId)?.GetTransform();
            _hasCrown = list[0] == _teamable.GetPlayerID();
        }

        private void UpdateTargetPlace()
        {
            Vector3 direction = Vector3.zero;
            foreach(Transform t in _otherPlayersTransforms)
            {
                direction += (t.position - transform.position).normalized;
            }
            direction = - direction.normalized;
            _targetPlace.position = transform.position + direction;
        }

        #endregion


        #region Utils & Tools
        #endregion


        #region Debug
        #endregion


        #region Private

        private Transform _crown;
        private Transform _playerWithCrown;
        private Transform _targetPlace;
        private List<Transform> _otherPlayersTransforms;
        private bool _hasCrown;

        #endregion
    }
}