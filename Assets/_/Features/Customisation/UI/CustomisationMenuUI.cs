using LocalPlayer.Runtime;
using System.Linq;
using UINavigation.Runtime;
using UnityEngine;
using UserInterface.Runtime;
using Data.Runtime;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace Customisation.Runtime.UI
{
    public class CustomisationMenuUI : MenuBase
    {
        #region Exposed

        [SerializeField] private SelectionUI[] _selection;
        [SerializeField] private int _itemCountPerRow;

        [SerializeField] private GarageMenuUI _parentMenu;
        [SerializeField] private CustomisationType _menuType;

        [SerializeField] private MenuBase _mainMenu;

        #endregion


        #region Unity API

        protected override void OnEnable()
        {
            base.OnEnable();
            Setup();
            _mainMenu.DisableInput();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            for (int i = 0; i < _playerPosition.Length; i++)
            {
                _selection[_playerPosition[i]].UnSelect(i);
            }

            _mainMenu.EnableInput();
        }

        #endregion


        #region Main

        private void Setup()
        {
            _previews.Clear();
            List<GameObject> players = LocalPlayerManager.s_instance.GetPlayersInMenu();
            _previews = players.Select(x => x.GetComponent<CustomisationPreview>()).ToList();

            int playerCount = LocalPlayerManager.s_instance.m_localPlayerCount;
            List<int> playerIndex = LocalPlayerManager.s_instance.GetPlayerIndex();

            _playerPosition = new int[playerCount];
            _playerLock = new bool[playerCount];
            _playerBackToLastMenu = new bool[playerCount];

            for (int i = 0; i < playerCount; i++)
            {
                SerializedCustomisationData playerData = CustomisationHelper.GetCustomisation(i);

                int index = 0;

                switch (_menuType)
                {
                    case CustomisationType.Car:
                        index = playerData.m_carIndex;
                        break;

                    case CustomisationType.Character:
                        index = playerData.m_characterIndex;
                        break;

                    case CustomisationType.Animation:
                        index = playerData.m_animationIndex;
                        break;
                }

                Color playerColor = LocalPlayerManager.s_instance.GetColorForPlayerID(playerIndex[i]);

                _selection[index].Select(i, playerColor);
                _playerPosition[i] = index;
                _playerLock[i] = false;
                _playerBackToLastMenu[i] = false;
            }

            
        }

        public void OnClickSelect()
        {
            int playerId = 0;
            if (LocalPlayerManager.s_instance.m_localPlayerCount > 1)
            {
                for (int i = 1; i < LocalPlayerManager.s_instance.m_localPlayerCount + 1; i++)
                {
                    if (DeviceHelper.Runtime.DeviceHelper.GetDeviceType(LocalPlayerManager.s_instance.GetDeviceForLocalPlayer(i)) == Enum.Runtime.DeviceType.Desktop)
                    {
                        playerId = i;
                    }
                }
            }
            else
            {
                playerId = 1;
            }
            if (playerId == 0) return;
            _playerLock[playerId - 1] = true;
            _playerBackToLastMenu[playerId - 1] = false;

            //Show lock image
            int position = _playerPosition[playerId - 1];
            _selection[position].Lock(playerId - 1);

            if (_playerLock.Count(x => !x) == 0)
            {
                //All player have lock choice

                // Save customisation config + back to parent menu
                CustomisationManager.s_instance.SavePlayerCustomisation();
                _parentMenu.DisplaySelectionMenu();
            }
        }

        public void OnClickCancel()
        {
            int playerId = 0;
            if (LocalPlayerManager.s_instance.m_localPlayerCount > 1)
            {
                for (int i = 1; i < LocalPlayerManager.s_instance.m_localPlayerCount + 1; i++)
                {
                    if (DeviceHelper.Runtime.DeviceHelper.GetDeviceType(LocalPlayerManager.s_instance.GetDeviceForLocalPlayer(i)) == Enum.Runtime.DeviceType.Desktop)
                    {
                        playerId = i;
                    }
                }
            }
            else
            {
                playerId = 1;
            }
            if (playerId == 0) return;
            if (_playerLock[playerId - 1]) _playerLock[playerId - 1] = false;
            else _playerBackToLastMenu[playerId - 1] = true;

            //Hide lock image
            int position = _playerPosition[playerId - 1];
            _selection[position].Unlock(playerId - 1);

            if (_playerBackToLastMenu.Count(x => x) == _playerBackToLastMenu.Length)
            {
                // Restore last customisation config + back to parent menu
                CustomisationManager.s_instance.RestoreLastSavedCustomisation();
                _parentMenu.DisplaySelectionMenu();
            }
        }

        public void OnClickRotatePlus()
        {
            int playerId = 0;
            if (LocalPlayerManager.s_instance.m_localPlayerCount > 1)
            {
                for (int i = 1; i < LocalPlayerManager.s_instance.m_localPlayerCount + 1; i++)
                {
                    if (DeviceHelper.Runtime.DeviceHelper.GetDeviceType(LocalPlayerManager.s_instance.GetDeviceForLocalPlayer(i)) == Enum.Runtime.DeviceType.Desktop)
                    {
                        playerId = i;
                    }
                }
            }
            else
            {
                playerId = 1;
            }
            if (playerId == 0) return;
            RotatePlayer(playerId, 15);
        }

        public void OnClickRotateMinus()
        {
            int playerId = 0;
            if (LocalPlayerManager.s_instance.m_localPlayerCount > 1)
            {
                for (int i = 1; i < LocalPlayerManager.s_instance.m_localPlayerCount + 1; i++)
                {
                    if (DeviceHelper.Runtime.DeviceHelper.GetDeviceType(LocalPlayerManager.s_instance.GetDeviceForLocalPlayer(i)) == Enum.Runtime.DeviceType.Desktop)
                    {
                        playerId = i;
                    }
                }
            }
            else
            {
                playerId = 1;
            }
            if (playerId == 0) return;
            RotatePlayer(playerId, -15);
        }

        public void RotatePlayer(int playerId, float value)
        {
            _previews[playerId - 1].RotateMesh(value);
        }

        public void UnselectCurrent(CustomisationSelection selection)
        {
            int playerId = 0;
            if (LocalPlayerManager.s_instance.m_localPlayerCount > 1)
            {
                for (int i = 1; i < LocalPlayerManager.s_instance.m_localPlayerCount + 1; i++)
                {
                    if (DeviceHelper.Runtime.DeviceHelper.GetDeviceType(LocalPlayerManager.s_instance.GetDeviceForLocalPlayer(i)) == Enum.Runtime.DeviceType.Desktop)
                    {
                        playerId = i;
                    }
                }
            }
            else
            {
                playerId = 1;
            }
            if (playerId == 0) return;
            _selection[_playerPosition[playerId - 1]].UnSelect(playerId - 1);
            _playerLock[playerId - 1] = false;
            _playerPosition[playerId - 1] = selection._selectionIndex;
        }

        #endregion


            #region Utils & Tools

        public override void OnSubmit_Performed(int playerId)
        {
            _playerLock[playerId - 1] = true;
            _playerBackToLastMenu[playerId - 1] = false;

            //Show lock image
            int position = _playerPosition[playerId - 1];
            _selection[position].Lock(playerId - 1);

            if (_playerLock.Count(x => !x) == 0)
            {
                //All player have lock choice

                // Save customisation config + back to parent menu
                CustomisationManager.s_instance.SavePlayerCustomisation();
                _parentMenu.DisplaySelectionMenu();
            }
        }

        public override void OnCancel_Performed(int playerId)
        {
            if (_playerLock[playerId - 1]) _playerLock[playerId - 1] = false;
            else _playerBackToLastMenu[playerId - 1] = true;

            //Hide lock image
            int position = _playerPosition[playerId - 1];
            _selection[position].Unlock(playerId - 1);

            if (_playerBackToLastMenu.Count(x => x) == _playerBackToLastMenu.Length)
            {
                // Restore last customisation config + back to parent menu
                CustomisationManager.s_instance.RestoreLastSavedCustomisation();
                _parentMenu.DisplaySelectionMenu();
            }
        }

        protected override void NavigateUp_Performed(int playerId)
        {
            if (_playerLock[playerId - 1]) return;
            UpdatePlayerPosition(playerId - 1, -_itemCountPerRow);
        }

        protected override void NavigateDown_Performed(int playerId)
        {
            if (_playerLock[playerId - 1]) return;
            UpdatePlayerPosition(playerId - 1, _itemCountPerRow);
        }

        protected override void NavigateLeft_Performed(int playerId)
        {
            if (_playerLock[playerId - 1]) return;
            UpdatePlayerPosition(playerId - 1, -1);
        }

        protected override void NavigateRight_Performed(int playerId)
        {
            if (_playerLock[playerId - 1]) return;
            UpdatePlayerPosition(playerId - 1, 1);
        }

        private void UpdatePlayerPosition(int playerId, int increment)
        {
            int position = _playerPosition[playerId];
            _selection[position].UnSelect(playerId);

            position += increment;

            if (position < 0)
            {
                position = (_selection.Length) + position;
            }
            else if (position > _selection.Length - 1)
            {
                position %= _selection.Length;
            }

            Color playerColor = LocalPlayerManager.s_instance.GetColorForPlayerID(playerId + 1);

            _playerPosition[playerId] = position;
            _selection[position].Select(playerId, playerColor);
        }

        public override void OnLeftTrigger_Performed(int playerId, float value)
        {
            RotatePlayer(playerId, value);
        }

        public override void OnRightTrigger_Performed(int playerId, float value)
        {
            RotatePlayer(playerId, - value);
        }

        #endregion


        #region Private

        private int[] _playerPosition;

        private bool[] _playerLock;
        private bool[] _playerBackToLastMenu;

        private List<CustomisationPreview> _previews = new();

        #endregion
    }
}