using UnityEngine;

namespace Utils.Runtime
{
    public class DontDestroyOnLoadScene : MonoBehaviour
    {
        #region Exposed

        [SerializeField] private GameObject[] _dontDestroy;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        #endregion


        #region Main

        private void Setup()
        {
            foreach (var item in _dontDestroy)
            {
                DontDestroyOnLoad(item);
            }            
        }

        #endregion
    }
}