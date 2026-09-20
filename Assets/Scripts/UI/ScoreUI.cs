using UnityEngine;
using TMPro; 
using Asteroids.Events;

namespace Asteroids.UI
{
    public class ScoreUI : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private IntEventChannelSO onScoreChangedChannel;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI scoreText;

        private void OnEnable()
        {
            if (onScoreChangedChannel != null)
                onScoreChangedChannel.OnEventRaised += UpdateScoreText;
        }

        private void OnDisable()
        {
            if (onScoreChangedChannel != null)
                onScoreChangedChannel.OnEventRaised -= UpdateScoreText;
        }

        private void UpdateScoreText(int newScore)
        {
            if (scoreText != null)
            {
                scoreText.text = $"SCORE: {newScore:D6}";
            }
        }
    }
}