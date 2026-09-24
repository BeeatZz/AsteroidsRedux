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

        private Camera mainCamera;
        private Coroutine ufoSpawnRoutine;
        private Coroutine waveClearRoutine;
        private int currentWave;
        private bool isGameOver;

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
        }

        private void OnDisable()
        {
            if (onGameOverChannel != null)
                onGameOverChannel.OnEventRaised -= HandleGameOver;
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

        private void StartWave(int waveNumber)
        {
            if (isGameOver) return;

            currentWave = waveNumber;
            onWaveStartedChannel?.RaiseEvent(currentWave);

            SpawnAsteroidWave();
            RestartUfoTimer();

            if (waveClearRoutine != null) StopCoroutine(waveClearRoutine);
            waveClearRoutine = StartCoroutine(WatchForWaveClear());
        }

        private void SpawnAsteroidWave()
        {
            if (config == null || asteroidPrefab == null || ObjectPool.Instance == null) return;

            int count = config.BaseAsteroidCount + config.AsteroidCountIncreasePerWave * (currentWave - 1);
            float speedMultiplier = Mathf.Min(
                1f + config.AsteroidSpeedIncreasePerWave * (currentWave - 1),
                config.MaxAsteroidSpeedMultiplier);

            for (int i = 0; i < count; i++)
            {
                Vector3 spawnPos = SpawnPoints.RandomEdgePosition(mainCamera, config.SpawnEdgePadding);
                GameObject asteroid = ObjectPool.Instance.Get(asteroidPrefab, spawnPos, Quaternion.identity);

                if (asteroid != null && asteroid.TryGetComponent<Rigidbody2D>(out var rb))
                {
                    rb.linearVelocity *= speedMultiplier;
                }
            }
        }

        // Polls for live Asteroid instances rather than counting spawn/destroy events,
        // so split fragments (spawned outside this manager, by Asteroid itself) are
        // always accounted for correctly without a second bookkeeping path.
        private IEnumerator WatchForWaveClear()
        {
            yield return null;

            float checkInterval = config != null ? config.WaveClearCheckInterval : 0.5f;
            while (FindObjectsByType<Asteroid>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length > 0)
            {
                yield return new WaitForSeconds(checkInterval);
            }

            yield return new WaitForSeconds(config != null ? config.WaveStartDelay : 2f);
            StartWave(currentWave + 1);
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

                if (currentWave >= config.FirstUfoWave)
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
