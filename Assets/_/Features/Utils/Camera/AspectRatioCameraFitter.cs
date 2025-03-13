using UnityEngine;

namespace Utils.Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    [ExecuteAlways]
    public class AspectRatioCameraFitter : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Camera _cam;

        private void OnValidate()
        {
            _cam ??= GetComponent<UnityEngine.Camera>();
        }

        public void LateUpdate()
        {
            var currentScreenResolution = new Vector2(Screen.width, Screen.height);

            // Don't run all the calculations if the screen resolution has not changed
            if (_lastResolution != currentScreenResolution)
            {
                CalculateCameraRect(currentScreenResolution);
            }
            else return;

            _lastResolution = currentScreenResolution;
            _cam.Render();
        }

        private void CalculateCameraRect(Vector2 currentScreenResolution)
        {
            var normalizedAspectRatio = _targetAspectRatio / currentScreenResolution;
            var size = normalizedAspectRatio / Mathf.Max(normalizedAspectRatio.x, normalizedAspectRatio.y);
            _cam.rect = new Rect(default, size) { center = _rectCenter };
        }

        #region Private

        private readonly Vector2 _targetAspectRatio = new(16, 9);
        private readonly Vector2 _rectCenter = new(0.5f, 0.5f);

        private Vector2 _lastResolution;

        #endregion
    }
}