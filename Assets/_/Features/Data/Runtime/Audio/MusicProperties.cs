using Sirenix.OdinInspector;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

namespace Data.Runtime
{
    [Serializable]
    public class MusicProperties
    {
        public AudioClip m_intro;
        public AudioClip m_loop;

       [SerializeField] private AudioMixerGroup _group;

        [Range(0f, 1f)]
        public float m_volume;

        public string m_mapName;

        [Button]
        private void Play()
        {
            GameObject go = new("Temp_Audio");
            AudioSource source = go.AddComponent<AudioSource>();
            source.clip = m_intro;
            source.volume = m_volume;
            source.outputAudioMixerGroup = _group;
            source.loop = false;
            source.Play();

            DestroyAudioSource(go, source.clip.length);
        }

        private async void DestroyAudioSource(GameObject go, float delay)
        {
            int d = UnityEngine.Mathf.RoundToInt(delay * 1000);
            await Task.Delay(d);
            GameObject.DestroyImmediate(go);
        }
    }
}