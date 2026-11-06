
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class FoodPickup : MonoBehaviour
{
    [Header("Hunger Zone")]
    public HungerZone hungerZone;

    [Header("Food Flash")]
    public float pulseSpeed = 2f;
    public float pulseSize = 0.08f;
    public Light foodLight;

    [Header("Interaction")]
    public GameObject interactionPrompt;

    
    [Header("Pickup Camera")]
    public Transform playerCamera;
    public PlayerController playerController;

    public float bendAngle = 55f;
    public float bendDuration = 0.65f;
    public float pickupPause = 0.35f;
    public float returnDuration = 0.8f;

private bool isPickingUp = false;


    private Vector3 originalScale;
    private float originalLightIntensity;

    private bool isHighlighted = false;
    private bool collected = false;
    private bool playerNearby = false;

    void Start()
    {
        originalScale = transform.localScale;

        if (foodLight != null)
        {
            originalLightIntensity = foodLight.intensity;
            foodLight.enabled = false;
        }

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    public void StartHighlight()
    {
        if (collected)
            return;

        isHighlighted = true;

        if (foodLight != null)
            foodLight.enabled = true;
    }

    void Update()
    {
        // Make the food pulse while hunger is active.
        if (isHighlighted && !collected)
        {
            float pulse = (Mathf.Sin(
                Time.time * pulseSpeed *
                Mathf.PI * 2f) + 1f) / 2f;

            transform.localScale = originalScale *
                (1f + pulse * pulseSize);

            if (foodLight != null)
            {
                foodLight.intensity =
                    originalLightIntensity *
                    (0.3f + pulse * 0.7f);
            }
        }

        // Only allow collecting when nearby and hungry.
        if (!playerNearby || collected ||
            hungerZone == null || !hungerZone.HungerActive)
            return;

        if (!isPickingUp &&
        Keyboard.current != null &&
        Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartCoroutine(PickupAnimation());
        }
    }

    
IEnumerator PickupAnimation()
{
    if (playerCamera == null)
        yield break;

    isPickingUp = true;

    if (interactionPrompt != null)
        interactionPrompt.SetActive(false);

    // Temporarily prevent player movement.
    if (playerController != null)
        playerController.enabled = false;

    Quaternion startRotation = playerCamera.localRotation;

    Quaternion downRotation =
        startRotation * Quaternion.Euler(
            bendAngle, 0f, 0f);

    float elapsed = 0f;

    // Slowly bend down.
    while (elapsed < bendDuration)
    {
        elapsed += Time.deltaTime;

        float t = Mathf.SmoothStep(
            0f, 1f,
            Mathf.Clamp01(elapsed /
            Mathf.Max(bendDuration, 0.01f)));

        playerCamera.localRotation =
            Quaternion.Slerp(
                startRotation,
                downRotation,
                t);

        yield return null;
    }

    playerCamera.localRotation = downRotation;

    // Pause briefly while picking up food.
    yield return new WaitForSeconds(pickupPause);

    // Collect the food.
    collected = true;

    if (hungerZone != null)
        hungerZone.CollectFood();

    // Hide the food model but keep this
    // script active until the camera recovers.
    foreach (Renderer foodRenderer
        in GetComponentsInChildren<Renderer>())
    {
        foodRenderer.enabled = false;
    }

    elapsed = 0f;

    // Raise the camera.
    while (elapsed < returnDuration)
    {
        elapsed += Time.deltaTime;

        float t = Mathf.SmoothStep(
            0f, 1f,
            Mathf.Clamp01(elapsed /
            Mathf.Max(returnDuration, 0.01f)));

        playerCamera.localRotation =
            Quaternion.Slerp(
                downRotation,
                startRotation,
                t);

        yield return null;
    }

    playerCamera.localRotation = startRotation;

    if (playerController != null)
        playerController.enabled = true;

    gameObject.SetActive(false);
}


    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() == null)
            return;

        playerNearby = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() == null)
            return;

        playerNearby = false;
    }

    void LateUpdate()
    {
        if (interactionPrompt != null)
        {
            bool canCollect =
            playerNearby &&
            !collected &&
            !isPickingUp &&
            hungerZone != null &&
            hungerZone.HungerActive;
        }
    }

    void CollectFood()
    {
        collected = true;

        if (hungerZone != null)
            hungerZone.CollectFood();

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        gameObject.SetActive(false);
    }
}

