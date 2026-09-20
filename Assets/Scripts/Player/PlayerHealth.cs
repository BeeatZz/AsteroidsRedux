using UnityEngine;
using Asteroids.Events;
using Asteroids.Pooling;

namespace Asteroids.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private VoidEventChannelSO onPlayerDiedChannel;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("EnemyBullet") || collision.CompareTag("Enemy"))
            {
                if (collision.TryGetComponent<PooledObject>(out var poolable))
                {
                    poolable.ReturnToPool();
                }

                Die();
            }
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