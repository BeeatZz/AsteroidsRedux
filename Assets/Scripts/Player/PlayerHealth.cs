using System.Collections;
using UnityEngine;
using Asteroids.Events;
using Asteroids.Combat;
using Asteroids.ScriptableObjects;

namespace Asteroids.Player
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        // Contact with an enemy is always lethal to both sides, regardless of the enemy's remaining health.
        private const int RammingDamage = 9999;

        [Header("Data Config")]
        [SerializeField] private ShipConfig shipConfig;

        [Header("Event Channels")]
        [SerializeField] private VoidEventChannelSO onPlayerDiedChannel;

        [Header("Respawn Invincibility")]
        [SerializeField] private float invincibilityDuration = 2f;
        [SerializeField] private float blinkInterval = 0.1f;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private bool isInvincible;
        private Coroutine invincibilityRoutine;

        // Set while the ship is in hyperspace; the blink routine must not make it visible.
        private bool isHidden;

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
            isHidden = false;
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }
        }

        // Called by PlayerHyperspace; wins over the invincibility blink while set.
        public void SetHidden(bool hidden)
        {
            isHidden = hidden;
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = !hidden;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("EnemyBullet"))
            {
                int incomingDamage = 1;
                if (collision.TryGetComponent<Bullet>(out var bullet) && !bullet.TryResolveHit(out incomingDamage)) return;

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
            // Two lethal contacts in the same physics step must only cost one life.
            if (!gameObject.activeSelf) return;

            if (shipConfig != null && shipConfig.DeathEffect != null)
            {
                shipConfig.DeathEffect.Play(transform.position);
            }

            if (onPlayerDiedChannel != null)
            {
                onPlayerDiedChannel.RaiseEvent();
            }

            gameObject.SetActive(false);
        }

        // Called by GameManager once the ship is repositioned and reactivated after a respawn.
        public void OnRespawned()
        {
            if (shipConfig != null && shipConfig.RespawnEffect != null)
            {
                shipConfig.RespawnEffect.Play(transform.position);
            }

            BeginInvincibility();
        }

        private void BeginInvincibility()
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
            bool blinkVisible = true;
            while (elapsed < invincibilityDuration)
            {
                blinkVisible = !blinkVisible;
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = blinkVisible && !isHidden;
                }

                yield return new WaitForSeconds(blinkInterval);
                elapsed += blinkInterval;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = !isHidden;
            }

            isInvincible = false;
            invincibilityRoutine = null;
        }
    }
}
