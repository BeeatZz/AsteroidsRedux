using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Asteroids.Events;
using Asteroids.Managers;

namespace Asteroids.UI
{
    public class GameOverUI : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private VoidEventChannelSO onGameOverChannel;
        [SerializeField] private IntEventChannelSO onScoreChangedChannel;

        [Header("UI Elements")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Final Score")]
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [Tooltip("{0} is the score. D6 pads it to six digits, matching the in-game score.")]
        [SerializeField] private string finalScoreFormat = "FINAL SCORE: {0:D6}";

        // Tracked from the score channel so this screen doesn't need a reference to ScoreManager.
        private int latestScore;

        private void Awake()
        {
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(HandleRestartClicked);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(HandleMainMenuClicked);
            }
        }

        private void OnEnable()
        {
            if (onGameOverChannel != null)
                onGameOverChannel.OnEventRaised += ShowGameOverPanel;

            if (onScoreChangedChannel != null)
                onScoreChangedChannel.OnEventRaised += HandleScoreChanged;
        }

        private void OnDisable()
        {
            if (onGameOverChannel != null)
                onGameOverChannel.OnEventRaised -= ShowGameOverPanel;

            if (onScoreChangedChannel != null)
                onScoreChangedChannel.OnEventRaised -= HandleScoreChanged;
        }

        private void HandleScoreChanged(int score)
        {
            latestScore = score;
        }

        private void ShowGameOverPanel()
        {
            if (finalScoreText != null)
            {
                finalScoreText.text = string.Format(finalScoreFormat, latestScore);
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }
        }

        private void HandleRestartClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
            }
        }

        private void HandleMainMenuClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ReturnToMainMenu();
            }
        }
    }
}
