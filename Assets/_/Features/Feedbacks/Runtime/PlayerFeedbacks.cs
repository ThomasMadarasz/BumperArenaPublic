using ScriptableEvent.Runtime;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.VFX;
using Enum.Runtime;
using MoreMountains.Feedbacks;
using System.Collections;
using ScreenShakes.Runtime;
using Interfaces.Runtime;
using HighlightPlus;
using System.Linq;
using Data.Runtime;
using Utils.Runtime;
using Audio.Runtime;

namespace Feedbacks.Runtime
{
    public class PlayerFeedbacks : FeedbackBase
    {
        #region Exposed

        [SerializeField] private GameEvent _onFeedbacksInitialized;
        [SerializeField] private GameEvent _onAllPlayerAsSceneLoaded;
        [SerializeField] private GameEventT _resetAllFeedbacks;
        [SerializeField] private GameEventT _onScreenShake;

        [SerializeField][TabGroup("VFX", "Common")] private GameEventT _onBoostPackIntake;
        [SerializeField][TabGroup("VFX", "Common")] private GameEventT _onRespawn;
        [SerializeField][TabGroup("VFX", "Common")] private GameEventT2 _onDie;
        [SerializeField][TabGroup("VFX", "Common")] private GameEventT _onPointScored;
        [SerializeField][TabGroup("VFX", "Common")] private GameEventT _onBoostStarted;
        [SerializeField][TabGroup("VFX", "Common")] private GameEventT _onBoostFinished;
        [SerializeField][TabGroup("VFX", "Common")] private GameEventT _onPlayerBoostGaugeChanged;
        [SerializeField][TabGroup("VFX", "Common")] private GameObject[] _hideOnDeath;
        [SerializeField][TabGroup("VFX", "Common")] private GameObject[] _hideOnDeathWithoutRescale;
        [SerializeField][TabGroup("VFX", "Common")] private GameObject _gaugeBoost;
        [SerializeField][TabGroup("VFX", "Common")] private GameObject _playerNumberUI;
        [SerializeField][TabGroup("VFX", "Common")] private GameEventT _playerFeedbacks;
        [SerializeField][TabGroup("VFX", "Common")] private GameEventT _onPlayerRespawnImmunityEnd;
        [SerializeField][TabGroup("VFX", "Common")] private VisualEffect _scorePointVFX;

        [SerializeField][BoxGroup("Management")] private GameEvent _onRoundStarted;
        [SerializeField][BoxGroup("Management")] private GameEvent _onRoundFinished;

        [SerializeField][TabGroup("VFX", "CTF")] private GameEventT _onFlagIntake;
        [SerializeField][TabGroup("VFX", "CTF")] private GameEventT _onFlagReset;

        [SerializeField][TabGroup("VFX", "CTF")] private PlayableDirector[] _takeFlagTimelines;
        [SerializeField][TabGroup("VFX", "CTF")] private PlayableDirector[] _resetFlagTimelines;

        [SerializeField][TabGroup("VFX", "Boost")] private VisualEffect _boostIntake;
        [SerializeField][TabGroup("VFX", "Boost")] private VisualEffect[] _boostGauge;
        [SerializeField][TabGroup("VFX", "Boost")] private VisualEffect _boost;
        [SerializeField][TabGroup("VFX", "Boost")] private Renderer _boostRenderer;
        [SerializeField][TabGroup("VFX", "Boost")] private ReactorData _boostReactorData;
        [SerializeField][TabGroup("VFX", "Boost")] private TrailData _boostTrailData;

