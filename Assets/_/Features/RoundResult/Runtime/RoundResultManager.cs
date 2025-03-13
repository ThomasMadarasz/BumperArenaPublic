using Archi.Runtime;
using Cinemachine;
using Interfaces.Runtime;
using ScriptableEvent.Runtime;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.VFX;
using Utils.Runtime;
using Utils.Translation;

namespace RoundResult.Runtime
{
    public class RoundResultManager : CBehaviour
    {
        #region Exposed

        public static RoundResultManager s_instance;

        [SerializeField][BoxGroup("Config")] private GameEventT _onRoundResultSceneFinished;
        [SerializeField][BoxGroup("Config")] private float _timeBeforeNextRound;
        [SerializeField][BoxGroup("Config")] private float _timeBeforeBackToMainMenu;

        [SerializeField][BoxGroup("Scene")] private GameObject _scoreScene;
        [SerializeField][BoxGroup("Scene")] private GameObject _winnerScene;

        [SerializeField][BoxGroup("Timeline")] private PlayableDirector[] _timelines;
        [SerializeField][BoxGroup("Timeline")] private PlayableDirector _duoTimeline;

        [SerializeField][BoxGroup("Positions")] private PlayerPosition[] _soloPositions;
        [SerializeField][BoxGroup("Positions")] private PlayerPosition[] _duoPositions;
        [SerializeField][BoxGroup("Positions")] private GameObject _soloPosGO;
        [SerializeField][BoxGroup("Positions")] private GameObject _duoPosGO;

        [SerializeField][BoxGroup("Camera")] private CinemachinePathBase[] _pathForPlayers;
        [SerializeField][BoxGroup("Camera")] private CinemachinePathBase _pathForDuo;
        [SerializeField][BoxGroup("Camera")] private CinemachineVirtualCamera _vcCam;

        [SerializeField][BoxGroup("Scoreboard")] private PlayerScoreboard[] _playerScores;

        [SerializeField][BoxGroup("LooseAnims")] private List<AnimationClip> _looseAnims;

        [SerializeField][BoxGroup("Winner of the game")] private Transform _winnerOfTheGameParent;
        [SerializeField][BoxGroup("Winner of the game")] private Animator _winnerOfTheGameAnimator;
        [SerializeField][BoxGroup("Winner of the game")] private PlayableDirector _winnerOfTheGameTimeline;
        [SerializeField][BoxGroup("Winner of the game")] private Renderer _groundLightEmissive;
        [SerializeField][BoxGroup("Winner of the game")] private Renderer _groundLightNormal;
        [SerializeField][BoxGroup("Winner of the game")] private float _groundLightEmissiveIntensity;
        [SerializeField][BoxGroup("Winner of the game")] private float _fireworktEmissiveIntensity;
        [SerializeField][BoxGroup("Winner of the game")] private VisualEffect[] _fireworks;

        [SerializeField] private GameEvent _onStartJingle;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        #endregion


        #region Main

        private void Setup()
        {
            s_instance = this;
            _winPointAnimation = Animator.StringToHash("anim_PointWin");
            _noWinPointAnimation = Animator.StringToHash("anim_PointHold");
            _scorePointColorShader = Shader.PropertyToID("_PlayerColor");
            _winnerOfTheGameAnimation = Animator.StringToHash("anim_WinnerOfTheGame");
            _nextRoundTimer = new(_timeBeforeNextRound, OnNextRoundTimerOver);
            _backToMainMenuTimer = new(_timeBeforeBackToMainMenu, OnBackToMainMenuTimerOver);
        }

