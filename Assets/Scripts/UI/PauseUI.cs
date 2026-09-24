using UnityEngine;
using UnityEngine.UI;
using Asteroids.Events;
using Asteroids.Managers;

namespace Asteroids.UI
{
    public class PauseUI : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private VoidEventChannelSO onGamePausedChannel;
        [SerializeField] private VoidEventChannelSO onGameResumedChannel;

        [Header("UI Elements")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button resumeButton;

        private void Awake()
        {
            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(HandleResumeClicked);
            }
        }

        private void OnEnable()
        {
            if (onGamePausedChannel != null)
                onGamePausedChannel.OnEventRaised += ShowPausePanel;

            if (onGameResumedChannel != null)
                onGameResumedChannel.OnEventRaised += HidePausePanel;
        }

        private void OnDisable()
        {
            if (onGamePausedChannel != null)
                onGamePausedChannel.OnEventRaised -= ShowPausePanel;

            if (onGameResumedChannel != null)
                onGameResumedChannel.OnEventRaised -= HidePausePanel;
        }

        private void ShowPausePanel()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
            }
        }

        private void HidePausePanel()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
        }

        private void HandleResumeClicked()
        {
            if (PauseManager.Instance != null)
            {
                PauseManager.Instance.Resume();
            }
        }
    }
}
