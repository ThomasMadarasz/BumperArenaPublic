using UnityEngine;

namespace CameraSwap.Runtime
{
    public class CameraSwapReset : MonoBehaviour
    {
        #region Exposed

        [SerializeField] private Material _fadeInMat;
        [SerializeField] private Material _fadeOutMat;

        [SerializeField] private float _maxOffset;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        #endregion


        #region Main

        private void Setup()
        {
            _fadeInMat.SetFloat(_offsetPropertyName, 0.001f);
            _fadeOutMat.SetFloat(_offsetPropertyName, _maxOffset);
        }

        #endregion

        #region Private

        private string _offsetPropertyName = "_Offset";

        #endregion
    }
}