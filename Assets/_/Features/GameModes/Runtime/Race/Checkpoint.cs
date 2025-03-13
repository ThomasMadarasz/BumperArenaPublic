using UnityEngine;
using Archi.Runtime;
using Mirror;
using Interfaces.Runtime;
using UnityEngine.VFX;
using Core.Runtime;
using System.Linq;
using System.Collections.Generic;

namespace GameModes.Runtime
{
    [RequireComponent(typeof(Collider))]
    public class Checkpoint : CNetBehaviour
    {
        #region Exposed

        [SerializeField] private VisualEffect[] _vfx;
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Color _deactivateColor;
        
        [SerializeField] private List<Transform> _crossingPointsTransforms;

        #endregion


        #region Unity API

        private void Awake()
        {
        	Setup();
            for (int i = 0; i < _renderer.materials.Length; i++)
            {
                if (_renderer.materials[i].name.Contains("mat_Checkpoint_Player"))
                {
                    MaterialPropertyBlock mpb = new();
                    _renderer.GetPropertyBlock(mpb, i);
                    mpb.SetColor("_PlayerColor", _deactivateColor);
                    _renderer.SetPropertyBlock(mpb, i);
                }
            }
            
        }

        [ServerCallback]
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            ITeamable teamable = other.GetComponent<ITeamable>();
            if (teamable == null) return;
            _raceManager.ValidateCheckpoint(teamable, this);
        }

        #endregion


        #region Main

        private void Setup()
        {
            List<float> distances  = new List<float>();
            for (int i = 0; i < _crossingPointsTransforms.Count; i++)
            {
                Vector3 nextPoint = i + 1 < _crossingPointsTransforms.Count ? _crossingPointsTransforms[i + 1].position : transform.position;
                distances.Add(Vector3.Distance(_crossingPointsTransforms[i].position, nextPoint));
            }
            _crossingPoints = _crossingPointsTransforms.Zip(distances, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);
        }

        public void RegisterManager(RaceManager raceManager) => _raceManager = raceManager;

        public Dictionary<Transform, float> GetCrossingPoints() => _crossingPoints;

        public void EnableCheckpointFeedbacks(ITeamable teamable, bool enable)
        {
            //Enable/Disable Visual Feedbacks

            int playerId = teamable.GetUniqueID();
            //Material playerMaterial = GameManager.s_instance.GetForcedPlayerMaterialWithUniqueID(teamable.GetUniqueID());
            int playerMaterialId = GameManager.s_instance.GetForcedPlayerMateriaIDlWithUniqueID(teamable.GetUniqueID());
            int materialId = GetMaterialIndexForPlayer(playerId);

            Material raceMaterial = _raceManager.GetMaterialForPlayer(playerMaterialId);
            Color color = enable ? raceMaterial.GetColor("_PlayerColor") : _deactivateColor;

            MaterialPropertyBlock mpb = new();
            _renderer.GetPropertyBlock(mpb, materialId);
            mpb.SetColor("_PlayerColor", color);
            _renderer.SetPropertyBlock(mpb, materialId);

            _vfx[playerId].SetVector4("PlayerColor", color);
            if (enable) _vfx[playerId].SendEvent("OnCheckPoint");
        }

        public void DisplayFullTurnVFX(ITeamable teamable)
        {
            int playerId = teamable.GetUniqueID();
            _vfx[playerId].SendEvent("OnFullTurn");
        }

        private int GetMaterialIndexForPlayer(int playerId)
        {
            int currentId = 0;
            int materialId = 0;

            for (int i = 0; i < _renderer.materials.Length; i++)
            {
                if (_renderer.materials[i].name.Contains("mat_Checkpoint_Player"))
                {
                    if (currentId == playerId)
                    {
                        materialId = i;
                        break;
                    }
                    else currentId++;
                }
            }

            return materialId;
        }


        #endregion


        #region Private

        private RaceManager _raceManager;
        private Dictionary<Transform, float> _crossingPoints = new Dictionary<Transform, float>();

        #endregion
    }
}