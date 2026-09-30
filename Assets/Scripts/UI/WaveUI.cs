using System.Collections;
using UnityEngine;
using TMPro;
using Asteroids.Events;

namespace Asteroids.UI
{
    /* Announces each new wave with a banner in the middle of the screen that fades in, holds and
     * fades out. Lives on the Canvas like the other UI scripts; the banner object can start
     * switched off. WaveManager raises the wave start after the shockwave and slow motion are
     * over, so this runs on game time and simply freezes with the pause menu.
     */
    public class WaveUI : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private IntEventChannelSO onWaveStartedChannel;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI waveBanner;

        [Header("Banner")]
        [Tooltip("{0} is replaced with the wave number.")]
        [SerializeField] private string bannerFormat = "WAVE {0}";
        [Min(0f)]
        [SerializeField] private float fadeInDuration = 0.4f;
        [Min(0f)]
        [SerializeField] private float holdDuration = 1f;
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
            if (waveBanner == null) return;

            if (bannerRoutine != null) StopCoroutine(bannerRoutine);
            bannerRoutine = StartCoroutine(ShowBanner(waveNumber));
        }

        private IEnumerator ShowBanner(int waveNumber)
        {
            waveBanner.text = string.Format(bannerFormat, waveNumber);
            waveBanner.alpha = 0f;
            waveBanner.gameObject.SetActive(true);

            yield return Fade(0f, 1f, fadeInDuration);
            yield return new WaitForSeconds(holdDuration);
            yield return Fade(1f, 0f, fadeOutDuration);

            waveBanner.gameObject.SetActive(false);
            bannerRoutine = null;
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
            {
                // Eased, so it glides in and out rather than ramping linearly.
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                waveBanner.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            waveBanner.alpha = to;
        }
    }
}
