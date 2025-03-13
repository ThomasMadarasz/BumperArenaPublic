using UnityEngine;
using Mirror;
using Interfaces.Runtime;
using Archi.Runtime;
using ScriptableEvent.Runtime;

namespace GameModes.Runtime
{
    [RequireComponent(typeof(Collider))]
    public class CrownTrigger : CNetBehaviour
    {
        #region Exposed

        [SerializeField] private CrownManager _crownManager;
        [SerializeField] private GameEvent _onRoundStart;

        #endregion


        #region Unity API

        private void Awake()
        {
            _onRoundStart.RegisterListener(OnRoundstart);
        }

        private void OnDestroy()
        {
            _onRoundStart.UnregisterListener(OnRoundstart);
        }

        [ServerCallback]
        private void OnTriggerEnter(Collider other)
        {
            if (!_roundStarted) return;
            if (_isPickedUp || !other.CompareTag("Player")) return;
            ITeamable teamable = other.GetComponent<ITeamable>();
            if (teamable == null) return;

            IFeedback feedback = other.gameObject.GetComponent<IFeedback>();
            _crownManager.GiveCrown(teamable, feedback);
            _isPickedUp = true;
            gameObject.SetActive(false);
        }

        #endregion


        #region Main

        private void OnRoundstart() => _roundStarted = true;

        #endregion


        #region Private

        private bool _isPickedUp = false;
        private bool _roundStarted;

        #endregion
    }
}