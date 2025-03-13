using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Archi.Runtime;
using Mirror;
using System.Linq;
using Interfaces.Runtime;

namespace AI.Runtime
{
    public class CaptureTheFlagAI : GameModeAI
    {
        #region Exposed

        public Transform m_enemyPlayerWithFlag
        {
            get { return _enemyWithFlag; }
        }

        public Transform m_allyFlagBase
        {
            get { return _flagBases[0]; }
        }

        public Transform m_enemyFlagBase
        {
            get { return _flagBases[1]; }
        }

        public bool m_isAllyFlagAtBase
        {
            get { return _isAllyFlagAtBase; }
        }

        public bool m_isEnemyFlagAtBase
        {
            get { return _isEnemyFlagAtBase; }
        }

        public bool m_isSelfCarryingFlag
        {
            get { return _isSelfCarryingFlag; }
        }


        #endregion


        #region Unity API


        [ServerCallback]
        private void Update()
        {
            if (!m_isRoundStarted || !_isSetup) return;
            List<int> list = _currentGameModeManager.GetPlayersWithObjective();
            _isSelfCarryingFlag = IsSelfCarryingFlag(list);
            _isAllyFlagAtBase = IsAllyFlagAtBase(list);
            _isEnemyFlagAtBase = IsEnemyFlagAtBase(list);
            if (!_isAllyFlagAtBase)
            {
                int enemyId = list.FirstOrDefault(x => x == _enemyTeamables[0].GetPlayerID() || x == _enemyTeamables[1].GetPlayerID());
                _enemyWithFlag = _enemyTeamables.FirstOrDefault(x => x.GetPlayerID() == enemyId).GetTransform();
            }
        }


        #endregion


        #region Main

        protected override void Setup() => base.Setup();

        protected override void SetupRound()
        {
            if (!this.enabled) return;
            base.SetupRound();
            Transform[] transforms = _currentGameModeManager.GetObjectivesTransforms();
            _flagBases[0] = transforms[_teamable.GetTeamID() - 1];
            _flagBases[1] = transforms[1 - (_teamable.GetTeamID() - 1)];
        }


        #endregion


        #region Utils & Tools

        private bool IsSelfCarryingFlag(List<int> list) => list.Contains(_teamable.GetPlayerID());

        private bool IsAllyFlagAtBase(List<int> list)
        {
            foreach (ITeamable team in _enemyTeamables)
            {
                if (list.Contains(team.GetPlayerID())) return false;
            }
            return true;
        }

        private bool IsEnemyFlagAtBase(List<int> list)
        {
            if (_isSelfCarryingFlag) return false;
            foreach(ITeamable team in _allyTeamables)
            {
                if (list.Contains(team.GetPlayerID())) return false;
            }
            return true;
        }

        #endregion


        #region Debug
        #endregion


        #region Private

        private Transform[] _flagBases = new Transform[2];
        private Transform _enemyWithFlag;

        private bool _isAllyFlagAtBase;
        private bool _isEnemyFlagAtBase;
        private bool _isSelfCarryingFlag;

        #endregion
    }
}