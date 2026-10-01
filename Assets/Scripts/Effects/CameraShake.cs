using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Asteroids.Events;
using Asteroids.Managers;

namespace Asteroids.Effects
{
    /* Shakes the camera when a shake is sent through the channel. Overlapping shakes add up
     * (capped by Max Offset), each easing out over its own duration.
     * Runs on game time, so it slows with SlowMotionEffect and freezes on the pause menu. Game over
     * also stops time, so a shake then finishes on real time instead of hanging.
     * The offset is only applied while this camera renders, so screen wrapping and spawning,
     * which read the camera during the frame, always see it at rest.
     */
    [RequireComponent(typeof(Camera))]
    public class CameraShake : MonoBehaviour
    {
        private class ActiveShake
        {
            public ScreenShakeSettings Settings;
            public float Elapsed;
            public float SeedX;
            public float SeedY;
        }

        [Header("Trigger")]
        [SerializeField] private ScreenShakeEventChannelSO shakeChannel;

        [Header("Limits")]
        [Tooltip("Scales every shake. 0 turns screen shake off.")]
        [Min(0f)]
        [SerializeField] private float intensity = 1f;
        [Tooltip("Caps the combined offset when several shakes overlap, in world units.")]
        [Min(0f)]
        [SerializeField] private float maxOffset = 0.6f;

        private readonly List<ActiveShake> shakes = new();
        private Camera shakeCamera;
        private Vector3 currentOffset;
        private Vector3 appliedOffset;

        private void Awake()
        {
            shakeCamera = GetComponent<Camera>();
        }

        private void OnEnable()
        {
            if (shakeChannel != null)
                shakeChannel.OnEventRaised += HandleShake;

            RenderPipelineManager.beginCameraRendering += HandleBeginCameraRendering;
            RenderPipelineManager.endCameraRendering += HandleEndCameraRendering;
        }

        private void OnDisable()
        {
            if (shakeChannel != null)
                shakeChannel.OnEventRaised -= HandleShake;

            RenderPipelineManager.beginCameraRendering -= HandleBeginCameraRendering;
            RenderPipelineManager.endCameraRendering -= HandleEndCameraRendering;

            RemoveOffset();
            shakes.Clear();
            currentOffset = Vector3.zero;
        }

        private void HandleShake(ScreenShakeSettings settings)
        {
            if (!settings.IsActive || intensity <= 0f) return;

            // Random seeds so shakes that start together don't move in lockstep.
            shakes.Add(new ActiveShake
            {
                Settings = settings,
                SeedX = Random.Range(0f, 1000f),
                SeedY = Random.Range(0f, 1000f)
            });
        }

        private void Update()
        {
            if (shakes.Count == 0)
            {
                currentOffset = Vector3.zero;
                return;
            }

            float step = GetTimeStep();
            Vector2 offset = Vector2.zero;

            for (int i = shakes.Count - 1; i >= 0; i--)
            {
                ActiveShake shake = shakes[i];
                shake.Elapsed += step;

                float progress = shake.Elapsed / shake.Settings.duration;
                if (progress >= 1f)
                {
                    shakes.RemoveAt(i);
                    continue;
                }

                // Perlin noise gives a jitter that wanders smoothly instead of teleporting each frame.
                float noiseTime = shake.Elapsed * shake.Settings.frequency;
                float x = Mathf.PerlinNoise(shake.SeedX, noiseTime) * 2f - 1f;
                float y = Mathf.PerlinNoise(shake.SeedY, noiseTime) * 2f - 1f;

                float falloff = (1f - progress) * (1f - progress);
                offset += new Vector2(x, y) * (shake.Settings.strength * falloff);
            }

            currentOffset = Vector2.ClampMagnitude(offset * intensity, maxOffset);
        }

        private void HandleBeginCameraRendering(ScriptableRenderContext context, Camera renderingCamera)
        {
            if (renderingCamera != shakeCamera || currentOffset == Vector3.zero) return;

            appliedOffset = currentOffset;
            transform.localPosition += appliedOffset;
        }

        private void HandleEndCameraRendering(ScriptableRenderContext context, Camera renderingCamera)
        {
            if (renderingCamera != shakeCamera) return;
            RemoveOffset();
        }

        private void RemoveOffset()
        {
            if (appliedOffset == Vector3.zero) return;

            transform.localPosition -= appliedOffset;
            appliedOffset = Vector3.zero;
        }

        private static float GetTimeStep()
        {
            // Same rule as ShockwaveEffect: time stopped by game over (not the pause menu) runs on real time.
            bool paused = PauseManager.Instance != null && PauseManager.Instance.IsPaused;
            bool frozenByGameOver = Time.timeScale <= 0f && !paused;
            return frozenByGameOver ? Time.unscaledDeltaTime : Time.deltaTime;
        }
    }
}
