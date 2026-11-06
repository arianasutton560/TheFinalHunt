
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class FoodPickup : MonoBehaviour
{
    [Header("Hunger Zone")]
    public HungerZone hungerZone;

    [Header("Food Flash")]
    public float pulseSpeed = 2f;
    public float pulseSize = 0.08f;
    public Light foodLight;

    [Header("Interaction")]
    public Transform player;
    public float interactionDistance = 3f;
    public GameObject interactionPrompt;

    [Header("Pickup Camera")]
    public Transform pickupPivot;
    public PlayerController playerController;

    public float bendDistance = 0.65f;
    public float forwardDistance = 0.15f;
    public float bendDuration = 0.7f;
    public float pickupPause = 0.4f;
    public float returnDuration = 0.8f;

    private Vector3 originalScale;
    private float originalLightIntensity;

    private bool isHighlighted = false;
    private bool collected = false;
    private bool isPickingUp = false;

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
        // Make the food pulse.
        if (isHighlighted && !collected)
        {
            float pulse = (Mathf.Sin(
                Time.time * pulseSpeed *
                Mathf.PI * 2f) + 1f) / 2f;

            transform.localScale =
                originalScale * (1f + pulse * pulseSize);

            if (foodLight != null)
            {
                foodLight.intensity =
                    originalLightIntensity *
                    (0.3f + pulse * 0.7f);
            }
        }

        if (player == null || hungerZone == null)
            return;

        float distance = Vector3.Distance(
            player.position,
            transform.position);

        bool canCollect =
            distance <= interactionDistance &&
            hungerZone.HungerActive &&
            !collected &&
            !isPickingUp;

        // Show or hide the E prompt.
        if (interactionPrompt != null)
            interactionPrompt.SetActive(canCollect);

        if (canCollect &&
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartCoroutine(PickupAnimation());
        }
    }

    IEnumerator PickupAnimation()
    {
        if (pickupPivot == null)
        {
            Debug.LogWarning("PickupPivot is not assigned!");
            yield break;
        }

        isPickingUp = true;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        // Stop player movement during pickup.
        if (playerController != null)
            playerController.enabled = false;

        Vector3 startPosition = pickupPivot.localPosition;

        Vector3 downPosition =
            startPosition +
            new Vector3(
                0f,
                -bendDistance,
                forwardDistance);

        float elapsed = 0f;

        // Lower the camera.
        while (elapsed < bendDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.SmoothStep(
                0f, 1f,
                Mathf.Clamp01(elapsed /
                Mathf.Max(bendDuration, 0.01f)));

            pickupPivot.localPosition =
                Vector3.Lerp(
                    startPosition,
                    downPosition,
                    t);

            yield return null;
        }

        pickupPivot.localPosition = downPosition;

        yield return new WaitForSeconds(pickupPause);

        // Collect the food.
        collected = true;

        if (hungerZone != null)
            hungerZone.CollectFood();

        // Hide the food model.
        foreach (Renderer foodRenderer in
            GetComponentsInChildren<Renderer>())
        {
            foodRenderer.enabled = false;
        }

        if (foodLight != null)
            foodLight.enabled = false;

        // Return the camera to standing height.
        elapsed = 0f;

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.SmoothStep(
                0f, 1f,
                Mathf.Clamp01(elapsed /
                Mathf.Max(returnDuration, 0.01f)));

            pickupPivot.localPosition =
                Vector3.Lerp(
                    downPosition,
                    startPosition,
                    t);

            yield return null;
        }

        pickupPivot.localPosition = startPosition;

        if (playerController != null)
            playerController.enabled = true;

        gameObject.SetActive(false);
    }
}
