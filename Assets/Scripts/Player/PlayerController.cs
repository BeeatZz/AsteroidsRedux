using UnityEngine;
using UnityEngine.InputSystem;
using Asteroids.ScriptableObjects;
using Asteroids.Utility;

namespace Asteroids.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Data Config")]
        [SerializeField] private ShipConfig shipConfig;

        [Header("Effects")]
        [SerializeField] private AudioSource thrustAudioSource;
        [SerializeField] private ParticleSystem thrustParticles;

        [Header("Wrapping Offset")]
        [Tooltip("Extra padding to allow full sprite clearance off-screen before wrapping.")]
        [SerializeField] private float wrapPadding = 0.5f;

        private Rigidbody2D rb;
        private Camera mainCamera;

        private float moveInput;
        private float turnInput;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            mainCamera = Camera.main;

            ApplyConfigSettings();
        }

        private void ApplyConfigSettings()
        {
            if (shipConfig == null) return;

            rb.linearDamping = shipConfig.LinearDrag;
            rb.angularDamping = 0f; // Disables physics resistance for manual rotation
        }

        private void Update()
        {
            ReadInput();

            HandleRotation();
            HandleThrustEffects();
            WrapScreen();
        }

        private void FixedUpdate()
        {
            if (shipConfig == null) return;

            // Linear acceleration along local forward/up axis
            if (moveInput > 0)
            {
                rb.AddForce(transform.up * (shipConfig.ThrustForce * moveInput));
            }

            // Clamp total linear velocity
            if (rb.linearVelocity.magnitude > shipConfig.MaxSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * shipConfig.MaxSpeed;
            }
        }

        private void ReadInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                moveInput = 0f;
                turnInput = 0f;
                return;
            }

            moveInput = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) ? 1f : 0f;

            turnInput = 0f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) turnInput = -1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) turnInput = 1f;
        }

        /* Snaps rotation directly to transform rather than applying torque physics.
         * This mimics responsive arcade steering controls.
         */
        private void HandleRotation()
        {
            if (shipConfig == null || turnInput == 0) return;

            float rotationAmount = -turnInput * shipConfig.RotationSpeed * Time.deltaTime;
            transform.Rotate(0, 0, rotationAmount);
        }

        private void HandleThrustEffects()
        {
            bool isThrusting = moveInput > 0;

            if (thrustParticles != null)
            {
                var emission = thrustParticles.emission;
                emission.enabled = isThrusting;
            }

            if (thrustAudioSource != null)
            {
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

        /* Converts world position into viewport space with padding to wrap only after the entire sprite leaves screen view.
         */
        private void WrapScreen()
        {
            ScreenWrapper.WrapWithWorldPadding(transform, mainCamera, wrapPadding);
        }
    }
}
