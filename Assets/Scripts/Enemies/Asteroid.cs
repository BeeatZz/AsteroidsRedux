using UnityEngine;
using Asteroids.Events;
using Asteroids.ScriptableObjects;
using Asteroids.Pooling;

namespace Asteroids.Enemies
{
    [RequireComponent(typeof(Rigidbody2D), typeof(PooledObject))]
    public class Asteroid : MonoBehaviour, IPoolable
    {
        [Header("Config")]
        [SerializeField] private AsteroidConfig config;

        [Header("Event Channels")]
        [SerializeField] private IntEventChannelSO onScoreAddedChannel;

        private Rigidbody2D rb;
        private PooledObject pooledObject;
        private Camera mainCamera;
        private float rotSpeed;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            pooledObject = GetComponent<PooledObject>();
            mainCamera = Camera.main;
        }

        public void OnSpawnFromPool()
        {
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
                if (collision.TryGetComponent<PooledObject>(out var bulletPoolable))
                {
                    bulletPoolable.ReturnToPool();
                }

                DestroyAsteroid();
            }
        }

        private void DestroyAsteroid()
        {
            if (onScoreAddedChannel != null && config != null)
            {
                onScoreAddedChannel.RaiseEvent(config.ScoreValue);
            }

            if (config != null && config.NextSizePrefab != null && ObjectPool.Instance != null)
            {
                for (int i = 0; i < config.SpawnCountOnDestroy; i++)
                {
                    ObjectPool.Instance.Get(config.NextSizePrefab, transform.position, Quaternion.identity);
                }
            }

            pooledObject.ReturnToPool();
        }

        private void WrapScreen()
        {
            if (mainCamera == null) return;

            Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);

            if (viewportPos.x > 1.05f) viewportPos.x = -0.05f;
            else if (viewportPos.x < -0.05f) viewportPos.x = 1.05f;

            if (viewportPos.y > 1.05f) viewportPos.y = -0.05f;
            else if (viewportPos.y < -0.05f) viewportPos.y = 1.05f;

            viewportPos.z = mainCamera.WorldToViewportPoint(transform.position).z;
            transform.position = mainCamera.ViewportToWorldPoint(viewportPos);
        }
    }
}