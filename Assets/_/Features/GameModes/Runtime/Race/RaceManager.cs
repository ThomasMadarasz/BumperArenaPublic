using System.Collections.Generic;
using UnityEngine;
using Interfaces.Runtime;
using System.Linq;

namespace GameModes.Runtime
{
    public class RaceManager : GameMode
    {
        #region Exposed

        [SerializeField] private RaceData _data;
        [SerializeField] private MaterialGameModeData[] _gameModeMaterials;

        #endregion


        #region Unity API

        public override void OnStartServer()
        {
            base.OnStartServer();
            Setup();
        }

        #endregion


        #region Main

        private void Setup()
        {
            _checkpoints = FindObjectsOfType<Checkpoint>();
#if UNITY_EDITOR
            if (_checkpoints.Length != 2)
            {
                Debug.LogError("Il doit y avoir 2 checkpoints dans le ld. Ni plus ni moins.");
                UnityEditor.EditorApplication.isPlaying = false;
            }
#endif
            foreach (Checkpoint checkpoint in _checkpoints) checkpoint.RegisterManager(this);
        }

        public void ValidateCheckpoint(ITeamable teamable, Checkpoint checkpoint)
        {
            if (_checkpointsState.Keys.FirstOrDefault(x => x == teamable) == null) _checkpointsState.Add(teamable, new List<Checkpoint>());
            if (_checkpointsState[teamable].Contains(checkpoint)) return;
            _checkpointsState[teamable].Add(checkpoint);
            checkpoint.EnableCheckpointFeedbacks(teamable, true);

            Debug.Log($"Joueur {teamable.GetPlayerID()} a passer le checkpoint {checkpoint.gameObject.name}");

            if (_checkpointsState[teamable].Count >= _checkpoints.Length)
            {
                _scoreManager.ScorePoint(teamable);
                if (_scoreManager.GetTeamableScore(teamable) >= _data.m_scoreToReachToWinRound) _gameManager.FinishRound();

                foreach (Checkpoint cp in _checkpointsState[teamable]) if (cp != checkpoint) cp.EnableCheckpointFeedbacks(teamable, false);
                _checkpointsState[teamable].Clear();
                _checkpointsState[teamable].Add(checkpoint);

                checkpoint.DisplayFullTurnVFX(teamable);
            }
        }

        public Material GetMaterialForPlayer(int id) => _gameModeMaterials.FirstOrDefault(x => x.m_materialIndex == id).m_material;

        public override Transform[] GetObjectivesTransforms()
        {
            Transform[] transforms = new Transform[_checkpoints.Length];
            for (int i = 0; i < _checkpoints.Length; i++)
            {
                transforms[i] = _checkpoints[i].transform;
            }
            return transforms;
        }

        public override Dictionary<ITeamable, List<Checkpoint>> GetCheckpointsState() => _checkpointsState;

        public override List<Checkpoint> GetCheckpoints() => _checkpoints.ToList();

        #endregion


        #region Private

        private Checkpoint[] _checkpoints;
        private Dictionary<ITeamable, List<Checkpoint>> _checkpointsState = new Dictionary<ITeamable, List<Checkpoint>>();

        #endregion
    }
}