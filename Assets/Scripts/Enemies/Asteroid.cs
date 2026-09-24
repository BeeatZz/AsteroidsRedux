using UnityEngine;
using Asteroids.Events;
using Asteroids.ScriptableObjects;
using Asteroids.Pooling;
using Asteroids.Combat;
using Asteroids.Utility;

namespace Asteroids.Enemies
{
    [RequireComponent(typeof(Rigidbody2D), typeof(PooledObject))]
    public class Asteroid : MonoBehaviour, IPoolable, IDamageable
    {
        private const float ScreenPadding = 0.05f;

        [Header("Config")]
        [SerializeField] private AsteroidConfig config;

        [Header("Event Channels")]
        [SerializeField] private IntEventChannelSO onScoreAddedChannel;

        private Rigidbody2D rb;
        private PooledObject pooledObject;
        private Camera mainCamera;
        private float rotSpeed;
        private int currentHealth;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            pooledObject = GetComponent<PooledObject>();
            mainCamera = Camera.main;
        }

        public void OnSpawnFromPool()
        {
            currentHealth = config != null ? config.Health : 1;

            if (config != null)
            {
                Vector2 randomDirection = Random.insideUnitCircle.normalized;
                rb.linearVelocity = randomDirection * config.MoveSpeed;
                rotSpeed = Random.Range(config.MinRotationSpeed, config.MaxRotationSpeed) * (Random.value > 0.5f ? 1 : -1);
            }
        }

        public void OnReturnToPool()
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        private void Update()
        {
            transform.Rotate(0, 0, rotSpeed * Time.deltaTime);
            WrapScreen();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Bullet"))
            {
                int incomingDamage = collision.TryGetComponent<Bullet>(out var bullet) ? bullet.ResolveHit() : 1;
                TakeDamage(incomingDamage);
            }
        }

        public void TakeDamage(int amount, bool awardScore = true)
        {
            currentHealth -= amount;
            if (currentHealth <= 0)
            {
                DestroyAsteroid(awardScore);
            }
        }

        // awardScore also gates splitting: a ramming kill destroys the asteroid outright,
        // it doesn't fragment it or reward the player, unlike a weapon kill.
        private void DestroyAsteroid(bool awardScore)
        {
            if (awardScore && onScoreAddedChannel != null && config != null)
            {
                onScoreAddedChannel.RaiseEvent(config.ScoreValue);
            }

            if (awardScore && config != null && config.NextSizePrefab != null && ObjectPool.Instance != null)
            {
                for (int i = 0; i < config.SpawnCountOnDestroy; i++)
                {
                    ObjectPool.Instance.Get(config.NextSizePrefab, transform.position, Quaternion.identity);
                }
            }

            // Unlike splitting, the explosion plays for ramming kills too.
            if (config != null && config.DestroyEffect != null)
            {
                config.DestroyEffect.Play(transform.position);
            }

            pooledObject.ReturnToPool();
        }

        private void WrapScreen()
        {
            ScreenWrapper.Wrap(transform, mainCamera, ScreenPadding);
        }
    }
}