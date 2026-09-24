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

        public static void WrapWithWorldPadding(Transform target, Camera camera, float worldPadding)
        {
            if (camera == null) return;

            Vector3 rightEdgeWorld = camera.ViewportToWorldPoint(new Vector3(1, 0, camera.nearClipPlane));
            Vector3 leftEdgeWorld = camera.ViewportToWorldPoint(new Vector3(0, 0, camera.nearClipPlane));
            float screenWidthInWorld = rightEdgeWorld.x - leftEdgeWorld.x;
            float viewportPadding = screenWidthInWorld > 0f ? worldPadding / screenWidthInWorld : 0f;

            Wrap(target, camera, viewportPadding);
        }
    }
}
