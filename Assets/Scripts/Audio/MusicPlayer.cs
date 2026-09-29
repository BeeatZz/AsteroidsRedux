using System;
using System.Collections;
using UnityEngine;
using Asteroids.Events;

namespace Asteroids.Audio
{
    /* Plays the gameplay track, fading it in after a short silence, and crossfades to an optional
     * pause track while the game is paused. Route this AudioSource to the mixer's Music group; the
     * pause track gets a second AudioSource on the same group, added automatically.
     * All timing is unscaled, because the pause fades run while Time.timeScale is 0.
     */
    [RequireComponent(typeof(AudioSource))]
    public class MusicPlayer : MonoBehaviour
    {
        [Header("Tracks")]
        [SerializeField] private AudioClip musicClip;
        [Tooltip("Optional. Plays on the pause menu in place of the gameplay track.")]
        [SerializeField] private AudioClip pauseClip;

        [Header("Start")]
        [Tooltip("Seconds of silence before the gameplay track starts.")]
        [Min(0f)]
        [SerializeField] private float startDelay = 1f;
        [Min(0f)]
        [SerializeField] private float fadeInDuration = 3f;

        [Header("Pause")]
        [Tooltip("Seconds to crossfade between the gameplay and pause tracks, both ways.")]
        [Min(0f)]
        [SerializeField] private float pauseFadeDuration = 0.75f;
        [Tooltip("Only used without a pause track: keep the gameplay track playing on the pause menu instead of fading it out.")]
        [SerializeField] private bool playWhilePaused = true;

        [Header("Event Channels")]
        [SerializeField] private VoidEventChannelSO onGamePausedChannel;
        [SerializeField] private VoidEventChannelSO onGameResumedChannel;

        private AudioSource gameplaySource;
        private AudioSource pauseSource;
        private Coroutine gameplayFade;
        private Coroutine pauseFade;
        private bool gameplayStarted;
        private bool isPaused;

        // Whether the gameplay track steps aside on the pause menu.
        private bool GameplayYieldsToPause => pauseClip != null || !playWhilePaused;

        private void Awake()
        {
            gameplaySource = GetComponent<AudioSource>();
            Configure(gameplaySource, musicClip);

            pauseSource = gameObject.AddComponent<AudioSource>();
            pauseSource.outputAudioMixerGroup = gameplaySource.outputAudioMixerGroup;
            Configure(pauseSource, pauseClip);
        }

        private void OnEnable()
        {
            if (onGamePausedChannel != null)
                onGamePausedChannel.OnEventRaised += HandleGamePaused;

            if (onGameResumedChannel != null)
                onGameResumedChannel.OnEventRaised += HandleGameResumed;
        }

        private void OnDisable()
        {
            if (onGamePausedChannel != null)
                onGamePausedChannel.OnEventRaised -= HandleGamePaused;

            if (onGameResumedChannel != null)
                onGameResumedChannel.OnEventRaised -= HandleGameResumed;
        }

        private void Start()
        {
            StartCoroutine(StartGameplayMusic());
        }

        private IEnumerator StartGameplayMusic()
        {
            if (musicClip == null) yield break;

            if (startDelay > 0f)
            {
                yield return new WaitForSecondsRealtime(startDelay);
            }

            gameplayStarted = true;
            gameplaySource.Play();

            // Paused during the opening silence: wait, silent, and fade in on resume instead.
            if (isPaused && GameplayYieldsToPause)
            {
                gameplaySource.Pause();
                yield break;
            }

            FadeGameplay(1f, fadeInDuration);
        }

        private void HandleGamePaused()
        {
            isPaused = true;

            if (gameplayStarted && GameplayYieldsToPause)
            {
                FadeGameplay(0f, pauseFadeDuration);
            }

            if (pauseClip != null)
            {
                // Restarted each time, so the pause track always opens from its beginning.
                pauseSource.Stop();
                pauseSource.volume = 0f;
                pauseSource.Play();
                FadePauseTrack(1f, pauseFadeDuration);
            }
        }

        private void HandleGameResumed()
        {
            isPaused = false;

            if (pauseClip != null)
            {
                FadePauseTrack(0f, pauseFadeDuration);
            }

            if (gameplayStarted && GameplayYieldsToPause)
            {
                // Picks up where it was paused rather than starting the track over.
                gameplaySource.UnPause();
                FadeGameplay(1f, pauseFadeDuration);
            }
        }

        // Fading out pauses the gameplay track, so resuming continues from the same spot.
        private void FadeGameplay(float target, float duration)
        {
            if (gameplayFade != null) StopCoroutine(gameplayFade);
            gameplayFade = StartCoroutine(Fade(gameplaySource, target, duration, gameplaySource.Pause));
        }

        private void FadePauseTrack(float target, float duration)
        {
            if (pauseFade != null) StopCoroutine(pauseFade);
            pauseFade = StartCoroutine(Fade(pauseSource, target, duration, pauseSource.Stop));
        }

        // Loudness is heard logarithmically, so a straight volume ramp sounds like the music jumps
        // in almost at once. Fading along a cube curve spreads the change evenly across the fade.
        private static IEnumerator Fade(AudioSource source, float target, float duration, Action onSilent)
        {
            float startLevel = Mathf.Pow(source.volume, 1f / 3f);
            float targetLevel = Mathf.Pow(target, 1f / 3f);

            for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                float level = Mathf.Lerp(startLevel, targetLevel, elapsed / duration);
                source.volume = level * level * level;
                yield return null;
            }

            source.volume = target;

            if (target <= 0f)
            {
                onSilent?.Invoke();
            }
        }

        private static void Configure(AudioSource source, AudioClip clip)
        {
            // Play On Awake may already have started it at full volume before this ran.
            source.Stop();
            source.clip = clip;
            source.loop = true;
            source.playOnAwake = false;
            source.volume = 0f;

            // PauseManager pauses every other sound; music handles the pause menu itself.
            source.ignoreListenerPause = true;
        }
    }
}
