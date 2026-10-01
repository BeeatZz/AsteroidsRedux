using UnityEngine;

namespace Asteroids.Enemies
{
    /* Live count of the enemies that are currently in play. Enemies add themselves when they're
     * enabled and remove themselves when they're disabled, which is exactly when the pool hands
     * them out and takes them back, so nothing has to search the scene to find out what's left.
     */
    public static class EnemyTracker
    {
        public static int AsteroidCount { get; private set; }
        public static int UfoCount { get; private set; }
        public static int TotalCount => AsteroidCount + UfoCount;

        public static void AsteroidEnabled() => AsteroidCount++;
        public static void AsteroidDisabled() => AsteroidCount = Mathf.Max(0, AsteroidCount - 1);

        public static void UfoEnabled() => UfoCount++;
        public static void UfoDisabled() => UfoCount = Mathf.Max(0, UfoCount - 1);

        // Statics outlive a stopped play session when domain reload is turned off.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            AsteroidCount = 0;
            UfoCount = 0;
        }
    }
}
