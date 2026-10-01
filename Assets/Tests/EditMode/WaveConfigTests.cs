using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Asteroids.ScriptableObjects;

namespace Asteroids.Tests
{
    public class WaveConfigTests
    {
        private WaveConfig config;

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<WaveConfig>();
            Set("baseAsteroidCount", 4);
            Set("asteroidCountIncreasePerWave", 2);
            Set("asteroidSpeedIncreasePerWave", 0.5f);
            Set("maxAsteroidSpeedMultiplier", 2f);
            Set("ufoSpawnIntervalBase", 20f);
            Set("ufoSpawnIntervalDecreasePerWave", 2f);
            Set("minUfoSpawnInterval", 6f);
            Set("ufoFireRateBase", 2f);
            Set("ufoFireRateDecreasePerWave", 0.25f);
            Set("minUfoFireRate", 1f);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(config);

        // The values are private serialized fields, so the tests set them directly to stay
        // independent of whatever the shipped asset is tuned to.
        private void Set(string field, object value)
        {
            typeof(WaveConfig)
                .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(config, value);
        }

        [Test]
        public void AsteroidCount_StartsAtTheBaseAndGrowsEachWave()
        {
            Assert.AreEqual(4, config.AsteroidCountForWave(1));
            Assert.AreEqual(6, config.AsteroidCountForWave(2));
            Assert.AreEqual(14, config.AsteroidCountForWave(6));
        }

        [Test]
        public void AsteroidSpeed_StartsAtOneAndGrowsEachWave()
        {
            Assert.AreEqual(1f, config.AsteroidSpeedMultiplierForWave(1), 0.0001f);
            Assert.AreEqual(1.5f, config.AsteroidSpeedMultiplierForWave(2), 0.0001f);
        }

        [Test]
        public void AsteroidSpeed_IsCappedAtTheMaximum()
        {
            Assert.AreEqual(2f, config.AsteroidSpeedMultiplierForWave(3), 0.0001f);
            Assert.AreEqual(2f, config.AsteroidSpeedMultiplierForWave(50), 0.0001f);
        }

        [Test]
        public void UfoSpawnInterval_ShrinksEachWaveDownToTheMinimum()
        {
            Assert.AreEqual(20f, config.UfoSpawnIntervalForWave(1), 0.0001f);
            Assert.AreEqual(18f, config.UfoSpawnIntervalForWave(2), 0.0001f);
            Assert.AreEqual(6f, config.UfoSpawnIntervalForWave(50), 0.0001f);
        }

        [Test]
        public void UfoFireRate_ShrinksEachWaveDownToTheMinimum()
        {
            Assert.AreEqual(2f, config.UfoFireRateForWave(1), 0.0001f);
            Assert.AreEqual(1.75f, config.UfoFireRateForWave(2), 0.0001f);
            Assert.AreEqual(1f, config.UfoFireRateForWave(50), 0.0001f);
        }
    }
}
