using Archi.Runtime;
using Interfaces.Runtime;
using ScriptableEvent.Runtime;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils.Runtime;

namespace Traps.Runtime
{
    public class BlackHole : CBehaviour
    {
        #region Exposed

        [FoldoutGroup("Components", expanded: false)][SerializeField] private GameEvent _onRoundStart;

        [FoldoutGroup("Components", expanded: false)][SerializeField] private Transform _graphics;

        [FoldoutGroup("Components", expanded: false)][SerializeField] private SphereCollider _deathZone;
        [FoldoutGroup("Components", expanded: false)][SerializeField] private SphereCollider _attractionZone;

        [BoxGroup("Graphic Size")][SerializeField] private float _graphicsStartSize;
        [BoxGroup("Graphic Size")][SerializeField] private float _graphicsEndSize;

        [BoxGroup("Attraction Force")][SerializeField] private float _attractionForceAtStart;
        [BoxGroup("Attraction Force")][SerializeField] private float _attractionForceAtEnd;
        [Space(5)]
        [BoxGroup("Attraction Force")][SerializeField] private float _attractionForceMutliplier;

        [Space(10)]
        [Tooltip("The ratio between the total graphic part and the graphic part of the kill zone")]
        [Range(0, 1)]
        [SerializeField] private float _deathZoneRadiusRatio;

        [SerializeField] private float _timeToReachMaxSize;



        #endregion


        #region Unity API

        private void Awake()
        {
            SetDefaultValues();

            //kill zone est un ratio de +- 0.6 (le kill fait 0.4)
            //taille max du radius theorique = start graphic size * 2 / start kill zone radius

            //kill zone radius = graphics raidus * 0.4

            _maxAttractionGain = _attractionForceAtEnd - _attractionForceAtStart;
            _remainingTime = _timeToReachMaxSize;
            _onRoundStart.RegisterListener(StartToUpdateSize);
            _sizeTimer = new(_timeToReachMaxSize, null);
            _sizeTimer.OnValueChanged += OnRemainingTimeChanged;
        }

        private void FixedUpdate() => UpdateSize();

        private void OnTriggerExit(Collider other)
        {
            ISubjectToAForce player = other.gameObject.GetComponent<ISubjectToAForce>();
            if (player == null) return;

            player.AddExternalForce(Vector3.zero);
        }

        private void OnTriggerStay(Collider other)
        {
            ISubjectToAForce player = other.gameObject.GetComponent<ISubjectToAForce>();
            if (player == null) return;

            float force = GetAttractionForce();
            Vector3 direction = other.transform.position - transform.position;
            //Vector3 forceVector = force * (_attractionZone.radius - Mathf.Clamp(direction.magnitude, 0, _attractionZone.radius)) * direction.normalized;

            Vector3 forceVector = (direction.normalized * force) * _attractionForceMutliplier;

            player.AddExternalForce(-forceVector);
        }

        #endregion


        #region Main

        private void SetDefaultValues()
        {
            _graphics.localScale = _graphicsStartSize * Vector3.one;
            _attractionZone.radius = _graphicsStartSize * 2;
            _deathZone.radius = (_graphicsStartSize * 2) * _deathZoneRadiusRatio;
        }

        private void UpdateSize()
        {
            if (!_canUpdateSize || _maxSizeReached) return;

            float graphicsSize = (_graphicsStartSize + (((_timeToReachMaxSize - _remainingTime) / (_timeToReachMaxSize / (_graphicsEndSize - _graphicsStartSize)))));
            graphicsSize = Mathf.Clamp(graphicsSize, _graphicsStartSize, _graphicsEndSize);
            _graphics.localScale = graphicsSize * Vector3.one;

            _attractionZone.radius = graphicsSize * 2;
            _deathZone.radius = (graphicsSize * 2) * _deathZoneRadiusRatio;

            if (graphicsSize == _graphicsEndSize) _maxSizeReached = true;
        }

        #endregion


        #region Utils & Tools

        private void OnRemainingTimeChanged(float value) => _remainingTime = value;

        private void StartToUpdateSize()
        {
            _sizeTimer.Start();
            _canUpdateSize = true;
        }

        private float GetAttractionForce()
        {
            float forceRatioOnTime = Mathf.Abs(_remainingTime - _timeToReachMaxSize) / _timeToReachMaxSize;
            forceRatioOnTime = Mathf.Clamp01(forceRatioOnTime);

            float additionalValue = _maxAttractionGain * forceRatioOnTime;
            float force = Mathf.Clamp(_attractionForceAtStart + additionalValue, _attractionForceAtStart, _attractionForceAtEnd);

            return force;
        }

        #endregion


        #region Private

        private Timer _sizeTimer;

        private float _remainingTime;
        private float _maxAttractionGain;

        private bool _canUpdateSize;
        private bool _maxSizeReached;

        #endregion
    }
}