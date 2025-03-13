using Sirenix.OdinInspector;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

namespace Data.Runtime
{
    [Serializable]
    public class SFXProperties
    {
        public AudioClip m_clip;
        [Range(0f, 1f)]
        public float m_volume;

        [SerializeField] private AudioMixerGroup _group;

        [Button]
        private void Play()
        {
            GameObject go = new("Temp_Audio");
            AudioSource source = go.AddComponent<AudioSource>();
            source.clip = m_clip;
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