using ScriptableEvent.Runtime;
using UnityEngine;

namespace Feedbacks.Runtime
{
    public class OvertimeUpdater : MonoBehaviour
    {
        #region Exposed

        [SerializeField] private Material _mat;
        [SerializeField] private Color _color;
        [SerializeField] private string _propertyName;
        [SerializeField] private GameEventT _onRemainingTimeChanged;
        [SerializeField] private GameEvent _onRoundStart;
        [SerializeField] private Animator _animator;
        [SerializeField] private AnimationClip _clip;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        private void OnDestroy()
        {
            _animator.Play("Empty");

            if (!_roundStartUnregister) _onRoundStart.UnregisterListener(OnRoundStart);
            _onRemainingTimeChanged.UnregisterListener(OnRemainingTimeChanged);
        }

        #endregion


        #region Main

        private void Setup()
        {
            int apropertyID = Shader.PropertyToID(_propertyName);
            _mat.SetColor(apropertyID, _color);

            _onRoundStart.RegisterListener(OnRoundStart);
        }

        private void OnRoundStart()
        {
            _onRoundStart.UnregisterListener(OnRoundStart);
            _roundStartUnregister = true;
            _onRemainingTimeChanged.RegisterListener(OnRemainingTimeChanged);
        }

        private void OnRemainingTimeChanged(object obj)
        {
            object[] values = (object[])obj;
            bool isOvertime = (bool)values[1];
            float remainingTime = (float)values[0];


            if (!isOvertime && !_isAnimationLaunched && remainingTime - 0.1f <= _clip.length)
            {
                _isAnimationLaunched = true;
                _animator.Play("anim_CloseToEnd");
                return;
            }

            if (isOvertime && !_isOvertimeAnimationLaunched)
            {
                _isOvertimeAnimationLaunched = true;
                _animator.Play("anim_Overtime");
                return;
            }
        }

        #endregion


        #region Private

        private bool _isAnimationLaunched;
        private bool _isOvertimeAnimationLaunched;
        private bool _roundStartUnregister;

        #endregion
    }
}