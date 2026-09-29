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
        [Tooltip("Seconds between each asteroid of a wave drifting in from off-screen. 0 sends them all at once.")]
        [Min(0f)]
        [SerializeField] private float asteroidSpawnInterval = 0.4f;
        [Tooltip("Asteroids aim at a random point at least this far in from the screen edges " +
                 "(viewport fraction, 0-0.5), so each one crosses into view.")]
        [Range(0f, 0.5f)]
        [SerializeField] private float asteroidAimInset = 0.25f;

        [Header("UFO")]
        [SerializeField] private int firstUfoWave = 2;
        [SerializeField] private float ufoSpawnIntervalBase = 20f;
        [SerializeField] private float ufoSpawnIntervalDecreasePerWave = 1.5f;
        [SerializeField] private float minUfoSpawnInterval = 6f;
        [SerializeField] private float ufoFireRateBase = 1.8f;
        [SerializeField] private float ufoFireRateDecreasePerWave = 0.15f;
        [SerializeField] private float minUfoFireRate = 0.6f;

        [Header("Pacing")]
        [Tooltip("Seconds of game time from a wave being cleared to the next wave's banner. " +
                 "Keep it longer than the shockwave (0.8 s) so the banner comes in after it.")]
        [Min(0f)]
        [SerializeField] private float waveStartDelay = 2f;
        [Tooltip("Seconds from the wave banner appearing to the first asteroid drifting in, " +
                 "so the banner has the screen to itself.")]
        [Min(0f)]
        [SerializeField] private float waveIntroDuration = 1.5f;
        [SerializeField] private float waveClearCheckInterval = 0.5f;
        [Tooltip("How far outside the screen UFOs spawn (viewport fraction). Asteroids use their own size instead.")]
        [SerializeField] private float spawnEdgePadding = 0.1f;

        public int BaseAsteroidCount => baseAsteroidCount;
        public int AsteroidCountIncreasePerWave => asteroidCountIncreasePerWave;
        public float AsteroidSpeedIncreasePerWave => asteroidSpeedIncreasePerWave;
        public float MaxAsteroidSpeedMultiplier => maxAsteroidSpeedMultiplier;
        public float AsteroidSpawnInterval => asteroidSpawnInterval;
        public float AsteroidAimInset => asteroidAimInset;

        public int FirstUfoWave => firstUfoWave;
        public float UfoSpawnIntervalBase => ufoSpawnIntervalBase;
        public float UfoSpawnIntervalDecreasePerWave => ufoSpawnIntervalDecreasePerWave;
        public float MinUfoSpawnInterval => minUfoSpawnInterval;
        public float UfoFireRateBase => ufoFireRateBase;
        public float UfoFireRateDecreasePerWave => ufoFireRateDecreasePerWave;
        public float MinUfoFireRate => minUfoFireRate;

        public float WaveStartDelay => waveStartDelay;
        public float WaveIntroDuration => waveIntroDuration;
        public float WaveClearCheckInterval => waveClearCheckInterval;
        public float SpawnEdgePadding => spawnEdgePadding;
    }
}
