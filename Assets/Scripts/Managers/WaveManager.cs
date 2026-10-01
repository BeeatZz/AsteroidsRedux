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
        [Tooltip("Optional. Wave 1 waits for this (GameIntro raises it once the controls hint is gone). Empty starts wave 1 straight away.")]
        [SerializeField] private VoidEventChannelSO onGameStartedChannel;
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
            if (onGameStartedChannel != null)
                onGameStartedChannel.OnEventRaised += HandleGameStarted;

            if (onGameOverChannel != null)
                onGameOverChannel.OnEventRaised += HandleGameOver;

            if (onEnemyDestroyedChannel != null)
                onEnemyDestroyedChannel.OnEventRaised += HandleEnemyDestroyed;
        }

        private void OnDisable()
        {
            if (onGameStartedChannel != null)
                onGameStartedChannel.OnEventRaised -= HandleGameStarted;

            if (onGameOverChannel != null)
                onGameOverChannel.OnEventRaised -= HandleGameOver;

            if (onEnemyDestroyedChannel != null)
                onEnemyDestroyedChannel.OnEventRaised -= HandleEnemyDestroyed;
        }

        private void Start()
        {
            if (onGameStartedChannel == null)
            {
                StartWave(1);
            }
        }

        private void HandleGameStarted()
        {
            if (currentWave == 0)
            {
                StartWave(1);
            }
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
            isSpawningWave = true;
            onWaveStartedChannel?.RaiseEvent(currentWave);

            if (asteroidSpawnRoutine != null) StopCoroutine(asteroidSpawnRoutine);
            asteroidSpawnRoutine = StartCoroutine(SpawnAsteroidWave());

            if (waveClearRoutine != null) StopCoroutine(waveClearRoutine);
            waveClearRoutine = StartCoroutine(WatchForWaveClear());
        }

        // After the wave banner's intro, sends the wave's asteroids in one at a time, each drifting
        // in from just off-screen. The UFO timer starts with them, so it counts from their arrival.
        private IEnumerator SpawnAsteroidWave()
        {
            if (config != null && config.WaveIntroDuration > 0f)
            {
                yield return new WaitForSeconds(config.WaveIntroDuration);
            }

            RestartUfoTimer();

            if (config != null && asteroidPrefab != null && ObjectPool.Instance != null)
            {
                int count = config.AsteroidCountForWave(currentWave);
                float speedMultiplier = config.AsteroidSpeedMultiplierForWave(currentWave);

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

        // Checks EnemyTracker rather than counting spawn/destroy events, so split fragments
        // (spawned outside this manager, by Asteroid itself) are always accounted for correctly
        // without a second bookkeeping path. A wave needs its UFOs cleared too, not just asteroids.
        // This is a cheap safety net for waves that end without a final kill event.
        private IEnumerator WatchForWaveClear()
        {
            yield return null;

            // Stops the moment HandleEnemyDestroyed sees the last kill, so the delay below always
            // starts with the shockwave; the slower poll only catches waves that end any other way.
            float checkInterval = config != null ? config.WaveClearCheckInterval : 0.5f;
            float nextCheck = 0f;
            while (!waveClearedRaised)
            {
                if (Time.time >= nextCheck)
                {
                    if (!isSpawningWave && !AnyEnemiesAlive()) break;
                    nextCheck = Time.time + checkInterval;
                }

                yield return null;
            }

            // Game time, like the shockwave, so this also waits out slow motion and the pause menu.
            yield return new WaitForSeconds(config != null ? config.WaveStartDelay : 2f);
            StartWave(currentWave + 1);
        }

        private static bool AnyAsteroidsAlive()
        {
            return EnemyTracker.AsteroidCount > 0;
        }

        private static bool AnyEnemiesAlive()
        {
            return EnemyTracker.TotalCount > 0;
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
                yield return new WaitForSeconds(config.UfoSpawnIntervalForWave(currentWave));

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
                controller.SetFireRate(config.UfoFireRateForWave(currentWave));
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
