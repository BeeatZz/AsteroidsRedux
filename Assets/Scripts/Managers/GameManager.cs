using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Asteroids.Events;
using Asteroids.Player;
using Asteroids.ScriptableObjects;

namespace Asteroids.Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Config")]
        [Tooltip("Supplies starting lives and the respawn delay.")]
        [SerializeField] private ShipConfig shipConfig;

        [Header("Extra Lives")]
        [Tooltip("A bonus life is awarded every time the score passes a multiple of this value.")]
        [SerializeField] private int extraLifeScoreInterval = 10000;
        [Tooltip("Bonus lives stop being awarded once the player has this many.")]
        [SerializeField] private int maxLives = 9;

        [Header("Scenes")]
        [Tooltip("Scene loaded by ReturnToMainMenu. Must be added to File > Build Profiles > Scene List.")]
        [SerializeField] private string mainMenuSceneName = "MainMenuScene";

        [Header("Event Channels")]
        [SerializeField] private VoidEventChannelSO onPlayerDeathChannel;
        [SerializeField] private IntEventChannelSO onLivesChangedChannel;
        [SerializeField] private VoidEventChannelSO onGameOverChannel;
        [SerializeField] private IntEventChannelSO onScoreChangedChannel;

        private GameObject playerObject;
        private Transform playerTransform;
        private Rigidbody2D playerRigidbody;
        private PlayerHealth playerHealth;
        private Vector3 spawnPosition;
        private Quaternion spawnRotation;

        private int currentLives;
        private int nextExtraLifeScore;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            if (onPlayerDeathChannel != null)
                onPlayerDeathChannel.OnEventRaised += HandlePlayerDeath;

            if (onScoreChangedChannel != null)
                onScoreChangedChannel.OnEventRaised += HandleScoreChanged;
        }

        private void OnDisable()
        {
            if (onPlayerDeathChannel != null)
                onPlayerDeathChannel.OnEventRaised -= HandlePlayerDeath;

            if (onScoreChangedChannel != null)
                onScoreChangedChannel.OnEventRaised -= HandleScoreChanged;
        }

        private void Start()
        {
            CachePlayer();

            currentLives = shipConfig != null ? shipConfig.StartingLives : 3;
            nextExtraLifeScore = extraLifeScoreInterval;
            onLivesChangedChannel?.RaiseEvent(currentLives);
        }

        private void CachePlayer()
        {
            playerObject = GameObject.FindWithTag("Player");
            if (playerObject == null) return;

            playerTransform = playerObject.transform;
            playerRigidbody = playerObject.GetComponent<Rigidbody2D>();
            playerHealth = playerObject.GetComponent<PlayerHealth>();

            // Captured once at level start so a respawn always returns to the original launch point,
            // not wherever the ship happened to die.
            spawnPosition = playerTransform.position;
            spawnRotation = playerTransform.rotation;
        }

        private void HandlePlayerDeath()
        {
            currentLives--;
            onLivesChangedChannel?.RaiseEvent(currentLives);

            if (currentLives > 0)
            {
                StartCoroutine(RespawnPlayerAfterDelay());
            }
            else
            {
                TriggerGameOver();
            }
        }

        private void HandleScoreChanged(int score)
        {
            // Score can still tick up after the last ship dies (e.g. a stray bullet landing).
            if (extraLifeScoreInterval <= 0 || currentLives <= 0) return;

            // Loop so a single large award that crosses several thresholds grants each life.
            while (score >= nextExtraLifeScore)
            {
                nextExtraLifeScore += extraLifeScoreInterval;

                if (currentLives < maxLives)
                {
                    currentLives++;
                    onLivesChangedChannel?.RaiseEvent(currentLives);
                }
            }
        }

        private IEnumerator RespawnPlayerAfterDelay()
        {
            yield return new WaitForSeconds(shipConfig != null ? shipConfig.RespawnDelay : 2f);
            RespawnPlayer();
        }

        private void RespawnPlayer()
        {
            if (playerObject == null) return;

            if (playerRigidbody != null)
            {
                playerRigidbody.linearVelocity = Vector2.zero;
                playerRigidbody.angularVelocity = 0f;
            }

            playerTransform.SetPositionAndRotation(spawnPosition, spawnRotation);
            playerObject.SetActive(true);
            playerHealth?.OnRespawned();
        }

        private void TriggerGameOver()
        {
            Time.timeScale = 0f;
            onGameOverChannel?.RaiseEvent();
        }

        public void RestartGame()
        {
            // SceneLoader resets time once the screen is black, so the game stays frozen while it fades.
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadSceneWithLoadingScreen(SceneManager.GetActiveScene().name);
                return;
            }

            Time.timeScale = 1f;
            AudioListener.pause = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void ReturnToMainMenu()
        {
            // Checked up front so the button fails with a clear message (and the game stays
            // paused) until the main menu scene exists and is in the build.
            if (!Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
            {
                Debug.LogWarning($"Main menu scene '{mainMenuSceneName}' isn't in the build's scene list yet.", this);
                return;
            }

            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene(mainMenuSceneName);
                return;
            }

            // Both survive scene loads, and the menu scene has no PauseManager to reset them.
            Time.timeScale = 1f;
            AudioListener.pause = false;
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
