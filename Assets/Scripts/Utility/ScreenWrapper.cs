using UnityEngine;

namespace Asteroids.Utility
{
    /* Shared screen-wrap math used by anything that loops around the play area
     * (player, asteroids, UFOs). Keeping it in one place avoids the wrap padding
     * behavior silently drifting apart between entities.
     */
    public static class ScreenWrapper
    {
        public static void Wrap(Transform target, Camera camera, float viewportPadding)
        {
            if (camera == null) return;

            Vector3 viewportPos = camera.WorldToViewportPoint(target.position);

            if (viewportPos.x > 1f + viewportPadding) viewportPos.x = -viewportPadding;
            else if (viewportPos.x < -viewportPadding) viewportPos.x = 1f + viewportPadding;

            if (viewportPos.y > 1f + viewportPadding) viewportPos.y = -viewportPadding;
            else if (viewportPos.y < -viewportPadding) viewportPos.y = 1f + viewportPadding;

            target.position = camera.ViewportToWorldPoint(viewportPos);
        }

        /* Wraps using a padding in world units, applied equally on both axes. Pass the
         * object's visual half-size so it only wraps once fully off-screen and reappears
         * just touching the opposite edge. A viewport-fraction padding can't do this:
         * it's a different world distance on x and y, and doesn't scale with object size,
         * which leaves an invisible band where small objects moving near-parallel to an
         * edge can drift for a long time without ever being seen.
         */
        public static void WrapWithWorldPadding(Transform target, Camera camera, float worldPadding)
        {
            if (camera == null) return;

            float depth = target.position.z - camera.transform.position.z;
            Vector3 min = camera.ViewportToWorldPoint(new Vector3(0f, 0f, depth));
            Vector3 max = camera.ViewportToWorldPoint(new Vector3(1f, 1f, depth));

            Vector3 pos = target.position;

            if (pos.x > max.x + worldPadding) pos.x = min.x - worldPadding;
            else if (pos.x < min.x - worldPadding) pos.x = max.x + worldPadding;

            if (pos.y > max.y + worldPadding) pos.y = min.y - worldPadding;
            else if (pos.y < min.y - worldPadding) pos.y = max.y + worldPadding;

            target.position = pos;
        }
    }
}
