using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableEvent.Runtime
{
    [CreateAssetMenu(fileName = "GameEvent", menuName = "ScriptableArchitecture/GameEvent", order = 0)]
    public class GameEvent : ScriptableObject
    {
        private List<Action> listeners = new();

        [Button]
        public void Raise()
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
                listeners[i]?.Invoke();
        }

        public void RegisterListener(Action listener)
        { listeners.Add(listener); }

        public void UnregisterListener(Action listener)
        { listeners.Remove(listener); }
    }
}