using Archi.Runtime;
using Audio.Runtime;
using Core.Data.Runtime;
using Data.Runtime;
using Interfaces.Runtime;
using ScriptableEvent.Runtime;
using Settings.Runtime;
using UnityEngine;
using UnityEngine.Playables;
using Utils.Runtime;

namespace Core.Runtime
{
    [RequireComponent(typeof(Collider))]
    public class SpawnPoint : CNetBehaviour
    {
        #region Exposed

        [SerializeField] private RespawnData _spawnData;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private Transform _arrow;
        [SerializeField] private PlayableDirector _playableDirector;
        [SerializeField] private Renderer _arrowRendererInside;
        [SerializeField] private Renderer _arrowRendererOutside;
        [SerializeField] private Renderer _doorRenderer;
        [SerializeField] private Collider[] _wallColliderToIgnore;
        [SerializeField] private GameObject _arrowMesh;
        [SerializeField] private float _timeToPlayTimelineBeforePlayerSpawn;
        [SerializeField] private float _timeToHideDoorColor;

        [SerializeField] private SFXData _spawnSfx;
        [SerializeField] private float _timeBeforeSpawnForPlaySfx;

        [SerializeField] private ParticleSystemRenderer _accelerationSpawnParticleRenderer;
        [SerializeField] private ParticleSystem _accelerationSpawnParticle;
        [SerializeField] private Material _respawnMat;
        [SerializeField] private int _keyboardIndex;
        [SerializeField] private int _playstationIndex;
        [SerializeField] private int _xboxIndex;

        [SerializeField] private GameEventT _onDeviceControlTypeUIChanged;

        [HideInInspector] public bool m_isAvailable = true;
        public int m_teamID;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        private void Update()
        {
            if (!_onRespawn) return;
            if (!_canRespawn) return;
            UpdateRespawn();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (other.gameObject.GetComponent<IKillable>().GetTeamable().GetUniqueID() != _uniquePlayerID) return;

            foreach (var col in _wallColliderToIgnore)
            {
                _player.UpdateColliderCollisionFor(col, false);
            }

            _player.DisableInvincibility();

            _onRespawn = false;
            _player = null;
            _uniquePlayerID = -1;
            m_isAvailable = true;
        }

        private void OnDestroy()
        {
            _onDeviceControlTypeUIChanged.UnregisterListener(OnDeviceControlTypeUIChanged);

            _initialSpawnTimer.OnValueChanged -= OnInitialTimerValueChanged;
            StopSpawnPlayer();
        }

        #endregion


        #region Main

        private void Setup()
        {
            _onDeviceControlTypeUIChanged.RegisterListener(OnDeviceControlTypeUIChanged);

            UpdateParticleMaterial((UIControlDeviceType)SettingsManager.s_instance.GetCurrentDeviceTypeUI());

            _accelerationSpawnParticleRenderer.gameObject.SetActive(false);
            _accelerationSpawnParticleRenderer.material = _respawnMat;

            _initialRotation = _arrow.transform.localRotation;

            _initialSpawnTimer = new(_spawnData.m_initialSpawnTime, RespawnPlayer);
            _initialSpawnTimer.OnValueChanged += OnInitialTimerValueChanged;

            _hideDoorColorTimer = new(_timeToHideDoorColor, OnHideDoorColorTimerOver);
            _hideDoorColorTimer.OnValueChanged += OnHideDoorColorTimerChanged;
        }

        public void StartSpawnPlayer()
        {
            if (_player == null) return;
            _canRespawn = true;
            _remainingTimeLastFrame = _spawnData.m_initialSpawnTime;
            _initialSpawnTimer.Start();

            float timelineTime = _spawnData.m_initialSpawnTime - _timeToPlayTimelineBeforePlayerSpawn;

            Invoke(nameof(PlayTimeline), timelineTime);
        }

        public void StopSpawnPlayer()
        {
            _initialSpawnTimer.Stop();
            _hideDoorColorTimer.Stop();
            _canRespawn = false;

            CancelInvoke();
        }

