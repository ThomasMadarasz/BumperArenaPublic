using Archi.Runtime;
using Dreamteck.Splines;
using Mirror;
using ScriptableEvent.Runtime;
using UnityEngine;

namespace SplineNetworking.Runtime
{
    [RequireComponent(typeof(SplineFollower))]
    public class SplineNetworkTransform : CNetBehaviour
    {
        #region Exposed

        [SerializeField] private GameEvent _onStartSpline;
        [SerializeField] private float _maxDistanceOffset;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        public override void OnStartServer()
        {
            base.OnStartServer();
            SetupServer();
        }

        #endregion


        #region Main

        private void Setup()
        {
            _spline = GetComponent<SplineFollower>();
            _spline.follow = false;
        }

        private void SetupServer()
        {
            _onStartSpline.RegisterListener(OnStartFollowing);
        }

        private void OnStartFollowing()
        {
            InvokeRepeating(nameof(SendPositionAtClient), 0, 1);
            Rpc_StartFollowing();
        }

        [ServerCallback]
        private void SendPositionAtClient()
        {
            Rpc_SyncPosition(transform.position);
        }

        #endregion


        #region Rpc

        [ClientRpc]
        private void Rpc_SyncPosition(Vector3 position)
        {
            if((transform.position - position).magnitude > _maxDistanceOffset)
            {
                transform.position = position;
            }            
        }

        [ClientRpc]
        private void Rpc_StartFollowing()
        {
            _spline.follow = true;
        }

        #endregion


        #region Private

        private SplineFollower _spline;

        #endregion
    }
}