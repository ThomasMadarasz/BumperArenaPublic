using Archi.Runtime;
using Data.Runtime;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using LocalPlayer.Runtime;




namespace Customisation.Runtime.UI
{
    public class CustomisationSelection : CBehaviour
    {
        #region Exposed

        public int _selectionIndex;
        [SerializeField] private CustomisationType _type;
        [SerializeReference] private SelectionUI _selectionUI;

        #endregion


        #region Main

        public Sprite LoadSprite()
        {
            if (_type != CustomisationType.Car && _type != CustomisationType.Character) return null;

            return CustomisationManager.s_instance.GetSprite(_selectionIndex, _type);
        }

        public string LoadText()
        {
            if (_type != CustomisationType.Animation) return null;

            return CustomisationManager.s_instance.GetName(_selectionIndex, _type);
        }

        public void Select(int playerIndex)
        {
            CustomisationManager.s_instance.UpdatePlayer(playerIndex, _selectionIndex, _type);
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
            _selectionUI.Select(playerId - 1, LocalPlayerManager.s_instance.GetColorForPlayerID(playerId));
            CustomisationManager.s_instance.UpdatePlayer(playerId, _selectionIndex, _type);
        }

        #endregion
    }
}