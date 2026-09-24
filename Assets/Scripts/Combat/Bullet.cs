using UnityEngine;
using Asteroids.Pooling;

namespace Asteroids.Combat
{
    [RequireComponent(typeof(Rigidbody2D), typeof(PooledObject))]
    public class Bullet : MonoBehaviour, IPoolable
    {
        [Header("Default Speed Settings")]
        [SerializeField] private float defaultSpeed = 12f;
        [SerializeField] private float defaultLifetime = 3f;
        [SerializeField] private int defaultDamage = 1;

        [Header("Visual Effects")]
        [SerializeField] private TrailRenderer trailRenderer;

        private Rigidbody2D rb;
        private PooledObject pooledObject;
        private float currentLifetime;
        private float maxLifetime;
        private float currentSpeed;
        private int currentDamage;

        public int Damage => currentDamage;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            pooledObject = GetComponent<PooledObject>();

            if (trailRenderer == null)
            {
                trailRenderer = GetComponentInChildren<TrailRenderer>();
            }
        }

        public void OnSpawnFromPool()
        {
            currentLifetime = 0f;
            maxLifetime = defaultLifetime;
            currentSpeed = defaultSpeed;
            currentDamage = defaultDamage;

            if (rb != null)
            {
                rb.linearVelocity = transform.up * currentSpeed;
            }

            if (trailRenderer != null)
            {
                trailRenderer.Clear();
            }
        }

        public void Initialize(float speed, float lifetime, int damage)
        {
            maxLifetime = lifetime;
            currentSpeed = speed;
            currentDamage = damage;

            if (rb != null)
            {
                rb.linearVelocity = transform.up * currentSpeed;
            }
        }

        public void OnReturnToPool()
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            if (trailRenderer != null)
            {
                trailRenderer.Clear();
            }
        }

        private void Update()
        {
            currentLifetime += Time.deltaTime;
            if (currentLifetime >= maxLifetime)
            {
                pooledObject.ReturnToPool();
            }
        }
    }
}