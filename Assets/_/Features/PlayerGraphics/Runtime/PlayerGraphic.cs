using Archi.Runtime;
using UnityEngine;
using Mirror;
using Interfaces.Runtime;
using Data.Runtime;
using Customisation.Runtime;
using Networking.Runtime;
using System.Collections;
using Core.Runtime;
using PlayerController.Runtime;
using Feedbacks.Runtime;

namespace PlayerGraphics.Runtime
{
    public class PlayerGraphic : CNetBehaviour, IPlayerGraphic, IPlayerCustomisation
    {
        #region Exposed

        [SerializeField] private Transform _meshParent;

        [SerializeField] private CustomisationData _customisationData;

        [SerializeField][SyncVar(hook = nameof(OnSerializedDataChanged))] private SerializedCustomisationData _serializedData;

        [SerializeField] private string _graphicLayerName;

        #endregion


        #region Unity API

        private void Awake()
        {
            PlayerLobbyNetwork playerLobby = GetComponent<PlayerLobbyNetwork>();
            if (playerLobby != null)
            {
                playerLobby.m_onPlayerMaterialIndexChanged += SetMaterialIndex;
                playerLobby.m_onAIMateriaChanged += SetAIMaterial;
            }
        }

        public override void OnStartAuthority()
        {
            base.OnStartAuthority();

            StartCoroutine(nameof(LoadConfigData), true);
            //_serializedData = CustomisationHelper.GetCustomisation();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (isOwned) return;

            StartCoroutine(nameof(LoadConfigData), false);
            //OnSerializedDataChanged(_serializedData, _serializedData);
        }

        private IEnumerator LoadConfigData(bool hasAuthority)
        {
            //if (hasAuthority && _localPlayerID == -1) Cmd_UpdateLocalPlayerID();

            while (_localPlayerID == -1)
            {
                yield return null;
            }

            if (!_useRandomMeshes)
            {
                if (hasAuthority) _serializedData = CustomisationHelper.GetCustomisation(_localPlayerID);
                else OnSerializedDataChanged(_serializedData, _serializedData);
            }
        }

        #endregion


        #region Utils & Tools

        private SerializedCustomisationData GetRandomCustomisation()
        {
            int carIndex = UnityEngine.Random.Range(0, _customisationData.m_cars.Length);
            int charIndex = UnityEngine.Random.Range(0, _customisationData.m_characters.Length);
            int animIndex = UnityEngine.Random.Range(0, _customisationData.m_animations.Length);

            SerializedCustomisationData data = new()
            {
                m_carIndex = carIndex,
                m_characterIndex = charIndex,
                m_animationIndex = animIndex,
            };

            return data;
        }

        private void OnSerializedDataChanged(SerializedCustomisationData oldValue, SerializedCustomisationData newValue)
        {
            GameObject highPolyCarMesh = CustomisationManager.s_instance.GetHighPolyMesh(newValue.m_carIndex, CustomisationType.Car);
            GameObject highPolyCharacterMesh = CustomisationManager.s_instance.GetHighPolyMesh(newValue.m_characterIndex, CustomisationType.Character);

            GameObject lowPolyCarMesh = CustomisationManager.s_instance.GetLowPolyMesh(newValue.m_carIndex, CustomisationType.Car);
            GameObject lowPolyCharacterMesh = CustomisationManager.s_instance.GetLowPolyMesh(newValue.m_characterIndex, CustomisationType.Character);

            GameObject playerReactorGO = CustomisationManager.s_instance.GetPlayerReactorObject(newValue.m_carIndex);

            if (_highPolyCarObj != null) Destroy(_highPolyCarObj.gameObject);
            if (_highPolyCharacterObj != null) Destroy(_highPolyCharacterObj.gameObject);

            if (_lowPolyCarObj != null) Destroy(_lowPolyCarObj.gameObject);
            if (_lowPolyCharacterObj != null) Destroy(_lowPolyCharacterObj.gameObject);

            if (_playerReactor != null) Destroy(_playerReactor.gameObject);

            _highPolyCarObj = Instantiate(highPolyCarMesh, _meshParent);
            _highPolyCharacterObj = Instantiate(highPolyCharacterMesh, _meshParent);

            _lowPolyCarObj = Instantiate(lowPolyCarMesh, _meshParent);
            _lowPolyCharacterObj = Instantiate(lowPolyCharacterMesh, _meshParent);

            _lpCarRenderer = _lowPolyCarObj.GetComponent<Renderer>();
            _lpCharacterRenderer = _lowPolyCharacterObj.GetComponentInChildren<Renderer>();

            _hpCarRenderer = _highPolyCarObj.GetComponent<Renderer>();
            _hpCharacterRenderer = _highPolyCharacterObj.GetComponentInChildren<Renderer>();

            GameObject reactorInstance = Instantiate(playerReactorGO, _meshParent);
            _playerReactor = reactorInstance.GetComponent<PlayerReactors>();

            CustomisationHelper.UpdateLayerFor(_highPolyCarObj, _graphicLayerName);
            CustomisationHelper.UpdateLayerFor(_highPolyCharacterObj, _graphicLayerName);
            CustomisationHelper.UpdateLayerFor(_lowPolyCarObj, _graphicLayerName);
            CustomisationHelper.UpdateLayerFor(_lowPolyCharacterObj, _graphicLayerName);

            SetUpMenuAnimator();

            SwitchLowToHighPolyMesh();
        }