        [SerializeField][TabGroup("VFX", "Crown")] private GameEventT _onTakeCrown;
        [SerializeField][TabGroup("VFX", "Crown")] private GameEventT _onCrownImmunityFinished;
        [SerializeField][TabGroup("VFX", "Crown")] private GameEventT _onCrownLost;
        [SerializeField][TabGroup("VFX", "Crown")] private Animator _crownAnimator;
        [SerializeField][TabGroup("VFX", "Crown")] private Animator _crownBumperAnimator;
        [SerializeField][TabGroup("VFX", "Crown")] private Animator _playerCrownAnimator;
        [SerializeField][TabGroup("VFX", "Crown")] private GameObject _crownGraphics;
        [SerializeField][TabGroup("VFX", "Crown")] private GameObject _crownVisualEffect;
        [SerializeField][TabGroup("VFX", "Crown")] private Renderer _bumperRenderer;
        [SerializeField][TabGroup("VFX", "Crown")] private HighlightEffect _bumperEffect;
        [SerializeField][TabGroup("VFX", "Crown")] private HighlightEffect _crownEffect;
        [SerializeField][TabGroup("VFX", "Crown")] private HighlightEffect _playerEffect;
        [SerializeField][TabGroup("VFX", "Crown")] private string _takeCrownAnimName;
        [SerializeField][TabGroup("VFX", "Crown")] private string _onCrownimmunityFinishedAnimName;
        [SerializeField][TabGroup("VFX", "Crown")] private string _onLostCrownAnimName;
        [SerializeField][TabGroup("VFX", "Crown")] private string _bumperImmunityOnAnimName;
        [SerializeField][TabGroup("VFX", "Crown")] private string _bumperImmunityOffAnimName;
        [SerializeField][TabGroup("VFX", "Crown")] private string _playerTakeCrownAnimName;
        [SerializeField][TabGroup("VFX", "Crown")] private string _playerLostCrownAnimName;
        [SerializeField][TabGroup("VFX", "Crown")] private LerpOutlineColor _crownLerp;

        [SerializeField][TabGroup("VFX", "Death")] private GameObject _electricFeedback;
        [SerializeField][TabGroup("VFX", "Death")] private float _electricDuration;
        [SerializeField][TabGroup("VFX", "Death")] private GameObject _explodedFeedback;
        [SerializeField][TabGroup("VFX", "Death")] private float _explodedDuration;
        [SerializeField][TabGroup("VFX", "Death")] private MMF_Player _crushedFeedback;
        [SerializeField][TabGroup("VFX", "Death")] private float _crushedDuration;
        [SerializeField][TabGroup("VFX", "Death")] private MMF_Player _suckedOutFeedback;
        [SerializeField][TabGroup("VFX", "Death")] private float _ejectedDuration;
        [SerializeField][TabGroup("VFX", "Death")] private float _ejectedRotationSpeed;
        [SerializeField][TabGroup("VFX", "Death")] private MMF_Player _spaceSuckedOutFeedback;
        [SerializeField][TabGroup("VFX", "Death")] private string _playeRespawnImmunityAnimName;
        [SerializeField][TabGroup("VFX", "Death")] private string _playerDeathBlackholeAnimName;
        [SerializeField][TabGroup("VFX", "Death")] private string _playerDeathSpaceAnimName;
        [SerializeField][TabGroup("VFX", "Death")] private string _playerCrushedAnimName;

        [SerializeField][TabGroup("VFX", "Torque")] private ReactorData _normalReactorData;
        [SerializeField][TabGroup("VFX", "Torque")] private ReactorData _torqueReactorData;
        [SerializeField][TabGroup("VFX", "Torque")] private TrailData _normalTrailData;
        [SerializeField][TabGroup("VFX", "Torque")] private TrailData _torqueTrailData;

        [SerializeField][TabGroup("VFX", "Void")] private ReactorData _voidReactorData;
        [SerializeField][TabGroup("VFX", "Void")] private TrailData _voidTrailData;

        [SerializeField][BoxGroup("SFX_Ingame")] private SFXData _onBoostSfxData;
        [SerializeField][BoxGroup("SFX_Ingame")] private SFXData _onTorqueSfxData;
        [SerializeField][BoxGroup("SFX_Ingame")] private SFXData _onGetPointSfxData;
        [SerializeField][BoxGroup("SFX_Ingame")] private SFXData _onDieBlackHoleSfxData;
        [SerializeField][BoxGroup("SFX_Ingame")] private SFXData _onDieHammerSfxData;
        [SerializeField][BoxGroup("SFX_Ingame")] private SFXData _onDieElecSfxData;
        [SerializeField][BoxGroup("SFX_Ingame")] private SFXData _onDieFireSfxData;
        [SerializeField][BoxGroup("SFX_Ingame")] private SFXData _onNoBoostSfxData;
        [SerializeField][BoxGroup("SFX_Ingame")] private SFXData _onBumpSfxData;

