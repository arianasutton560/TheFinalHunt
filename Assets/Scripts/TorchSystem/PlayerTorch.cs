
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTorch : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WendigoAtmosphere atmosphere;
    [SerializeField] private GameObject torchHolder;
    [SerializeField] private Light torchLight;
    [SerializeField] private GameObject interactionPrompt;

    [Header("Fire Flicker")]
    [SerializeField] private float baseIntensity = 2.5f;
    [SerializeField] private float flickerAmount = 0.35f;
    [SerializeField] private float flickerSpeed = 8f;

    private bool torchEquipped = false;

    private void Start()
    {
        // Torch must begin hidden.
        if (torchHolder != null)
            torchHolder.SetActive(false);

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    private void Update()
    {
        if (atmosphere == null || torchHolder == null)
            return;

        // Torch becomes usable only after night has arrived.
        bool torchAvailable = atmosphere.IsNight;

        if (!torchAvailable)
        {
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);

            return;
        }

        // Show prompt when the torch is not equipped.
        if (interactionPrompt != null)
            interactionPrompt.SetActive(!torchEquipped);

        // E toggles the torch on and off.
        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            ToggleTorch();
        }

        // Flickering only while the torch is equipped.
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

    private void ToggleTorch()
    {
        torchEquipped = !torchEquipped;

        torchHolder.SetActive(torchEquipped);

        if (torchEquipped)
        {
            Debug.Log("Torch equipped.");
        }
        else
        {
            Debug.Log("Torch put away.");
        }

        if (interactionPrompt != null)
            interactionPrompt.SetActive(!torchEquipped);
    }
}

