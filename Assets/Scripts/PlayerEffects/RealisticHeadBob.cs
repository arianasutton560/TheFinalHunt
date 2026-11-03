
using UnityEngine;

public class RealisticHeadBob : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioClip[] footstepClips;

    [Header("Walking Movement")]
    [SerializeField] private float bobFrequency = 1.8f;
    [SerializeField] private float bobHeight = 0.035f;
    [SerializeField] private float bobSideAmount = 0.018f;
    [SerializeField] private float returnSmoothness = 10f;
    [SerializeField] private float minimumSpeed = 0.15f;

    [Header("Footsteps")]
    [SerializeField] private float footstepVolume = 0.45f;

    private Vector3 originalPosition;
    private float bobTimer;
    private int lastFootstep = -1;
    private bool wasMoving;

    private void Awake()
    {
        originalPosition = transform.localPosition;

        if (controller == null)
            controller = GetComponentInParent<CharacterController>();
        if (playerController == null)
        playerController = GetComponentInParent<PlayerController>();
    }

    private void LateUpdate()
    {
        if (controller == null)
            return;

        Vector3 velocity = controller.velocity;
        velocity.y = 0f;

        float speed = playerController != null
        ? playerController.HorizontalSpeed
        : velocity.magnitude;
        //Debug.Log("Player speed: " + speed + " | Grounded: " + controller.isGrounded);

        bool moving =
            controller.isGrounded &&
            speed > minimumSpeed;

        Vector3 targetPosition = originalPosition;

        if (moving)
        {
            // A full cycle represents two footsteps.
            float previousTimer = bobTimer;

            bobTimer += Time.deltaTime *
                bobFrequency * Mathf.PI * 2f;

            float verticalBob =
                Mathf.Sin(bobTimer) * bobHeight;

            float horizontalBob =
                Mathf.Cos(bobTimer) * bobSideAmount;

            targetPosition += new Vector3(
                horizontalBob,
                verticalBob,
                0f
            );

            // Play a footstep every half-cycle.
            int previousStep =
                Mathf.FloorToInt(previousTimer / Mathf.PI);

            int currentStep =
                Mathf.FloorToInt(bobTimer / Mathf.PI);

        }
        else
        {
            // Reset walking rhythm when stopped.
            bobTimer = 0f;
        }

        // Smoothly move camera toward its target.
        float blend = 1f -
            Mathf.Exp(-returnSmoothness * Time.deltaTime);

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPosition,
            blend
        );

        // Start the footstep loop when walking.
        if (footstepSource != null)
        {
            if (moving && !wasMoving)
            {
                if (footstepSource.clip != null)
                {
                    footstepSource.loop = true;
                    footstepSource.volume = footstepVolume;
                    footstepSource.Play();
                }
            }
            else if (!moving && wasMoving)
            {
                footstepSource.Stop();
            }
        }

        wasMoving = moving;
    }

}

