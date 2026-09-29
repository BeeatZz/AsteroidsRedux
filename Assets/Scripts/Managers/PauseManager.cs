using UnityEngine;
using UnityEngine.InputSystem;
using Asteroids.Events;

namespace Asteroids.Managers
{
    public class PauseManager : MonoBehaviour
    {
        public static PauseManager Instance { get; private set; }

        [Header("Event Channels")]
        [SerializeField] private VoidEventChannelSO onGamePausedChannel;
        [SerializeField] private VoidEventChannelSO onGameResumedChannel;
        [SerializeField] private VoidEventChannelSO onGameOverChannel;

        public bool IsPaused { get; private set; }

        // Once the game has ended, Escape/P should no longer toggle pause over the game-over screen.
        private bool isGameOver;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // AudioListener.pause is global and survives scene loads, so a restart must clear it.
            AudioListener.pause = false;
        }

        private void OnEnable()
        {
            if (onGameOverChannel != null)
                onGameOverChannel.OnEventRaised += HandleGameOver;
        }

        private void OnDisable()
        {
            if (onGameOverChannel != null)
                onGameOverChannel.OnEventRaised -= HandleGameOver;
        }

        private void Update()
        {
            if (isGameOver || Keyboard.current == null) return;

            if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame)
            {
                TogglePause();
            }
        }

        private void HandleGameOver()
        {
            isGameOver = true;
        }

        public void TogglePause()
        {
            if (IsPaused) Resume();
            else Pause();
        }

        public void Pause()
        {
            if (IsPaused) return;

            IsPaused = true;
            Time.timeScale = 0f;

            // timeScale doesn't stop audio. This pauses every source except those with
            // ignoreListenerPause set (UISoundPlayer, and MusicPlayer by default).
            AudioListener.pause = true;
            onGamePausedChannel?.RaiseEvent();
        }

        public void Resume()
        {
            if (!IsPaused) return;

            IsPaused = false;
            Time.timeScale = 1f;
            AudioListener.pause = false;
            onGameResumedChannel?.RaiseEvent();
        }
    }
}
