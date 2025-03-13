using Archi.Runtime;
using Interfaces.Runtime;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace Traps.Runtime
{
    public class DetectionZone : CBehaviour
    {
        #region Exposed

        [SerializeField] private UnityEvent _onObjectDetected;

        #endregion


        #region Unity API

        [ServerCallback]
        private void OnTriggerStay(Collider other)
        {
            if (other.GetComponent<IKillable>() == null) return;
            _onObjectDetected?.Invoke();
        }

        #endregion
    }
}