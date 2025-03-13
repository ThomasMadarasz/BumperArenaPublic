using AI.Data.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Runtime
{
    public class AirHockeyAI : GameModeAI
    {
        #region Exposed

        [SerializeField] private AirHockeyAIData _data;

        public Transform m_allyGoal => _objectives[1];
        
        public Transform m_enemyGoal => _objectives[2];

        public Transform m_puck => _objectives[0];

        public Transform m_wantedTransform => WantedTransform();

        public bool m_isGameRunning=> _currentGameModeManager.IsGameRunning();

        #endregion


        #region UnityApi

        void OnDrawGizmos()
        {
            if (!this.enabled || _wantedTransform == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(_wantedTransform.position, 1);
        }

        private void Update()
        {
            if (_wantedTransform == null) return;
            _wantedTransform.position = _objectives[0].position
                + _data.m_toGoalOffset * (_distanceBetweenGoals - (_objectives[1].position - _objectives[0].position).magnitude)/ _distanceBetweenGoals * (_objectives[1].position - _objectives[0].position).normalized
                + _data.m_toGoalOffset * (_distanceBetweenGoals - (_objectives[2].position - _objectives[0].position).magnitude) / _distanceBetweenGoals * (_objectives[0].position - _objectives[2].position).normalized
                + _puckRigidbody.velocity * _data.m_toPuckOffset;
        }

        #endregion


        #region Main

        protected override void SetupRound()
        {
            if (!this.enabled) return;
            base.SetupRound();
            Transform[] transforms = _currentGameModeManager.GetObjectivesTransforms();
            _objectives[0] = transforms[0];
            _objectives[1] = transforms[_teamable.GetTeamID()];
            _objectives[2] = transforms[2 - (_teamable.GetTeamID() - 1)];
            _puckRigidbody = _currentGameModeManager.GetPhysicObjectiveRigidbody();
            _wantedTransform = new GameObject().transform;
            _distanceBetweenGoals = Vector3.Distance(_objectives[1].position, _objectives[2].position);
        }


        #endregion


        #region Utils & Tools

        private Transform WantedTransform()
        {
            return _wantedTransform;
        }

        #endregion


        #region Private

        private Transform[] _objectives = new Transform[3];

        private Rigidbody _puckRigidbody;

        private Transform _wantedTransform;

        private float _distanceBetweenGoals;

        #endregion
    }
}