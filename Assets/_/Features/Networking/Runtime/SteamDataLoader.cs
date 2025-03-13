using Data.Runtime;
using ScriptableEvent.Runtime;
using UnityEngine;

namespace Networking.Runtime
{
    public class SteamDataLoader : MonoBehaviour
    {
        #region Exposed

        [SerializeField] private GameEvent _onSteamDataLoaded;

#if UNITY_EDITOR
        [SerializeField] private GameEvent _onSteamDataLoaded_Debug;
#endif

        #endregion


        #region Unity API

        private void Start() => Setup();

        #endregion


        #region Main

        private void Setup()
        {
            //if (!SteamManager.Initialized) return;

            _playerSteamData = PlayerData.s_instance.m_playerSteamData;

#if UNITY_EDITOR
            _onSteamDataLoaded_Debug?.Raise();
#endif
            _onSteamDataLoaded?.Raise();

            //c_imageLoaded = Callback<AvatarImageLoaded_t>.Create(OnImageLoaded);

            //_playerSteamData._steamID = SteamUser.GetSteamID().m_SteamID;
            //_playerSteamData._playerName = SteamFriends.GetPersonaName();
            //GetPlayerIcon();
        }

//        private void OnImageLoaded(AvatarImageLoaded_t callback)
//        {
//            if (callback.m_steamID.m_SteamID != _playerSteamData._steamID) return;

//            _playerSteamData._profilePicture = GetSteamImageAsTexture(callback.m_iImage);
//#if UNITY_EDITOR
//            _onSteamDataLoaded_Debug?.Raise();
//#endif
//            _onSteamDataLoaded?.Raise();
//        }

        //private Texture2D GetSteamImageAsTexture(int iImage)
        //{
        //    Texture2D texture = null;

        //    bool isValid = SteamUtils.GetImageSize(iImage, out uint width, out uint height);
        //    if (isValid)
        //    {
        //        byte[] image = new byte[width * height * 4];

        //        isValid = SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4));

        //        if (isValid)
        //        {
        //            texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
        //            texture.LoadRawTextureData(image);
        //            texture.Apply();
        //        }
        //    }

        //    _playerSteamData._avatarReceived = true;
        //    return texture;
        //}

//        private void GetPlayerIcon()
//        {
//            int imageId = SteamFriends.GetLargeFriendAvatar((CSteamID)_playerSteamData._steamID);
//            if (imageId == -1) return;
//            _playerSteamData._profilePicture = GetSteamImageAsTexture(imageId);
//#if UNITY_EDITOR
//            _onSteamDataLoaded_Debug?.Raise();
//#endif
//            _onSteamDataLoaded?.Raise();
//        }

        #endregion


        #region Private

        private PlayerSteamData _playerSteamData;

        //protected Callback<AvatarImageLoaded_t> c_imageLoaded;

        #endregion
    }
}