using System.Collections;
using UnityEngine;
using TMPro;
using Asteroids.Events;

namespace Asteroids.UI
{
    /* Shows the wave number: an optional corner label that always shows the current wave, and a
     * banner that fades in, holds and fades out each time a new wave starts. Lives on the Canvas
     * like the other UI scripts; the banner object can start switched off.
     * Timing is unscaled, so the wave-clear slow motion doesn't drag the banner out.
     */
    public class WaveUI : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private IntEventChannelSO onWaveStartedChannel;

        [Header("UI Elements")]
        [Tooltip("Optional. Always shows the current wave, e.g. in a corner of the HUD.")]
        [SerializeField] private TextMeshProUGUI waveLabel;
        [Tooltip("Optional. Announces each new wave in the middle of the screen.")]
        [SerializeField] private TextMeshProUGUI waveBanner;

        [Header("Banner")]
        [Min(0f)]
        [SerializeField] private float fadeInDuration = 0.3f;
        [Min(0f)]
        [SerializeField] private float holdDuration = 1.2f;
        [Min(0f)]
        [SerializeField] private float fadeOutDuration = 0.6f;

        private Coroutine bannerRoutine;

        private void Awake()
        {
            if (waveBanner != null)
            {
                waveBanner.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (onWaveStartedChannel != null)
                onWaveStartedChannel.OnEventRaised += HandleWaveStarted;
        }

        private void OnDisable()
        {
            if (onWaveStartedChannel != null)
                onWaveStartedChannel.OnEventRaised -= HandleWaveStarted;

            bannerRoutine = null;
            if (waveBanner != null)
            {
                waveBanner.gameObject.SetActive(false);
            }
        }

        private void HandleWaveStarted(int waveNumber)
        {
            if (waveLabel != null)
            {
                waveLabel.text = $"WAVE {waveNumber}";
            }

            if (waveBanner != null)
            {
                if (bannerRoutine != null) StopCoroutine(bannerRoutine);
                bannerRoutine = StartCoroutine(ShowBanner(waveNumber));
            }
        }

        private IEnumerator ShowBanner(int waveNumber)
        {
            waveBanner.text = $"WAVE {waveNumber}";
            waveBanner.alpha = 0f;
            waveBanner.gameObject.SetActive(true);

            yield return Fade(0f, 1f, fadeInDuration);
            yield return new WaitForSecondsRealtime(holdDuration);
            yield return Fade(1f, 0f, fadeOutDuration);

            waveBanner.gameObject.SetActive(false);
            bannerRoutine = null;
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                waveBanner.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }

            waveBanner.alpha = to;
        }
    }
}
