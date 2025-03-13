using UnityEngine;

namespace Data.Runtime
{
    [CreateAssetMenu(fileName = "NewMusicData", menuName = "Audio/MusicData")]
    public class MusicData : ScriptableObject
    {
        public MusicProperties[] _musics;
        public MusicProperties _jingle;
    }
}