        [SerializeField][BoxGroup("SFX_Menu")] SFXData _backSfxData;
        [SerializeField][BoxGroup("SFX_Menu")] SFXData _lockSfxData;
        [SerializeField][BoxGroup("SFX_Menu")] SFXData _switchSfxData;
        [SerializeField][BoxGroup("SFX_Menu")] SFXData _selectSfxData;

        [SerializeField][BoxGroup("Rumble")] private RumbleData _onTakeFlagData;
        [SerializeField][BoxGroup("Rumble")] private RumbleData _onScorePointData;
        [SerializeField][BoxGroup("Rumble")] private RumbleData _onDieData;

        [SerializeField] private SpecificGamepadFeedbacks _specificGamepadFeedbacks;
        [SerializeField] private RumbleManager _rumbleManager;

        [SerializeField][BoxGroup("ScreenShake")] private ScreenShakeData _scorePoint_DeathMatch;
        [SerializeField][BoxGroup("ScreenShake")] private ScreenShakeData _scorePoint_CTF;
        [SerializeField][BoxGroup("ScreenShake")] private ScreenShakeData _scorePoint_Hockey;
        [SerializeField][BoxGroup("ScreenShake")] private ScreenShakeData _stealCrownScreenShake;

        [SerializeField][BoxGroup("BoostBar")][ColorUsage(true, true)] private Color _normalBoostBarColor;
        [SerializeField][BoxGroup("BoostBar")][ColorUsage(true, true)] private Color _voidBoostBarColor;
        [SerializeField][BoxGroup("BoostBar")][ColorUsage(true, true)] private Color _normalBoostBarFillColor;
        [SerializeField][BoxGroup("BoostBar")][ColorUsage(true, true)] private Color _voidBoostBarFillColor;
        [SerializeField][BoxGroup("BoostBar")] private Renderer _boostBarRenderer;
        [SerializeField][BoxGroup("BoostBar")] private VisualEffect[] _boostBarFillEffects;

        #endregion


        #region Unity API

        private void Awake()
        {
            _takeCrownAnim = Animator.StringToHash(_takeCrownAnimName);
            _crownImmunityFinishedAnim = Animator.StringToHash(_onCrownimmunityFinishedAnimName);
            _lostCrownAnim = Animator.StringToHash(_onLostCrownAnimName);

            _bumperImmunityOnAnim = Animator.StringToHash(_bumperImmunityOnAnimName);
            _bumperImmunityOffAnim = Animator.StringToHash(_bumperImmunityOffAnimName);

            _playerTakeCrownAnim = Animator.StringToHash(_playerTakeCrownAnimName);
            _playerLostCrownAnim = Animator.StringToHash(_playerLostCrownAnimName);

            _playerRespawnImmunityAnim = Animator.StringToHash(_playeRespawnImmunityAnimName);
            _playerDeathBlackholeAnim = Animator.StringToHash(_playerDeathBlackholeAnimName);
            _playerDeathSpaceAnim = Animator.StringToHash(_playerDeathSpaceAnimName);
            _playerCrushedAnim = Animator.StringToHash(_playerCrushedAnimName);

            Setup();
        }

        private void OnDestroy()
        {
            UnregisterEvents();
        }

        #endregion


        #region Main

        private void Setup()
        {
            _pGraphic = GetComponent<IPlayerGraphic>();
            RegisterEvents();

            RemoveTorqueFeedback();
        }

        public void SetupGamepad()
        {
            _rumbleManager = GetComponent<RumbleManager>();
            _rumbleManager.Setup();
            _specificGamepadFeedbacks.Setup(_rumbleManager.GetCurrentGamepad());
        }

        private void OnAllPlayerReady()
        {
            _isInitialized = true;
            _onFeedbacksInitialized?.Raise();
        }

        #endregion


        #region Main

