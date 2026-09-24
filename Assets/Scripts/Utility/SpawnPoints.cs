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
    }
}
