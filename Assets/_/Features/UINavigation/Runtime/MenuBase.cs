using UnityEngine;

namespace UINavigation.Runtime
{
    public abstract class MenuBase : MonoBehaviour
    {
        protected virtual void OnEnable() => EnableInput();

        protected virtual void OnDisable() => DisableInput();

        public virtual void DisableInput() => UINavigationManager.s_instance.DisableMenu(this);
        public virtual void EnableInput() => UINavigationManager.s_instance.EnableMenu(this);

        public virtual void OnMenu_Performed(int playerId) { }
        public virtual void OnCancel_Performed(int playerId) { }
        public virtual void OnSubmit_Performed(int playerId) { }
        public virtual void OnSubmit_Started(int playerId) { }
        public virtual void OnSubmit_Canceled(int playerId) { }
        public virtual void OnNavigate_Performed(int playerId, Vector2 value)
        {
            if (value.x > .1f) NavigateRight_Performed(playerId);
            if (value.x < -.1f) NavigateLeft_Performed(playerId);
            if (value.y > .1f) NavigateUp_Performed(playerId);
            if (value.y < -.1f) NavigateDown_Performed(playerId);
        }

        protected virtual void NavigateUp_Performed(int playerId) { }
        protected virtual void NavigateDown_Performed(int playerId) { }
        protected virtual void NavigateLeft_Performed(int playerId) { }
        protected virtual void NavigateRight_Performed(int playerId) { }

        public virtual void OnNorthButton_Performed(int playerdId) { }
        public virtual void OnWestButton_Performed(int playerdId) { }

        public virtual void OnRightShoulder_Performed(int playerdId) { }
        public virtual void OnLeftShoulder_Performed(int playerdId) { }

        public virtual void OnRightTrigger_Performed(int playerdId, float value) { }
        public virtual void OnLeftTrigger_Performed(int playerdId, float value) { }
    }
}