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
        [SerializeField] private Button settingsButton;

        [Header("Screens")]
        [SerializeField] private SettingsUI settingsUI;

        private void Awake()
        {
            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(HandleResumeClicked);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(HandleSettingsClicked);
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
            // Resuming with Escape while settings are open should close everything. Closing settings
            // re-shows the pause panel through its callback, so it has to happen first.
            if (settingsUI != null)
            {
                settingsUI.Close();
            }

            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
        }

        private void HandleSettingsClicked()
        {
            if (settingsUI == null) return;

            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }

            settingsUI.Open(ShowPausePanel);
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
