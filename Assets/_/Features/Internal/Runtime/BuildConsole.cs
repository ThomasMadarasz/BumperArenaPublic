using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace Internal.Runtime
{
    public class BuildConsole : MonoBehaviour, IDragHandler, IBeginDragHandler
    {
        #region Exposed

        [SerializeField] private Transform _parent;
        [SerializeField] private GameObject _logPrefab;

        [SerializeField] private int _maxLogCount;

        #endregion


        #region Unity API

        private void Awake() => Application.logMessageReceived += OnLogMessageReceived;

        #endregion


        #region Main

        private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            GameObject obj = Instantiate(_logPrefab, _parent);
            TextMeshProUGUI txt = obj.GetComponent<TextMeshProUGUI>();

            Color color = Color.white;

            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                case LogType.Assert:
                    color = Color.red;
                    break;

                case LogType.Warning:
                    color = Color.yellow;
                    break;

                case LogType.Log:
                    color = Color.white;
                    break;
            }

            txt.text = condition;
            txt.color = color;

            _queue.Enqueue(obj);

            if (_queue.Count > _maxLogCount)
                Destroy(_queue.Dequeue());
        }

        public void OnBeginDrag(PointerEventData eventData) => _lastMousePosition = eventData.position;

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 currentMousePosition = eventData.position;
            Vector2 diff = currentMousePosition - _lastMousePosition;
            RectTransform rect = GetComponent<RectTransform>();

            Vector3 newPosition = rect.position + new Vector3(diff.x, diff.y, transform.position.z);
            Vector3 oldPos = rect.position;
            rect.position = newPosition;
            if (!IsRectTransformInsideSreen(rect))
            {
                rect.position = oldPos;
            }
            _lastMousePosition = currentMousePosition;
        }

        #endregion


        #region Utils & Tools

        private bool IsRectTransformInsideSreen(RectTransform rectTransform)
        {
            bool isInside = false;
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            int visibleCorners = 0;
            Rect rect = new Rect(0, 0, Screen.width, Screen.height);
            foreach (Vector3 corner in corners)
            {
                if (rect.Contains(corner)) visibleCorners++;
            }
            if (visibleCorners == 4) isInside = true;

            return isInside;
        }

        #endregion


        #region Private

        private Queue<GameObject> _queue = new();

        private Vector2 _lastMousePosition;

        #endregion
    }
}