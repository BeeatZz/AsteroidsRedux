using System.Collections;
using UnityEngine;
using Asteroids.Events;
using Asteroids.Managers;

namespace Asteroids.Effects
{
    /* Sends a ring of screen distortion out from a world position when a channel fires.
     * Drives the global values read by the Shockwave shader, which runs as a Full Screen Pass
     * renderer feature on the 2D Renderer. By default it runs on game time, so it slows down
     * with SlowMotionEffect and freezes on the pause menu.
     */
    public class ShockwaveEffect : MonoBehaviour
    {
        private static readonly int CenterId = Shader.PropertyToID("_ShockwaveCenter");
        private static readonly int RadiusId = Shader.PropertyToID("_ShockwaveRadius");
        private static readonly int ThicknessId = Shader.PropertyToID("_ShockwaveThickness");
        private static readonly int StrengthId = Shader.PropertyToID("_ShockwaveStrength");

        [Header("Trigger")]
        [SerializeField] private Vector3EventChannelSO triggerChannel;

        [Header("Shape (fractions of the screen height)")]
        [Tooltip("How far the ring travels before it has fully faded.")]
        [Min(0f)]
        [SerializeField] private float maxRadius = 0.9f;
        [Tooltip("Width of the distorted band.")]
        [Min(0.001f)]
        [SerializeField] private float thickness = 0.08f;

        [Header("Strength")]
        [Tooltip("How far pixels are pushed at the start, as a fraction of the screen height.")]
        [Min(0f)]
        [SerializeField] private float strength = 0.03f;
        [Tooltip("Seconds for the ring to reach Max Radius (game time when Follow Game Speed is on).")]
        [Min(0.01f)]
        [SerializeField] private float duration = 0.8f;

        [Header("Timing")]
        [Tooltip("On: the ring slows down with slow motion and freezes while paused. Off: always full speed.")]
        [SerializeField] private bool followGameSpeed = true;

        private Camera mainCamera;
        private Coroutine waveRoutine;

        private void Awake()
        {
            mainCamera = Camera.main;
            ResetShaderValues();
        }

        private void OnEnable()
        {
            if (triggerChannel != null)
                triggerChannel.OnEventRaised += HandleTriggered;
        }

        private void OnDisable()
        {
            if (triggerChannel != null)
                triggerChannel.OnEventRaised -= HandleTriggered;

            if (waveRoutine != null)
            {
                StopCoroutine(waveRoutine);
                waveRoutine = null;
            }

            // Globals outlive the scene, so never leave the screen distorted.
            ResetShaderValues();
        }

        private void HandleTriggered(Vector3 worldPosition)
        {
            if (mainCamera == null) return;

            if (waveRoutine != null) StopCoroutine(waveRoutine);
            waveRoutine = StartCoroutine(WaveRoutine(mainCamera.WorldToViewportPoint(worldPosition)));
        }

        private IEnumerator WaveRoutine(Vector2 viewportCenter)
        {
            Shader.SetGlobalVector(CenterId, viewportCenter);
            Shader.SetGlobalFloat(ThicknessId, thickness);

            for (float elapsed = 0f; elapsed < duration; elapsed += GetTimeStep())
            {
                float progress = elapsed / duration;

                // Fast at first, then coasting; the ring weakens as it spreads.
                float easedRadius = 1f - (1f - progress) * (1f - progress);
                Shader.SetGlobalFloat(RadiusId, easedRadius * maxRadius);
                Shader.SetGlobalFloat(StrengthId, strength * (1f - progress));

                yield return null;
            }

            ResetShaderValues();
            waveRoutine = null;
        }

        private float GetTimeStep()
        {
            if (!followGameSpeed) return Time.unscaledDeltaTime;

            // Game over also stops time; let the ring finish rather than hang over the Game Over screen.
            bool frozenByGameOver = Time.timeScale <= 0f && !IsPaused();
            return frozenByGameOver ? Time.unscaledDeltaTime : Time.deltaTime;
        }

        private static bool IsPaused()
        {
            return PauseManager.Instance != null && PauseManager.Instance.IsPaused;
        }

        private static void ResetShaderValues()
        {
            Shader.SetGlobalFloat(StrengthId, 0f);
            Shader.SetGlobalFloat(RadiusId, 0f);
        }
    }
}
