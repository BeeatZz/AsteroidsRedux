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
    }
}