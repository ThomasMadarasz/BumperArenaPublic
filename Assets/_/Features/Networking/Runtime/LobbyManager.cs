using Archi.Runtime;
using Data.Runtime;
using Mirror;
using System.Collections.Generic;
using UnityEngine;

namespace Networking.Runtime
{
    public class LobbyManager : CNetBehaviour
    {
        #region Exposed

        public static LobbyManager s_instance;

        [SerializeField] private CustomisationData _customisationData;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        #endregion


        #region Main

        private void Setup()
        {
            s_instance = this;

            foreach (var item in _customisationData.m_materials)
            {
                _playerColorAvailability.Add(true);
            }

            foreach (var item in _customisationData.m_materials)
            {
                _playerIndex.Add(0);
            }
        }

        [ServerCallback]
        public int GetNextAvailableMaterialIndex(int materialIndex, int playerIndex)
        {
            int index = materialIndex;
            _playerColorAvailability[index] = true;
            _playerIndex[index] = 0;

            bool find = false;

            do
            {
                index++;
                index %= _customisationData.m_materials.Length;

                if (_playerColorAvailability[index])
                    find = true;

            } while (!find);

            _playerColorAvailability[index] = false;
            _playerIndex[index] = playerIndex;

            return index;
        }

        [ServerCallback]
        public int GetPreviousAvailableMaterialIndex(int materialIndex, int playerIndex)
        {
            int index = materialIndex;
            _playerColorAvailability[index] = true;
            _playerIndex[index] = 0;

            bool find = false;

            do
            {
                index--;
                if (index < 0) index = _customisationData.m_materials.Length - 1;

                if (_playerColorAvailability[index])
                    find = true;

            } while (!find);

            _playerColorAvailability[index] = false;
            _playerIndex[index] = playerIndex;

            return index;
        }

        [ServerCallback]
        public int GetFirstAvailableMaterialIndex(int playerIndex)
        {
            int index = 0;

            for (int i = 0; i < _customisationData.m_materials.Length; i++)
            {
                if (_playerColorAvailability[i])
                {
                    index = i;
                    break;
                }
            }
            _playerColorAvailability[index] = false;
            _playerIndex[index] = playerIndex;

            return index;
        }

        [ServerCallback]
        public Dictionary<int, int> GetPlayerMaterialAndIndex()
        {
            Dictionary<int, int> colorIndex = new();

            for (int i = 0; i < _playerColorAvailability.Count; i++)
            {
                if (_playerColorAvailability[i]) continue;

                colorIndex.Add(_playerIndex[i], i);
            }

            return colorIndex;
        }

        [ServerCallback]
        public List<int> GetAvailableMaterials()
        {
            List<int> colors = new();

            for (int i = 0; i < _playerColorAvailability.Count; i++)
            {
                if (!_playerColorAvailability[i]) continue;

                colors.Add(i);
            }

            return colors;
        }

        #endregion


        #region Private

        private List<bool> _playerColorAvailability = new();
        private List<int> _playerIndex = new();

        #endregion
    }
}