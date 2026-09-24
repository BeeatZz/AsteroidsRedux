using UnityEngine;
using TMPro;
using Asteroids.Events;

namespace Asteroids.UI
{
    public class LivesUI : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private IntEventChannelSO onLivesChangedChannel;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI livesText;

        private void OnEnable()
        {
            if (onLivesChangedChannel != null)
                onLivesChangedChannel.OnEventRaised += UpdateLivesText;
        }

        private void OnDisable()
        {
            if (onLivesChangedChannel != null)
                onLivesChangedChannel.OnEventRaised -= UpdateLivesText;
        }

        private void UpdateLivesText(int newLives)
        {
            if (livesText != null)
            {
                livesText.text = $"LIVES: {Mathf.Max(newLives, 0)}";
            }
        }
    }
}
