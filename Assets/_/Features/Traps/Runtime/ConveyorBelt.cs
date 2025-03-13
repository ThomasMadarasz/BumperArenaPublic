using Archi.Runtime;
using Interfaces.Runtime;
using Mirror;
using UnityEngine;

namespace Traps.Runtime
{
    [RequireComponent(typeof(Collider))]
    public class ConveyorBelt : CBehaviour
    {
        #region Exposed

        [SerializeField] private Transform _startPoint;
        [SerializeField] private Transform _endPoint;

        [SerializeField] private float _conveyorSpeed;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        [ServerCallback]
        private void OnTriggerStay(Collider other)
        {
            ISubjectToAForce player = other.GetComponent<ISubjectToAForce>();
            if (player == null) return;

            player.AddExternalForce(_direction * _conveyorSpeed);
        }

        [ServerCallback]
        private void OnTriggerExit(Collider other)
        {
            ISubjectToAForce player = other.GetComponent<ISubjectToAForce>();
            if (player == null) return;

            player.AddExternalForce(Vector3.zero);
        }

        #endregion


        #region Main

        private void Setup()
        {
            _direction = _endPoint.position - _startPoint.position;
            _direction.y = 0;
            _direction = _direction.normalized;
        }

        #endregion


        #region Utils & Tools
        #endregion


        #region Debug
        #endregion


        #region Private

        private Vector3 _direction;

        #endregion
    }
}