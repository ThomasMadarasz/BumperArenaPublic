using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TimelineVolume.Runtime
{
    [ExecuteAlways]
    public class VolumeUpdater : MonoBehaviour
    {
        #region Exposed

        [SerializeField] private VolumeProfile _volume;

        public float m_bloomIntensity;
        public float m_chromaticAberration;

        private void Awake()
        {
            m_bloomIntensity = BloomIntensity;
            m_chromaticAberration = ChromaticAberration;
        }

        private void Update()
        {
            if (m_bloomIntensity != _bloomIntensity) BloomIntensity = m_bloomIntensity;
            if (m_chromaticAberration != _chromaticAberration) ChromaticAberration = m_chromaticAberration;
        }

        private void OnDestroy()
        {
            if(VolumeReset.s_instance == null)
            {
                Debug.LogWarning("Volume reset instance is null");
                return;
            }
            VolumeReset.s_instance.ResetVolume();
        }

        #endregion


        #region Accessor

        public float BloomIntensity
        {
            get
            {
                //if (_bloomIntensity == float.MinValue) LoadBloom();
                return _bloomIntensity;
            }
            set
            {
                //if (_bloomIntensity == float.MinValue) LoadBloom();
                if (_bloomIntensity == value) return;

                _volume.TryGet<Bloom>(out var bloom);
                bloom.intensity.Override(value);
                _bloomIntensity = value;
            }
        }

        public float ChromaticAberration
        {
            get
            {
                //if (_chromaticAberration == float.MinValue) LoadChromaticAberration();
                return _chromaticAberration;
            }
            set
            {
                //if (_chromaticAberration == float.MinValue) LoadChromaticAberration();
                if (_chromaticAberration == value) return;

                _volume.TryGet<ChromaticAberration>(out var chr);
                chr.intensity.Override(value);
                _chromaticAberration = value;
            }
        }

        #endregion


        #region Utils & Tools

        //private void LoadBloom()
        //{
        //    _volume.TryGet<Bloom>(out var bloom);
        //    _bloomIntensity = bloom.intensity.GetValue<float>();
        //}

        //private void LoadChromaticAberration()
        //{
        //    _volume.TryGet<ChromaticAberration>(out var chr);
        //    _chromaticAberration = chr.intensity.GetValue<float>();
        //}

        #endregion


        #region Private

        private float _bloomIntensity = 0.19f;
        private float _chromaticAberration = 0f;

        #endregion
    }
}