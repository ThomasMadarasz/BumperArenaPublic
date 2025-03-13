using Interfaces.Runtime;
using UnityEngine;
using Archi.Runtime;

namespace Traps.Runtime
{
    [RequireComponent(typeof(Collider))]
    public class Bounce : CBehaviour, IBouncable
    {
        #region Exposed

        [SerializeField] private float _bounceFactor;

        #endregion


        #region Main

        public float GetBounceFactor() => _bounceFactor;

        #endregion
    }
}