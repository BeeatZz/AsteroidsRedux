using UnityEngine;
using Asteroids.Pooling;

namespace Asteroids.Combat
{
    [RequireComponent(typeof(Rigidbody2D), typeof(PooledObject))]
    public class Bullet : MonoBehaviour, IPoolable
    {
        [SerializeField] private TrailRenderer trailRenderer;

        private Rigidbody2D rb;
        private PooledObject pooledObject;
        private float currentLifetime;
        private float maxLifetime;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            pooledObject = GetComponent<PooledObject>();

            // Auto-fetch TrailRenderer if not manually assigned in Inspector
            if (trailRenderer == null)
            {
                trailRenderer = GetComponent<TrailRenderer>();
            }
        }

        public void Initialize(float speed, float lifetime)
        {
            maxLifetime = lifetime;
            currentLifetime = 0f;
            rb.linearVelocity = transform.up * speed;
        }

        public void OnSpawnFromPool()
        {
            currentLifetime = 0f;

            // Clear previous trailing line geometry before displaying new bullet path
            if (trailRenderer != null)
            {
                trailRenderer.Clear();
            }
        }

        public void OnReturnToPool()
        {
            rb.linearVelocity = Vector2.zero;

            // Clear trail history when returning to pool
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