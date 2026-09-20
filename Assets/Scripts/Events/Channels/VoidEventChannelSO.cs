using System;
using UnityEngine;

namespace Asteroids.Events
{
    [CreateAssetMenu(fileName = "NewVoidEventChannel", menuName = "Asteroids/Events/Void Event Channel")]
    public class VoidEventChannelSO : ScriptableObject
    {
        public event Action OnEventRaised;

        public void RaiseEvent()
        {
            OnEventRaised?.Invoke();
        }
    }
}