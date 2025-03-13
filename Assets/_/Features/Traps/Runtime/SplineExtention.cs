using Dreamteck.Splines;
using ScriptableEvent.Runtime;
using UnityEngine;

namespace Traps.Runtime
{
    [RequireComponent(typeof(SplineFollower))]
    public class SplineExtention : MonoBehaviour
    {
        #region Exposed

        [SerializeField] private GameEvent _onRoundFinished;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        private void OnDestroy()
        {
            _onRoundFinished.UnregisterListener(OnRoundFinished);
        }

        #endregion


        #region Main

        private void Setup()
        {
            _follower = GetComponent<SplineFollower>();
            _onRoundFinished.RegisterListener(OnRoundFinished);
        }

        private void OnRoundFinished()
        {
            _follower.follow = false;
        }

        #endregion


        #region Private

        private SplineFollower _follower;

        #endregion
    }
}