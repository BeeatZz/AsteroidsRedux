using UnityEngine;
using Asteroids.Events;
using Asteroids.Pooling;
using Asteroids.ScriptableObjects;
using Asteroids.Combat;
using Asteroids.Utility;

namespace Asteroids.Enemies.AI
{
    [RequireComponent(typeof(Rigidbody2D), typeof(PooledObject))]
    public class UfoController : MonoBehaviour, IPoolable, IDamageable
    {
        [Header("Configuration")]
        [SerializeField] private UfoConfigSO config;

        [Header("Event Channels")]
        [SerializeField] private IntEventChannelSO onScoreAddedChannel;
        [Tooltip("Raised with this enemy's position once it's out of play, however it died.")]
        [SerializeField] private Vector3EventChannelSO onEnemyDestroyedChannel;
        [Tooltip("Sends the config's Destroy Shake to CameraShake.")]
        [SerializeField] private ScreenShakeEventChannelSO onScreenShakeChannel;

        [Header("Combat Setup")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform firePoint;

        [Header("Visuals")]
        [Tooltip("Child holding the sprite. Only this spins, so the collider and fire point stay put.")]
        [SerializeField] private Transform visual;

        [Header("Audio Components")]
        [SerializeField] private AudioSource engineAudioSource;
        [SerializeField] private AudioSource laserAudioSource;
        [SerializeField] private AudioClip fireSoundClip;

        private Rigidbody2D rb;
        private PooledObject pooledObject;
        private Transform playerTransform;
        private Camera mainCamera;
        private int currentHealth;
        private float? fireRateOverride;
        private float spinDirection = 1f;

        public StateMachine StateMachine { get; private set; }
        public UfoEntryState EntryState { get; private set; }
        public UfoEngageState EngageState { get; private set; }

        public UfoConfigSO Config => config;
        public float MoveSpeed => config != null ? config.MoveSpeed : 3.5f;
        public float MinDirectionChangeTime => config != null ? config.MinDirectionChangeTime : 1.2f;
        public float MaxDirectionChangeTime => config != null ? config.MaxDirectionChangeTime : 2.5f;
        public float ChaseWeight => config != null ? config.ChaseWeight : 0.6f;
        public float FireRate => fireRateOverride ?? (config != null ? config.FireRate : 1.8f);

        // Lets a spawner (e.g. WaveManager) ramp difficulty per-wave without needing
        // a separate config asset per wave; null falls back to the shared config value.
        public void SetFireRate(float fireRate)
        {
            fireRateOverride = fireRate;
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            pooledObject = GetComponent<PooledObject>();
            mainCamera = Camera.main;

            // Normally set on spawn; covers a UFO placed directly in a scene.
            currentHealth = config != null ? config.Health : 1;

            StateMachine = new StateMachine();
            EntryState = new UfoEntryState(this, StateMachine);
            EngageState = new UfoEngageState(this, StateMachine);
        }

        private void Start()
        {
            FindPlayer();
            StateMachine.Initialize(EntryState);
        }

        public void OnSpawnFromPool()
        {
            currentHealth = config != null ? config.Health : 1;
            spinDirection = Random.value > 0.5f ? 1f : -1f;
            FindPlayer();
            if (StateMachine != null && EntryState != null)
            {
                StateMachine.Initialize(EntryState);
            }
        }

        public void OnReturnToPool()
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            if (engineAudioSource != null) engineAudioSource.Stop();
            fireRateOverride = null;
            if (visual != null) visual.localRotation = Quaternion.identity;
        }

        private void Update()
        {
            StateMachine.Update();
            UpdateAudioVolume();
            Spin();
        }

        // Game time, so the spin freezes on pause and slows with the wave-clear slow motion.
        private void Spin()
        {
            if (visual == null || config == null) return;
            visual.Rotate(0f, 0f, config.SpinSpeed * spinDirection * Time.deltaTime);
        }

