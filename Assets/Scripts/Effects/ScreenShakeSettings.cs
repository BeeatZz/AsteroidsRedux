using System;
using UnityEngine;

namespace Asteroids.Effects
{
    // One screen shake, set per config (each asteroid size, UFO, ship) and sent to CameraShake
    // through a ScreenShakeEventChannelSO.
    [Serializable]
    public struct ScreenShakeSettings
    {
        [Tooltip("Largest camera offset at the start, in world units. 0 turns the shake off.")]
        [Min(0f)] public float strength;
        [Tooltip("Game seconds the shake lasts. It eases out over this time.")]
        [Min(0f)] public float duration;
        [Tooltip("How fast the camera jitters, in shakes per second. Higher feels more violent.")]
        [Min(0f)] public float frequency;

        public ScreenShakeSettings(float strength, float duration, float frequency)
        {
            this.strength = strength;
            this.duration = duration;
            this.frequency = frequency;
        }

        public bool IsActive => strength > 0f && duration > 0f;
    }
}
