using ScriptableEvent.Runtime;
using UnityEngine;

namespace Audio.Runtime
{
    public class SfxSource : MonoBehaviour
    {
        private void OnDestroy()
        {
            if (_isMenuSfx) return;
            _onPauseMenuDisplayed.UnregisterListener(PauseSound);
            _onPauseMenuHidden.UnregisterListener(UnPauseSound);
            _onBackToMainMenu.UnregisterListener(OnBackToMainMenu);
        }

        private void Update()
        {
            if (!_isPlaying) return;
            _remainingTime -= Time.deltaTime;

            if (_remainingTime <= 0) Destroy(gameObject);
        }

        public void Setup(AudioSource source, float sfxDuration, GameEvent onPauseMenuDisplayed, GameEvent onPauseMenuHidden, GameEvent onBackToMainMenu, bool isMenuSfx)
        {
            _isMenuSfx = isMenuSfx;
            if (!isMenuSfx)
            {
                _onPauseMenuDisplayed = onPauseMenuDisplayed;
                _onPauseMenuHidden = onPauseMenuHidden;
                _onBackToMainMenu = onBackToMainMenu;

                _onPauseMenuDisplayed.RegisterListener(PauseSound);
                _onPauseMenuHidden.RegisterListener(UnPauseSound);
                _onBackToMainMenu.RegisterListener(OnBackToMainMenu);
            }

            _source = source;
            _remainingTime = sfxDuration;
            _source.Play();
            _isPlaying = true;
        }

        private void PauseSound()
        {
            _isPlaying = false;
            _source.Pause();
        }

        private void UnPauseSound()
        {
            if (_isPlaying) return;
            _source.UnPause();
            _isPlaying = true;
        }

        private void OnBackToMainMenu()
        {
            Destroy(gameObject);
        }

        private GameEvent _onPauseMenuDisplayed;
        private GameEvent _onPauseMenuHidden;
        private GameEvent _onBackToMainMenu;

        private bool _isPlaying;
        private bool _isMenuSfx;

        private float _remainingTime;
        private AudioSource _source;
    }
}