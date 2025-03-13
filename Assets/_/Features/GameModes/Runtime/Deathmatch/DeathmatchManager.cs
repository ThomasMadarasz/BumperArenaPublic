using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using ScriptableEvent.Runtime;
using Interfaces.Runtime;
using PlayerController.Parameters.Runtime;
using Utils.Runtime;
using Core.Runtime;
using System.Linq;

namespace GameModes.Runtime
{
    public class DeathmatchManager : GameMode
    {
        #region Exposed

        [SerializeField] private DeathmatchData _data;
        [SerializeField] private GameEventT2 _onPlayerDie;

        #endregion


        #region Unity API
        public override void OnStartServer()
        {
            base.OnStartServer();
            Setup();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        #endregion


        #region Main
        private void Setup()
        {
            _onPlayerDie.RegisterListener(OnPlayerDie);
        }

        protected override void OnCollisionWithPlayer(object parameters)
        {
            CollisionParameters cp = (CollisionParameters)parameters;

            CollisionParameters[] collisionParameters = base.GetParameters(cp);

            if (collisionParameters == null) return;

            ITeamable[] teamables = new ITeamable[] { collisionParameters[0].m_ownerTeamable, collisionParameters[1].m_ownerTeamable };

            if (teamables[0].GetTeamID() == teamables[1].GetTeamID()) return;

            foreach (CollisionParameters p in collisionParameters)
            {
                float duration = p.m_bumpTime * _data.m_tagLengthFactor;
                if (duration <= 0.01f) 
                {
                    Debug.Log("weird duration");
                }
                Timer tagTimer = new Timer(duration, OnTagTimerOver);
                tagTimer.Start();
                ITeamable[] playersTeamables = new ITeamable[] { p.m_ownerTeamable, p.m_otherTeamable };

                Timer existingTimer = _tags.FirstOrDefault(x => x.Value[1].GetPlayerID() == playersTeamables[1].GetPlayerID()).Key;

                if(existingTimer != null)
                {
                    existingTimer.Stop();
                    _tags.Remove(existingTimer);
                    Debug.Log(" Previous tag has been removed for the following :");
                }

                _tags.Add(tagTimer, playersTeamables);
                Debug.Log($"Tag player {p.m_ownerTeamable.GetPlayerID()} by player {p.m_otherTeamable.GetPlayerID()} for {duration} seconds");

            }
        }

        private void OnPlayerDie(object args, object param)
        {
            object[] argsArray = (object[])args;
            ITeamable teamable = (ITeamable)argsArray[1];

            Timer t = _tags.FirstOrDefault(x => x.Value[1].GetPlayerID() == teamable.GetPlayerID() && !x.Key.IsTimeOver()).Key;

            if(t == null) Debug.Log($"Player died on his own");
            else
            {
                _scoreManager.ScorePoint(_tags[t][0]);
                Debug.Log($"Player of team {_tags[t][0].GetPlayerID()} killed player of team {_tags[t][1].GetPlayerID()} ");
                t.Stop();
                _tags.Remove(t);
            }
        }

        private void OnTagTimerOver()
        {
            Debug.Log($"before {_tags.Count}");
            _tags = _tags.Where(x => !x.Key.IsTimeOver()).ToDictionary(y => y.Key, y => y.Value);
            Debug.Log(_tags.Count);
        }

        #endregion


        #region Utils & Tools
        #endregion


        #region Debug
        #endregion


        #region Private

        private Dictionary<Timer, ITeamable[]> _tags = new Dictionary<Timer, ITeamable[]>();

        #endregion
    }
}