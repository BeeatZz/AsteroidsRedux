using System;
using UnityEngine;

namespace Asteroids.Events
{
    [CreateAssetMenu(fileName = "NewVector3EventChannel", menuName = "Asteroids/Events/Vector3 Event Channel")]
    public class Vector3EventChannelSO : ScriptableObject
    {
        public event Action<Vector3> OnEventRaised;

        public void RaiseEvent(Vector3 location)
        {
            OnEventRaised?.Invoke(location);
        }
    }
}