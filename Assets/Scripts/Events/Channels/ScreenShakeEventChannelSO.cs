using System;
using UnityEngine;
using Asteroids.Effects;

namespace Asteroids.Events
{
    [CreateAssetMenu(fileName = "NewScreenShakeEventChannel", menuName = "Asteroids/Events/Screen Shake Event Channel")]
    public class ScreenShakeEventChannelSO : ScriptableObject
    {
        public event Action<ScreenShakeSettings> OnEventRaised;

        public void RaiseEvent(ScreenShakeSettings shake)
        {
            OnEventRaised?.Invoke(shake);
        }
    }
}
