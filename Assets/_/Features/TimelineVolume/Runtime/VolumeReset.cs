using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TimelineVolume.Runtime
{
    public class VolumeReset : MonoBehaviour
    {
        #region Exposed

        public static VolumeReset s_instance;

        [SerializeField] private VolumeProfile _volume;

        [SerializeField] private float _defaultBloomIntensity;
        [SerializeField] private float _defaultChromaticAberration;

        [SerializeField] private Material _ovetimeMat;

        [SerializeField] private bool _resetOnAwake;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        #endregion


        #region Main

        private void Setup()
        {
            if (_resetOnAwake) ResetVolume();

            if (s_instance != null)
            {
                Destroy(s_instance.gameObject);
            }

            s_instance = this;
        }

        public void ResetVolume()
        {
            _volume.TryGet<ChromaticAberration>(out var chr);
            chr.intensity.Override(_defaultChromaticAberration);

            _volume.TryGet<Bloom>(out var bloom);
            bloom.intensity.Override(_defaultBloomIntensity);

            _ovetimeMat.SetFloat("_AlphaPower", 0);
        }

        #endregion
    }
}