        private void RegisterEvents()
        {
            _resetAllFeedbacks.RegisterListener(ResetAllVFX);
            _playerFeedbacks.RegisterListener(EnableDisableFeedbacks);
            _onBoostPackIntake.RegisterListener(VFX_BoostPackIntake);
            _onBoostStarted.RegisterListener(VFX_OnBoostStarted);
            _onBoostFinished.RegisterListener(VFX_OnBoostFinished);

            _onPlayerBoostGaugeChanged.RegisterListener(VFX_OnGaugeChanged);
            _onAllPlayerAsSceneLoaded?.RegisterListener(OnAllPlayerReady);

            _onDie.RegisterListener(VFX_Die);
            _onRespawn.RegisterListener(VFX_Respawn);
            _onPlayerRespawnImmunityEnd.RegisterListener(VFX_RespawnImmunityEnd);

            _onPointScored.RegisterListener(VFX_ScorePoint);

            _onFlagIntake.RegisterListener(VFX_FlagIntake);
            _onFlagReset.RegisterListener(VFX_FlagReset);

            _onTakeCrown.RegisterListener(VFX_TakeCrown);
            _onCrownImmunityFinished.RegisterListener(VFX_CrownImmunityFinished);
            _onCrownLost.RegisterListener(VFX_LostCrown);

            _onRoundStarted.RegisterListener(OnRoundStarted);
            _onRoundFinished.RegisterListener(OnRoundStarted);
        }

        private void UnregisterEvents()
        {
            _resetAllFeedbacks.UnregisterListener(ResetAllVFX);
            _playerFeedbacks.UnregisterListener(EnableDisableFeedbacks);
            _onBoostPackIntake.UnregisterListener(VFX_BoostPackIntake);
            _onBoostStarted.UnregisterListener(VFX_OnBoostStarted);
            _onBoostFinished.UnregisterListener(VFX_OnBoostFinished);

            _onPlayerBoostGaugeChanged.UnregisterListener(VFX_OnGaugeChanged);
            _onAllPlayerAsSceneLoaded?.UnregisterListener(OnAllPlayerReady);

            _onDie.UnregisterListener(VFX_Die);
            _onRespawn.UnregisterListener(VFX_Respawn);
            _onPlayerRespawnImmunityEnd.UnregisterListener(VFX_RespawnImmunityEnd);

            _onPointScored.UnregisterListener(VFX_ScorePoint);

            _onFlagIntake.UnregisterListener(VFX_FlagIntake);
            _onFlagReset.UnregisterListener(VFX_FlagReset);

            _onTakeCrown.UnregisterListener(VFX_TakeCrown);
            _onCrownImmunityFinished.UnregisterListener(VFX_CrownImmunityFinished);
            _onCrownLost.UnregisterListener(VFX_LostCrown);

            _onRoundStarted.UnregisterListener(OnRoundStarted);
            _onRoundFinished.UnregisterListener(OnRoundStarted);
        }

        private void OnRoundStarted() => _roundStarted = true;
        private void OnRoundsFinished() => _roundStarted = false;

        public void PlayNoBoostSfx()
        {
            PlaySfxFeedback(_onNoBoostSfxData, false);
        }

        public void PlayTorqueSfx()
        {
            PlaySfxFeedback(_onTorqueSfxData, false);
        }

        public void PlayBumpSfx()
        {
            PlaySfxFeedback(_onBumpSfxData, false);
        }

        public void PlayRumble(RumbleData data)
        {
            if (!_useRumble) return;

            _rumbleManager.Rumble(data);
        }

        public void PlayScreenShake(ScreenShakeData data)
        {
            ScreenShakeParameters parameters = new ScreenShakeParameters(data);
            _onScreenShake.Raise(parameters);
        }

        public void PlayMenuBackSfx()
        {
            PlaySfxFeedback(_backSfxData, true);
        }

        public void PlayMenuSelectSfx()
        {
            PlaySfxFeedback(_selectSfxData, true);
        }

        public void PlayMenuSwitchSfx()
        {
            PlaySfxFeedback(_switchSfxData, true);
        }

        public void PlayMenuLockSfx()
        {
            PlaySfxFeedback(_lockSfxData, true);
        }

        public void ApplyTorqueFeedback()
        {
            if (!_isInitialized || !_roundStarted) return;
            if (_reactors == null) _reactors = GetComponentInChildren<PlayerReactors>();
            _reactors.UpdateReactorDataInGame(_torqueReactorData);
            _reactors.UpdateTrailDataInGame(_torqueTrailData);
        }

