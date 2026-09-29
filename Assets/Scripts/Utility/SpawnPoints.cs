using UnityEngine;

namespace Asteroids.Utility
{
    /* Shared "just outside the screen" spawn-position math, so new enemies never
     * pop in on top of the player. Mirrors ScreenWrapper's viewport-space approach.
     */
    public static class SpawnPoints
    {
        public static Vector3 RandomEdgePosition(Camera camera, float viewportPadding)
        {
            if (camera == null) return Vector3.zero;

            int edge = Random.Range(0, 4);
            float along = Random.value;
            float x, y;

            switch (edge)
            {
                case 0: x = -viewportPadding; y = along; break; // left
                case 1: x = 1f + viewportPadding; y = along; break; // right
                case 2: x = along; y = -viewportPadding; break; // bottom
                default: x = along; y = 1f + viewportPadding; break; // top
            }

            float depth = Mathf.Abs(camera.transform.position.z);
            Vector3 world = camera.ViewportToWorldPoint(new Vector3(x, y, depth));
            world.z = 0f;
            return world;
        }

        /* A random spot just past one screen edge, worldPadding world units out on either axis.
         * Pass the object's wrap padding (its visual half-size) and it starts fully hidden but
         * right on ScreenWrapper.WrapWithWorldPadding's threshold, so it isn't wrapped straight
         * to the opposite side on its first frame.
         */
        public static Vector3 JustOffscreen(Camera camera, float worldPadding)
        {
            if (camera == null) return Vector3.zero;

            GetWorldBounds(camera, out Vector3 min, out Vector3 max);

            // Nudged a hair inside the threshold; the wrap check is strict, but float rounding isn't.
            float offset = Mathf.Max(0f, worldPadding - 0.001f);
            float alongX = Random.Range(min.x, max.x);
            float alongY = Random.Range(min.y, max.y);

            return Random.Range(0, 4) switch
            {
                0 => new Vector3(min.x - offset, alongY, 0f), // left
                1 => new Vector3(max.x + offset, alongY, 0f), // right
                2 => new Vector3(alongX, min.y - offset, 0f), // bottom
                _ => new Vector3(alongX, max.y + offset, 0f), // top
            };
        }

        // A random point on screen at least viewportInset (0-0.5) away from every edge.
        public static Vector3 RandomOnScreen(Camera camera, float viewportInset)
        {
            if (camera == null) return Vector3.zero;

            float inset = Mathf.Clamp(viewportInset, 0f, 0.5f);
            float depth = Mathf.Abs(camera.transform.position.z);
            Vector3 world = camera.ViewportToWorldPoint(new Vector3(
                Random.Range(inset, 1f - inset), Random.Range(inset, 1f - inset), depth));
            world.z = 0f;
            return world;
        }

        private static void GetWorldBounds(Camera camera, out Vector3 min, out Vector3 max)
        {
            float depth = Mathf.Abs(camera.transform.position.z);
            min = camera.ViewportToWorldPoint(new Vector3(0f, 0f, depth));
            max = camera.ViewportToWorldPoint(new Vector3(1f, 1f, depth));
        }
    }
}
