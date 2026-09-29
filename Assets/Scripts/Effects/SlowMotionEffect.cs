using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using Asteroids.Events;
using Asteroids.Managers;

namespace Asteroids.Effects
{
    /* Briefly slows the game down when a channel fires (e.g. the last enemy of a wave dying),
     * then eases back to full speed. Can also pitch mixer groups down with it, so the audio slows too.
     * Runs on unscaled time, holds while the game is paused, and gives up without touching
     * timeScale if the game ends, since game over freezes time itself.
     */
    public class SlowMotionEffect : MonoBehaviour
    {
        [Header("Trigger")]
        [SerializeField] private Vector3EventChannelSO triggerChannel;
        [SerializeField] private VoidEventChannelSO onGameOverChannel;

        [Header("Timing (real seconds)")]
        [Tooltip("Game speed at the slowest point (1 = normal).")]
        [Range(0.05f, 1f)]
        [SerializeField] private float slowTimeScale = 0.2f;
        [Tooltip("How long the game stays at its slowest.")]
        [Min(0f)]
        [SerializeField] private float holdDuration = 0.35f;
        [Tooltip("How long it takes to ease back to normal speed.")]
        [Min(0f)]
        [SerializeField] private float recoverDuration = 0.6f;

        [Header("Audio")]
        [Tooltip("Optional. Leave empty to keep the audio at normal speed.")]
        [SerializeField] private AudioMixer mixer;
        [Tooltip("Exposed mixer parameters bound to a group's Pitch, e.g. SFXPitch (and MusicPitch to slow the music too).")]
        [SerializeField] private string[] pitchParameters = { "SFXPitch" };
        [Tooltip("Audio pitch/speed at the slowest point (1 = normal). Eases back alongside the game speed.")]
        [Range(0.1f, 1f)]
        [SerializeField] private float slowPitch = 0.6f;

        private float defaultFixedDeltaTime;
        private Coroutine slowRoutine;
        private bool warnedAboutMissingParameter;

        private void Awake()
        {
            defaultFixedDeltaTime = Time.fixedDeltaTime;
        }

        private void OnEnable()
        {
            if (triggerChannel != null)
                triggerChannel.OnEventRaised += HandleTriggered;

            if (onGameOverChannel != null)
                onGameOverChannel.OnEventRaised += HandleGameOver;
        }

        private void OnDisable()
        {
            if (triggerChannel != null)
                triggerChannel.OnEventRaised -= HandleTriggered;

            if (onGameOverChannel != null)
                onGameOverChannel.OnEventRaised -= HandleGameOver;

            // fixedDeltaTime and the mixer outlive the scene, so a restart mid-slowdown must not inherit them.
            StopSlowdown();
        }

        private void HandleTriggered(Vector3 position)
        {
            if (slowRoutine != null) StopCoroutine(slowRoutine);
            slowRoutine = StartCoroutine(SlowdownRoutine());
        }

        private void HandleGameOver()
        {
            StopSlowdown();
        }

        private IEnumerator SlowdownRoutine()
        {
            float elapsed = 0f;
            float total = holdDuration + recoverDuration;

            while (elapsed < total)
            {
                if (IsPaused())
                {
                    // The pause menu owns timeScale while it's open, and its sounds (UI sits under SFX)
                    // shouldn't come out slowed; pick up where we left off after.
                    SetPitch(1f);
                }
                else
                {
                    float recoverProgress = recoverDuration > 0f
                        ? Mathf.Clamp01((elapsed - holdDuration) / recoverDuration)
                        : 1f;
                    float easedProgress = recoverProgress * recoverProgress * (3f - 2f * recoverProgress);

                    ApplySlowdown(easedProgress);
                    elapsed += Time.unscaledDeltaTime;
                }

                yield return null;
            }

            ApplySlowdown(1f);
            slowRoutine = null;
        }

        private void StopSlowdown()
        {
            if (slowRoutine != null)
            {
                StopCoroutine(slowRoutine);
                slowRoutine = null;
            }

            Time.fixedDeltaTime = defaultFixedDeltaTime;
            SetPitch(1f);
        }

        // 0 = slowest, 1 = back to normal. Physics steps shrink with time so movement stays
        // smooth instead of stepping visibly.
        private void ApplySlowdown(float progress)
        {
            float scale = Mathf.Lerp(slowTimeScale, 1f, progress);
            Time.timeScale = scale;
            Time.fixedDeltaTime = defaultFixedDeltaTime * scale;

            SetPitch(Mathf.Lerp(slowPitch, 1f, progress));
        }

        private void SetPitch(float pitch)
        {
            if (mixer == null) return;

            foreach (string parameter in pitchParameters)
            {
                if (!mixer.SetFloat(parameter, pitch) && !warnedAboutMissingParameter)
                {
                    warnedAboutMissingParameter = true;
                    Debug.LogWarning($"AudioMixer '{mixer.name}' has no exposed parameter '{parameter}'.", this);
                }
            }
        }

        private static bool IsPaused()
        {
            return PauseManager.Instance != null && PauseManager.Instance.IsPaused;
        }
    }
}