        public void RemoveTorqueFeedback()
        {
            if (!_isInitialized || !_roundStarted) return;
            if (_reactors == null) _reactors = GetComponentInChildren<PlayerReactors>();
            if (_reactors == null) return;
            _reactors.UpdateReactorDataInGame(_normalReactorData);
            _reactors.UpdateTrailDataInGame(_normalTrailData);
        }

        public void ApplyVoidFeedback()
        {
            if (!_isInitialized || !_roundStarted) return;
            if (_reactors == null) _reactors = GetComponentInChildren<PlayerReactors>();
            _reactors.UpdateReactorDataInGame(_voidReactorData);
            _reactors.UpdateTrailDataInGame(_voidTrailData);

            UpdateBoostBarColor(_voidBoostBarColor, _voidBoostBarFillColor);
        }

        public void RemoveVoidFeedback()
        {
            if (!_isInitialized || !_roundStarted) return;
            if (_reactors == null) _reactors = GetComponentInChildren<PlayerReactors>();
            _reactors.UpdateReactorDataInGame(_normalReactorData);
            _reactors.UpdateTrailDataInGame(_normalTrailData);

            UpdateBoostBarColor(_normalBoostBarColor, _normalBoostBarFillColor);
        }

        public void UpdatePlayerColor(Color color)
        {
            _playerColor = color;
            _specificGamepadFeedbacks.UpdateLightBarColor(color);
        }

        private void ResetAllVFX(object obj)
        {
            if (!CheckIndexAndValidateParameters(obj, out FeedbackParameters param)) return;

            _crownVisualEffect.SetActive(false);
            _crownGraphics.SetActive(false);

            foreach (var item in _takeFlagTimelines)
            {
                item.gameObject.SetActive(false);
            }

            foreach (var item in _resetFlagTimelines)
            {
                item.gameObject.SetActive(false);
            }

            HideAllFlags();

            _playerNumberUI.SetActive(false);

            if (_playerCrownAnimator.isActiveAndEnabled) _playerCrownAnimator.Play("Empty");
            if (_playerCrownAnimator.isActiveAndEnabled) _crownAnimator.Play("Empty");
            if (_playerCrownAnimator.isActiveAndEnabled) _crownBumperAnimator.Play("Empty");
        }

        private void EnableDisableFeedbacks(object obj)
        {
            bool active = (bool)obj;

            _gaugeBoost.SetActive(active);
            RemoveTorqueFeedback();
        }

        private void VFX_OnGaugeChanged(object obj)
        {
            if (!CheckIndexAndValidateParameters(obj, out FeedbackParameters param)) return;

            int numberOfBoost = (int)param.m_params[0];
            float matValue = (float)param.m_params[1];
            UpdateBoostGauge(numberOfBoost, matValue);
        }

        private void VFX_OnBoostStarted(object obj)
        {
            if (!CheckIndexAndValidateParameters(obj, out FeedbackParameters param)) return;

            if (!_isInitialized) return;
            if (_reactors == null) _reactors = GetComponentInChildren<PlayerReactors>();
            _reactors.UpdateReactorDataInGame(_boostReactorData);
            _reactors.UpdateTrailDataInGame(_boostTrailData);

            PlaySfxFeedback(_onBoostSfxData, false);
        }

        private void VFX_OnBoostFinished(object obj)
        {
            if (!CheckIndexAndValidateParameters(obj, out FeedbackParameters param)) return;

            if (!_isInitialized) return;
            if (_reactors == null) _reactors = GetComponentInChildren<PlayerReactors>();
            _reactors.UpdateReactorDataInGame(_normalReactorData);
            _reactors.UpdateTrailDataInGame(_normalTrailData);
        }

        private void VFX_BoostPackIntake(object obj)
        {
            if (!CheckIndexAndValidateParameters(obj, out FeedbackParameters param)) return;

            bool isFullBoost = (bool)param.m_params[0];
            int numberOfBoost = (int)param.m_params[1];

            int boostParam = isFullBoost ? 2 : 1;
            _boostIntake.SetInt("_BoostLevel", boostParam);
            _boostIntake.SendEvent("OnTakeBoost");
        }

