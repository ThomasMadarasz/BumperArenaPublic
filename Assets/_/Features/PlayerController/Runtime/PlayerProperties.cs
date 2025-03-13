using UnityEngine;
using Archi.Runtime;
using Inputs.Runtime;
using PlayerController.Data.Runtime;
using Interfaces.Runtime;
using ScriptableEvent.Runtime;
using SceneManager.runtime;
using Feedbacks.Runtime;
using Utils.Translation;

namespace PlayerController.Runtime
{
    [RequireComponent(typeof(InputsReader))]
    public class PlayerProperties : CNetBehaviour, ITeamable, IPlayer
    {
        #region Exposed

        [SerializeField] private PlayerControllerData _controllerData;
        [SerializeField] private PlayerBoostData _boostData;
        [SerializeField] private PlayerCollisionsData _collData;
        [SerializeField] private PlayerDeathData _deathData;

        [SerializeField] private GameEvent _onRoundStart;
        [SerializeField] private GameEvent _onRoundFinished;
        [SerializeField] private GameEvent _onGameReady;
        [SerializeField] private GameEventT _onScorePoint;

        [SerializeField] private GameEventT _onPlayerOrientationChanged;
        [SerializeField] private GameEventT _onRumbleValueChanged;

        [SerializeField] private GameEvent _onPauseMenuDisplayed;
        [SerializeField] private GameEvent _onPauseMenuHidden;

        [SerializeField] private string _UIActionMap;
        [SerializeField] private string _GameActionMap;

        [SerializeField] private PlayerNumberUI _playerNumberUI;

        #region Accessor

        public PlayerControllerData m_controllerData
        {
            get { return _controllerData; }
        }

        public PlayerBoostData m_boostData
        {
            get { return _boostData; }
        }

        public PlayerCollisionsData m_collData
        {
            get { return _collData; }
        }

        public PlayerDeathData m_deathData
        {
            get { return _deathData; }
        }

        public InputsReader m_inputs
        {
            get { return _inputs; }
        }

        public PlayerProperties m_properties
        {
            get { return this; }
        }

        public ITeamable m_teamable
        {
            get { return this; }
        }

        #endregion

        [HideInInspector]
        public Vector2 m_aiInputs;


        internal Vector3 m_attractionForce;

        internal float i_speedMultiplier = 1;
        internal float i_accelerationMultiplier = 1;
        internal float i_bumpDuration;

        internal bool i_isGameStarted = false;
        internal bool i_isGameOver = true;
        internal bool i_isBumped;
        internal bool i_isBoosted;
        internal bool i_isOrientationWorld;
        internal bool i_isCollisionsImmune;
        internal bool i_isTorqueApplied;
        internal bool i_isInvincible;

        internal Quaternion i_respawnRotation;

        internal int i_localPlayerID;

        public bool i_isAlive = true;
        public bool m_isAnAI;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        private void Start()
        {
            _feedback = GetComponent<IFeedback>();
            _playerFeedback = GetComponent<PlayerFeedbacks>();
            _playerFeedback.UpdateUseRumble(_useRumbleOnStart);
        }

        private void Update()
        {
            if (i_isGameOver)
            {
                rigidBody.velocity = Vector3.zero;
                rigidBody.angularVelocity = Vector3.zero;

                i_isBumped = false;
                i_isTorqueApplied = false;
            }
        }

        private void OnDestroy()
        {
            _inputs.m_onMenuPerformed -= OnMenuPerformed;

            _onRoundStart.UnregisterListener(OnRoundStart);
            _onRoundFinished.UnregisterListener(OnRoundFinished);

            _onGameReady.UnregisterListener(OnGameReady);

            _onPlayerOrientationChanged.UnregisterListener(OnOrientationValueChanged);
            _onRumbleValueChanged.UnregisterListener(OnUseRumbleValueChanged);

            _onPauseMenuDisplayed.UnregisterListener(OnPauseMenuDisplayed);
            _onPauseMenuHidden.UnregisterListener(OnPauseMenuHidden);
        }

        #endregion


        #region Main

