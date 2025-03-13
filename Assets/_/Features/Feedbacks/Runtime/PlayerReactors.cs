using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.VFX;

namespace Feedbacks.Runtime
{
    public class PlayerReactors : MonoBehaviour
    {
        #region Exposed

        [SerializeField][BoxGroup("Menu")] private VisualEffect[] _reactorsMenu;
        [SerializeField][BoxGroup("Menu")] private VisualEffect _sparkleMenu;
        [SerializeField][BoxGroup("Menu")] private TrailRenderer[] _trailsMenu;
        [SerializeField][BoxGroup("Menu")] private GameObject _testMeshMenu;
        [SerializeField][BoxGroup("Menu")] private GameObject _parentMenu;

        [SerializeField][BoxGroup("Game")] private VisualEffect[] _reactorsGame;
        [SerializeField][BoxGroup("Game")] private VisualEffect _sparkleGame;
        [SerializeField][BoxGroup("Game")] private TrailRenderer[] _trailsGame;
        [SerializeField][BoxGroup("Game")] private GameObject _testMeshGame;
        [SerializeField][BoxGroup("Game")] private GameObject _parentGame;

        [SerializeField][BoxGroup("Common")] private float _emissiveIntensity;

        [SerializeField][FoldoutGroup("VFX")] private string _trailColorPropertyName;
        [SerializeField][FoldoutGroup("VFX")] private string _reactorLifeTimePropertyName;
        [SerializeField][FoldoutGroup("VFX")] private string _reactorSizeOverLifePropertyName;
        [SerializeField][FoldoutGroup("VFX")] private string _reactorStartSizePropertyName;
        [SerializeField][FoldoutGroup("VFX")] private string _reactorStartSpeedPropertyName;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        #endregion


        #region Main

        private void Setup()
        {
            Destroy(_testMeshGame);
            Destroy(_testMeshMenu);

            _trailColorPropertyID = Shader.PropertyToID(_trailColorPropertyName);

            _reactorLifeTimePropertyID = Shader.PropertyToID(_reactorLifeTimePropertyName);
            _reactorSizeOverLifePropertyID = Shader.PropertyToID(_reactorSizeOverLifePropertyName);
            _reactorStartSizePropertyID = Shader.PropertyToID(_reactorStartSizePropertyName);
            _reactorStartSpeedPropertyID = Shader.PropertyToID(_reactorStartSpeedPropertyName);
        }

        public void UpdateTrailColor(Color color)
        {
            ChangeTrailColor(color, _trailsGame);
            ChangeTrailColor(color, _trailsMenu);
        }

        public void EnableGameFeedbacks() => EnableDisableFeedbacks(_parentGame, _reactorsGame, _sparkleGame, _trailsGame, true);

        public void DisableGameFeedbacks() => EnableDisableFeedbacks(_parentGame, _reactorsGame, _sparkleGame, _trailsGame, false);


        public void EnableMenuFeedbacks() => EnableDisableFeedbacks(_parentMenu, _reactorsMenu, _sparkleMenu, _trailsGame, true);

        public void DisableMenuFeedbacks() => EnableDisableFeedbacks(_parentMenu, _reactorsMenu, _sparkleMenu, _trailsGame, false);

        public void UpdateReactorDataInGame(ReactorData data)
        {
            foreach (var reactor in _reactorsGame)
            {
                reactor.SetFloat(_reactorLifeTimePropertyID, data.m_lifeTime);
                reactor.SetAnimationCurve(_reactorSizeOverLifePropertyID, data.m_sizeOverLife);
                reactor.SetVector2(_reactorStartSizePropertyID, data.m_startSize);
                reactor.SetFloat(_reactorStartSpeedPropertyID, data.m_startSpeed);
            }
        }

        public void UpdateTrailDataInGame(TrailData data)
        {
            foreach (var trail in _trailsGame)
            {
                trail.time = data.m_time;
            }
        }

        #endregion


        #region Utils & Tools

        private void EnableDisableFeedbacks(GameObject parent, VisualEffect[] vfx, VisualEffect sparkle, TrailRenderer[] trails, bool enable)
        {
            parent.SetActive(enable);

            sparkle.enabled = enable;
            sparkle.gameObject.SetActive(enable);

            foreach (var reactor in vfx)
            {
                reactor.enabled = enable;
                reactor.gameObject.SetActive(enable);
            }

            foreach (var trail in trails)
            {
                trail.enabled = enable;
                trail.gameObject.SetActive(enable);
            }
        }

        private void ChangeTrailColor(Color color, TrailRenderer[] trails)
        {
            MaterialPropertyBlock block = new();
            trails[0].GetPropertyBlock(block);

            Vector4 col = (Vector4)color * _emissiveIntensity;
            col.w = 10;

            block.SetColor(_trailColorPropertyID, col);

            foreach (var trail in trails)
            {
                trail.SetPropertyBlock(block);
            }
        }

        #endregion


        #region Private

        private int _trailColorPropertyID;

        private int _reactorLifeTimePropertyID;
        private int _reactorSizeOverLifePropertyID;
        private int _reactorStartSizePropertyID;
        private int _reactorStartSpeedPropertyID;

        #endregion
    }
}