        private void VFX_Die(object other, object obj2)
        {
            if (!CheckIndexAndValidateParameters(obj2, out FeedbackParameters param)) return;
            object[] args = (object[])other;
            DeathType type = (DeathType)args[2];

            //ScreenShakeParameters ssParameters = new ScreenShakeParameters(_deathSsDatas.m_deathDatas[DeathType.None]);

            if (type == DeathType.None)
            {
                HideOnDeath();

            }
            else if (type == DeathType.Electric)
            {
                //ssParameters = new ScreenShakeParameters(_deathSsDatas.m_deathDatas[DeathType.Electric]);
                _electricFeedback.SetActive(true);
                Invoke(nameof(HideElectricDeath), _electricDuration);
                HideOnDeath();

                PlaySfxFeedback(_onDieFireSfxData, false);
            }
            else if (type == DeathType.Exploded)
            {
                //ssParameters = new ScreenShakeParameters(_deathSsDatas.m_deathDatas[DeathType.Exploded]);
                _explodedFeedback.SetActive(true);
                Invoke(nameof(HideExplodedDeath), _explodedDuration);
                HideOnDeath();

                PlaySfxFeedback(_onDieFireSfxData, false);
            }
            else if (type == DeathType.Crushed)
            {
                //ssParameters = new ScreenShakeParameters(_deathSsDatas.m_deathDatas[DeathType.Crushed]);
                _crushedFeedback.Initialization();
                _crushedFeedback.PlayFeedbacks();
                _playerCrownAnimator.Play(_playerCrushedAnim);
                Invoke(nameof(HideOnDeath), _playerCrownAnimator.GetCurrentAnimatorStateInfo(0).length);

                PlaySfxFeedback(_onDieHammerSfxData, false);
            }
            else if (type == DeathType.SuckedOut)
            {
                //ssParameters = new ScreenShakeParameters(_deathSsDatas.m_deathDatas[DeathType.SuckedOut]);
                _suckedOutFeedback.Initialization();
                _suckedOutFeedback.PlayFeedbacks();
                _playerCrownAnimator.Play(_playerDeathBlackholeAnim);
                Invoke(nameof(HideOnDeath), _suckedOutFeedback.GetFeedbackOfType<MMF_RotatePositionAround>().AnimateRotationDuration);

                PlaySfxFeedback(_onDieBlackHoleSfxData, false);
            }
            else if (type == DeathType.Ejected)
            {
                //ssParameters = new ScreenShakeParameters(_deathSsDatas.m_deathDatas[DeathType.Ejected]);
                StartCoroutine(FloatInSpace());
            }

            //_onScreenShake.Raise(ssParameters);
            RemoveTorqueFeedback();
            PlayRumble(_onDieData);

            if (_reactors == null) _reactors = GetComponentInChildren<PlayerReactors>();
            if (_reactors == null) return;
            _reactors.DisableGameFeedbacks();
        }

        private void VFX_RespawnImmunityEnd(object obj)
        {
            if (!CheckIndexAndValidateParameters(obj, out FeedbackParameters param)) return;

            _playerCrownAnimator.Play(_playerLostCrownAnim);
        }

        private void VFX_Respawn(object obj)
        {
            if (!CheckIndexAndValidateParameters(obj, out FeedbackParameters param)) return;

            UpdateBoostBarColor(_normalBoostBarColor, _normalBoostBarFillColor);

            bool showMeshes = (bool)param.m_params[0];
            bool isAnAI = (bool)param.m_params[1];
            bool customInvok = (bool)param.m_params[2];

            transform.localScale = Vector3.one;
            foreach (var item in _hideOnDeath)
            {
                item.SetActive(showMeshes);
                item.transform.localScale = Vector3.one;
                item.transform.localEulerAngles = Vector3.zero;
                item.transform.localPosition = Vector3.zero;
            }

            foreach (var item in _hideOnDeathWithoutRescale)
            {
                item.SetActive(showMeshes);
            }

            if (customInvok)
            {
                _playerCrownAnimator.Play("Empty");
                _playerEffect.innerGlow = 0;
                _playerNumberUI.SetActive(false);
                _playerCrownAnimator.transform.localScale = Vector3.one;
            }
            else
            {
                _playerCrownAnimator.Play(_playerRespawnImmunityAnim);
                _playerEffect.innerGlowColor = _playerColor;
                _playerNumberUI.SetActive(true);

                if (_reactors == null) _reactors = GetComponentInChildren<PlayerReactors>();
                if (_reactors == null) return;
                _reactors.EnableGameFeedbacks();
            }
        }

