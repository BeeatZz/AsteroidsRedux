using UnityEngine;
using UnityEngine.UI;
using Asteroids.Events;
using Asteroids.Managers;

namespace Asteroids.UI
{
    public class GameOverUI : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private VoidEventChannelSO onGameOverChannel;

        [Header("UI Elements")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Button restartButton;

        private void Awake()
        {
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(HandleRestartClicked);
            }
        }

        private void OnEnable()
        {
            if (onGameOverChannel != null)
                onGameOverChannel.OnEventRaised += ShowGameOverPanel;
        }

        private void OnDisable()
        {
            if (onGameOverChannel != null)
                onGameOverChannel.OnEventRaised -= ShowGameOverPanel;
        }

        private void ShowGameOverPanel()
        {
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
    }
}
