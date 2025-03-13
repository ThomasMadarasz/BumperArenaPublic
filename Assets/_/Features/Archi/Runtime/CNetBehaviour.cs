using Mirror;
using UnityEngine;

namespace Archi.Runtime
{
    public class CNetBehaviour : NetworkBehaviour
    {
        #region Exposed

        private Transform _transform;
        public new Transform transform =>
            _transform ? _transform : _transform = GetComponent<Transform>();

        private Rigidbody _rigidbody;
        public Rigidbody rigidBody =>
            _rigidbody ? _rigidbody : _rigidbody = GetComponent<Rigidbody>();

        private Collider _collider;
        public new Collider collider =>
            _collider ? _collider : _collider = GetComponent<Collider>();


        #endregion
    }
}