        private void VFX_ScorePoint(object obj)
        {
            if (!CheckIndexAndValidateParameters(obj, out FeedbackParameters param)) return;

            int currentGameModeId = (int)param.m_params[0];
            int valueForVfx = 0;

            if (!_animator) _animator = _pGraphic.GetInGameAnimator();
            if (_animator)
            {
                _animator.Play(_pGraphic.GetInGameScoreAnimStateName());
                _animator.speed = 1;
            }

            ScreenShakeData screenShakeData = null;

            switch (currentGameModeId)
            {
                case 0:
                    //DM
                    valueForVfx = 1;
                    screenShakeData = _scorePoint_DeathMatch;
                    break;

                case 1:
                    //Air Hockey
                    valueForVfx = 3;
                    screenShakeData = _scorePoint_Hockey;
                    break;

                case 2:
                    //Race
                    valueForVfx = 2;
                    break;

                case 3:
                    //CTF
                    valueForVfx = 0;
                    screenShakeData = _scorePoint_CTF;
                    break;

                case 4:
                    //Crown
                    return;
            }

            _scorePointVFX.SetInt("_GameMode", valueForVfx);
            _scorePointVFX.SendEvent("OnScorePoint");

            PlaySfxFeedback(_onGetPointSfxData, false);

            PlayRumble(_onScorePointData);
            PlayScreenShake(screenShakeData);
        }

        private void VFX_FlagIntake(object obj)
        {
            if (!CheckIndexAndValidateParameters(obj, out FeedbackParameters param)) return;

            _hasFlag = true;

            int teamID = (int)param.m_params[0];

            _takeFlagTimelines[teamID].gameObject.SetActive(true);
            _takeFlagTimelines[teamID].Play();

            PlayRumble(_onTakeFlagData);
        }

        private void VFX_FlagReset(object obj)
        {
            if (!CheckIndexAndValidateParameters(obj, out FeedbackParameters param)) return;

            if (!_hasFlag) return;
            _hasFlag = false;

            int index = _takeFlagTimelines[0].gameObject.activeInHierarchy ? 0 : 1;

            StartCoroutine(nameof(HideFlagOnTimelineFinished), index);
        }

        private void VFX_TakeCrown(object obj)
        {
            if (!CheckIndexAndValidateParameters(obj, out FeedbackParameters param)) return;

            float immunityDuration = (float)param.m_params[0];

            _crownGraphics.SetActive(true);
            _crownVisualEffect.SetActive(true);

            _crownAnimator.Play(_takeCrownAnim);
            _crownBumperAnimator.Play(_bumperImmunityOnAnim);
            _playerCrownAnimator.Play(_playerTakeCrownAnim);

            _bumperEffect.innerGlowColor = _playerColor;
            _playerEffect.innerGlowColor = _playerColor;
            _crownEffect.innerGlowColor = _playerColor;

            for (int i = 0; i < _crownEffect.glowPasses.Length; i++)
            {
                _crownEffect.glowPasses[i].color = _playerColor;
            }

            _crownLerp.SetInitialColor(_playerColor);
            _crownEffect.outlineColor = _playerColor;
        }

        private void VFX_CrownImmunityFinished(object obj)
        {
            if (!CheckIndexAndValidateParameters(obj, out FeedbackParameters param)) return;

            _bumperRenderer.enabled = false;
            _crownAnimator.Play(_crownImmunityFinishedAnim);
            _crownBumperAnimator.Play(_bumperImmunityOffAnim);
            _playerCrownAnimator.Play(_playerLostCrownAnim);
        }

        private void VFX_LostCrown(object obj)
        {
            if (!CheckIndexAndValidateParameters(obj, out FeedbackParameters param)) return;

            _crownAnimator.Play(_lostCrownAnim);
            _crownVisualEffect.SetActive(false);
            _crownGraphics.SetActive(false);

            PlayScreenShake(_stealCrownScreenShake);
        }

