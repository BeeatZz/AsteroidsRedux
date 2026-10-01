using UnityEngine;

namespace Asteroids.UI
{
    /* Loading screen decoration: the ship cruises around at a steady speed, drifting into slow,
     * random turns, and banks back towards the middle whenever it nears the screen edge.
     * Its start point, heading and turns are random each time, so no two loading screens match.
     */
    public class LoadingShipFlyer : MonoBehaviour
    {
        [Header("Flight")]
        [Tooltip("World units per second.")]
        [Min(0f)]
        [SerializeField] private float speed = 4f;
        [Tooltip("Fastest the ship turns on its own, in degrees per second.")]
        [Min(0f)]
        [SerializeField] private float maxTurnRate = 120f;
        [Tooltip("How quickly the turning changes. Lower gives long lazy arcs, higher gives twitchier flying.")]
        [Min(0f)]
        [SerializeField] private float wanderRate = 0.35f;

        [Header("Screen Edges")]
        [Tooltip("Once the ship is this close to an edge (fraction of the screen, 0-0.5), it turns back towards the middle.")]
        [Range(0f, 0.5f)]
        [SerializeField] private float edgeMargin = 0.15f;
        [Tooltip("How hard it turns back, in degrees per second.")]
        [Min(0f)]
        [SerializeField] private float returnTurnRate = 200f;

        [Header("Sprite")]
        [Tooltip("Degrees to add so the sprite's nose leads. The ship sprite points up, so -90.")]
        [SerializeField] private float headingOffset = -90f;

        private Camera mainCamera;
        private float heading;
        private float noiseSeed;
        private float time;

        private void Awake()
        {
            mainCamera = FindSceneCamera();
            noiseSeed = Random.Range(0f, 1000f);
            heading = Random.Range(0f, 360f);

            if (mainCamera != null)
            {
                // Somewhere in the middle area, so it doesn't open by turning away from an edge.
                float inset = edgeMargin + 0.1f;
                Vector3 start = mainCamera.ViewportToWorldPoint(new Vector3(
                    Random.Range(inset, 1f - inset),
                    Random.Range(inset, 1f - inset),
                    0f));
                start.z = 0f;
                transform.position = start;
            }

            ApplyRotation();
        }

        private void Update()
        {
            if (mainCamera == null) return;

            float step = Time.unscaledDeltaTime;
            time += step;

            Vector3 viewport = mainCamera.WorldToViewportPoint(transform.position);
            bool nearEdge = viewport.x < edgeMargin || viewport.x > 1f - edgeMargin
                         || viewport.y < edgeMargin || viewport.y > 1f - edgeMargin;

            if (nearEdge)
            {
                Vector2 toCentre = (Vector2)(mainCamera.transform.position - transform.position);
                float centreHeading = Mathf.Atan2(toCentre.y, toCentre.x) * Mathf.Rad2Deg;
                heading = Mathf.MoveTowardsAngle(heading, centreHeading, returnTurnRate * step);
            }
            else
            {
                // Perlin noise makes the turning drift smoothly between left and right.
                float turn = Mathf.PerlinNoise(noiseSeed, time * wanderRate) * 2f - 1f;
                heading += turn * maxTurnRate * step;
            }

            float radians = heading * Mathf.Deg2Rad;
            transform.position += new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f) * (speed * step);
            ApplyRotation();
        }

        // The camera in this ship's own scene. The game scene loads underneath the loading screen with
        // its own MainCamera, so this camera can't be tagged MainCamera and Camera.main can't be trusted.
        private Camera FindSceneCamera()
        {
            foreach (GameObject root in gameObject.scene.GetRootGameObjects())
            {
                Camera found = root.GetComponentInChildren<Camera>();
                if (found != null) return found;
            }

            return Camera.main;
        }

        private void ApplyRotation()
        {
            transform.rotation = Quaternion.Euler(0f, 0f, heading + headingOffset);
        }
    }
}
