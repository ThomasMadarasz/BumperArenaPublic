using Archi.Runtime;
using Data.Runtime;
using Interfaces.Runtime;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Utils.Runtime;

namespace Customisation.Runtime
{
    public class CustomisationPreview : CBehaviour, IPlayerCustomisation
    {
        #region Exposed

        [SerializeField] private Transform _meshParent;

        [SerializeField] private CustomisationData _data;

        [SerializeField] private int _defaultCarMeshIndex;
        [SerializeField] private int _defaultCharacterMeshIndex;
        [SerializeField] private int _defaultAnimationIndex;
        [SerializeField] private int _defaultMaterialIndex;

        [SerializeField] private float _rotationSpeed;

        [SerializeField] private string _graphicLayerName;

        [SerializeField] List<AnimationClip> _idleAnims;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        #endregion


        #region Main

        private void Setup()
        {
            GameObject characterMeshPrefab = CustomisationManager.s_instance.GetHighPolyMesh(_currentCharacterIndex, CustomisationType.Character);
            GameObject carMeshPrefab = CustomisationManager.s_instance.GetHighPolyMesh(_currentCarIndex, CustomisationType.Car);

            _carMesh = Instantiate(carMeshPrefab, _meshParent);
            _characterMesh = Instantiate(characterMeshPrefab, _meshParent);

            SetUpMenuAnimator();
            StartCoroutine(PlayRandomIdle());
        }

        public SerializedCustomisationData GetCurrentConfig() => GetCurrentData();

        public void ChangeMesh(GameObject meshPrefrab, CustomisationType type, int index)
        {
            UpdateMesh(meshPrefrab, type, index);

            GameObject mesh = type == CustomisationType.Car ? _carMesh : _characterMesh;
            Renderer renderer = type == CustomisationType.Car ? mesh.GetComponent<Renderer>() : mesh.GetComponentInChildren<Renderer>();
            CustomisationHelper.UpdateMaterial(renderer, type, _mat, _emissiveMat);
        }

        public void ChangeMaterial(int index)
        {
            _currentMaterialIndex = index;
            _mat = GetMaterial();
            _emissiveMat = GetEmissiveMaterial();

            CustomisationHelper.UpdateMaterial(_carMesh.GetComponent<Renderer>(), CustomisationType.Car, _mat, _emissiveMat);
            CustomisationHelper.UpdateMaterial(_characterMesh.GetComponentInChildren<Renderer>(), CustomisationType.Character, _mat, _emissiveMat);
        }

        public void ChangeAnimation(int index)
        {
            _currentAnimationIndex = index;
            _menuAnimator.Play(_data.m_animations[index].m_nameMenu.name);
            StopCoroutine(PlayRandomIdle());
        }

        public void SetDefaultAnimation()
        {
            StopAllCoroutines();
            _menuAnimator.Rebind();
            StartCoroutine(PlayRandomIdle());
        }

        public void RotateMesh(float rotationSpeed)
        {
            float speed = _rotationSpeed * rotationSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up * speed);
        }

        //public void ChangeToNextMaterial()
        //{
        //    _mat = CustomisationManager.s_instance.GetNextMaterial(ref _currentMaterialIndex);

        //    CustomisationHelper.UpdateMaterial(_carMesh.GetComponent<Renderer>(), CustomisationType.Car, _mat,_emissiveMat, _emissiveIntensity);
        //    CustomisationHelper.UpdateMaterial(_characterMesh.GetComponentInChildren<Renderer>(), CustomisationType.Character, _mat, _emissiveMat, _emissiveIntensity);
        //}

        //public void ChangeToPreviousMaterial()
        //{
        //    _mat = CustomisationManager.s_instance.GetPreviousMaterial(ref _currentMaterialIndex);

        //    CustomisationHelper.UpdateMaterial(_carMesh.GetComponent<Renderer>(), CustomisationType.Car, _mat, _emissiveMat, _emissiveIntensity);
        //    CustomisationHelper.UpdateMaterial(_characterMesh.GetComponentInChildren<Renderer>(), CustomisationType.Character, _mat, _emissiveMat, _emissiveIntensity);
        //}

        #endregion


        #region Utils & Tools

        private Material GetMaterial()
        {
            return CustomisationManager.s_instance.GetMaterial(_currentMaterialIndex);
        }

        private Material GetEmissiveMaterial()
        {
            return CustomisationManager.s_instance.GetEmissiveMaterial(_currentMaterialIndex);
        }

