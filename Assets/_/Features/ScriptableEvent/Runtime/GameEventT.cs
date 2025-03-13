using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableEvent.Runtime
{
    [CreateAssetMenu(fileName = "GameEventT", menuName = "ScriptableArchitecture/GameEventWithParameter", order = 1)]
    public class GameEventT : ScriptableObject
    {
        private List<Action<object>> listeners = new();

        public void Raise(object owner)
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
                listeners[i]?.Invoke(owner);
        }

        public void RegisterListener(Action<object> listener)
        { listeners.Add(listener); }

        public void UnregisterListener(Action<object> listener)
        { listeners.Remove(listener); }
    }
}