        public void SetupManager(List<GameObject> players, bool hasAGameWinner, int winnerTeamId, bool isDuoMode, List<ITeamable> teams, List<Color> colors, List<Color> emissiveColors, List<Color> scoreboardColors, Dictionary<int, int> scores)
        {
            players = players.OrderBy(x => x.GetComponent<ITeamable>().GetPlayerID()).ToList();

            _players = players;
            _isDuoMode = isDuoMode;
            _winnerTeamID = winnerTeamId;
            _teams = teams;
            _scores = scores;
            _colors = colors;
            _emissiveColors = emissiveColors;
            _scoreboardColors = scoreboardColors;

            _graphics.Clear();

            for (int i = 0; i < players.Count; i++)
            {
                _graphics.Add(_teams[i], players[i].GetComponent<IPlayerGraphic>());
            }

            foreach (IPlayerGraphic graphic in _graphics.Values)
            {
                graphic.SwitchLowToHighPolyMesh();
                if (_isDuoMode)
                {
                    graphic.ApplyDefaultMaterial();
                }
            }

            _onStartJingle.Raise();

            if (hasAGameWinner)
            {
                Debug.Log($"Team {winnerTeamId} has won the game");
                SetupWinnerScene();
            }
            else
            {
                Debug.Log($"Team {winnerTeamId} has won the round");
                SetupScoreScene();
            }
        }

        private void SetupScoreScene()
        {
            _scoreScene.SetActive(true);

            for (int i = 0; i < _colors.Count; i++)
            {
                string playerNumberKey = _teams[i].IsAnAI() ? "PlayerNumberKey_AI" : "PlayerNumberKey";
                string playerNumberTxt = TranslationManager.Translate(playerNumberKey);

                int playerIndex = _teams[i].GetPlayerID() + 1;

                _playerScores[i].m_playerNumber.text = $"{playerNumberTxt}{playerIndex}";
                _playerScores[i].m_playerNumber.color = _emissiveColors[i];

                //Renderer renderer = _playerScores[i].m_animators[i].gameObject.GetComponent<Renderer>();
                //MaterialPropertyBlock block = new();
                //renderer.GetPropertyBlock(block);

                //block.SetColor(_scorePointColorShader, _scoreboardColors[i]);

                foreach (var anim in _playerScores[i].m_animators)
                {
                    Renderer r = anim.gameObject.GetComponent<Renderer>();
                    //r.SetPropertyBlock(block);
                    r.material.SetColor(_scorePointColorShader, _scoreboardColors[i]);
                }
            }

            for (int i = 0; i < _teams.Count; i++)
            {
                int id = _teams[i].GetUniqueID();
                int score = _scores[id];

                PlayerScoreboard scoreboard = _playerScores[i];

                for (int j = 0; j < score; j++)
                {
                    scoreboard.m_animators[j].gameObject.SetActive(true);
                    scoreboard.m_animators[j].Play(_noWinPointAnimation);                    
                }
            }

            SetPlayerPositionInScoreScene();
            SetupCameraInScoreScene();
            PlayTimelineInScoreScene();
            SetPlayersAnimations();
        }

        private void SetupWinnerScene()
        {
            _winnerScene.SetActive(true);

            // Set player position
            ITeamable winner = _teams.FirstOrDefault(x => x.GetTeamID() == _winnerTeamID);

            int index = _teams.IndexOf(winner);
            _players[index].transform.SetParent(_winnerOfTheGameParent);
            _players[index].transform.position = _winnerOfTheGameParent.position;
            _players[index].transform.rotation = _winnerOfTheGameParent.rotation;

            //Play animation (player movement)
            _winnerOfTheGameAnimator.Play(_winnerOfTheGameAnimation);

            //Play timeline
            _winnerOfTheGameTimeline.Play();
            StartTimer(_winnerOfTheGameTimeline.duration, _backToMainMenuTimer);

            //Play victory animation
            _graphics[winner].GetMenuAnimator().Play(_graphics[winner].GetMenuWinAnimStateName());

            //Get winner color
            Color winnerColor = _colors[index];
            Color winnerEmissiveColor = _emissiveColors[index];

            //Update ground light color
            MaterialPropertyBlock normalBlock = new();
            _groundLightNormal.GetPropertyBlock(normalBlock);
            normalBlock.SetColor("_BaseColor", winnerColor);
            _groundLightNormal.SetPropertyBlock(normalBlock);

            //Update ground light emissive color
            MaterialPropertyBlock emissiveBlock = new();
            _groundLightEmissive.GetPropertyBlock(emissiveBlock);

            emissiveBlock.SetColor("_EmissionColor", winnerEmissiveColor);

            _groundLightEmissive.SetPropertyBlock(emissiveBlock);

            Vector4 fireworkColor = winnerColor * _fireworktEmissiveIntensity;

            foreach (var item in _fireworks)
            {
                item.SetVector4("PlayerColor", fireworkColor);
            }
        }

