using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Asteroids.Enemies;
using Asteroids.Pooling;

namespace Asteroids.UI
{
    /* Decoration for the main menu: sends the real asteroid prefabs in from off-screen one at a
     * time, then lets them drift, spin and wrap exactly as in the game. Nothing in the menu can hit them, so they
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

        [Tooltip("Seconds after the menu opens before the first asteroid drifts in.")]
        [Min(0f)]
        [SerializeField] private float startDelay = 2f;

        [Tooltip("Seconds between each asteroid drifting in from off-screen. 0 sends them all at once.")]
        [Min(0f)]
        [SerializeField] private float spawnInterval = 0.6f;

        [Tooltip("Asteroids aim at a random point at least this far in from the screen edges " +
                 "(viewport fraction, 0-0.5), so each one crosses into view.")]
        [Range(0f, 0.5f)]
        [SerializeField] private float aimInset = 0.25f;

        private IEnumerator Start()
        {
            if (Camera.main == null) yield break;

            // Shuffled so the sizes arrive mixed rather than all the big ones first.
            var queue = new List<GameObject>();
            foreach (var entry in asteroids)
            {
                if (entry.prefab == null) continue;
                for (int i = 0; i < entry.count; i++) queue.Add(entry.prefab);
            }

            for (int i = queue.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (queue[i], queue[j]) = (queue[j], queue[i]);
            }

            // Waits are unscaled, in case the menu is ever opened with time still slowed or paused.
            if (startDelay > 0f)
            {
                yield return new WaitForSecondsRealtime(startDelay);
            }

            for (int i = 0; i < queue.Count; i++)
            {
                if (i > 0 && spawnInterval > 0f)
                {
                    yield return new WaitForSecondsRealtime(spawnInterval);
                }

                Spawn(queue[i]);
            }
        }

        private void Spawn(GameObject prefab)
        {
            // Placed properly by EnterFromOffscreen below, before it's ever drawn.
            GameObject instance = Instantiate(prefab, transform.position, Quaternion.identity, transform);

            // No pool in the menu, so run the same setup ObjectPool.Get does: this is what gives
            // each asteroid its random direction and spin.
            foreach (var poolable in instance.GetComponents<IPoolable>())
            {
                poolable.OnSpawnFromPool();
            }

            if (instance.TryGetComponent<Asteroid>(out var asteroid))
            {
                asteroid.SetSpeedMultiplier(speedMultiplier);
                asteroid.EnterFromOffscreen(aimInset);
            }
        }
    }
}
