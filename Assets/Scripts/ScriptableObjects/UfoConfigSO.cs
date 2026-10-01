using UnityEngine;
using Asteroids.Effects;

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
        [SerializeField] private int health = 3;

        [Header("Engage Behaviour")]
        [Tooltip("The UFO picks a new heading after a random time between these two values, in seconds.")]
        [SerializeField] private float minDirectionChangeTime = 1.2f;
        [SerializeField] private float maxDirectionChangeTime = 2.5f;
        [Tooltip("How much of the heading points straight at the player. The rest is sideways strafing.")]
        [Range(0f, 1f)]
        [SerializeField] private float chaseWeight = 0.6f;

        [Header("Visuals")]
        [Tooltip("Degrees per second the sprite spins, visual only. Direction is picked at random on each spawn. 0 keeps it still.")]
        [SerializeField] private float spinSpeed = 90f;

        [Header("Audio Parameters")]
        [SerializeField] private float maxAudioDistance = 0.5f;

        [Header("Effects")]
        [SerializeField] private EffectSO destroyEffect;
        [Tooltip("Screen shake when this UFO is destroyed. Strength 0 turns it off.")]
        [SerializeField] private ScreenShakeSettings destroyShake = new(0.3f, 0.35f, 25f);

        public float MoveSpeed => moveSpeed;
        public float ScreenPadding => screenPadding;
        public float FireRate => fireRate;
        public int ScoreValue => scoreValue;
        public int Health => health;
        public float MinDirectionChangeTime => minDirectionChangeTime;
        public float MaxDirectionChangeTime => maxDirectionChangeTime;
        public float ChaseWeight => chaseWeight;
        public float SpinSpeed => spinSpeed;
        public float MaxAudioDistance => maxAudioDistance;
        public EffectSO DestroyEffect => destroyEffect;
        public ScreenShakeSettings DestroyShake => destroyShake;
    }
}