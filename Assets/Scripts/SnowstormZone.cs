
using UnityEngine;

public class SnowstormZone : MonoBehaviour
{
    [Header("Snow Particles")]
    public ParticleSystem snowParticles;

    [Header("Snowstorm Intensity")]
    public float maximumSnowfall = 600f;
    public float fadeInDuration = 10f;
    public float fadeOutDuration = 5f;

    
    [Header("Blizzard Wind Audio")]
    public AudioSource windAudio;
    public float maximumWindVolume = 0.85f;

    // Other scripts can read the current storm intensity.
    public float StormIntensity { get; private set; }


    private bool playerInside = false;
    private float intensity = 0f;

    void Start()
    {
        if (windAudio != null)
        {
            windAudio.Stop();
            windAudio.volume = 0f;
            windAudio.loop = true;
        }
        if (snowParticles != null)
        {
            var emission = snowParticles.emission;
            emission.rateOverTime = 0f;

            snowParticles.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() == null)
            return;

        playerInside = true;

        if (snowParticles != null &&
            !snowParticles.isPlaying)
        {
            snowParticles.Play();
        }
        if (windAudio != null && !windAudio.isPlaying)
        {
            windAudio.Play();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() == null)
            return;

        playerInside = false;
    }

    void Update()
    {
        if (snowParticles == null)
            return;

        float target = playerInside ? 1f : 0f;

        float duration = playerInside
            ? fadeInDuration
            : fadeOutDuration;

        intensity = Mathf.MoveTowards(
            intensity,
            target,
            Time.deltaTime / Mathf.Max(duration, 0.01f));

        float strength = Mathf.SmoothStep(
            0f, 1f, intensity);

        // Share storm strength with the atmosphere system.
        StormIntensity = strength;

        // Wind gets louder as the snowstorm grows.
        if (windAudio != null)
        {
            windAudio.volume = maximumWindVolume * strength;

            if (!playerInside && intensity <= 0f &&
                windAudio.isPlaying)
            {
                windAudio.Stop();
            }
        }

        var emission = snowParticles.emission;

        emission.rateOverTime =
            maximumSnowfall * strength;

        // Stop emitting once the storm fades.
        if (!playerInside && intensity <= 0f &&
            snowParticles.isEmitting)
        {
            snowParticles.Stop(
                true,
                ParticleSystemStopBehavior.StopEmitting);
        }
    }
}

