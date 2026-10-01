using UnityEngine;
using Asteroids.Effects;

namespace Asteroids.ScriptableObjects
{
    [CreateAssetMenu(fileName = "NewAsteroidConfig", menuName = "Asteroids/Config/Asteroid Config")]
    public class AsteroidConfig : ScriptableObject
    {
        [Header("Gameplay Stats")]
        [SerializeField] private int scoreValue = 100;
        [SerializeField] private int health = 1;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float minRotationSpeed = 20f;
        [SerializeField] private float maxRotationSpeed = 100f;

        [Header("Visuals")]
        [Tooltip("Each asteroid picks one of these at random when it spawns. Leave empty to keep the prefab's sprite.")]
        [SerializeField] private Sprite[] sprites;

        [Header("Splitting")]
        [SerializeField] private GameObject nextSizePrefab;
        [SerializeField] private int spawnCountOnDestroy = 2;

        [Header("Effects")]
        [SerializeField] private EffectSO destroyEffect;
        [Tooltip("Screen shake when this asteroid is destroyed. Strength 0 turns it off.")]
        [SerializeField] private ScreenShakeSettings destroyShake = new(0.15f, 0.25f, 25f);

        public int ScoreValue => scoreValue;
        public int Health => health;
        public float MoveSpeed => moveSpeed;
        public float MinRotationSpeed => minRotationSpeed;
        public float MaxRotationSpeed => maxRotationSpeed;
        public Sprite[] Sprites => sprites;
        public GameObject NextSizePrefab => nextSizePrefab;
        public int SpawnCountOnDestroy => spawnCountOnDestroy;
        public EffectSO DestroyEffect => destroyEffect;
        public ScreenShakeSettings DestroyShake => destroyShake;
    }
}