        public void SetMaterialIndex(int index)
        {
            _materialIndex = index;
            OnMaterialChanged(_materialIndex);
        }

        public void SetAIMaterial()
        {
            //if (_currentCarRenderer == null || _currentCharacterRenderer == null) return;

            //Material mat = _customisationData.m_AIMaterial;

            //CustomisationHelper.UpdateMaterial(_currentCarRenderer, CustomisationType.Car, mat, _emissiveIntensity);
            //CustomisationHelper.UpdateMaterial(_currentCharacterRenderer, CustomisationType.Character, mat, _emissiveIntensity);
        }

        public Material GetMaterial()
        {
            return _customisationData.m_materials[_materialIndex];
        }

        public Material GetEmissiveMaterial()
        {
            return _customisationData.m_emissiveMaterials[_materialIndex];
        }

        public Material GetScoreboardMaterial()
        {
            return _customisationData.m_scoreboardMaterials[_materialIndex];
        }

        public void SetLocalPlayerID(int id) => _localPlayerID = id;

        public int GetLocalPlayerID() => _localPlayerID;

        public void ShowGraphics()
        { }

        public void ForceMaterial(int id, bool useDuoMaterial)
        {
            _useDuoModeForForcedMaterial = useDuoMaterial;
            _forceMaterialId = id;
            OnForcedMaterialchanged(_forceMaterialId);
        }

        public void ApplyDefaultMaterial()
        {
            OnMaterialChanged(_materialIndex);
        }

        public void SwitchHighToLowPolyMesh()
        {
            _lowPolyCarObj.gameObject.SetActive(true);
            _lowPolyCharacterObj.gameObject.SetActive(true);

            _highPolyCarObj.gameObject.SetActive(false);
            _highPolyCharacterObj.gameObject.SetActive(false);

            _playerReactor.DisableMenuFeedbacks();
            _playerReactor.EnableGameFeedbacks();
        }

        public void SwitchLowToHighPolyMesh()
        {
            _lowPolyCarObj.gameObject.SetActive(false);
            _lowPolyCharacterObj.gameObject.SetActive(false);

            _highPolyCarObj.gameObject.SetActive(true);
            _highPolyCharacterObj.gameObject.SetActive(true);

            _playerReactor.DisableGameFeedbacks();
            _playerReactor.EnableMenuFeedbacks();
        }

        public void SetUpInGameAnimator() => _inGameAnimator = _lowPolyCharacterObj.GetComponent<Animator>();

        public void SetUpMenuAnimator() => _menuAnimator = _highPolyCharacterObj.GetComponent<Animator>();

        public Animator GetInGameAnimator() => _inGameAnimator;
        public Animator GetMenuAnimator() => _menuAnimator;