        public void SpawnPlayer(IKillable player)
        {
            m_isAvailable = false;
            _isRespawn = false;
            _isDuoMode = GameManager.s_instance.GetLastTeamModeGame() == Enum.Runtime.TeamModeEnum.Duo;

            ResetValues(player);
        }

        public void RespawnPlayer(IKillable player)
        {
            m_isAvailable = false;
            _isRespawn = true;
            ResetValues(player);

            _remainingTime = _spawnData.m_respawnTime;
            _onRespawn = true;

            float timelineTime = _spawnData.m_respawnTime - _timeToPlayTimelineBeforePlayerSpawn;

            Invoke(nameof(PlayTimeline), timelineTime);
        }

        private void UpdateRespawn()
        {
            _remainingTime -= Time.deltaTime;

            if (_player.PlayerUseRespawnAccelerationBonus())
            {
                _remainingTime -= Time.deltaTime * (_spawnData.m_timeReductionBonus - 1);
            }

            float ratio = CalculateRatio(_remainingTime, _spawnData.m_respawnTime);
            UpdateUV(ratio);
            UpdateArrowRotation(Time.deltaTime);

            if (_remainingTime <= _timeBeforeSpawnForPlaySfx && !_sfxSoundPlayed)
            {
                _sfxSoundPlayed = true;
                AudioManager.s_instance.PlaySfx(_spawnSfx._sfx.GetRandom(),false);
            }

            if (_remainingTime <= 0)
            {
                RespawnPlayer();
            }
        }

        #endregion


        #region Utils & Tools

        private void UpdateParticleMaterial(UIControlDeviceType deviceType)
        {
            var textureSheet = _accelerationSpawnParticle.textureSheetAnimation;

            switch (deviceType)
            {
                case UIControlDeviceType.Keyboard:
                    textureSheet.rowIndex = _keyboardIndex;
                    break;

                case UIControlDeviceType.Playstation:
                    textureSheet.rowIndex = _playstationIndex;
                    break;

                case UIControlDeviceType.Xbox:
                    textureSheet.rowIndex = _xboxIndex;
                    break;
            }
        }

        private void OnDeviceControlTypeUIChanged(object obj)
        {
            int value = (int)obj;
            UpdateParticleMaterial((UIControlDeviceType)value);
        }

        private void ResetValues(IKillable player)
        {
            _player = player;

            if (!_player.GetPlayerPorperties().IsAnAI())
            {
                _accelerationSpawnParticleRenderer.gameObject.SetActive(true);
                _accelerationSpawnParticle.Play();
            }

            _uniquePlayerID = player.GetTeamable().GetUniqueID();

            foreach (var col in _wallColliderToIgnore)
            {
                _player.UpdateColliderCollisionFor(col, true);
            }

            int uniquePlayerId = player.GetTeamable().GetUniqueID();
            Material teamMat = GameManager.s_instance.GetForcedPlayerEmissiveMaterialWithUniqueID(uniquePlayerId);
            Material playerMat = GameManager.s_instance.GetPlayerEmissiveMaterialWithUniqueID(uniquePlayerId);

            UpdateMaterialColor(teamMat, playerMat, _isDuoMode);

            _arrowMesh.SetActive(true);
            _arrow.transform.localRotation = _initialRotation;

            OnHideDoorColorTimerOver();
            ShowDoorColor();
        }

        private void UpdateMaterialColor(Material playerMat, Material teamMat, bool isDuoMode)
        {
            _playerMat = playerMat;
            OnPlayerMaterialChanged(playerMat, teamMat, isDuoMode);
        }

        private void UpdateUV(float value)
        {
            _arrowRendererInside.material.SetFloat("_UVCoordinate", value);

            int index = GetIndexForDoorRenderer();
            if (index == -1) return;

            _doorRenderer.materials[index].SetFloat("_UVCoordinate", value);
        }

        private void RespawnPlayer()
        {
            _player.MoveTo(_spawnPoint);

            _onRespawn = false;
            _arrowMesh.SetActive(false);

            if (_isRespawn) _player.Respawn(_spawnPoint, _arrow.transform.rotation);
            else _player.Spawn(_spawnPoint, _arrow.transform.rotation);

            _hideDoorColorTimer.Start();

            _sfxSoundPlayed = false;

            if (!_player.GetPlayerPorperties().IsAnAI())
            {
                _accelerationSpawnParticleRenderer.gameObject.SetActive(false);
                _accelerationSpawnParticle.Stop();
            }
        }

