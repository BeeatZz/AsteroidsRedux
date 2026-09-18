using UnityEngine;

// This lets us create the scriptable object from the context menu
[CreateAssetMenu(fileName = "NewShipConfig", menuName = "Asteroids/Config/Ship Config")]
public class ShipConfig : ScriptableObject
{
    /* After this lines, we have all the elements that are configurable for a ship.
     * The main difference between this and creating a ship class, is that a scriptable object
     * allows us to not only modify stats in runtime without recompiling, but it also lets us
     * create different ship variations without new classes, which in turn reduces bloat in the project files.
     * They are also a good way to let less code-savy people actually balance and create new ships, as long as 
     * we keep all our systems generic for the base ship class that all ships will use
     */
    [Header("Movement Settings")]
    [Tooltip("Forward acceleration force.")]
    [SerializeField] private float thrustForce = 10f;

    [Tooltip("Rotation speed in degrees per second.")]
    [SerializeField] private float rotationSpeed = 180f;

    [Tooltip("Maximum velocity ship can reach.")]
    [SerializeField] private float maxSpeed = 12f;

    [Tooltip("Drag applied when not thrusting.")]
    [SerializeField] private float linearDrag = 0.5f;

    [Header("Visuals & Audio")]
    [SerializeField] private GameObject thrustParticlesPrefab;
    [SerializeField] private GameObject explosionParticlesPrefab;
    [SerializeField] private AudioClip thrustAudioClip;
    [SerializeField] private AudioClip deathAudioClip;

    [Header("Audio Tuning")]
    [SerializeField] private float audioFadeSpeed = 5f;
    public float AudioFadeSpeed => audioFadeSpeed;

    // Public getters expose data without allowing runtime modification of the asset
    public float ThrustForce => thrustForce;
    public float RotationSpeed => rotationSpeed;
    public float MaxSpeed => maxSpeed;
    public float LinearDrag => linearDrag;

    public GameObject ThrustParticlesPrefab => thrustParticlesPrefab;
    public GameObject ExplosionParticlesPrefab => explosionParticlesPrefab;
    public AudioClip ThrustAudioClip => thrustAudioClip;
    public AudioClip DeathAudioClip => deathAudioClip;
}