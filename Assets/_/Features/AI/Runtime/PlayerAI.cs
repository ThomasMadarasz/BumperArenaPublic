using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interfaces.Runtime;
using System.Linq;
using ScriptableEvent.Runtime;
using Mirror;
using Archi.Runtime;
using GameModes.Runtime;
using PlayerController.Runtime;
using AI.Data.Runtime;
using Traps.Runtime;
using NodeCanvas.Framework;

namespace AI.Runtime
{
    [RequireComponent(typeof(PlayerProperties))]
    public class PlayerAI : CNetBehaviour, IAI
    {
        #region Exposed

        [SerializeField] private PlayerAIData _data;

        [SerializeField] private GameEvent _onGameSceneReady;
        [SerializeField] private GameEvent _onRoundStarted;
        [SerializeField] private GameEvent _onRoundFinished;

        [Tooltip("0 : Deathmatch\n1 : AirHockey\n2 : Race\n3 : CaptureTheFlag\n4 : Crown")]
        [SerializeField] private GameModeAI[] _aiGameModes = new GameModeAI[5];

        [HideInInspector] public Vector2 m_inputs;

        [HideInInspector] internal ITeamable i_teamable;
        [HideInInspector] internal List<ITeamable> i_teamables = new List<ITeamable>();
        [HideInInspector] internal List<ITeamable> i_enemyTeamables = new List<ITeamable>();
        [HideInInspector] internal List<ITeamable> i_allyTeamables = new List<ITeamable>();

        [HideInInspector] internal GameMode i_currentGameModeManager;

        #region Accessor

        public int m_gamemodeId
        {
            get { return i_currentGameModeManager.m_gameModeId; }
        }

        public Transform m_closestPlayer
        {
            get { return GetClosestEnemyPlayer(); }
        }

        public List<RaycastLogic> m_hittingRaycasts
        {
            get { return _hittingRaycasts; }
        }

        public bool m_isHittingRaycasts
        {
            get { return IsThereHittingRaycasts(); }
        }

        public List<BoostPack> m_boostPackes
        {
            get { return _boostPackes; }
        }

        public bool m_isRoundStarted
        {
            get
            {
                if (_currentGameModeAI == null) return false;
                return _currentGameModeAI.m_isRoundStarted;
            }
        }

        #endregion


        #endregion


        #region Unity API

        public override void OnStartServer() => Setup();

        [ServerCallback]
        private void FixedUpdate()
        {
            ShootRaycasts();
        }

        private void OnDestroy()
        {
            _onGameSceneReady.UnregisterListener(OnGameSceneReady);
            _onRoundStarted.UnregisterListener(OnRoundStarted);
            _onRoundFinished.UnregisterListener(OnRoundFinished);
        }

        #endregion


        #region Main

        private void Setup()
        {
            _onGameSceneReady.RegisterListener(OnGameSceneReady);
            _onRoundStarted.RegisterListener(OnRoundStarted);
            _onRoundFinished.RegisterListener(OnRoundFinished);
        }

        private Transform GetClosestEnemyPlayer()
        {
            Transform closestPlayer = null;
            float currentDistance = 0;
            foreach (ITeamable team in i_enemyTeamables)
            {
                if (!team.IsAlive()) continue;
                Transform t = team.GetTransform();
                Vector3 potentialTargetPos = t.position + (t.forward * _data.m_anticipationOffset);
                float distance = Vector3.Distance(transform.position, potentialTargetPos);
                float angleWithOffset = Vector3.Angle(transform.forward, (potentialTargetPos - transform.position).normalized);
                float angle = Vector3.Angle(transform.forward, (t.position - transform.position).normalized);

                if (closestPlayer == null || angleWithOffset <= _data.m_maxAngleToConsiderAPlayerAsAPotentialTarget && distance <= currentDistance)
                {
                    currentDistance = distance;
                    closestPlayer = t;
                }
            }
            return closestPlayer;
        }

