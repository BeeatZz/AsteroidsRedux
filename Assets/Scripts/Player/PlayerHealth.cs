using UnityEngine;
using Asteroids.Events;

namespace Asteroids.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private VoidEventChannelSO onPlayerDiedChannel;

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