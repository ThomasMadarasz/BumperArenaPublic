using Archi.Runtime;
using ScriptableEvent.Runtime;
using UnityEngine;

namespace Traps.Runtime
{
    public class SpinningHammer : CBehaviour
    {
        #region Exposed

        [SerializeField] private float _rotationPerSeconds;

        [SerializeField] private bool _inverseRotation;

        [SerializeField] private GameEvent _onRoundFinished;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        private void Update()
        {
            if (_roundFinished) return;
            Rotate();
        }

        private void OnDestroy()
        {
            _onRoundFinished.UnregisterListener(OnRoundFinished);
        }

        #endregion


        #region Main

        private void Setup()
        {
            _anglePerFrame = (360 * _rotationPerSeconds);

            if (_inverseRotation)
                _anglePerFrame = -_anglePerFrame;

            _onRoundFinished.RegisterListener(OnRoundFinished);
        }

        private void Rotate() => transform.Rotate(Vector3.up, _anglePerFrame * Time.deltaTime);

        private void OnRoundFinished()
        {
            _roundFinished = true;
        }

        #endregion


        #region Private

        private float _anglePerFrame;
        private bool _roundFinished;

        #endregion
    }
}