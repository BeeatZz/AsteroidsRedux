using System;
using UnityEngine;

namespace Asteroids.Events
{
    [CreateAssetMenu(fileName = "NewIntEventChannel", menuName = "Asteroids/Events/Int Event Channel")]
    public class IntEventChannelSO : ScriptableObject
    {
        public event Action<int> OnEventRaised;

        public void RaiseEvent(int value)
        {
            OnEventRaised?.Invoke(value);
        }
    }
}