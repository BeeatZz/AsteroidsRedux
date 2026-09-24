using System.Collections;
using UnityEngine;
using Asteroids.Events;
using Asteroids.Pooling;
using Asteroids.Combat;

namespace Asteroids.Player
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        // Contact with an enemy is always lethal to both sides, regardless of the enemy's remaining health.
        private const int RammingDamage = 9999;

        [Header("Event Channels")]
        [SerializeField] private VoidEventChannelSO onPlayerDiedChannel;

        [Header("Respawn Invincibility")]
        [SerializeField] private float invincibilityDuration = 2f;
        [SerializeField] private float blinkInterval = 0.1f;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private bool isInvincible;
        private Coroutine invincibilityRoutine;

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        private void OnDisable()
        {
            if (invincibilityRoutine != null)
            {
                StopCoroutine(invincibilityRoutine);
                invincibilityRoutine = null;
            }

            isInvincible = false;
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("EnemyBullet"))
            {
                int incomingDamage = collision.TryGetComponent<Bullet>(out var bullet) ? bullet.Damage : 1;

                if (collision.TryGetComponent<PooledObject>(out var poolable))
                {
                    poolable.ReturnToPool();
                }

                TakeDamage(incomingDamage);
            }
            else if (collision.CompareTag("Enemy"))
            {
                if (collision.TryGetComponent<IDamageable>(out var damageable))
                {
                    // Ramming destroys the enemy too, but the player shouldn't be rewarded for dying.
                    damageable.TakeDamage(RammingDamage, awardScore: false);
                }

                TakeDamage(RammingDamage);
            }
        }

        public void TakeDamage(int amount, bool awardScore = true)
        {
            if (isInvincible) return;

            Die();
        }

        public void Die()
        {
            if (onPlayerDiedChannel != null)
            {
                onPlayerDiedChannel.RaiseEvent();
            }

            gameObject.SetActive(false);
        }

        // Called by GameManager once the ship is repositioned and reactivated after a respawn.
        public void BeginInvincibility()
        {
            if (invincibilityRoutine != null)
            {
                StopCoroutine(invincibilityRoutine);
            }

            invincibilityRoutine = StartCoroutine(InvincibilityRoutine());
        }

        private IEnumerator InvincibilityRoutine()
        {
            isInvincible = true;

            float elapsed = 0f;
            while (elapsed < invincibilityDuration)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = !spriteRenderer.enabled;
                }

                yield return new WaitForSeconds(blinkInterval);
                elapsed += blinkInterval;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }

            isInvincible = false;
            invincibilityRoutine = null;
        }
    }
}
