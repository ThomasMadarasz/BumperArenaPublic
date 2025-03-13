using Archi.Runtime;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UINavigation.Runtime
{
    public class UINavigationManager : CBehaviour
    {
        #region Exposed

        public static UINavigationManager s_instance;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        #endregion


        #region Main

        private void Setup()
        {
            s_instance = this;
        }

        public void EnableMenu(MenuBase menu)
        {
            if (_menus.ContainsKey(menu)) _menus[menu] = true;
            else _menus.Add(menu, true);
        }

        public void DisableMenu(MenuBase menu)
        {
            if (_menus.ContainsKey(menu)) _menus[menu] = false;
            else Debug.LogError("Menu not exist in Dictionary!");
        }

        public void OnMenu_Performed(int playerId) => GetActiveMenus().ForEach(x => x.OnMenu_Performed(playerId));

        public void OnCancel_Performed(int playerId) => GetActiveMenus().ForEach(x => x.OnCancel_Performed(playerId));

        public void OnSubmit_Performed(int playerId) => GetActiveMenus().ForEach(x => x.OnSubmit_Performed(playerId));
        public void OnSubmit_Started(int playerId) => GetActiveMenus().ForEach(x => x.OnSubmit_Started(playerId));
        public void OnSubmit_Canceled(int playerId) => GetActiveMenus().ForEach(x => x.OnSubmit_Canceled(playerId));

        public void OnNavigate_Performed(int playerId, Vector2 value) => GetActiveMenus().ForEach(x => x.OnNavigate_Performed(playerId, value));

        public void OnNorthButton_Performed(int playerId) => GetActiveMenus().ForEach(x => x.OnNorthButton_Performed(playerId));

        public void OnWestButton_Performed(int playerId) => GetActiveMenus().ForEach(x => x.OnWestButton_Performed(playerId));

        public void OnRightShoulder_Performed(int playerId) => GetActiveMenus().ForEach(x => x.OnRightShoulder_Performed(playerId));

        public void OnLeftShoulder_Performed(int playerId) => GetActiveMenus().ForEach(x => x.OnLeftShoulder_Performed(playerId));

        public void OnRightTrigger_Performed(int playerId, float value) => GetActiveMenus().ForEach(x => x.OnRightTrigger_Performed(playerId, value));

        public void OnLeftTrigger_Performed(int playerId, float value) => GetActiveMenus().ForEach(x => x.OnLeftTrigger_Performed(playerId, value));

        #endregion


        #region Utils & Tools

        private List<MenuBase> GetActiveMenus() => _menus.Where(x => x.Value).Select(x => x.Key).ToList();

        public int GetActiveMenuCount() => GetActiveMenus().Count;

        #endregion


        #region Private

        private Dictionary<MenuBase, bool> _menus = new();

        #endregion
    }
}