        private void FixedUpdate()
        {
            StateMachine.FixedUpdate();
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
            // without this it would score and explode a second time.
            if (currentHealth <= 0) return;

            currentHealth -= amount;
            if (currentHealth <= 0)
            {
                Die(awardScore);
            }
        }

        private void Die(bool awardScore)
        {
            if (awardScore && onScoreAddedChannel != null && config != null)
            {
                onScoreAddedChannel.RaiseEvent(config.ScoreValue);
            }

            if (config != null && config.DestroyEffect != null)
            {
                config.DestroyEffect.Play(transform.position);
            }

            if (config != null && config.DestroyShake.IsActive)
            {
                onScreenShakeChannel?.RaiseEvent(config.DestroyShake);
            }

            Vector3 position = transform.position;

            if (pooledObject != null && !string.IsNullOrEmpty(pooledObject.PoolKey))
            {
                pooledObject.ReturnToPool();
            }
            else
            {
                // Destroy only takes effect at the end of the frame; deactivating first takes the
                // UFO out of play now, so listeners counting what's left don't still see it.
                gameObject.SetActive(false);
                Destroy(gameObject);
            }

            onEnemyDestroyedChannel?.RaiseEvent(position);
        }

        public void Move(Vector2 direction)
        {
            rb.linearVelocity = direction * MoveSpeed;
        }

        public bool IsInsideScreen()
        {
            if (mainCamera == null) return false;

            float padding = config != null ? config.ScreenPadding : 0.05f;
            Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);

            return viewportPos.x >= (0f + padding) && viewportPos.x <= (1f - padding) &&
                   viewportPos.y >= (0f + padding) && viewportPos.y <= (1f - padding);
        }

        public bool HasPlayerTarget()
        {
            if (playerTransform == null || !playerTransform.gameObject.activeInHierarchy)
            {
                FindPlayer();
            }
            return playerTransform != null && playerTransform.gameObject.activeInHierarchy;
        }

        public Vector2 GetDirectionToPlayer()
        {
            if (!HasPlayerTarget()) return Vector2.right;
            return (playerTransform.position - transform.position).normalized;
        }

        public Vector2 GetDirectionToScreenCenter()
        {
            if (mainCamera == null) return Vector2.right;
            Vector3 centerWorld = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, mainCamera.nearClipPlane));
            return ((Vector2)centerWorld - (Vector2)transform.position).normalized;
        }

        public void ShootAtPlayer()
        {
            if (!IsInsideScreen() || bulletPrefab == null || ObjectPool.Instance == null) return;

            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
            Vector2 direction = GetDirectionToPlayer();
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            Quaternion spawnRot = Quaternion.AngleAxis(angle, Vector3.forward);

            ObjectPool.Instance.Get(bulletPrefab, spawnPos, spawnRot);

            if (laserAudioSource != null && fireSoundClip != null)
            {
                laserAudioSource.PlayOneShot(fireSoundClip);
            }
        }

        public void WrapScreen()
        {
            float padding = config != null ? config.ScreenPadding : 0.05f;
            ScreenWrapper.Wrap(transform, mainCamera, padding);
        }

        private void UpdateAudioVolume()
        {
            if (engineAudioSource == null || mainCamera == null) return;

            if (!engineAudioSource.isPlaying)
            {
                engineAudioSource.loop = true;
                engineAudioSource.Play();
            }

            if (IsInsideScreen())
            {
                engineAudioSource.volume = 1f;
            }
            else
            {
                Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);

                float distanceX = Mathf.Max(0f, -viewportPos.x, viewportPos.x - 1f);
                float distanceY = Mathf.Max(0f, -viewportPos.y, viewportPos.y - 1f);
                float distanceOffscreen = Mathf.Sqrt(distanceX * distanceX + distanceY * distanceY);

                float maxDistance = config != null ? config.MaxAudioDistance : 0.5f;
                float normalizedVolume = Mathf.Clamp01(1f - (distanceOffscreen / maxDistance));

                engineAudioSource.volume = normalizedVolume;
            }
        }

        private void FindPlayer()
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }
    }
}