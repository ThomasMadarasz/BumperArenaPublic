using Archi.Runtime;
using Data.Runtime;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Customisation.Runtime
{
    public class CustomisationManager : CBehaviour
    {
        #region Exposed

        [SerializeField] private CustomisationData _data;

        public static CustomisationManager s_instance;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        #endregion


        #region Main

        private void Setup()
        {
            s_instance = this;
        }

        public void ResetPlayerPreviews()
        {
            _playerPreviews = null;
        }

        public void UpdatePlayer(int playerIndex, int index, CustomisationType type)
        {
            if (_playerPreviews == null) GetPlayerPreview();

            CustomisationPreview preview = _playerPreviews.FirstOrDefault(x => x.GetLocalPlayerID() == playerIndex);

            if (type == CustomisationType.Animation)
            {
                preview.ChangeAnimation(index);
            }
            else if (type == CustomisationType.Car || type == CustomisationType.Character)
            {
                GameObject mesh = GetHighPolyMesh(index, type);
                preview.ChangeMesh(mesh, type, index);
            }
        }

        public void SavePlayerCustomisation()
        {
            if (_playerPreviews == null) GetPlayerPreview();

            SerializedData data = CustomisationHelper.GetSavedData();

            foreach (var p in _playerPreviews)
            {
                data.m_data[p.GetLocalPlayerID() - 1] = p.GetCurrentConfig();
                p.SetDefaultAnimation();
            }

            CustomisationHelper.SaveAllCustomisationData(data);
        }

        public void RestoreLastSavedCustomisation()
        {
            if (_playerPreviews == null) GetPlayerPreview();

            foreach (var p in _playerPreviews)
            {
                p.ShowGraphics();
            }
        }

        #endregion


        #region Utils & Tools

        private void GetPlayerPreview()
        {
            _playerPreviews = FindObjectsOfType<CustomisationPreview>();
        }

        public GameObject GetHighPolyMesh(int index, CustomisationType type)
        {
            if (type != CustomisationType.Car && type != CustomisationType.Character) return null;

            CustomisationInfo[] infos = type == CustomisationType.Character ? _data.m_characters : _data.m_cars;

            return infos[index].m_highPoly;
        }

        public GameObject GetLowPolyMesh(int index, CustomisationType type)
        {
            if (type != CustomisationType.Car && type != CustomisationType.Character) return null;

            CustomisationInfo[] infos = type == CustomisationType.Character ? _data.m_characters : _data.m_cars;

            return infos[index].m_lowPoly;
        }

        public string GetInGameScoreAnimStateName(int index) => _data.m_animations[index].m_nameInGame.name;

        public string GetMenuWinAnimStateName(int index) => _data.m_animations[index].m_nameMenu.name;

        public GameObject GetPlayerReactorObject(int index)
        {
            return _data.m_carsReactors[index];
        }

        public Material GetMaterial(int index)
        {
            return _data.m_materials[index];
        }

        public Material GetEmissiveMaterial(int index)
        {
            return _data.m_emissiveMaterials[index];
        }

        public Material GetNextMaterial(ref int index)
        {
            index++;
            index %= _data.m_materials.Length;

            return GetMaterial(index);
        }

        public Material GetPreviousMaterial(ref int index)
        {
            index--;
            if (index < 0) index = _data.m_materials.Length - 1;

            return GetMaterial(index);
        }

        public Sprite GetSprite(int index, CustomisationType type)
        {
            if (type != CustomisationType.Car && type != CustomisationType.Character) return null;

            CustomisationInfo[] infos = type == CustomisationType.Character ? _data.m_characters : _data.m_cars;
            return infos[index].m_sprite;
        }

        public string GetName(int index, CustomisationType type)
        {
            if (type != CustomisationType.Animation) return null;

            return _data.m_animations[index].m_displayName;
        }

        #endregion


        #region Private

        private CustomisationPreview[] _playerPreviews;

        #endregion
    }
}