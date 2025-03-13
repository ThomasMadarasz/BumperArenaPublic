using Archi.Runtime;
using Dreamteck.Splines;
using UnityEngine;

namespace Traps.Runtime
{
    public class SplineHelper : CNetBehaviour
    {
        #region Exposed

        [SerializeField] private SplineFollower _spline;

        #endregion


        #region Unity API

        private void Awake()
        {
            _spline.follow = false;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            _spline.follow = true;
        }

        #endregion
    }
}