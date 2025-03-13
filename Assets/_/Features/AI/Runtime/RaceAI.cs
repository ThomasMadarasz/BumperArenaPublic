using GameModes.Runtime;
using Interfaces.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AI.Runtime
{
    public class RaceAI : GameModeAI
    {
        #region Exposed

        public Transform m_nextCheckPoint
        {
            get { return _nextCheckPointTransform; }
        }

        #endregion


        #region Unity API

        private void Update()
        {
            if (!m_isRoundStarted || !_isSetup) return;
            _checkpointsState = _currentGameModeManager.GetCheckpointsState();
            List<Checkpoint> myCheckpointsValidated = _checkpointsState.FirstOrDefault(x => x.Key.GetPlayerID() == _teamable.GetPlayerID()).Value;
            List<Checkpoint> myCheckpointsUnvalidated = _currentGameModeManager.GetCheckpoints();
            List<Transform> myCheckpointsWanted = new List<Transform>(_checkpoints);
            if(myCheckpointsValidated != null)
            {
                foreach (Checkpoint cp in myCheckpointsValidated)
                {
                    myCheckpointsWanted.Remove(cp.transform);
                    myCheckpointsUnvalidated.Remove(cp);
                }
            }
            float distance = float.MaxValue;
            _nextCheckPointTransform = null;
            _nextCheckPoint = null;
            foreach(Checkpoint cp in myCheckpointsUnvalidated)
            {
                float newDistance = Vector3.Distance(cp.transform.position, transform.position);
                if(newDistance <= distance)
                {
                    distance = newDistance; 
                    _nextCheckPoint = cp;
                    _nextCheckPointTransform = cp.transform;
                }
            }
            Dictionary<Transform, float> dic = _nextCheckPoint.GetCrossingPoints();
            distance = float.MaxValue;
            for (int i = 0; i < dic.Count; i++)
            {
                float newDistance = Vector3.Distance(dic.ElementAt(i).Key.position, transform.position);
                float checkDistance = i + 1 < dic.Count ? Vector3.Distance(transform.position, dic.ElementAt(i + 1).Key.position) : Vector3.Distance(transform.position, _nextCheckPoint.transform.position);
                if (newDistance <= distance && dic.ElementAt(i).Value < checkDistance)
                {
                    distance = newDistance;
                    _nextCheckPointTransform = dic.ElementAt(i).Key;
                }

            }

            if(dic != null && dic.Count > 0 && Vector3.Distance(transform.position, _nextCheckPoint.transform.position) < dic.ElementAt(dic.Count - 1).Value)
            {
                _nextCheckPointTransform = _nextCheckPoint.transform;
            }
            //Previous test
            //foreach (Transform t in dic.Keys)
            //{
            //    float newDistance = Vector3.Distance(t.position, transform.position);
            //    if (newDistance <= distance && dic[t] < Vector3.Distance(transform.position, _nextCheckPoint.transform.position))
            //    {
            //        distance = newDistance;
            //        _nextCheckPointTransform = t;
            //    }
            //}

            //Debug
            //if (myCheckpointsValidated == null || myCheckpointsValidated.Count < 1) return;
            //Dictionary<Transform, float> dic = myCheckpointsValidated[0].GetCrossingPoints();
            //foreach (Transform t in dic.Keys)
            //{
            //    Debug.Log(t.name + " " + dic[t]);
            //}
        }

        #endregion


        #region Main
        protected override void SetupRound()
        {
            if (!this.enabled) return;
            base.SetupRound();
            _checkpoints = _currentGameModeManager.GetObjectivesTransforms().ToList();
        }

        #endregion


        #region Private

        private List<Transform> _checkpoints = new List<Transform>();
        private Transform _nextCheckPointTransform;
        private Checkpoint _nextCheckPoint;
        private Dictionary<ITeamable, List<Checkpoint>> _checkpointsState;

        #endregion
    }
}