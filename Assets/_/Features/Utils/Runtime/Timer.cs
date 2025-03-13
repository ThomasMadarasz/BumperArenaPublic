using System;

namespace Utils.Runtime
{
    public class Timer
    {
        #region Exposed

        public Action<float> OnValueChanged;

        #endregion


        #region Main

        /// <summary>
        /// Use Start() function to launch the timer
        /// Remark : if timer is repeteable, dont forget to use Stop() function before destroying timer instance
        /// </summary>
        /// <param name="t">Timer duration</param>
        /// <param name="callback">Callback automatically called when timer is finished</param>
        /// <param name="repeating">Set this parameter to true if you want to make this timer repetable</param>
        public Timer(float t, Action callback = null, bool repeating = false)
        {
            _duration = t;
            _callback = callback;
            _repeating = repeating;
        }

        protected virtual void SubstractTime(float time)
        {
            if (_isFinished) return;

            _remainingTime -= time;
            OnValueChanged?.Invoke(_remainingTime);

            if (IsTimeOver())
            {
                if (_repeating) ResetTimer();
                else Stop();

                _callback?.Invoke();
            }
        }

        public void ResetTimer()
        {
            _remainingTime = _duration;
        }

        public bool IsTimeOver() => _remainingTime <= 0;

        public void Stop()
        {
            _isFinished = true;
            _isRunning = false;
            UpdateCaller.OnUpdate -= SubstractTime;
        }

        public void Start()
        {
            _isFinished = false;
            _isRunning = true;
            ResetTimer();
            UpdateCaller.OnUpdate += SubstractTime;
        }

        public bool IsRunning() => _isRunning;
        #endregion


        #region Private

        private float _remainingTime;
        private float _duration;

        private bool _isFinished;
        private bool _isRunning;
        private bool _repeating;

        private Action _callback;

        #endregion
    }
}