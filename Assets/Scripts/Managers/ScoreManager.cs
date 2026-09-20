using UnityEngine;
using Asteroids.Events;

namespace Asteroids.Managers
{
    public class ScoreManager : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private IntEventChannelSO onScoreAddedChannel;
        [SerializeField] private IntEventChannelSO onScoreChangedChannel; 

        public int CurrentScore { get; private set; }

        private void OnEnable()
        {
            if (onScoreAddedChannel != null)
                onScoreAddedChannel.OnEventRaised += AddScore;
        }

        private void OnDisable()
        {
            if (onScoreAddedChannel != null)
                onScoreAddedChannel.OnEventRaised -= AddScore;
        }

        private void AddScore(int points)
        {
            CurrentScore += points;

            if (onScoreChangedChannel != null)
                onScoreChangedChannel.RaiseEvent(CurrentScore);
        }
    }
}