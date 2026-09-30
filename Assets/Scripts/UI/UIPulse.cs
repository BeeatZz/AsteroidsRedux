using UnityEngine;

namespace Asteroids.UI
{
    /* Smoothly grows and shrinks this object around its pivot, like Minecraft's title splash text.
     * Runs on unscaled time, so it keeps going while the game is paused or slowed down.
     */
    public class UIPulse : MonoBehaviour
    {
        [Tooltip("How far the scale swings either side of its starting size (0.06 = ±6%).")]
        [Min(0f)]
        [SerializeField] private float amplitude = 0.06f;

        [Tooltip("Seconds for one full grow-and-shrink cycle.")]
        [Min(0.01f)]
        [SerializeField] private float period = 1.2f;

        private Vector3 baseScale;

        private void Awake()
        {
            baseScale = transform.localScale;
        }

        private void Update()
        {
            float wave = Mathf.Sin(Time.unscaledTime * (2f * Mathf.PI / period));
            transform.localScale = baseScale * (1f + amplitude * wave);
        }

        private void OnDisable()
        {
            transform.localScale = baseScale;
        }
    }
}
