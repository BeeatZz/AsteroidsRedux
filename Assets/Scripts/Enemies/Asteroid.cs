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
        private const float FallbackWrapPadding = 0.5f;

        [Header("Config")]
        [SerializeField] private AsteroidConfig config;

        [Header("Event Channels")]
        [SerializeField] private IntEventChannelSO onScoreAddedChannel;

        private Rigidbody2D rb;
        private PooledObject pooledObject;
        private Camera mainCamera;
        private SpriteRenderer spriteRenderer;
        private float wrapPadding = FallbackWrapPadding;
        private float rotSpeed;
        private int currentHealth;
        private float speedMultiplier = 1f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            pooledObject = GetComponent<PooledObject>();
            mainCamera = Camera.main;
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            // Normally set on spawn; covers an asteroid placed directly in a scene.
            currentHealth = config != null ? config.Health : 1;
        }

        public void OnSpawnFromPool()
        {
            currentHealth = config != null ? config.Health : 1;
            wrapPadding = CalculateWrapPadding();

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
            speedMultiplier = 1f;
        }

        // Lets a spawner (e.g. WaveManager) scale speed per-wave; split fragments
        // inherit it so later waves don't slow down as asteroids break apart.
        public void SetSpeedMultiplier(float multiplier)
        {
            speedMultiplier = multiplier;

            if (config != null)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * (config.MoveSpeed * speedMultiplier);
            }
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
                int incomingDamage = 1;
                if (collision.TryGetComponent<Bullet>(out var bullet) && !bullet.TryResolveHit(out incomingDamage)) return;

                TakeDamage(incomingDamage);
            }
        }

        public void TakeDamage(int amount, bool awardScore = true)
        {
            // Already destroyed earlier this physics step (e.g. two bullets landing at once);
            // without this it would score, split and explode a second time.
            if (currentHealth <= 0) return;

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
                    GameObject fragment = ObjectPool.Instance.Get(config.NextSizePrefab, transform.position, Quaternion.identity);

                    if (fragment != null && fragment.TryGetComponent<Asteroid>(out var fragmentAsteroid))
                    {
                        fragmentAsteroid.SetSpeedMultiplier(speedMultiplier);
                    }
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
            ScreenWrapper.WrapWithWorldPadding(transform, mainCamera, wrapPadding);
        }

        // Half the sprite's size in world units, so each asteroid size wraps exactly when
        // it fully leaves the screen. Measured on spawn, when the pool has just reset
        // rotation to identity, so the AABB isn't inflated by the sprite's spin.
        private float CalculateWrapPadding()
        {
            if (spriteRenderer == null) return FallbackWrapPadding;

            Vector3 extents = spriteRenderer.bounds.extents;
            return Mathf.Max(extents.x, extents.y);
        }
    }
}