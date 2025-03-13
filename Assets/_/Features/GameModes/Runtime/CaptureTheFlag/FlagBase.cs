using UnityEngine;
using Archi.Runtime;
using Interfaces.Runtime;
using UnityEngine.Playables;
using System;
using ScriptableEvent.Runtime;

namespace GameModes.Runtime
{
    public class FlagBase : CNetBehaviour
    {
        #region Exposed

        [SerializeField] private CaptureTheFlagManager CtfManager;
        [SerializeField] private FlagBase _oppositeFlagBase;

        [SerializeField] internal int i_teamID;
        [SerializeField] private PlayableDirector _pickupTimeline;
        [SerializeField] private GameObject _flagPickupGraphics;

        [SerializeField] private PlayableDirector _goalTimeline;
        [SerializeField] private GameObject _flagGoalGraphics;

        [SerializeField] private PlayableDirector _respawnAfterScoreTimeline;
        [SerializeField] private GameObject _flagRespawnAfterScoreGraphics;

        [SerializeField] private PlayableDirector _respawnNoScoreTimeline;
        [SerializeField] private GameObject _flagRespawnNoScoreGraphics;

        [SerializeField] private GameEvent _onAllPlayerReady;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        private void OnDestroy()
        {
            _onAllPlayerReady.UnregisterListener(InitialSpawnFlag);
        }

        private void OnTriggerStay(Collider other)
        {
            if (_isPickedUp) return;
            if (_onTriggerStayPlaying) return;
            _onTriggerStayPlaying = true;

            ITeamable teamable = other.gameObject.GetComponent<ITeamable>();

            if (teamable == null)
            {
                _onTriggerStayPlaying = false;
                return;
            }

            int teamId = teamable.GetTeamID();
            int playerId = teamable.GetPlayerID();

            IFeedback feedBack = other.gameObject.GetComponent<IFeedback>();

            if (_isFlagAtBase && teamId != i_teamID)
            {
                StealFlag(teamable, feedBack);
                _isPickedUp = true;
                //Debug.LogWarning($"Flag {i_teamID} take");
            }
            else if (_isFlagAtBase && teamId == i_teamID && CtfManager.IsPlayerWithFlag(playerId))
            {
                CtfManager.PlayerScored(playerId, teamable, feedBack);
                _oppositeFlagBase.ResetFlag(true);
            }

            _onTriggerStayPlaying = false;
        }

        #endregion


        #region Main

        private void Setup()
        {
            _pickupTimeline.extrapolationMode = DirectorWrapMode.Hold;
            _goalTimeline.extrapolationMode = DirectorWrapMode.Hold;
            _respawnAfterScoreTimeline.extrapolationMode = DirectorWrapMode.Hold;
            _respawnNoScoreTimeline.extrapolationMode = DirectorWrapMode.Hold;
            _isFlagAtBase = true;
            _isPickedUp = false;

            _onAllPlayerReady.RegisterListener(InitialSpawnFlag);
        }

        private void InitialSpawnFlag() => VfxRespawnFlag(false);

        private void StealFlag(ITeamable teamable, IFeedback player)
        {
            CtfManager.PlayerStoleFlag(teamable, player);
            _isFlagAtBase = false;
            VfxStealFlag();
        }

        public void ResetOppositeFlag(bool scorePoint)
        {
            //_oppositeFlagBase._isFlagAtBase = true;
            //_oppositeFlagBase._isPickedUp = false;
            //_oppositeFlagBase.VfxRespawnFlag(scorePoint);

            _oppositeFlagBase.ResetFlag(scorePoint);
        }

        public void ResetFlag(bool scorePoint)
        {
            _isFlagAtBase = true;
            _isPickedUp = false;
            VfxRespawnFlag(scorePoint);

            Debug.LogWarning($"Flag {i_teamID} reset");
        }

        public void Goal()
        {
            VfxGoal();
        }

        private void HidePickupFlagGraphics() => _flagPickupGraphics?.SetActive(false);

        private void ShowPickupFlagGraphics() => _flagPickupGraphics?.SetActive(true);

        private void HideGoalFlagGraphics() => _flagGoalGraphics?.SetActive(false);

        private void ShowGoalFlagGraphics() => _flagGoalGraphics?.SetActive(true);

        private void HideRespawnAfterScoreFlagGraphics() => _flagRespawnAfterScoreGraphics?.SetActive(false);

        private void ShowRespawnAfterScoreFlagGraphics() => _flagRespawnAfterScoreGraphics?.SetActive(true);

        private void HideRespawnNoScoreFlagGraphics() => _flagRespawnNoScoreGraphics?.SetActive(false);

        private void ShowRespawnNoScoreFlagGraphics() => _flagRespawnNoScoreGraphics?.SetActive(true);

        #endregion


        #region VFX

        private void VfxStealFlag()
        {
            HideRespawnAfterScoreFlagGraphics();
            HideRespawnNoScoreFlagGraphics();

            ShowPickupFlagGraphics();
            _pickupTimeline.Play();

            float duration = Convert.ToSingle(_pickupTimeline.duration);
            Invoke(nameof(HidePickupFlagGraphics), duration);
        }

        private void VfxRespawnFlag(bool pointScored)
        {
            if (pointScored)
            {
                ShowRespawnAfterScoreFlagGraphics();
                _respawnAfterScoreTimeline.Play();
            }
            else
            {
                ShowRespawnNoScoreFlagGraphics();
                _respawnNoScoreTimeline.Play();
            }
        }

        private void VfxGoal()
        {
            ShowGoalFlagGraphics();
            _goalTimeline.Play();

            float duration = Convert.ToSingle(_goalTimeline.duration);
            Invoke(nameof(HideGoalFlagGraphics), duration);
        }

        #endregion


        #region Utils

        public bool IsFlagAtBase() => _isFlagAtBase;

        #endregion


        #region Private

        private bool _isFlagAtBase = true;
        private bool _isPickedUp = false;
        private bool _onTriggerStayPlaying;

        #endregion
    }
}