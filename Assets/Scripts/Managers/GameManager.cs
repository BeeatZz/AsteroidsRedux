using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Asteroids.Events;
using Asteroids.Player;

namespace Asteroids.Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Config")]
        [SerializeField] private int startingLives = 3;
        [SerializeField] private float respawnDelay = 2f;

        [Header("Extra Lives")]
        [Tooltip("A bonus life is awarded every time the score passes a multiple of this value.")]
        [SerializeField] private int extraLifeScoreInterval = 10000;
        [Tooltip("Bonus lives stop being awarded once the player has this many.")]
        [SerializeField] private int maxLives = 9;

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

            currentLives = startingLives;
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
            yield return new WaitForSeconds(respawnDelay);
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
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
