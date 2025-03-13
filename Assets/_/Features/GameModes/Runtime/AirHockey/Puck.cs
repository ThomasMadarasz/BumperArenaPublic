using UnityEngine;
using Archi.Runtime;
using Mirror;
using UnityEngine.Playables;
using System;
using UnityEngine.VFX;

namespace GameModes.Runtime
{
    [RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody), typeof(NetworkIdentity))]
    public class Puck : CNetBehaviour
    {
        #region Exposed

        [SerializeField] private PuckData _data;
        [SerializeField] private Transform[] _spawnTransforms;
        [SerializeField] private int _thisLayer;
        [SerializeField] private int _playerLayer;

        [SerializeField] private GameObject _respawnGo;
        [SerializeField] private GameObject _goalGo;

        [SerializeField] private PlayableDirector _timelineSpawn;
        [SerializeField] private PlayableDirector _timelineGoal;

        [SerializeField] private VisualEffect _goalVfxPrefab;
        [SerializeField] private float _vfxGoalDuration;

        [HideInInspector] public bool m_isActive;

        #endregion


        #region Unity API

        private void Awake()
        {
            _timelineGoal.extrapolationMode = DirectorWrapMode.Hold;
            _timelineSpawn.extrapolationMode = DirectorWrapMode.Hold;

            Setup();
        }

        #endregion


        #region Main

        [ServerCallback]
        private void Setup()
        {
            rigidBody.mass = _data.m_mass;
            _pmat = GetComponent<Collider>().material;
            _pmat.dynamicFriction = _data.m_dynamicFriction;
            _pmat.staticFriction = _data.m_staticFriction;
            _pmat.bounciness = _data.m_bounciness;
            _pmat.frictionCombine = _data.m_frictionCombine;
            _pmat.bounceCombine = _data.m_bounceCombine;
        }

        public void Spawn()
        {
            transform.position = _spawnTransforms[UnityEngine.Random.Range(0, _spawnTransforms.Length)].position;
            rigidBody.velocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;
            rigidBody.ResetInertiaTensor();
            Appear();
        }

        private void Appear()
        {
            Rpc_AppearEffect();

            Invoke(nameof(EnableCollisions), Convert.ToSingle(_timelineSpawn.duration));
        }

        private void EnableCollisions()
        {
            Physics.IgnoreLayerCollision(_thisLayer, _playerLayer, false);
            m_isActive = true;
        }

        private void Disable() => gameObject.SetActive(false);

        private void DisableCollisions()
        {
            Physics.IgnoreLayerCollision(_thisLayer, _playerLayer, true);
            m_isActive = false;
        }

        public void Disappear(Vector3 vfxPosition, Vector3 vfxRotation, bool spawnGoalVfx = true)
        {
            Rpc_DisappearEffect(vfxPosition, vfxRotation, spawnGoalVfx);
            DisableCollisions();
            Invoke(nameof(Disable), Convert.ToSingle(_timelineGoal.duration));
        }

        public void Disappear()
        {
            Rpc_DisappearEffect(Vector3.zero, Vector3.zero, false);
            DisableCollisions();
            Invoke(nameof(Disable), Convert.ToSingle(_timelineGoal.duration));
        }

        public void Freeze()
        {
            rigidBody.velocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;
            rigidBody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationZ;
        }

        #endregion


        #region Rpc

        [ClientRpc]
        private void Rpc_DisappearEffect(Vector3 vfxPosition, Vector3 vfxRotation, bool spawnGoalVfx)
        {
            _respawnGo.SetActive(false);
            _goalGo.SetActive(true);

            _timelineGoal.Play();

            if (spawnGoalVfx) Rpc_SpawnGoalVfx(vfxPosition, vfxRotation);
        }

        [ClientRpc]
        private void Rpc_AppearEffect()
        {
            _respawnGo.SetActive(true);
            _goalGo.SetActive(false);

            _timelineSpawn.Play();
        }

        [ClientRpc]
        private void Rpc_SpawnGoalVfx(Vector3 vfxPosition, Vector3 vfxRotation)
        {
            VisualEffect vfx = Instantiate(_goalVfxPrefab, vfxPosition, Quaternion.Euler(vfxRotation));
            vfx.SendEvent("OnStart");

            Destroy(vfx.gameObject, _vfxGoalDuration);
        }

        #endregion


        #region Private

        private PhysicMaterial _pmat;

        #endregion
    }
}