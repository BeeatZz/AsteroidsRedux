using NUnit.Framework;
using Asteroids.Enemies;

namespace Asteroids.Tests
{
    public class EnemyTrackerTests
    {
        [SetUp]
        public void SetUp() => EnemyTracker.Reset();

        [TearDown]
        public void TearDown() => EnemyTracker.Reset();

        [Test]
        public void StartsEmpty()
        {
            Assert.AreEqual(0, EnemyTracker.TotalCount);
        }

        [Test]
        public void CountsAsteroidsAndUfosSeparately()
        {
            EnemyTracker.AsteroidEnabled();
            EnemyTracker.AsteroidEnabled();
            EnemyTracker.UfoEnabled();

            Assert.AreEqual(2, EnemyTracker.AsteroidCount);
            Assert.AreEqual(1, EnemyTracker.UfoCount);
            Assert.AreEqual(3, EnemyTracker.TotalCount);
        }

        [Test]
        public void DisablingEnemies_BringsTheCountsBackDown()
        {
            EnemyTracker.AsteroidEnabled();
            EnemyTracker.UfoEnabled();

            EnemyTracker.AsteroidDisabled();
            EnemyTracker.UfoDisabled();

            Assert.AreEqual(0, EnemyTracker.TotalCount);
        }

        [Test]
        public void CountsNeverGoNegative()
        {
            EnemyTracker.AsteroidDisabled();
            EnemyTracker.UfoDisabled();

            Assert.AreEqual(0, EnemyTracker.AsteroidCount);
            Assert.AreEqual(0, EnemyTracker.UfoCount);
        }

        [Test]
        public void Reset_ClearsEverything()
        {
            EnemyTracker.AsteroidEnabled();
            EnemyTracker.UfoEnabled();

            EnemyTracker.Reset();

            Assert.AreEqual(0, EnemyTracker.TotalCount);
        }
    }
}
