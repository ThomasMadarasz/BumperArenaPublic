using Sirenix.OdinInspector;
using UnityEngine;

namespace Utils.Runtime
{
    public class FreezeRotation : MonoBehaviour
    {
        #region Exposed

        [SerializeField] private Vector3 _rotation;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        private void Update() => UpdateRotation();

        #endregion


        #region Main

        private void Setup()
        {
            _quat = Quaternion.Euler(_rotation);
        }

        [Button]
        private void SaveRotation()
        {
            _rotation = transform.rotation.eulerAngles;
        }

        private void UpdateRotation()
        {
            transform.rotation = _quat;
        }

        #endregion


        #region Private

        private Quaternion _quat;

        #endregion
    }
}