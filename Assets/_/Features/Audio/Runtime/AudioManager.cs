using Data.Runtime;
using ScriptableEvent.Runtime;
using Sirenix.OdinInspector;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio.Runtime
{
    public class AudioManager : MonoBehaviour
    {
        #region Exposed

        public static AudioManager s_instance;

        [SerializeField][BoxGroup("Mixer")] private AudioMixerGroup _musicMixer;
        [SerializeField][BoxGroup("Mixer")] private AudioMixerGroup _sfxMixer;

        [SerializeField][BoxGroup("Audio Source")] private AudioSource _musicSourceIntro;
        [SerializeField][BoxGroup("Audio Source")] private AudioSource _musicSourceLoop;
        [SerializeField][BoxGroup("Audio Source")] private AudioSource _musicSourceMainMenu;

        [SerializeField][BoxGroup("Game Event")] private GameEventT _onStartMusic;
        [SerializeField][BoxGroup("Game Event")] private GameEvent _onStopMusic;
        [SerializeField][BoxGroup("Game Event")] private GameEvent _onStartMainMenuMusic;
        [SerializeField][BoxGroup("Game Event")] private GameEvent _onStopMainMenuMusic;
        [SerializeField][BoxGroup("Game Event")] private GameEvent _onBackToMainMenu;
        [SerializeField][BoxGroup("Game Event")] private GameEvent _onStartJingle;

        [SerializeField][BoxGroup("Pause")] private GameEvent _onPauseMenuDisplayed;
        [SerializeField][BoxGroup("Pause")] private GameEvent _onPauseMenuHidden;

        [SerializeField][BoxGroup("Main Menu")] private MusicData _mainMenuMusicData;

        [SerializeField] private MusicData _musicData;

        [SerializeField] private float _musicFadeDuration;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        #endregion


        #region Main

        private void Setup()
        {
            s_instance = this;

            _musicSourceIntro.playOnAwake = false;
            _musicSourceIntro.mute = false;
            _musicSourceIntro.outputAudioMixerGroup = _musicMixer;

            _musicSourceLoop.playOnAwake = false;
            _musicSourceLoop.mute = false;
            _musicSourceLoop.loop = true;
            _musicSourceLoop.outputAudioMixerGroup = _musicMixer;

            _musicSourceMainMenu.playOnAwake = false;
            _musicSourceMainMenu.mute = false;
            _musicSourceMainMenu.loop = true;
            _musicSourceMainMenu.outputAudioMixerGroup = _musicMixer;

            _onStartMusic.RegisterListener(OnStartMusic);
            _onStopMusic.RegisterListener(OnStopMusic);

            _onStartMainMenuMusic.RegisterListener(OnStartMainMenuMusic);
            _onStopMainMenuMusic.RegisterListener(OnStopMainMenuMusic);

            _onStartJingle.RegisterListener(OnStartJingle);
        }

        public void PlaySfx(SFXProperties prop, bool isMenuSfx)
        {
            GameObject temp = new GameObject("Temp_Audio");
            temp.transform.parent = this.transform;
            AudioSource source = temp.AddComponent<AudioSource>();
            SfxSource sfxSource = temp.AddComponent<SfxSource>();

            source.playOnAwake = false;
            source.clip = prop.m_clip;
            source.volume = prop.m_volume;
            source.outputAudioMixerGroup = _sfxMixer;

            sfxSource.Setup(source, prop.m_clip.length + .1f, _onPauseMenuDisplayed, _onPauseMenuHidden, _onBackToMainMenu, isMenuSfx);
        }

        private void OnStartMusic(object obj)
        {
            string mapName = obj as string;
            if (string.IsNullOrWhiteSpace(mapName))
            {
                Debug.LogError("Impossible to play music without map name");
                return;
            }

            MusicProperties props = _musicData._musics.FirstOrDefault(x => x.m_mapName == mapName);
            if (props == null)
            {
                Debug.LogError($"No music found for map : {mapName}");
                return;
            }

            _currentProps = props;

            PlayIntro();
            StartCoroutine(nameof(StartLoop), props.m_intro.length);
        }

        private void OnStopMusic()
        {
            StopAllCoroutines();

            if (_musicSourceIntro.isPlaying) StartCoroutine(nameof(FadeMusic), _musicSourceIntro);
            else StartCoroutine(nameof(FadeMusic), _musicSourceLoop);
        }

        private void OnStartMainMenuMusic()
        {
            int index = Random.Range(0, _mainMenuMusicData._musics.Length);
            MusicProperties prop = _mainMenuMusicData._musics[index];

            _musicSourceMainMenu.clip = prop.m_intro;
            _musicSourceMainMenu.volume = prop.m_volume;

            _musicSourceMainMenu.Play();
        }

        private void OnStopMainMenuMusic()
        {
            StopAllCoroutines();
            StartCoroutine(nameof(FadeMusic), _musicSourceMainMenu);
        }

        private void OnStartJingle()
        {
            StopAllCoroutines();
            PlayJingle();
        }

        #endregion


        #region Utils & Tools

        private void PlayIntro()
        {
            _musicSourceIntro.clip = _currentProps.m_intro;
            _musicSourceIntro.volume = _currentProps.m_volume;
            _musicSourceIntro.Play();
        }

        private void PlayLoop()
        {
            _musicSourceLoop.clip = _currentProps.m_loop;
            _musicSourceLoop.volume = _currentProps.m_volume;
            _musicSourceLoop.Play();
        }

        private void PlayJingle()
        {
            _musicSourceIntro.clip = _musicData._jingle.m_intro;
            _musicSourceIntro.volume = _musicData._jingle.m_volume;
            _musicSourceIntro.Play();
        }

        private IEnumerator FadeMusic(AudioSource source)
        {
            while (source.volume > 0)
            {
                source.volume -= Time.deltaTime / _musicFadeDuration;
                yield return null;
            }

            source.Stop();
        }

        private IEnumerator StartLoop(float duration)
        {
            float remainingTime = duration;
            while (remainingTime > 0)
            {
                remainingTime -= Time.unscaledDeltaTime;
                yield return null;
            }
            PlayLoop();
        }

        #endregion


        #region Private

        private MusicProperties _currentProps;

        #endregion
    }
}