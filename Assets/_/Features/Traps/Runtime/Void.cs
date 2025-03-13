using Archi.Runtime;
using Interfaces.Runtime;
using Mirror;
using PlayerController.Data.Runtime;
using UnityEngine;

namespace Traps.Runtime
{
    [RequireComponent(typeof(Collider))]
    public class Void : CBehaviour
    {
        #region Exposed

        [SerializeField] private PlayerControllerData _controllerData;
        [SerializeField] private PlayerDecelerationData _decelerationData;

        #endregion


        #region Unity API   

        [ServerCallback]
        private void OnTriggerEnter(Collider other)
        {
            IBoostable boost = other.GetComponent<IBoostable>();
            if (boost != null)
                boost.DisableBoost();

            IMoveable moveable = other.GetComponent<IMoveable>();
            if (moveable != null)
                moveable.ChangeMoveData(_controllerData, _decelerationData);
        }

        [ServerCallback]
        private void OnTriggerExit(Collider other)
        {
            IBoostable boost = other.GetComponent<IBoostable>();
            if (boost != null)
                boost.EnableBoost();

            IMoveable moveable = other.GetComponent<IMoveable>();
            if (moveable != null)
                moveable.ChangeMoveData(null, null);
        }

        #endregion
    }
}