        private void PlayTimeline()
        {
            _playableDirector.Play();
        }

        private void OnInitialTimerValueChanged(float value)
        {
            float timeInterval = _remainingTimeLastFrame - value;
            float ratio = CalculateRatio(value, _spawnData.m_initialSpawnTime);
            UpdateUV(ratio);
            UpdateArrowRotation(timeInterval);

            _remainingTimeLastFrame = value;

            if (value <= _timeBeforeSpawnForPlaySfx && !_sfxSoundPlayed)
            {
                _sfxSoundPlayed = true;
                AudioManager.s_instance.PlaySfx(_spawnSfx._sfx.GetRandom(), false);
            }
        }

        private void OnHideDoorColorTimerChanged(float value)
        {
            int index = GetIndexForDoorRenderer();
            if (index == -1) return;

            float ratio = value / _timeToHideDoorColor;
            _doorRenderer.materials[index].SetFloat("_Disable", ratio);
        }

        private void OnHideDoorColorTimerOver()
        {
            int index = GetIndexForDoorRenderer();
            if (index == -1) return;

            _doorRenderer.materials[index].SetFloat("_Disable", 0);
        }

        private void ShowDoorColor()
        {
            int index = GetIndexForDoorRenderer();
            if (index == -1) return;

            _doorRenderer.materials[index].SetFloat("_Disable", 1);
        }

        private int GetIndexForDoorRenderer()
        {
            if (_doorRenderer != null)
            {
                int matIndex = -1;

                for (int i = 0; i < _doorRenderer.materials.Length; i++)
                {
                    if (_doorRenderer.materials[i].name.Contains("mat_spawner_arrow"))
                    {
                        matIndex = i;
                        break;
                    }
                }

                return matIndex;
            }

            return -1;
        }

        private float CalculateRatio(float time, float maxTime)
        {
            float ratio = 1 - Mathf.Clamp01(time / maxTime);
            ratio /= 2;

            return ratio;
        }

        private void UpdateArrowRotation(float timeInterval)
        {
            float maxAngle = _spawnData.m_maxRotationAngle / 2;
            float inputAngle = _player.GetRespawnDirection(transform);

            if (inputAngle != 0)
            {
                float rotZ = Mathf.Clamp(inputAngle, -maxAngle, maxAngle);
                Vector3 arrowRot = new(0, -rotZ, 0);

                arrowRot.y -= _arrow.transform.localRotation.eulerAngles.y;

                Quaternion rotation = Quaternion.Lerp(_arrow.transform.localRotation, Quaternion.Euler(arrowRot), timeInterval * _spawnData.m_rotationSpeedMultiplier);
                _arrow.transform.localRotation = rotation;
            }
        }

        private void OnPlayerMaterialChanged(Material insideMat, Material outsideMat, bool isDuoMode)
        {
            if (isDuoMode)
            {
                _arrowRendererInside.material.SetColor("_EmissionColor", outsideMat.GetColor("_EmissionColor"));
                _arrowRendererOutside.material.SetColor("_BaseColor", insideMat.GetColor("_BaseColor"));
            }
            else
            {
                _arrowRendererInside.material.SetColor("_EmissionColor", insideMat.GetColor("_EmissionColor"));
            }

            int index = GetIndexForDoorRenderer();
            if (index == -1) return;

            _doorRenderer.materials[index].SetColor("_EmissionColor", insideMat.GetColor("_EmissionColor"));
        }

        #endregion


        #region Private

        private IKillable _player;

        private float _remainingTime;
        private float _remainingTimeLastFrame;

        private bool _isRespawn;
        private bool _onRespawn;
        private bool _canRespawn;
        private bool _sfxSoundPlayed;
        private bool _isDuoMode;

        private int _uniquePlayerID = -1;

        private Material _playerMat;

        private Timer _initialSpawnTimer;
        private Timer _hideDoorColorTimer;

        private Quaternion _initialRotation;

        #endregion
    }
}