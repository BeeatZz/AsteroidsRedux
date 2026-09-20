using UnityEngine;

namespace Asteroids.ScriptableObjects
{
    [CreateAssetMenu(fileName = "NewAsteroidConfig", menuName = "Asteroids/Config/Asteroid Config")]
    public class AsteroidConfig : ScriptableObject
    {
        [Header("Gameplay Stats")]
        [SerializeField] private int scoreValue = 100;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float minRotationSpeed = 20f;
        [SerializeField] private float maxRotationSpeed = 100f;

        [Header("Splitting")]
        [SerializeField] private GameObject nextSizePrefab;
        [SerializeField] private int spawnCountOnDestroy = 2;

        public int ScoreValue => scoreValue;
        public float MoveSpeed => moveSpeed;
        public float MinRotationSpeed => minRotationSpeed;
        public float MaxRotationSpeed => maxRotationSpeed;
        public GameObject NextSizePrefab => nextSizePrefab;
        public int SpawnCountOnDestroy => spawnCountOnDestroy;
    }
}