        private void UpdateMesh(GameObject meshPrefrab, CustomisationType type, int index)
        {
            switch (type)
            {
                case CustomisationType.Car:
                    Destroy(_carMesh);
                    _carMesh = Instantiate(meshPrefrab, _meshParent);
                    CustomisationHelper.UpdateLayerFor(_carMesh.gameObject, _graphicLayerName);
                    _currentCarIndex = index;
                    break;

                case CustomisationType.Character:
                    Destroy(_characterMesh);
                    _menuAnimator = null;
                    _characterMesh = Instantiate(meshPrefrab, _meshParent);
                    CustomisationHelper.UpdateLayerFor(_characterMesh.gameObject, _graphicLayerName);
                    _currentCharacterIndex = index;
                    SetUpMenuAnimator();
                    break;

                default:
                    Debug.LogError("Specified Type is not correct!");
                    break;
            }
        }

        private void LoadCustomisationData()
        {
            SerializedCustomisationData data = CustomisationHelper.GetCustomisation(_localPlayerId - 1);

            if (data == null) data = CreateDefaultConfig();

            _loadedConfig = data;
        }

        private SerializedCustomisationData CreateDefaultConfig()
        {
            //Create default config
            SerializedCustomisationData data = new SerializedCustomisationData()
            {
                m_animationIndex = _defaultAnimationIndex,
                m_carIndex = _defaultCarMeshIndex,
                m_characterIndex = _defaultCharacterMeshIndex,
                m_materialIndex = _defaultMaterialIndex
            };

            //Save default config file
            SerializedData defaultData = new();
            defaultData.FillArrayWithDefaultConfig(data);

            CustomisationHelper.SaveAllCustomisationData(defaultData);

            return data;
        }

        private void UpdateWithLoadedData()
        {
            if (_loadedConfig == null)
            {
                Debug.LogError("Loaded config cannot be null!");
                return;
            }

            _currentCarIndex = _loadedConfig.m_carIndex;
            _currentCharacterIndex = _loadedConfig.m_characterIndex;
            _currentAnimationIndex = _loadedConfig.m_animationIndex;

            _mat = GetMaterial();
            _emissiveMat = GetEmissiveMaterial();

            GameObject characterMeshPrefab = CustomisationManager.s_instance.GetHighPolyMesh(_currentCharacterIndex, CustomisationType.Character);
            GameObject carMeshPrefab = CustomisationManager.s_instance.GetHighPolyMesh(_currentCarIndex, CustomisationType.Car);

            ChangeMesh(characterMeshPrefab, CustomisationType.Character, _currentCharacterIndex);
            ChangeMesh(carMeshPrefab, CustomisationType.Car, _currentCarIndex);
        }

        private SerializedCustomisationData GetCurrentData()
        {
            SerializedCustomisationData data = new SerializedCustomisationData()
            {
                m_carIndex = _currentCarIndex,
                m_characterIndex = _currentCharacterIndex,
                m_animationIndex = _currentAnimationIndex,
                m_materialIndex = _currentMaterialIndex
            };

            return data;
        }

        public void SetLocalPlayerID(int id) => _localPlayerId = id;

        public int GetLocalPlayerID() => _localPlayerId;

        public void ShowGraphics()
        {
            LoadCustomisationData();
            UpdateWithLoadedData();
        }

        private void SetUpMenuAnimator() => _menuAnimator = _characterMesh.GetComponent<Animator>();

        private IEnumerator PlayRandomIdle()
        {
            while (true)
            {
                AnimatorStateInfo stateInfo = _menuAnimator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.IsName("DefaultState"))
                {
                    AnimationClip currentIdleAnim = _idleAnims.GetRandom();
                    _idleAnims.Remove(currentIdleAnim);
                    _menuAnimator.CrossFade(currentIdleAnim.name, 0.1f);
                    if (_previousIdleAnim) _idleAnims.Add(_previousIdleAnim);
                    _previousIdleAnim = currentIdleAnim;
                }
                yield return new WaitForSeconds(Random.Range(7f, 10f));
            }
        }

        #endregion


        #region Private

        private GameObject _carMesh;
        private GameObject _characterMesh;

        private Material _mat;
        private Material _emissiveMat;

        private int _currentAnimationIndex;
        private int _currentCarIndex;
        private int _currentCharacterIndex;
        private int _currentMaterialIndex;

        private int _localPlayerId;

        SerializedCustomisationData _loadedConfig;

        private Animator _menuAnimator;

        private AnimationClip _previousIdleAnim;

        #endregion
    }
}