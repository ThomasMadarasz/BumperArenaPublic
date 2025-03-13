using Data.Runtime;
using Sirenix.OdinInspector;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.Translation;

namespace Voting.UI.Runtime
{
    public class GameModeChoiceUI : MonoBehaviour
    {
        #region Exposed

        [SerializeField] private TextMeshProUGUI _modeNameTxt;

        [SerializeField][BoxGroup("Map 1")] private TextMeshProUGUI _mapName1Txt;
        [SerializeField][BoxGroup("Map 1")] private Image _mapImage1;

        [SerializeField][BoxGroup("Map 2")] private TextMeshProUGUI _mapName2Txt;
        [SerializeField][BoxGroup("Map 2")] private Image _mapImage2;

        #endregion


        #region Main

        public void Setup(GameModeData data, int[] maps)
        {
            _modeNameTxt.text = TranslationManager.Translate(data.m_name);

            MapData map1 = data.m_maps.FirstOrDefault(x => x.m_logicIndex == maps[0]);
            MapData map2 = data.m_maps.FirstOrDefault(x => x.m_logicIndex == maps[1]);

            _mapName1Txt.text = map1.m_displayName;
            _mapImage1.sprite = map1.m_votingImage;

            _mapName2Txt.text = map2.m_displayName;
            _mapImage2.sprite = map2.m_votingImage;
        }

        #endregion
    }
}