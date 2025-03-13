using UnityEngine;

namespace Archi.Runtime
{
    public class CBehaviour : MonoBehaviour
    {
        #region Exposed

        private Transform _transform;
        public new Transform transform =>
            _transform ? _transform : _transform = GetComponent<Transform>();

        private Rigidbody _rigidbody;
        public Rigidbody rigidBody =>
            _rigidbody ? _rigidbody : _rigidbody = GetComponent<Rigidbody>();

        #endregion
    }
}