        #endregion


        #region Utils & Tools

        private void UpdateBoostBarColor(Color boostBarColor, Color boostBarFillColor)
        {
            _boostBarRenderer.material.SetColor("_BoostColor", boostBarColor);

            foreach (var item in _boostBarFillEffects)
            {
                item.SetVector4("Color", boostBarFillColor);
            }
        }

        public void UpdateUseRumble(bool value)
        {
            _useRumble = value;
        }

        private void HideAllFlags()
        {
            _hasFlag = false;

            foreach (var item in _takeFlagTimelines)
            {
                item.gameObject.SetActive(false);
            }

            foreach (var item in _resetFlagTimelines)
            {
                item.gameObject.SetActive(false);
            }
        }

        private IEnumerator HideFlagOnTimelineFinished(int index)
        {
            if (_takeFlagTimelines[index].state == PlayState.Playing) _takeFlagTimelines[index].Stop();

            _takeFlagTimelines[index].gameObject.SetActive(false);
            _resetFlagTimelines[index].gameObject.SetActive(true);

            _resetFlagTimelines[index].Play();
            yield return new WaitForSeconds((float)_resetFlagTimelines[index].duration);
            _resetFlagTimelines[index].gameObject.SetActive(false);
        }

        private void UpdateBoostGauge(int numberOfAvailableBoost, float matValue)
        {
            for (int i = 0; i < _boostGauge.Length; i++)
            {
                _boostGauge[i].enabled = false;
            }

            for (int i = 0; i < numberOfAvailableBoost; i++)
            {
                _boostGauge[i].enabled = true;
            }

            _boostRenderer.material.SetFloat("_UVCoordinate", matValue);
        }

        private void HideOnDeath()
        {
            foreach (var item in _hideOnDeath)
            {
                item.SetActive(false);
            }

            foreach (var item in _hideOnDeathWithoutRescale)
            {
                item.SetActive(false);
            }

        }

        private void HideElectricDeath() => _electricFeedback.SetActive(false);

        private void HideExplodedDeath() => _explodedFeedback.SetActive(false);

        private IEnumerator FloatInSpace()
        {
            float elapsedTime = 0;
            Vector3 randomAxis = UnityEngine.Random.insideUnitSphere.normalized;
            while (elapsedTime < _ejectedDuration)
            {
                _hideOnDeath[0].transform.RotateAround(transform.position + Vector3.up, randomAxis, _ejectedRotationSpeed * Time.deltaTime);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            _spaceSuckedOutFeedback.PlayFeedbacks();
            _playerCrownAnimator.Play(_playerDeathSpaceAnim);
            Invoke(nameof(HideOnDeath), _playerCrownAnimator.GetCurrentAnimatorStateInfo(0).length);

            StopCoroutine(FloatInSpace());
        }

        private bool CheckPlayerIndex(int index) => index == p_index;

        private bool CheckIndexAndValidateParameters(object obj, out FeedbackParameters param)
        {
            param = obj as FeedbackParameters;
            if (param == null) return false;
            if (!_isInitialized) return false;

            return param.m_id == p_index;
        }

        private void PlaySfxFeedback(SFXData data, bool isMenuSfx)
        {
            AudioManager.s_instance.PlaySfx(data._sfx.GetRandom(), isMenuSfx);
        }

        #endregion


        #region Private

        private int _takeCrownAnim;
        private int _crownImmunityFinishedAnim;
        private int _lostCrownAnim;
        private int _playerRespawnImmunityAnim;
        private int _playerDeathBlackholeAnim;
        private int _playerDeathSpaceAnim;
        private int _playerCrushedAnim;
        private int _bumperImmunityOnAnim;
        private int _bumperImmunityOffAnim;
        private int _playerTakeCrownAnim;
        private int _playerLostCrownAnim;

        private bool _isInitialized;
        private bool _hasFlag;
        private bool _useRumble;
        private bool _roundStarted;

        private Color _playerColor;

        private Animator _animator;

        private PlayerReactors _reactors;

        private IPlayerGraphic _pGraphic;

        #endregion
    }
}