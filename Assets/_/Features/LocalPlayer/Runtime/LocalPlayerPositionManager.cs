using System.Linq;
using UnityEngine;

namespace LocalPlayer.Runtime
{
    public class LocalPlayerPositionManager : MonoBehaviour
    {
        #region Exposed

        public static LocalPlayerPositionManager s_instance;

        [SerializeField] private PlayerPositionData[] _positionsData;
        [SerializeField] private PlayerPositionData[] _garagePositionsData;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        #endregion


        #region Main

        private void Setup()
        {
            if (s_instance != null)
            {
                Destroy(LocalPlayerPositionManager.s_instance.gameObject);
            }

            LocalPlayerPositionManager.s_instance = this;

            LocalPlayerManager.s_instance.OnPlayerPositionsSceneChanged();
        }

        public Transform[] GetPositionFor(int playerCount, bool inGarage)
        {
            PlayerPositionData[] positions = inGarage ? _garagePositionsData : _positionsData;

            foreach (var item in _garagePositionsData)
            {
                item.m_go.SetActive(false);
            }

            foreach (var item in _positionsData)
            {
                item.m_go.SetActive(false);
            }

            PlayerPositionData posData = positions.FirstOrDefault(x => x.m_playerCount == playerCount);
            posData.m_go.SetActive(true);

            return posData.m_positions;
        }

        #endregion
    }
}