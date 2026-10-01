using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Asteroids.Managers;

namespace Asteroids.UI
{
    /* Title screen: Play loads the gameplay scene (through the loading screen), Settings reuses the same SettingsUI as the
     * pause menu, and Quit exits. Lives on the menu Canvas, like the other UI scripts.
     */
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Scenes")]
        [Tooltip("Scene loaded by Play. Must be added to File > Build Profiles > Scene List.")]
        [SerializeField] private string gameSceneName = "GameScene";

        [Header("UI Elements")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Screens")]
        [SerializeField] private SettingsUI settingsUI;

        private void Awake()
        {
            if (playButton != null)
            {
                playButton.onClick.AddListener(HandlePlayClicked);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(HandleSettingsClicked);
            }

            if (quitButton != null)
            {
                // A browser tab can't be closed from inside the game, so the button would do nothing.
                if (Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    quitButton.gameObject.SetActive(false);
                }
                else
                {
                    quitButton.onClick.AddListener(HandleQuitClicked);
                }
            }
        }

        private void Start()
        {
            // Returning from a paused game or the game-over screen; GameManager resets these too,
            // but the menu can also be the first scene played in the editor.
            Time.timeScale = 1f;
            AudioListener.pause = false;

            ShowMainPanel();
        }

        private void Update()
        {
            // Escape backs out of settings, matching the Back button.
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame
                && settingsUI != null && settingsUI.IsOpen)
            {
                settingsUI.Close();
            }
        }

        private void ShowMainPanel()
        {
            if (mainPanel != null)
            {
                mainPanel.SetActive(true);
            }
        }

        private void HandlePlayClicked()
        {
            // Checked up front so a missing build entry fails with a clear message.
            if (!Application.CanStreamedLevelBeLoaded(gameSceneName))
            {
                Debug.LogWarning($"Game scene '{gameSceneName}' isn't in the build's scene list.", this);
                return;
            }

            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadSceneWithLoadingScreen(gameSceneName);
            }
            else
            {
                SceneManager.LoadScene(gameSceneName);
            }
        }

        private void HandleSettingsClicked()
        {
            if (settingsUI == null) return;

            if (mainPanel != null)
            {
                mainPanel.SetActive(false);
            }

            settingsUI.Open(ShowMainPanel);
        }

        private void HandleQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
