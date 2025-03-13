using Data.Runtime;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UINavigation.Runtime;
using UnityEngine;
using Voting.Runtime;

namespace Voting.UI.Runtime
{
    public class VotingUI : MenuBase
    {
        #region Exposed

        [SerializeField] private TextMeshProUGUI _timerTxt;

        [SerializeField] private GameModeChoiceUI _mode1;
        [SerializeField] private GameModeChoiceUI _mode2;

        [SerializeField] private ChoiceUI[] _choices;
        [SerializeField] private ChoiceUI _defaultChoice;
        [SerializeField] private Color _defaultColor;

        [SerializeField] private int _remainingTimeAfterAllPlayerLock;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        private void OnDestroy()
        {
            VotingManager.s_instance.m_onTimerValueChanged -= OnTimerValueChanged;
            VotingManager.s_instance.m_onSelectionFinished -= OnSelectionFinished;
            VotingManager.s_instance.m_onTimerFinished -= OnTimerFinished;
        }

        #endregion


        #region Main

        private void Setup()
        {
            VotingManager.s_instance.m_onTimerValueChanged += OnTimerValueChanged;
            VotingManager.s_instance.m_onSelectionFinished += OnSelectionFinished;
            VotingManager.s_instance.m_onTimerFinished += OnTimerFinished;
        }

        public void UpdateReadyStatus()
        {
            _isReady = !_isReady;
            UpdateReadyStatus(_isReady);
        }

        #endregion


        #region Utils & Tools

        private void OnTimerFinished() => _onTimerFinished = true;

        private void OnSelectionFinished(ModeInfos[] infos, ModeData modeData)
        {
            _playerIndexAndLocalIndex = VotingManager.s_instance.GetPlayersIndex();

            int count = _playerIndexAndLocalIndex.Count(x => x.Value != -1);
            _localPlayersIndex = new int[count];
            _playerLock = new bool[count];

            int i = 0;
            foreach (var item in _playerIndexAndLocalIndex.Where(x => x.Value != -1))
            {
                _localPlayersIndex[i] = item.Key;
                _playerLock[i] = false;
                i++;
            }

            _playerPositions = new int[_localPlayersIndex.Length];
            _playerColors = new Color[_playerPositions.Length];

            int j = 0;
            foreach (var item in _playerIndexAndLocalIndex.Where(x => x.Value != -1))
            {
                _playerColors[j] = VotingManager.s_instance.GetLocalPlayersColors(item.Value);
                j++;
            }

            UpdateSelection(infos, _playerColors.Length, modeData);

            _onTimerFinished = false;
        }

        private void OnTimerValueChanged(float time)
        {
            int roundTime = Mathf.RoundToInt(time);

            if (roundTime == _remainingTime) return;
            _remainingTime = roundTime;
            UpdateTimer(time);
        }

        protected override void NavigateDown_Performed(int playerId) => OnNavigationPerformed(playerId, 2);

        protected override void NavigateLeft_Performed(int playerId) => OnNavigationPerformed(playerId, -1);

        protected override void NavigateRight_Performed(int playerId) => OnNavigationPerformed(playerId, 1);

        protected override void NavigateUp_Performed(int playerId) => OnNavigationPerformed(playerId, -2);

        public override void OnSubmit_Performed(int playerId)
        {
            //int index = _localPlayersIndex.ToList().IndexOf(playerId);
            //_playerLock[index] = true;
            //int pos = GetPositionFromPlayerID(playerId);
            //Lock(pos, index, _playerColors[index]);

            //bool allPlayerLock = !(_playerLock.Count(x => !x) > 0);

            //if (_remainingTime > _remainingTimeAfterAllPlayerLock && allPlayerLock)
            //{
            //    VotingManager.s_instance.ForceTimerValue(_remainingTimeAfterAllPlayerLock);
            //}
        }

        public override void OnCancel_Performed(int playerId)
        {
            //int index = _localPlayersIndex.ToList().IndexOf(playerId);
            //_playerLock[index] = false;
            //int pos = GetPositionFromPlayerID(playerId);
            //Unlock(pos, index, _playerColors[index]);
        }

        private void OnNavigationPerformed(int playerId, int increment)
        {
            if (_onTimerFinished) return;

            int index = _localPlayersIndex.ToList().IndexOf(playerId);
            if (_playerLock[index]) return;

            int pos = GetPositionFromPlayerID(playerId);

            Unselect(pos, index);
            VotingManager.s_instance.UpdateVoteCount(pos, GetVoteFor(pos));

            pos += increment;
            if (pos < 0) pos = _choices.Length - Mathf.Abs(pos);
            else pos %= _choices.Length;

            SetPositionForPlayerID(playerId, pos);

            Select(pos, index, _playerColors[index]);
            VotingManager.s_instance.UpdateVoteCount(pos, GetVoteFor(pos));
        }

        private int GetPositionFromPlayerID(int playerId) => _playerPositions[playerId - 1];

        private void SetPositionForPlayerID(int playerId, int pos) => _playerPositions[playerId - 1] = pos;

        #endregion


        #region Other

        private void UpdateTimer(float time)
        {
            _timerTxt.text = time.ToString("00");
        }

        private void UpdateSelection(ModeInfos[] infos, int playerCount, ModeData modeData)
        {
            GameModeData mode1 = modeData.m_gameModesData.FirstOrDefault(x => x.m_logicIndex == infos[0].m_modeLogicIndex);
            GameModeData mode2 = modeData.m_gameModesData.FirstOrDefault(x => x.m_logicIndex == infos[1].m_modeLogicIndex);

            _mode1.Setup(mode1, infos[0].m_mapsLogicIndex);
            _mode2.Setup(mode2, infos[1].m_mapsLogicIndex);

            foreach (var item in _choices)
            {
                item.SetPlayerCount(playerCount);
                item.ResetVotes(_defaultColor);
            }

            for (int i = 0; i < _localPlayersIndex.Length; i++)
            {
                _defaultChoice.Select(_localPlayersIndex[i] - 1, _playerColors[i]);
                int defaultMapIndex = _choices.ToList().IndexOf(_defaultChoice);

                int voteCount = GetVoteFor(defaultMapIndex);
                VotingManager.s_instance.UpdateVoteCount(defaultMapIndex, voteCount);
            }
        }

        private void Unselect(int pos, int playerId)
        {
            _choices[pos].UnSelect(playerId, _defaultColor);
        }

        private void Select(int pos, int playerId, Color playerColor)
        {
            _choices[pos].Select(playerId, playerColor);
        }

        private void Lock(int pos, int playerId, Color playerColor)
        {
            _choices[pos].Lock(playerId, playerColor);
        }

        private void Unlock(int pos, int playerId, Color playerColor)
        {
            _choices[pos].Unlock(playerId, playerColor);
        }

        private int GetVoteFor(int pos)
        {
            return _choices[pos].GetVoteCount();
        }

        private void UpdateReadyStatus(bool value)
        {
            _playerReadyCount = value ? _playerReadyCount + 1 : _playerReadyCount - 1;

            if (_playerReadyCount != VotingManager.s_instance.GetPlayerCount()) return;
            VotingManager.s_instance.AllPlayerIsReady();
        }

        #endregion


        #region Private

        private int _remainingTime;
        private int _playerReadyCount;

        private bool _isReady;
        private bool _onTimerFinished;

        private int[] _playerPositions;
        private int[] _localPlayersIndex;
        private bool[] _playerLock;
        private Color[] _playerColors;

        private Dictionary<int, int> _playerIndexAndLocalIndex;

        #endregion
    }
}