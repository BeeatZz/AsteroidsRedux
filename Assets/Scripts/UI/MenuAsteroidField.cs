using System;
using System.Collections.Generic;
using UnityEngine;
using Asteroids.Enemies;
using Asteroids.Pooling;

namespace Asteroids.UI
{
    /* Decoration for the main menu: scatters the real asteroid prefabs across the screen and lets
     * them drift, spin and wrap exactly as in the game. Nothing in the menu can hit them, so they
     * never score, split or raise events.
     */
    public class MenuAsteroidField : MonoBehaviour
    {
        [Serializable]
        private struct AsteroidEntry
        {
            public GameObject prefab;
            [Min(0)] public int count;
        }

        [SerializeField] private List<AsteroidEntry> asteroids = new();

        [Tooltip("Scales each asteroid's configured speed. Below 1 keeps the backdrop calm.")]
        [Min(0f)]
        [SerializeField] private float speedMultiplier = 0.5f;

        private void Start()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null) return;

            foreach (var entry in asteroids)
            {
                if (entry.prefab == null) continue;

                for (int i = 0; i < entry.count; i++)
                {
                    Spawn(entry.prefab, RandomOnScreenPosition(mainCamera));
                }
            }
        }

        private void Spawn(GameObject prefab, Vector3 position)
        {
            GameObject instance = Instantiate(prefab, position, Quaternion.identity, transform);

            // No pool in the menu, so run the same setup ObjectPool.Get does: this is what gives
            // each asteroid its random direction and spin.
            foreach (var poolable in instance.GetComponents<IPoolable>())
            {
                poolable.OnSpawnFromPool();
            }

            if (instance.TryGetComponent<Asteroid>(out var asteroid))
            {
                asteroid.SetSpeedMultiplier(speedMultiplier);
            }
        }

        // Starts them spread over the screen rather than at the edges, so the menu isn't empty
        // for the first few seconds.
        private static Vector3 RandomOnScreenPosition(Camera camera)
        {
            float depth = Mathf.Abs(camera.transform.position.z);
            Vector3 world = camera.ViewportToWorldPoint(new Vector3(UnityEngine.Random.value, UnityEngine.Random.value, depth));
            world.z = 0f;
            return world;
        }
    }
}
