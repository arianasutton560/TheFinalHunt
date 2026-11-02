
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerTorch : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WendigoAtmosphere atmosphere;
    [SerializeField] private GameObject torchHolder;
    [SerializeField] private Animator torchAnimator;
    [SerializeField] private Light torchLight;
    [SerializeField] private GameObject interactionPrompt;

    [Header("Fire Flicker")]
    [SerializeField] private float baseIntensity = 2.5f;
    [SerializeField] private float flickerAmount = 0.35f;
    [SerializeField] private float flickerSpeed = 8f;

    [Header("Animation")]
    [SerializeField] private float equipDuration = 0.65f;

    private bool torchEquipped = false;
    private bool isAnimating = false;

    private void Start()
    {
        // Keep the torch active so its Animator can work.
        if (torchHolder != null)
            torchHolder.SetActive(true);

        // Keep the light off until equipped.
        if (torchLight != null)
            torchLight.enabled = false;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    private void Update()
    {
        if (atmosphere == null || torchHolder == null)
            return;

        bool torchAvailable = atmosphere.IsNight;

        if (!torchAvailable)
        {
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);

            return;
        }

        if (interactionPrompt != null)
            interactionPrompt.SetActive(!torchEquipped && !isAnimating);

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame &&
            !RustlingBush.IsPlayerNearBush &&
            !torchEquipped &&
            !isAnimating)
        {
            StartCoroutine(EquipTorch());
        }

        if (torchEquipped && torchLight != null)
        {
            float noise = Mathf.PerlinNoise(
                Time.time * flickerSpeed,
                0f
            );

            torchLight.intensity =
                baseIntensity +
                (noise * 2f - 1f) * flickerAmount;
        }
    }

    private IEnumerator EquipTorch()
    {
        isAnimating = true;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        // Play the animation.
        if (torchAnimator != null)
            torchAnimator.SetTrigger("Equip");

        // Wait for the equip animation.
        yield return new WaitForSeconds(equipDuration);

        torchEquipped = true;
        isAnimating = false;

        if (torchLight != null)
            torchLight.enabled = true;

        Debug.Log("Torch equipped.");
    }
}
