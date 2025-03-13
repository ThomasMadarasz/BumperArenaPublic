using HighlightPlus;
using System.Collections;
using UnityEngine;

namespace Feedbacks.Runtime
{
    public class LerpOutlineColor : MonoBehaviour
    {
        #region Exposed

        [SerializeField] private float _lerpDuration;
        [ColorUsage(true, true)]
        [SerializeField] private Color _targetedColor;
        [SerializeField] private HighlightEffect _effect;

        #endregion


        #region Main

        public void StartLerp() => StartCoroutine(nameof(LerpRoutine));
        public void SetInitialColor(Color color) => _initialColor = color;

        private IEnumerator LerpRoutine()
        {
            float elepsedTime = _lerpDuration;
            while (elepsedTime > 0)
            {
                elepsedTime -= Time.deltaTime;
                float ratio = 1 - (elepsedTime / _lerpDuration);

                Color col = Color.Lerp(_initialColor, _targetedColor, ratio);
                _effect.outlineColor = col;
                yield return null;
            }
        }

        #endregion


        #region Private

        private Color _initialColor;

        #endregion
    }
}