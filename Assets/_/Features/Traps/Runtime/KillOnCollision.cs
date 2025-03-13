using Archi.Runtime;
using Interfaces.Runtime;
using UnityEngine;
using Enum.Runtime;

namespace Traps.Runtime
{
    [RequireComponent(typeof(Collider))]
    public class KillOnCollision : CBehaviour
    {
        #region Exposed

        [SerializeField] DeathType _deathType;

        #endregion


        #region Unity API

        private void OnCollisionEnter(Collision collision)
        {
            collision.gameObject.GetComponent<IKillable>()?.Kill(_deathType);
        }

        #endregion
    }
}