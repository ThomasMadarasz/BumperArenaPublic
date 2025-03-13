using System;
using UnityEngine;

namespace Interfaces.Runtime
{
    public interface IInputsReader
    {
        public event Action m_onBoostPerformed;
        public event Action m_onMenuPerformed;

        public event Action<Vector2> m_onNavigatePerformed;
        public event Action m_onSubmitPerformed;
        public event Action m_onSubmitStarted;
        public event Action m_onSubmitCanceled;
        public event Action m_onCancelPerformed;

        public event Action m_onNorthButtonPerformed;
        public event Action m_onWestButtonPerformed;

        public event Action m_onRightShoulderPerformed;
        public event Action m_onLeftShoulderPerformed;

        public event Action<float> m_onRightTriggerPerformed;
        public event Action<float> m_onLeftTriggerPerformed;

        public void EnableActionMap(string mapName);

        public void DisableActionMap(string mapName);

        public void ActivateInput();
        public void DeactivateInput();

        public Vector2 GetMoveDirection();
    }
}