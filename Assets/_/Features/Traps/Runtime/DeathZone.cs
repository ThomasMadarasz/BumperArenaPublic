using Archi.Runtime;
using Interfaces.Runtime;
using UnityEngine;
using Enum.Runtime;

namespace Traps.Runtime
{
    [RequireComponent(typeof(Collider))]
    public class DeathZone : CBehaviour
    {
        #region Exposed

        [SerializeField] DeathType _deathType;

        #endregion


        #region Unity API

        private void OnTriggerEnter(Collider other)
        {
            IKillable killable = other.GetComponent<IKillable>();
            if (killable == null) return;
            killable.Kill(_deathType);
        }

        #endregion
    }
}