        #endregion


        #region Utils & Tools

        private void SetPlayerPositionInScoreScene()
        {
            PlayerPosition[] pos;

            if (_isDuoMode)
            {
                _duoPosGO.SetActive(true);
                pos = _duoPositions;
            }
            else
            {
                _soloPosGO.SetActive(true);
                pos = _soloPositions;
            }

            for (int i = 0; i < _players.Count; i++)
            {
                Transform newPos;

                if (_teams[i].GetTeamID() == _winnerTeamID) newPos = pos[i].m_winPos;
                else newPos = pos[i].m_losePos;

                newPos.gameObject.SetActive(true);

                _players[i].transform.position = newPos.position;
                _players[i].transform.rotation = newPos.rotation;
            }
        }

        private void SetupCameraInScoreScene()
        {
            CinemachineTrackedDolly dolly = _vcCam.GetCinemachineComponent<CinemachineTrackedDolly>();
            CinemachinePathBase path;

            if (_isDuoMode) path = _pathForDuo;
            else
            {
                int index = _teams.IndexOf(_teams.FirstOrDefault(x => x.GetTeamID() == _winnerTeamID));
                path = _pathForPlayers[index];
            }

            dolly.m_Path = path;
        }

        private void PlayTimelineInScoreScene()
        {
            PlayableDirector timeline;

            if (_isDuoMode) timeline = _duoTimeline;
            else
            {
                int index = _teams.IndexOf(_teams.FirstOrDefault(x => x.GetTeamID() == _winnerTeamID));
                timeline = _timelines[index];
            }

            timeline.Play();
            StartTimer(timeline.duration, _nextRoundTimer);
        }

        private async void StartTimer(double duration, Timer timer)
        {
            int delay = Mathf.RoundToInt((float)duration * 1000);
            await Task.Delay(delay);
            timer.Start();
        }

        private void OnNextRoundTimerOver()
        {
            _onRoundResultSceneFinished?.Raise(false);
        }

        private void OnBackToMainMenuTimerOver()
        {
            _onRoundResultSceneFinished?.Raise(true);
        }

        private void SetPlayersAnimations()
        {
            List<AnimationClip> anims = _looseAnims;
            foreach (ITeamable teamable in _graphics.Keys)
            {
                if (teamable.GetTeamID() == _winnerTeamID) _graphics[teamable].GetMenuAnimator().Play(_graphics[teamable].GetMenuWinAnimStateName());
                else
                {
                    AnimationClip anim = anims.GetRandom();
                    anims.Remove(anim);
                    _graphics[teamable].GetMenuAnimator().Play(anim.name);
                }
            }
        }

        #endregion


        #region Animation

        public void PlayFFAScoreAnimation(int playerIndex)
        {
            PlayerScoreboard score = _playerScores[playerIndex];

            int index = -1;
            for (int i = 0; i < score.m_animators.Length; i++)
            {
                if (score.m_animators[i].gameObject.activeInHierarchy) index++;
                else break;
            }

            score.m_animators[index].Play(_winPointAnimation);
        }

        public void PlayDuoScoreAnimation()
        {
            for (int i = 0; i < _teams.Count; i++)
            {
                if (_teams[i].GetTeamID() == _winnerTeamID)
                {
                    PlayFFAScoreAnimation(i);
                }
            }
        }

        #endregion


        #region Private

        private List<GameObject> _players;
        private List<ITeamable> _teams;
        private List<Color> _colors;
        private List<Color> _emissiveColors;
        private List<Color> _scoreboardColors;

        private Dictionary<int, int> _scores;
        private Dictionary<ITeamable, IPlayerGraphic> _graphics = new Dictionary<ITeamable, IPlayerGraphic>();

        private Timer _nextRoundTimer;
        private Timer _backToMainMenuTimer;

        private int _winnerTeamID;
        private int _winPointAnimation;
        private int _noWinPointAnimation;
        private int _scorePointColorShader;
        private int _winnerOfTheGameAnimation;

        private bool _isDuoMode;

        #endregion
    }
}