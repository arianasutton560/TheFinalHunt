
using System.Collections;
using UnityEngine;

public class DistortionZone : MonoBehaviour
{
    public PlayerVisionDistortion playerVision;

    [Header("Disorientation Audio")]
    public AudioSource distortionAudio;
    public float maximumVolume = 0.7f;
    public float fadeInDuration = 3f;
    public float fadeOutDuration = 4f;

    private bool hasTriggered = false;
    private bool playerInside = false;
    private Coroutine audioFade;

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() == null)
            return;

        // Each zone can only activate once.
        if (hasTriggered)
            return;

    Debug.Log("Activating distortion audio!");
        hasTriggered = true;
        playerInside = true;

        if (playerVision != null)
            playerVision.EnterZone(this);

        if (distortionAudio != null)
        {
            distortionAudio.volume = 0f;
            distortionAudio.Play();

            audioFade = StartCoroutine(
                FadeAudio(maximumVolume, fadeInDuration));
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() == null)
            return;

        if (!playerInside)
            return;

        playerInside = false;

        if (playerVision != null)
            playerVision.ExitZone(this);

        if (distortionAudio != null)
        {
            if (audioFade != null)
                StopCoroutine(audioFade);

            audioFade = StartCoroutine(
                FadeAudio(0f, fadeOutDuration));
        }
    }

    IEnumerator FadeAudio(float targetVolume, float duration)
    {
        if (distortionAudio == null)
            yield break;

        float startVolume = distortionAudio.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / Mathf.Max(duration, 0.01f));

            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            distortionAudio.volume = Mathf.Lerp(
                startVolume, targetVolume, smoothT);

            yield return null;
        }

        distortionAudio.volume = targetVolume;

        if (targetVolume <= 0f)
            distortionAudio.Stop();

        audioFade = null;
    }

    void OnDisable()
    {
        if (playerInside && playerVision != null)
            playerVision.ExitZone(this);

        playerInside = false;

        if (audioFade != null)
        {
            StopCoroutine(audioFade);
            audioFade = null;
        }

        if (distortionAudio != null)
            distortionAudio.Stop();
    }
}