        public string GetInGameScoreAnimStateName() => CustomisationManager.s_instance.GetInGameScoreAnimStateName(_serializedData.m_animationIndex);

        public string GetMenuWinAnimStateName() => CustomisationManager.s_instance.GetMenuWinAnimStateName(_serializedData.m_animationIndex);

        public int GetMaterialIndex() => _materialIndex;

        public void UseRandomMeshes()
        {
            _useRandomMeshes = true;
            _serializedData = GetRandomCustomisation();
        }

        public void UseRandomMeshes(object data)
        {
            _useRandomMeshes = true;
            _serializedData = (SerializedCustomisationData)data;
        }

        public object GetRandomCustomisationData()
        {
            return _serializedData;
        }

        #endregion


        #region Hook

        private void OnMaterialChanged(int newValue)
        {
            if (_materialIndex == -1) return;
            if (_hpCarRenderer == null || _lpCarRenderer == null || _hpCharacterRenderer == null || _lpCharacterRenderer == null) return;

            Material mat = _customisationData.m_materials[newValue];
            Material emissiveMat = _customisationData.m_emissiveMaterials[newValue];

            CustomisationHelper.UpdateMaterial(_hpCarRenderer, CustomisationType.Car, mat, emissiveMat);
            CustomisationHelper.UpdateMaterial(_lpCarRenderer, CustomisationType.Car, mat, emissiveMat);
            CustomisationHelper.UpdateMaterial(_hpCharacterRenderer, CustomisationType.Character, mat, emissiveMat);
            CustomisationHelper.UpdateMaterial(_lpCharacterRenderer, CustomisationType.Character, mat, emissiveMat);

            //GetComponentInChildren<PlayerNumberUI>().SetColor(mat.color);
            _playerReactor.UpdateTrailColor(mat.color);
        }

        private void OnForcedMaterialchanged(int newValue)
        {
            Material mat;
            Material emissiveMat;

            if (_useDuoModeForForcedMaterial)
            {
                mat = _customisationData.m_teamModeDuoMaterials[newValue];
                emissiveMat = _customisationData.m_teamModeDuoEmissiveMaterials[newValue];
            }
            else
            {
                mat = GameManager.s_instance.GetForcedPlayerMaterialWithUniqueID(newValue);
                emissiveMat = GameManager.s_instance.GetForcedPlayerEmissiveMaterialWithUniqueID(newValue);
            }
            Material playerNumberMat = _customisationData.m_materials[_materialIndex];
            GetComponentInChildren<PlayerNumberUI>().SetColor(playerNumberMat.color);

            CustomisationHelper.UpdateMaterial(_hpCarRenderer, CustomisationType.Car, mat, emissiveMat);
            CustomisationHelper.UpdateMaterial(_lpCarRenderer, CustomisationType.Car, mat, emissiveMat);
            CustomisationHelper.UpdateMaterial(_hpCharacterRenderer, CustomisationType.Character, mat, emissiveMat);
            CustomisationHelper.UpdateMaterial(_lpCharacterRenderer, CustomisationType.Character, mat, emissiveMat);

            //GetComponentInChildren<PlayerNumberUI>()?.SetColor(mat.color);
            GetComponent<PlayerFeedbacks>()?.UpdatePlayerColor(mat.color);

            _playerReactor.UpdateTrailColor(mat.color);
        }

        #endregion


        #region Private

        private int _materialIndex = -1;
        private bool _useDuoModeForForcedMaterial;
        private int _forceMaterialId = -1;

        private Renderer _hpCarRenderer;
        private Renderer _hpCharacterRenderer;
        private Renderer _lpCarRenderer;
        private Renderer _lpCharacterRenderer;

        private GameObject _highPolyCharacterObj;
        private GameObject _highPolyCarObj;
        private GameObject _lowPolyCharacterObj;
        private GameObject _lowPolyCarObj;

        private PlayerReactors _playerReactor;

        private int _localPlayerID = -1;

        private bool _useRandomMeshes;
        private Animator _inGameAnimator;
        private Animator _menuAnimator;

        #endregion
    }
}