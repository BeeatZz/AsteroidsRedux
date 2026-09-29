using System.Collections;
using UnityEngine;
using Asteroids.Events;
using Asteroids.ScriptableObjects;
using Asteroids.Pooling;
using Asteroids.Utility;
using Asteroids.Enemies;
using Asteroids.Enemies.AI;

namespace Asteroids.Managers
{
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance { get; private set; }

        [System.Serializable]
        private class UfoVariant
        {
            public GameObject prefab;
            [Tooltip("Earliest wave this variant is allowed to spawn on.")]
            public int minWave = 1;
            [Tooltip("Relative chance of being picked among the variants currently eligible.")]
            public float weight = 1f;
        }

        [Header("Config")]
        [SerializeField] private WaveConfig config;

        [Header("Spawn Prefabs")]
        [SerializeField] private GameObject asteroidPrefab;
        [SerializeField] private UfoVariant[] ufoVariants;

        [Header("Event Channels")]
        [SerializeField] private IntEventChannelSO onWaveStartedChannel;
        [SerializeField] private VoidEventChannelSO onGameOverChannel;
        [Tooltip("Raised by asteroids and UFOs when they're destroyed, with their position.")]
        [SerializeField] private Vector3EventChannelSO onEnemyDestroyedChannel;
        [Tooltip("Raised when the kill that empties the wave lands, with where it happened.")]
        [SerializeField] private Vector3EventChannelSO onWaveClearedChannel;

