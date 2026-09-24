using UnityEngine;

namespace Asteroids.ScriptableObjects
{
    [CreateAssetMenu(fileName = "NewWaveConfig", menuName = "Asteroids/Config/Wave Config")]
    public class WaveConfig : ScriptableObject
    {
        [Header("Asteroids")]
        [SerializeField] private int baseAsteroidCount = 4;
        [SerializeField] private int asteroidCountIncreasePerWave = 2;
        [SerializeField] private float asteroidSpeedIncreasePerWave = 0.15f;
        [SerializeField] private float maxAsteroidSpeedMultiplier = 2.5f;

        [Header("UFO")]
        [SerializeField] private int firstUfoWave = 2;
        [SerializeField] private float ufoSpawnIntervalBase = 20f;
        [SerializeField] private float ufoSpawnIntervalDecreasePerWave = 1.5f;
        [SerializeField] private float minUfoSpawnInterval = 6f;
        [SerializeField] private float ufoFireRateBase = 1.8f;
        [SerializeField] private float ufoFireRateDecreasePerWave = 0.15f;
        [SerializeField] private float minUfoFireRate = 0.6f;

        [Header("Pacing")]
        [SerializeField] private float waveStartDelay = 2f;
        [SerializeField] private float waveClearCheckInterval = 0.5f;
        [SerializeField] private float spawnEdgePadding = 0.1f;

        public int BaseAsteroidCount => baseAsteroidCount;
        public int AsteroidCountIncreasePerWave => asteroidCountIncreasePerWave;
        public float AsteroidSpeedIncreasePerWave => asteroidSpeedIncreasePerWave;
        public float MaxAsteroidSpeedMultiplier => maxAsteroidSpeedMultiplier;

        public int FirstUfoWave => firstUfoWave;
        public float UfoSpawnIntervalBase => ufoSpawnIntervalBase;
        public float UfoSpawnIntervalDecreasePerWave => ufoSpawnIntervalDecreasePerWave;
        public float MinUfoSpawnInterval => minUfoSpawnInterval;
        public float UfoFireRateBase => ufoFireRateBase;
        public float UfoFireRateDecreasePerWave => ufoFireRateDecreasePerWave;
        public float MinUfoFireRate => minUfoFireRate;

        public float WaveStartDelay => waveStartDelay;
        public float WaveClearCheckInterval => waveClearCheckInterval;
        public float SpawnEdgePadding => spawnEdgePadding;
    }
}
