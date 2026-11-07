
using UnityEngine;
using UnityEngine.Rendering;

public class HungerZone : MonoBehaviour
{
    [Header("Heartbeat")]
    public AudioSource heartbeatAudio;
    public float maximumVolume = 0.8f;

    [Header("Red Screen")]
    public CanvasGroup hungerRedOverlay;
    public float redFlashSpeed = 2f;
    public float maximumRedAlpha = 0.35f;

    
    [Header("Hunger Vision Distortion")]
    public Volume hungerVisionVolume;
    public float maximumDistortion = 0.6f;


    [Header("Transition")]
    public float fadeInDuration = 3f;
    public float fadeOutDuration = 3f;

    [Header("Food")]
    public FoodPickup food;

    private bool hungerStarted = false;
    private bool foodCollected = false;
    private float intensity = 0f;

    [Header("Movement Slowdown")]
    public PlayerController playerController;

    [Range(0.1f, 1f)]
    public float hungerSlowMultiplier = 0.4f;

    public bool HungerActive
    {
        get { return hungerStarted && !foodCollected; }
    }

    void Start()
    {
        if (hungerVisionVolume != null)
            hungerVisionVolume.weight = 0f;

        if (hungerRedOverlay != null)
            hungerRedOverlay.alpha = 0f;

        if (heartbeatAudio != null)
        {
            heartbeatAudio.Stop();
            heartbeatAudio.volume = 0f;
            heartbeatAudio.loop = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() == null)
            return;

        if (hungerStarted)
            return;

        hungerStarted = true;

        if (heartbeatAudio != null)
            heartbeatAudio.Play();

        if (food != null)
            food.StartHighlight();
    }

    void Update()
    {
        float target = HungerActive ? 1f : 0f;

        float duration = HungerActive
            ? fadeInDuration
            : fadeOutDuration;

        intensity = Mathf.MoveTowards(
            intensity,
            target,
            Time.deltaTime / Mathf.Max(duration, 0.01f));

        float strength = Mathf.SmoothStep(
            0f, 1f, intensity);
                    
        if (hungerVisionVolume != null)
        {
            hungerVisionVolume.weight =
                strength * maximumDistortion;
        }
                    
        if (playerController != null)
        {
            playerController.hungerSpeedMultiplier =
                Mathf.Lerp(
                    1f,
                    hungerSlowMultiplier,
                    strength);
        }

        // Flash the screen red.
        if (hungerRedOverlay != null)
        {
            float flash = (Mathf.Sin(
                Time.time * redFlashSpeed *
                Mathf.PI * 2f) + 1f) / 2f;

            hungerRedOverlay.alpha =
                flash * maximumRedAlpha * strength;
        }

        // Increase or decrease the heartbeat volume.
        if (heartbeatAudio != null)
        {
            heartbeatAudio.volume =
                maximumVolume * strength;

            if (foodCollected &&
                intensity <= 0f &&
                heartbeatAudio.isPlaying)
            {
                heartbeatAudio.Stop();
            }
        }
    }

    void OnDisable()
    {
        if (playerController != null)
            playerController.hungerSpeedMultiplier = 1f;

        if (hungerVisionVolume != null)
            hungerVisionVolume.weight = 0f;
    }

    public void CollectFood()
    {
        if (!HungerActive)
            return;

        foodCollected = true;
    }
}