        private void Setup()
        {
            _inputs = GetComponent<InputsReader>();
            _inputs.m_onMenuPerformed += OnMenuPerformed;

            _onRoundStart.RegisterListener(OnRoundStart);
            _onRoundFinished.RegisterListener(OnRoundFinished);

            _onGameReady.RegisterListener(OnGameReady);

            _onPlayerOrientationChanged.RegisterListener(OnOrientationValueChanged);
            _onRumbleValueChanged.RegisterListener(OnUseRumbleValueChanged);

            _onPauseMenuDisplayed.RegisterListener(OnPauseMenuDisplayed);
            _onPauseMenuHidden.RegisterListener(OnPauseMenuHidden);
        }

        public void SetTeamID(int id) => _teamId = id;

        public int GetTeamID() => _teamId;

        public void SetPlayerID(int id) => _playerId = id;

        public int GetPlayerID() => _playerId;

        public bool IsAlive() => i_isAlive;

        public Transform GetTransform() => transform;

        public void SetImmuneToCollisions(bool immune) => i_isCollisionsImmune = immune;

        public void SetSpeedMultiplier(float multiplier) => i_speedMultiplier = multiplier;

        public void SetAccelerationMultiplier(float multiplier) => i_accelerationMultiplier = multiplier;

        public void ResetSpeedMultipliers()
        {
            i_speedMultiplier = 1;
            i_accelerationMultiplier = 1;
        }

        public bool IsAnAI() => m_isAnAI;

        private void OnMenuPerformed()
        {
            if (_inputs.IsCurrentActionMapIsUI()) return;
            SceneLoader.s_instance.LoadPauseMenu();
        }

        #endregion


        #region Utils & Tools

        private void OnOrientationValueChanged(object obj)
        {
            object[] values = (object[])obj;
            int playerID = (int)values[0];
            bool useWorldOrientation = (bool)values[1];

            if (playerID != i_localPlayerID) return;
            i_isOrientationWorld = useWorldOrientation;
        }

        private void OnUseRumbleValueChanged(object obj)
        {
            object[] values = (object[])obj;
            int playerID = (int)values[0];
            bool useRumble = (bool)values[1];

            if (playerID != i_localPlayerID) return;
            _playerFeedback.UpdateUseRumble(useRumble);
        }

        private void OnPauseMenuDisplayed()
        {
            if (!m_isAnAI)
            {
                _inputs.EnableActionMap(_UIActionMap);
                _inputs.DisableActionMap(_GameActionMap);
            }
        }

        private void OnPauseMenuHidden()
        {
            if (!m_isAnAI)
            {
                _inputs.EnableActionMap(_GameActionMap);
                _inputs.DisableActionMap(_UIActionMap);
            }
        }

        public void OnScorePoint(int gamemModeId)
        {
            FeedbackParameters param = new FeedbackParameters() { m_id = _feedback.GetID(), m_params = new object[1] { gamemModeId } };
            _onScorePoint?.Raise(param);
        }

        private void OnGameReady()
        {
            if (!m_isAnAI)
            {
                _inputs.EnableActionMap(_GameActionMap);
                _inputs.DisableActionMap(_UIActionMap);
            }
        }

        private void OnRoundStart()
        {
            i_isGameOver = false;
            i_isGameStarted = true;
            ResetSpeedMultipliers();
        }

        private void OnRoundFinished()
        {
            i_isGameOver = true;

            if (!m_isAnAI)
            {
                _inputs.EnableActionMap(_UIActionMap);
                _inputs.DisableActionMap(_GameActionMap);
            }
        }

        public int GetUniqueID() => _uniquePlayerID;

        public void SetUniqueID(int id) => _uniquePlayerID = id;

        public void SetLocalPlayerID(int id,bool isAI)
        {
            if (isAI)
            {
                string playerLetter = TranslationManager.Translate("PlayerNumberKey_AI");
                _playerNumberUI.SetText($"{playerLetter}{id+1}");
            }
            else
            {
                i_localPlayerID = id;

                string playerLetter = TranslationManager.Translate("PlayerNumberKey");
                _playerNumberUI.SetText($"{playerLetter}{id}");
            }
        }

        public void SetWorldOrientation(bool useWorld) => i_isOrientationWorld = useWorld;

        public void SetUseRumble(bool useRumble)
        {
            _useRumbleOnStart = useRumble;
        }

        #endregion


        #region Private

        private InputsReader _inputs;

        private int _teamId;
        private int _playerId;
        private int _uniquePlayerID;

        private bool _useRumbleOnStart;

        private IFeedback _feedback;
        private PlayerFeedbacks _playerFeedback;

        #endregion
    }
}