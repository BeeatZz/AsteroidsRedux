using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Data Config")]
    [SerializeField] private ShipConfig shipConfig;

    [Header("Effects")]
    [SerializeField] private AudioSource thrustAudioSource;
    [SerializeField] private ParticleSystem thrustParticles;

    private Rigidbody2D rb;
    private Camera mainCamera;

    // Cached input values captured during frame updates
    private float moveInput;
    private float turnInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;

        // Apply initial physics properties specified in our ScriptableObject asset
        ApplyConfigSettings();
    }

    /* Assigns physical properties like drag directly from the config asset onto the Rigidbody2D.
     * This avoids hardcoding physics behaviors directly inside code.
     */
    private void ApplyConfigSettings()
    {
        if (shipConfig == null) return;

        rb.linearDamping = shipConfig.LinearDrag;
        rb.angularDamping = 0.5f;
    }

    private void Update()
    {
        /* Read player inputs in standard Update to prevent input dropping.
         * Axis values are captured here and processed during FixedUpdate physics passes.
         */
        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");

        HandleThrustEffects();
        WrapScreen();
    }

    private void FixedUpdate()
    {
        if (shipConfig == null) return;

        // FixedUpdate handles physical time integration automatically; omit Time.fixedDeltaTime in forces
        if (turnInput != 0)
        {
            rb.AddTorque(-turnInput * shipConfig.RotationSpeed);
        }
        if (moveInput > 0)
        {
            rb.AddForce(transform.up * (shipConfig.ThrustForce * moveInput));
        }

        // Clamp total linear velocity so force application cannot accelerate ship past MaxSpeed setting
        if (rb.linearVelocity.magnitude > shipConfig.MaxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * shipConfig.MaxSpeed;
        }
    }

    /* Manages audio loops and particle emissions based on player thrust input.
     * Toggling existing components avoids Garbage Collector overhead associated with runtime Instantiate calls.
     */
    private void HandleThrustEffects()
    {
        bool isThrusting = moveInput > 0;

        // Toggle particle emission
        if (thrustParticles != null)
        {
            var emission = thrustParticles.emission;
            emission.enabled = isThrusting;
        }

        // Smoothly fade audio volume up/down
        if (thrustAudioSource != null)
        {
            // Ensure audio source is active and looping
            if (!thrustAudioSource.isPlaying)
            {
                thrustAudioSource.loop = true;
                thrustAudioSource.volume = 0f;
                thrustAudioSource.Play();
            }

            float targetVolume = isThrusting ? 1f : 0f;
            float fadeSpeed = shipConfig != null ? shipConfig.AudioFadeSpeed : 5f;

            thrustAudioSource.volume = Mathf.MoveTowards(
                thrustAudioSource.volume,
                targetVolume,
                fadeSpeed * Time.deltaTime
            );
        }
    }

    /* Converts world position into normalized viewport coordinates (0.0 to 1.0 range).
     * Automatically wraps the player to the opposite side whenever they cross screen boundaries.
     */
    private void WrapScreen()
    {
        Vector3 position = transform.position;
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(position);

        // Horizontal screen wrap
        if (viewportPos.x > 1) viewportPos.x = 0;
        else if (viewportPos.x < 0) viewportPos.x = 1;

        // Vertical screen wrap
        if (viewportPos.y > 1) viewportPos.y = 0;
        else if (viewportPos.y < 0) viewportPos.y = 1;

        // Maintain original world Z position so the camera doesn't lose sight of the transform
        viewportPos.z = mainCamera.WorldToViewportPoint(transform.position).z;
        transform.position = mainCamera.ViewportToWorldPoint(viewportPos);
    }
}