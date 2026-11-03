
using System.Collections;
using UnityEngine;

public class SquirrelAttackCinematic : MonoBehaviour
{
    [Header("Player")]
    public PlayerController playerController;
    public Transform playerCamera;
    public Transform grabPoint;

    [Header("Squirrel")]
    public Transform squirrel;
    public Animator squirrelAnimator;

    [Header("Effects")]
    public ParticleSystem bloodParticles;
    public CanvasGroup redVignette;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip squeakSound;
    public AudioClip crunchSound;
    public AudioClip chewingSound;
    public AudioSource encounterBackgroundAudio;

    [Header("Movement")]
    public float escapeDistance = 0.8f;
    public float escapeDuration = 0.35f;
    public float lungeDistance = 0.35f;
    public float struggleDuration = 1.4f;
    public float chewingDuration = 2.4f;

    private bool isPlaying;

    public void PlayCinematic()
    {
        Debug.Log("PlayCinematic was called!");
        if (!isPlaying)
            StartCoroutine(AttackSequence());
    }

    IEnumerator AttackSequence()
    {
        if (squirrel == null || playerCamera == null ||
            grabPoint == null)
            yield break;

        
        isPlaying = true;

        if (encounterBackgroundAudio != null)
        {
            encounterBackgroundAudio.loop = true;
            encounterBackgroundAudio.Play();

            Debug.Log("Encounter audio started: " +
                    encounterBackgroundAudio.isPlaying);

            Debug.Log("Background audio object: " +
                    encounterBackgroundAudio.gameObject.name);
        }
        else
        {
            Debug.LogError(
                "Encounter Background Audio is NOT assigned!");
        }


        if (playerController != null)
            playerController.enabled = false;

        // Turn off normal squirrel movement animation.
        if (squirrelAnimator != null)
            squirrelAnimator.enabled = false;

        Vector3 originalCameraPosition =
            playerCamera.localPosition;

        Vector3 startPosition = squirrel.position;
        Vector3 escapeDirection =
            squirrel.position - playerCamera.position;

        escapeDirection.y = 0f;

        if (escapeDirection.sqrMagnitude < 0.01f)
            escapeDirection = playerCamera.forward;

        escapeDirection.Normalize();

        // 1. Squirrel attempts to flee.
        float elapsed = 0f;

        while (elapsed < escapeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(
                elapsed / escapeDuration);

            squirrel.position = Vector3.Lerp(
                startPosition,
                startPosition +
                    escapeDirection * escapeDistance,
                t);

            // Player lunges during the escape.
            playerCamera.localPosition =
                originalCameraPosition +
                Vector3.forward *
                (lungeDistance * t);

            yield return null;
        }

        // 2. Grab squirrel in front of camera.
        squirrel.SetParent(grabPoint, true);

        Vector3 grabbedPosition = Vector3.zero;
        Quaternion grabbedRotation = Quaternion.identity;

        elapsed = 0f;

        if (audioSource != null &&
            squeakSound != null)
        {
            audioSource.clip = squeakSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        // 3. Squirrel struggles.
        while (elapsed < struggleDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(
                elapsed / struggleDuration);

            squirrel.localPosition =
                grabbedPosition +
                new Vector3(
                    Mathf.Sin(Time.time * 30f) * 0.035f,
                    Mathf.Sin(Time.time * 24f) * 0.025f,
                    0f);

            squirrel.localRotation =
                grabbedRotation *
                Quaternion.Euler(
                    Mathf.Sin(Time.time * 22f) * 12f,
                    0f,
                    Mathf.Sin(Time.time * 28f) * 15f);

            // Pulsing red edges.
            if (redVignette != null)
            {
                redVignette.alpha =
                    0.12f +
                    Mathf.Abs(Mathf.Sin(
                        Time.time * 5f)) * 0.15f;
            }

            // Ease camera back from the lunge.
            playerCamera.localPosition =
                Vector3.Lerp(
                    originalCameraPosition +
                        Vector3.forward * lungeDistance,
                    originalCameraPosition,
                    t);

            yield return null;
        }

        // 4. Squeaking and struggling stop.
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        if (audioSource != null &&
            crunchSound != null)
            audioSource.PlayOneShot(crunchSound);

        if (bloodParticles != null)
            bloodParticles.Play();

        squirrel.localRotation = grabbedRotation;
        squirrel.localPosition = grabbedPosition;

        // Brief visual impact.
        if (redVignette != null)
            redVignette.alpha = 0.4f;

        yield return new WaitForSeconds(0.3f);

        // 5. Chewing with subtle camera movement.
        if (audioSource != null &&
            chewingSound != null)
            audioSource.PlayOneShot(chewingSound);

        elapsed = 0f;

        while (elapsed < chewingDuration)
        {
            elapsed += Time.deltaTime;

            playerCamera.localPosition =
                originalCameraPosition +
                new Vector3(
                    0f,
                    Mathf.Sin(Time.time * 9f) * 0.012f,
                    0f);

            if (redVignette != null)
            {
                float fade = 1f -
                    Mathf.Clamp01(
                        elapsed / chewingDuration);

                redVignette.alpha =
                    (0.10f +
                    Mathf.Abs(Mathf.Sin(
                        Time.time * 4f)) * 0.12f)
                    * fade;
            }

            yield return null;
        }

        // 6. Return control to the player.
        squirrel.gameObject.SetActive(false);

        playerCamera.localPosition =
            originalCameraPosition;

        if (redVignette != null)
            redVignette.alpha = 0f;

        if (audioSource != null)
            audioSource.Stop();

        if (playerController != null)
            playerController.enabled = true;

        if (encounterBackgroundAudio != null)
            encounterBackgroundAudio.Stop();
        isPlaying = false;
    }
}

