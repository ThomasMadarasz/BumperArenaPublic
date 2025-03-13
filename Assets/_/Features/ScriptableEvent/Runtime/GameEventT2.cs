using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableEvent.Runtime
{
    [CreateAssetMenu(fileName = "GameEventT", menuName = "ScriptableArchitecture/GameEventWith2Parameters", order = 2)]
    public class GameEventT2 : ScriptableObject
    {
        private List<Action<object, object>> listeners = new();

        public void Raise(object owner, object other)
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
                listeners[i]?.Invoke(owner, other);
        }

        public void RegisterListener(Action<object, object> listener)
        { listeners.Add(listener); }

        public void UnregisterListener(Action<object, object> listener)
        { listeners.Remove(listener); }
    }
}