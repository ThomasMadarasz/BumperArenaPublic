using UnityEngine;

namespace Data.Runtime
{
    [CreateAssetMenu(fileName = "PlayerSteamData", menuName = "Data/PlayerData")]
    public class PlayerSteamData : ScriptableObject
    {
        public ulong _steamID;
        public string _playerName;
        public Texture2D _profilePicture;
        public bool _avatarReceived;


        private void Awake() => RestoreDefaultValue();

        private void RestoreDefaultValue()
        {
            _avatarReceived = false;
            _profilePicture = null;
            _steamID = default(ulong);
        }
    }
}