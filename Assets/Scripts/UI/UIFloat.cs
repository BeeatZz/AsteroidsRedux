using UnityEngine;

namespace Asteroids.UI
{
    /* Makes a UI element drift gently around where it sits, as if it were floating in space.
     * Each instance starts at a random point in the noise, so a row of buttons doesn't move in step.
     * Runs on unscaled time, so it keeps going in the pause and game over menus.
     */
    [RequireComponent(typeof(RectTransform))]
    public class UIFloat : MonoBehaviour
    {
        [Tooltip("How far it wanders from its resting spot, in canvas units.")]
        [Min(0f)]
        [SerializeField] private float radius = 5f;

        [Tooltip("How fast it drifts. Keep it low for a slow, subtle float.")]
        [Min(0f)]
        [SerializeField] private float speed = 0.35f;

        private RectTransform rectTransform;
        private Vector2 seed;
        private Vector2 appliedOffset;

        private void Awake()
        {
            rectTransform = (RectTransform)transform;
            seed = new Vector2(Random.Range(0f, 100f), Random.Range(0f, 100f));
        }

        private void OnDisable()
        {
            rectTransform.anchoredPosition -= appliedOffset;
            appliedOffset = Vector2.zero;
        }

        private void Update()
        {
            float t = Time.unscaledTime * speed;
            Vector2 offset = new Vector2(
                Mathf.PerlinNoise(seed.x + t, 17.3f) - 0.5f,
                Mathf.PerlinNoise(91.7f, seed.y + t) - 0.5f) * (2f * radius);

            // Adds only the change since last frame, so a layout group or another script moving
            // the element's resting position isn't fought over.
            rectTransform.anchoredPosition += offset - appliedOffset;
            appliedOffset = offset;
        }
    }
}
