using UnityEngine;
using UnityEngine.InputSystem;

public class CursorHelper : MonoBehaviour
{
    #region Exposed

    [SerializeField] private float _timeBeforeHideCursor;

    #endregion


    #region Unity API

    private void Update()
    {
        Vector2 currentPos = Mouse.current.position.ReadValue();
        if (currentPos != _lastMousePos)
        {
            _lastMousePos = currentPos;
            _remaininTimeBeforeHideCursor = _timeBeforeHideCursor;
            ShowHideCursror(true);
        }
        else
        {
            if (_cursorIsHide) return;
            _remaininTimeBeforeHideCursor -= Time.deltaTime;

            if (_remaininTimeBeforeHideCursor <= 0) ShowHideCursror(false);
        }
    }

    #endregion


    #region Main

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    private static void BeforeSplashScreen()
    {
        Cursor.visible = false;
    }

    private void ShowHideCursror(bool show)
    {
        Cursor.visible = show;
        _cursorIsHide = !show;
    }

    #endregion


    #region Private

    private Vector2 _lastMousePos;
    private float _remaininTimeBeforeHideCursor;
    private bool _cursorIsHide = true;

    #endregion
}