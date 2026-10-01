using System.Collections;
using UnityEngine;
using Asteroids.Events;

namespace Asteroids.Managers
{
    /* Opens the game scene before any wave: the ship is free to fly, then the controls hint fades
     * in, holds, and fades out again, and only then is Game Started raised so WaveManager sends wave 1.
     * The hint is a world-space canvas drawn under the ship, so the player can fly over it.
     * Game time, so the whole intro holds on the pause menu.
     */
    public class GameIntro : MonoBehaviour
    {
        [Header("Controls Hint")]
        [Tooltip("CanvasGroup on the controls canvas. Optional: without it the intro only waits.")]
        [SerializeField] private CanvasGroup controlsGroup;

        [Header("Timing (game seconds)")]
        [Tooltip("Wait before the controls start fading in, counted from the scene opening.")]
        [Min(0f)]
        [SerializeField] private float startDelay = 1f;
        [Min(0f)]
        [SerializeField] private float fadeInDuration = 0.75f;
        [Tooltip("How long the controls stay fully visible.")]
        [Min(0f)]
        [SerializeField] private float holdDuration = 4f;
        [Min(0f)]
        [SerializeField] private float fadeOutDuration = 0.75f;
        [Tooltip("Pause between the controls disappearing and wave 1 starting.")]
        [Min(0f)]
        [SerializeField] private float startGameDelay = 0.5f;

        [Header("Event Channels")]
        [Tooltip("Raised once the intro is over. WaveManager starts wave 1 on it.")]
        [SerializeField] private VoidEventChannelSO onGameStartedChannel;

        private void Awake()
        {
            // Left visible in the scene so it's easy to edit; hidden before the first frame is drawn.
            if (controlsGroup != null)
            {
                controlsGroup.alpha = 0f;
                controlsGroup.gameObject.SetActive(true);
            }
        }

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(startDelay);

            if (controlsGroup != null)
            {
                yield return FadeControls(1f, fadeInDuration);
                yield return new WaitForSeconds(holdDuration);
                yield return FadeControls(0f, fadeOutDuration);
                controlsGroup.gameObject.SetActive(false);
            }

            yield return new WaitForSeconds(startGameDelay);
            onGameStartedChannel?.RaiseEvent();
        }

        private IEnumerator FadeControls(float target, float duration)
        {
            float start = controlsGroup.alpha;

            for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
            {
                controlsGroup.alpha = Mathf.Lerp(start, target, elapsed / duration);
                yield return null;
            }

            controlsGroup.alpha = target;
        }
    }
}
