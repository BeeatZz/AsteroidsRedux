using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Asteroids.ScriptableObjects;
using Asteroids.Effects;

namespace Asteroids.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerHealth))]
    public class PlayerHyperspace : MonoBehaviour
    {
        private const int MaxSafeSpotAttempts = 10;

        [Header("Data Config")]
        [SerializeField] private ShipConfig shipConfig;

        [Header("Re-entry Area")]
        [Tooltip("Keeps re-entry away from the screen edges, in viewport units (0-0.5).")]
        [SerializeField] private float viewportMargin = 0.1f;

        private Rigidbody2D rb;
        private PlayerHealth playerHealth;
        private Camera mainCamera;

        private float nextJumpTime;
        private Coroutine jumpRoutine;

        // Read by PlayerController/PlayerShooter so the ship can't steer or fire while it's gone.
        public bool IsInHyperspace { get; private set; }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            playerHealth = GetComponent<PlayerHealth>();
            mainCamera = Camera.main;
        }

        private void OnDisable()
        {
            if (jumpRoutine != null)
            {
                StopCoroutine(jumpRoutine);
                jumpRoutine = null;
            }

            ExitHyperspace();
        }

        private void Update()
        {
            bool jumpPressed = (Keyboard.current != null && Keyboard.current.leftShiftKey.wasPressedThisFrame)
                || (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame);

            if (jumpPressed)
            {
                TryJump();
            }
        }

        private void TryJump()
        {
            // Same pause guard as PlayerShooter: Update keeps running at timeScale 0.
            if (Time.timeScale <= 0f || shipConfig == null || IsInHyperspace || Time.time < nextJumpTime) return;

            jumpRoutine = StartCoroutine(JumpRoutine());
        }

        private IEnumerator JumpRoutine()
        {
            PlayEffect(shipConfig.HyperspaceExitEffect);
            EnterHyperspace();

            yield return new WaitForSeconds(shipConfig.HyperspaceDuration);

            transform.position = FindReentryPosition();
            ExitHyperspace();
            PlayEffect(shipConfig.HyperspaceEnterEffect);

            // Cooldown starts on re-entry so it isn't eaten by the time spent in hyperspace.
            nextJumpTime = Time.time + shipConfig.HyperspaceCooldown;
            jumpRoutine = null;

            if (Random.value < shipConfig.HyperspaceFailChance)
            {
                // Bypasses respawn invincibility on purpose: a malfunction is not a hit.
                playerHealth.Die();
            }
        }

        private void PlayEffect(EffectSO effect)
        {
            if (effect != null)
            {
                effect.Play(transform.position);
            }
        }

        private void EnterHyperspace()
        {
            IsInHyperspace = true;

            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            // Taking the body out of the simulation also stops its trigger collisions,
            // so nothing can hit the ship while it's gone.
            rb.simulated = false;
            playerHealth.SetHidden(true);
        }

        private void ExitHyperspace()
        {
            if (!IsInHyperspace) return;

            IsInHyperspace = false;
            rb.simulated = true;
            playerHealth.SetHidden(false);
        }

        /* Picks a random on-screen point, preferring one with no enemy nearby.
         * Falls back to the last candidate if no clear spot turns up, so the jump stays a gamble.
         */
        private Vector3 FindReentryPosition()
        {
            if (mainCamera == null) return transform.position;

            Vector3 candidate = transform.position;
            for (int i = 0; i < MaxSafeSpotAttempts; i++)
            {
                candidate = RandomOnScreenPosition();
                if (IsClearOfEnemies(candidate)) break;
            }

            return candidate;
        }

        private Vector3 RandomOnScreenPosition()
        {
            float x = Random.Range(viewportMargin, 1f - viewportMargin);
            float y = Random.Range(viewportMargin, 1f - viewportMargin);
            float depth = Mathf.Abs(mainCamera.transform.position.z);

            Vector3 world = mainCamera.ViewportToWorldPoint(new Vector3(x, y, depth));
            world.z = 0f;
            return world;
        }

        private bool IsClearOfEnemies(Vector3 position)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(position, shipConfig.HyperspaceSafeRadius);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Enemy") || hit.CompareTag("EnemyBullet")) return false;
            }

            return true;
        }
    }
}
