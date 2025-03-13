using Archi.Runtime;
using ScriptableEvent.Runtime;
using TMPro;
using UnityEngine;
using Utils.Translation;

namespace Core.UI.Runtime
{
    public class ScoreUI : CBehaviour
    {
        #region Exposed

        [SerializeField] private TextMeshProUGUI _playerTxt;
        [SerializeField] private TextMeshProUGUI _scoreTxt;
        [SerializeField] private GameEvent _onSelectedLanguageChanged;

        public int m_playerId { get { return _playerId; } }
        public int m_teamId { get { return _teamId; } }

        #endregion


        #region Unity API

        private void Awake()
        {
            _onSelectedLanguageChanged.RegisterListener(RefreshLanguage);
        }

        private void OnDestroy()
        {
            _onSelectedLanguageChanged.UnregisterListener(RefreshLanguage);
        }

        #endregion


        #region Main

        private void RefreshLanguage()
        {
            if (!_isInitialized) return;

            if (_isDuoMode)
            {
                string teamChar = TranslationManager.Translate("Team");
                _playerTxt.text = $"{teamChar} {_teamId}";
            }
            else
            {
                string playerChar = _isAI ? TranslationManager.Translate("PlayerNumberKey_AI") : TranslationManager.Translate("PlayerNumberKey");
                _playerTxt.text = $"{playerChar}{_playerId + 1}";
            }
        }

        public void UpdatePlayer(int value, bool isAI,bool isDuoMode,int teamID)
        {
            _playerId = value;
            _teamId = teamID;
            _isDuoMode = isDuoMode;
            _isAI = isAI;

            if (isDuoMode)
            {
                string teamChar =TranslationManager.Translate("Team");
                _playerTxt.text = $"{teamChar} {teamID}";
            }
            else
            {
                string playerChar = isAI ? TranslationManager.Translate("PlayerNumberKey_AI") : TranslationManager.Translate("PlayerNumberKey");
                _playerTxt.text = $"{playerChar}{_playerId + 1}";
            }

            _isInitialized = true;
        }

        public void UpdateScore(int value)
        {
            _scoreValue = value;
            _scoreTxt.text = _scoreValue.ToString();
        }

        public void UpdateColorText(Color color)
        {
            _playerTxt.color = color;
            _scoreTxt.color = color;
        }

        #endregion


        #region Private

        private int _playerId;
        private int _teamId;
        private int _scoreValue;
        private bool _isDuoMode;
        private bool _isInitialized;
        private bool _isAI;

        #endregion
    }
}