        private Camera mainCamera;
        private Coroutine ufoSpawnRoutine;
        private Coroutine waveClearRoutine;
        private Coroutine asteroidSpawnRoutine;
        private int currentWave;
        private bool isGameOver;
        private bool waveClearedRaised;
        // True while a wave's asteroids are still drifting in; the wave can't be cleared until
        // they've all arrived, even if the player destroys every one that's out so far.
        private bool isSpawningWave;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            mainCamera = Camera.main;
        }

        private void OnEnable()
        {
            if (onGameOverChannel != null)
                onGameOverChannel.OnEventRaised += HandleGameOver;

            if (onEnemyDestroyedChannel != null)
                onEnemyDestroyedChannel.OnEventRaised += HandleEnemyDestroyed;
        }

        private void OnDisable()
        {
            if (onGameOverChannel != null)
                onGameOverChannel.OnEventRaised -= HandleGameOver;

            if (onEnemyDestroyedChannel != null)
                onEnemyDestroyedChannel.OnEventRaised -= HandleEnemyDestroyed;
        }

        private void Start()
        {
            StartWave(1);
        }

        private void HandleGameOver()
        {
            isGameOver = true;
            StopAllCoroutines();
        }

        // Enemies report their death after they've left play (and after any split fragments have
        // spawned), so "nothing left alive" here means this kill was the one that emptied the wave.
        private void HandleEnemyDestroyed(Vector3 position)
        {
            if (waveClearedRaised || isSpawningWave || AnyEnemiesAlive()) return;

            waveClearedRaised = true;
            onWaveClearedChannel?.RaiseEvent(position);
        }

        private void StartWave(int waveNumber)
        {
            if (isGameOver) return;

            currentWave = waveNumber;
            waveClearedRaised = false;
            onWaveStartedChannel?.RaiseEvent(currentWave);

            if (asteroidSpawnRoutine != null) StopCoroutine(asteroidSpawnRoutine);
            isSpawningWave = true;
            asteroidSpawnRoutine = StartCoroutine(SpawnAsteroidWave());
            RestartUfoTimer();

            if (waveClearRoutine != null) StopCoroutine(waveClearRoutine);
            waveClearRoutine = StartCoroutine(WatchForWaveClear());
        }

        // Sends the wave's asteroids in one at a time, each drifting in from just off-screen.
        private IEnumerator SpawnAsteroidWave()
        {
            if (config != null && asteroidPrefab != null && ObjectPool.Instance != null)
            {
                int count = config.BaseAsteroidCount + config.AsteroidCountIncreasePerWave * (currentWave - 1);
                float speedMultiplier = Mathf.Min(
                    1f + config.AsteroidSpeedIncreasePerWave * (currentWave - 1),
                    config.MaxAsteroidSpeedMultiplier);

                for (int i = 0; i < count; i++)
                {
                    if (i > 0 && config.AsteroidSpawnInterval > 0f)
                    {
                        yield return new WaitForSeconds(config.AsteroidSpawnInterval);
                    }

                    // Handed out off-screen; EnterFromOffscreen then picks the exact edge spot.
                    Vector3 spawnPos = SpawnPoints.RandomEdgePosition(mainCamera, config.SpawnEdgePadding);
                    GameObject asteroid = ObjectPool.Instance.Get(asteroidPrefab, spawnPos, Quaternion.identity);

                    if (asteroid != null && asteroid.TryGetComponent<Asteroid>(out var asteroidComponent))
                    {
                        asteroidComponent.SetSpeedMultiplier(speedMultiplier);
                        asteroidComponent.EnterFromOffscreen(config.AsteroidAimInset);
                    }
                }
            }

            isSpawningWave = false;
            asteroidSpawnRoutine = null;
        }

        // Polls for live enemies rather than counting spawn/destroy events, so split fragments
        // (spawned outside this manager, by Asteroid itself) are always accounted for correctly
        // without a second bookkeeping path. A wave needs its UFOs cleared too, not just asteroids.
        private IEnumerator WatchForWaveClear()
        {
            yield return null;

            float checkInterval = config != null ? config.WaveClearCheckInterval : 0.5f;
            while (isSpawningWave || AnyEnemiesAlive())
            {
                yield return new WaitForSeconds(checkInterval);
            }

            yield return new WaitForSeconds(config != null ? config.WaveStartDelay : 2f);
            StartWave(currentWave + 1);
        }

        private static bool AnyAsteroidsAlive()
        {
            return FindAnyObjectByType<Asteroid>(FindObjectsInactive.Exclude) != null;
        }

        private static bool AnyEnemiesAlive()
        {
            return AnyAsteroidsAlive() || FindAnyObjectByType<UfoController>(FindObjectsInactive.Exclude) != null;
        }

        private void RestartUfoTimer()
        {
            if (ufoSpawnRoutine != null) StopCoroutine(ufoSpawnRoutine);
            ufoSpawnRoutine = StartCoroutine(UfoSpawnLoop());
        }

        private IEnumerator UfoSpawnLoop()
        {
            if (config == null) yield break;

            while (true)
            {
                float interval = Mathf.Max(
                    config.MinUfoSpawnInterval,
                    config.UfoSpawnIntervalBase - config.UfoSpawnIntervalDecreasePerWave * (currentWave - 1));

                yield return new WaitForSeconds(interval);

                // Once the asteroids are gone the wave is only waiting on its UFOs, so no new ones
                // join; otherwise a wave could be kept open indefinitely.
                if (currentWave >= config.FirstUfoWave && AnyAsteroidsAlive())
                {
                    SpawnUfo();
                }
            }
        }

        private void SpawnUfo()
        {
            GameObject prefab = PickUfoVariant();
            if (prefab == null || ObjectPool.Instance == null) return;

            Vector3 spawnPos = SpawnPoints.RandomEdgePosition(mainCamera, config.SpawnEdgePadding);
            GameObject ufo = ObjectPool.Instance.Get(prefab, spawnPos, Quaternion.identity);

            if (ufo != null && ufo.TryGetComponent<UfoController>(out var controller))
            {
                float fireRate = Mathf.Max(
                    config.MinUfoFireRate,
                    config.UfoFireRateBase - config.UfoFireRateDecreasePerWave * (currentWave - 1));
                controller.SetFireRate(fireRate);
            }
        }

        // Weighted pick among variants unlocked for the current wave, so e.g. a
        // tougher UFO (its toughness just comes from a different UfoConfigSO
        // health value on its own prefab) can be held back until later waves.
        private GameObject PickUfoVariant()
        {
            if (ufoVariants == null || ufoVariants.Length == 0) return null;

            float totalWeight = 0f;
            foreach (var variant in ufoVariants)
            {
                if (variant.prefab != null && currentWave >= variant.minWave)
                {
                    totalWeight += Mathf.Max(0f, variant.weight);
                }
            }

            if (totalWeight <= 0f) return null;

            float roll = Random.Range(0f, totalWeight);
            foreach (var variant in ufoVariants)
            {
                if (variant.prefab == null || currentWave < variant.minWave) continue;

                roll -= Mathf.Max(0f, variant.weight);
                if (roll <= 0f) return variant.prefab;
            }

            return null;
        }
    }
}