        private void RegisterTeamables()
        {
            if (i_teamable == null) i_teamable = GetComponent<ITeamable>();
            if (!i_teamable.IsAnAI()) this.enabled = false;
            if (i_teamables.Count == 0)
            {
                PlayerProperties[] _properties = FindObjectsOfType<PlayerProperties>();
                foreach (PlayerProperties p in _properties) i_teamables.Add(p.GetComponent<ITeamable>());
            }
            i_enemyTeamables.Clear();
            i_allyTeamables.Clear();
            i_enemyTeamables = i_teamables.Where(x => x.GetTeamID() != i_teamable.GetTeamID()).ToList();
            i_allyTeamables = i_teamables.Where(x => x.GetTeamID() == i_teamable.GetTeamID() && x != i_teamable).ToList();
        }

        [ServerCallback]
        private void SetupAIGameMode()
        {
            for (int i = 0; i < _aiGameModes.Length; i++)
            {
                if (i == i_currentGameModeManager.m_gameModeId)
                {
                    _aiGameModes[i].enabled = true;
                    _currentGameModeAI = _aiGameModes[i];
                }
                else if (_aiGameModes[i] != null) _aiGameModes[i].enabled = false;
            }
        }

        [ServerCallback]
        private void GetBoostPackes()
        {
            _boostPackes = FindObjectsOfType<BoostPack>().ToList();
        }

        private void ShootRaycasts()
        {
            _hittingRaycasts.Clear();
            float halfRaycastNumber = _data.m_raycastNumber / 2;
            float angleStep = (_data.m_raycastMaxAngle - _data.m_raycastBaseAngleOffset) / (halfRaycastNumber - 1);
            float distanceStep = (_data.m_longestRaycastDistance - _data.m_shortestRaycastDistance) / (halfRaycastNumber - 1);

            for (int i = 0; i < halfRaycastNumber; i++)
            {
                float distance = _data.m_longestRaycastDistance - (i * distanceStep);

                for (int j = 0; j < 2; j++)
                {
                    if (i == 0 && j == 1) continue;
                    float angle = (i * angleStep + _data.m_raycastBaseAngleOffset) * (j * 2 - 1);
                    Vector3 raycastDirection = Quaternion.AngleAxis(angle, Vector3.up) * transform.forward;
                    RaycastHit hit;
                    Debug.DrawRay(transform.position, raycastDirection * distance, Color.green);
                    if (Physics.Raycast(transform.position, raycastDirection, out hit, distance, _data.m_layerMask)) _hittingRaycasts.Add(new RaycastLogic(hit, i, angle, hit.distance, hit.collider.gameObject.layer));
                }
            }
        }


        #endregion


        #region Utils & Tools

        private GameMode DetectGameModeManager() => FindObjectOfType<GameMode>();

        private bool IsThereHittingRaycasts() => _hittingRaycasts.Count > 0;

        public void DisableAI()
        {
            _p = GetComponent<PlayerProperties>();
            _p.m_isAnAI = false;

            this.enabled = false;

            _graph = GetComponent<GraphOwner>();
            _graph.enabled = false;
        }

        public void EnableAI()
        {
            _p = GetComponent<PlayerProperties>();
            _p.m_isAnAI = true;

            this.enabled = true;

            _graph = GetComponent<GraphOwner>();
            _graph.enabled = true;
        }

        #endregion


        #region Events

        [ServerCallback]
        private void OnGameSceneReady()
        {
            if (_p.m_isAnAI)
            {
                _graph.enabled = true;
                _graph.RestartBehaviour();
            }
            RegisterTeamables();
            i_currentGameModeManager = DetectGameModeManager();
#if UNITY_EDITOR
            if (i_currentGameModeManager == null)
            {
                Debug.LogError("Faut ajouter le GameMode Manager dans le ld :')");
                UnityEditor.EditorApplication.isPlaying = false;
            }
#endif
            SetupAIGameMode();
            GetBoostPackes();
        }

        [ServerCallback]
        private void OnRoundStarted() => _currentGameModeAI.m_isRoundStarted = true;

        [ServerCallback]
        private void OnRoundFinished()
        {
            _currentGameModeAI.m_isRoundStarted = false;
            _graph.enabled = false;
        }

        #endregion


        #region Private

        private GameModeAI _currentGameModeAI;

        private List<RaycastLogic> _hittingRaycasts = new List<RaycastLogic>();

        private List<BoostPack> _boostPackes = new List<BoostPack>();

        private GraphOwner _graph;
        private PlayerProperties _p;

        #endregion
    }
}