using Archi.Runtime;
using Core.Runtime;
using Mirror;
using ScriptableEvent.Runtime;
using TMPro;
using UnityEngine;
using Utils.Translation;

namespace Core.UI.Runtime
{
    public class TimerUI : CNetBehaviour
    {
        #region Exposed

        [SerializeField] private TextMeshProUGUI _timerTxt;
        [SerializeField] private TextMeshProUGUI _timerValueTxt;

        [SerializeField] private GameEvent _onRoundStart;
        [SerializeField] private GameEvent _onSelectedLanguageChanged;

        #endregion


        #region Unity API

        [ServerCallback]
        private void Awake() => Setup();

        [ServerCallback]
        private void OnDestroy()
        {
            ScoreManager.s_instance.m_onTimerValueChanged -= UpdateTimer;
            _onRoundStart.UnregisterListener(OnRoundStart);
            _onSelectedLanguageChanged.RegisterListener(RefreshLanguage);
        }

        #endregion


        #region Main

        private void Setup()
        {
            ScoreManager.s_instance.m_onTimerValueChanged += UpdateTimer;
            _onRoundStart.RegisterListener(OnRoundStart);
            _onSelectedLanguageChanged.RegisterListener(RefreshLanguage);
        }

        private void RefreshLanguage()
        {
            string txt = _isOvertime ? "Overtime" : "Remaining Time";
            _timerTxt.text = TranslationManager.Translate(txt);
        }

        private void UpdateTimer(float time, bool isOvertime)
        {
            _timerValue = time;
            _isOvertime = isOvertime;
        }

        #endregion


        #region Utils & Tools

        private void OnRoundStart()
        {
            _timerTxt.text = TranslationManager.Translate("Remaining Time");
        }

        private void OnTimerValueChanged(float oldValue, float newValue)
        {
            _timerValueTxt.text = newValue.ToString("00");
        }

        private void OnOvertimeValueChanged(bool oldValue, bool newValue)
        {
            string txt = newValue ? "Overtime" : "Remaining Time";
            _timerTxt.text = TranslationManager.Translate(txt);
        }

        #endregion


        #region Private

        [SyncVar(hook = nameof(OnTimerValueChanged))] private float _timerValue;
        [SyncVar(hook = nameof(OnOvertimeValueChanged))] private bool _isOvertime;

        #endregion
    }
}