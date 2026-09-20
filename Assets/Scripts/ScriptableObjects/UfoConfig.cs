using UnityEngine;

namespace Asteroids.ScriptableObjects
{
    [CreateAssetMenu(fileName = "NewUfoConfig", menuName = "Asteroids/Config/UFO Config")]
    public class UfoConfigSO : ScriptableObject
    {
        [Header("Movement Stats")]
        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField] private float screenPadding = 0.05f;

        [Header("Combat Stats")]
        [SerializeField] private float fireRate = 1.8f;
        [SerializeField] private int scoreValue = 200;

        [Header("Audio Parameters")]
        [SerializeField] private float maxAudioDistance = 0.5f;

        public float MoveSpeed => moveSpeed;
        public float ScreenPadding => screenPadding;
        public float FireRate => fireRate;
        public int ScoreValue => scoreValue;
        public float MaxAudioDistance => maxAudioDistance;
    }
}