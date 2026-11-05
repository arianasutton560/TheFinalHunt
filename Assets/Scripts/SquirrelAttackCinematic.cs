
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
    public Transform squirrelEscapePoint;

    [Header("Effects")]
    public CanvasGroup redVignette;

    [Header("Red Vignette Flash")]
    [Range(0f, 1f)]
    public float minimumRedAlpha = 0.05f;

    [Range(0f, 1f)]
    public float maximumRedAlpha = 0.65f;

    public float redFlashSpeed = 3f;

    [Header("Blood Splatter UI")]
    public CanvasGroup bloodSplatterOverlay;
    public float bloodSplatterMaxAlpha = 0.9f;
    public float bloodSplatterFadeDuration = 2.5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip squeakSound;
    public AudioClip crunchSound;
    public AudioClip chewingSound;
    public AudioSource encounterBackgroundAudio;

    [Header("Encounter Audio Fade")]
    public float audioFadeDuration = 4f;

    [Header("Movement")]
    public float escapeDuration = 1.2f;
    public float lungeDistance = 0.35f;
    public float struggleDuration = 1.4f;
    public float chewingDuration = 2.4f;

    [Header("Escape Camera")]
    public float cameraLookSpeed = 8f;
    public float lungeDuration = 0.25f;
    public float lungeStopDistance = 0.65f;
    public float squirrelTurnSpeed = 12f;

    private bool isPlaying;

    public void PlayCinematic()
    {
        Debug.Log("PlayCinematic was called!");
        if (!isPlaying)
            StartCoroutine(AttackSequence());
    }

    
    float GetRedFlash()
    {
        float pulse = (Mathf.Sin(
            Time.time * redFlashSpeed * Mathf.PI * 2f
        ) + 1f) / 2f;

        return Mathf.Lerp(
            minimumRedAlpha,
            maximumRedAlpha,
            pulse
        );
    }


    IEnumerator FadeOutEncounterAudio()
    {
        if (encounterBackgroundAudio == null)
            yield break;

        float startingVolume = encounterBackgroundAudio.volume;
        float elapsed = 0f;

        while (elapsed < audioFadeDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsed / Mathf.Max(audioFadeDuration, 0.01f));

            // Smoothly reduce the volume.
            float smoothProgress =
                Mathf.SmoothStep(0f, 1f, progress);

            encounterBackgroundAudio.volume =
                Mathf.Lerp(startingVolume, 0f, smoothProgress);

            yield return null;
        }

        encounterBackgroundAudio.Stop();

        // Restore volume for future use.
        encounterBackgroundAudio.volume = startingVolume;
    }

    IEnumerator FadeOutBloodSplatter()
    {
        if (bloodSplatterOverlay == null)
            yield break;

        float elapsed = 0f;
        float startAlpha = bloodSplatterOverlay.alpha;

        while (elapsed < bloodSplatterFadeDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / Mathf.Max(bloodSplatterFadeDuration, 0.01f));

            bloodSplatterOverlay.alpha =
                Mathf.Lerp(startAlpha, 0f, t);

            yield return null;
        }

        bloodSplatterOverlay.alpha = 0f;
    }
    IEnumerator AttackSequence()
    {
        if (squirrel == null || playerCamera == null ||
            grabPoint == null)
            yield break;

        
        isPlaying = true;
        if (bloodSplatterOverlay != null)
        bloodSplatterOverlay.alpha = 0f;

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

        Vector3 originalCameraPosition =
            playerCamera.localPosition;

        Quaternion originalCameraRotation =
            playerCamera.localRotation;

        
        
        // 1. Squirrel escapes toward its destination.
        Vector3 startPosition = squirrel.position;
        Vector3 escapeEnd = squirrelEscapePoint.position;

        Vector3 runDirection = escapeEnd - startPosition;
        runDirection.y = 0f;

        if (squirrelAnimator != null)
        {
            squirrelAnimator.enabled = true;
            squirrelAnimator.ResetTrigger("Run");
            squirrelAnimator.SetTrigger("Run");
        }

        float elapsed = 0f;

        while (elapsed < escapeDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / Mathf.Max(escapeDuration, 0.01f));

            Vector3 newPosition = Vector3.Lerp(
                startPosition, escapeEnd, t);

            // Keep the squirrel on the terrain.
            Terrain terrain = Terrain.activeTerrain;

            if (terrain != null)
            {
                newPosition.y =
                    terrain.SampleHeight(newPosition) +
                    terrain.transform.position.y;
            }

            squirrel.position = newPosition;

            // Turn squirrel toward its destination.
            if (runDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(runDirection);

                squirrel.rotation = Quaternion.Slerp(
                    squirrel.rotation,
                    targetRotation,
                    squirrelTurnSpeed * Time.deltaTime);
            }

            // Camera watches the squirrel escape.
            Vector3 lookDirection =
                squirrel.position - playerCamera.position;

            if (lookDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetLook =
                    Quaternion.LookRotation(lookDirection);

                playerCamera.rotation = Quaternion.Slerp(
                    playerCamera.rotation,
                    targetLook,
                    cameraLookSpeed * Time.deltaTime);
            }

            yield return null;
        }

        // Stop running animation.
        if (squirrelAnimator != null)
            squirrelAnimator.enabled = false;

        // 2. Powerful camera lunge toward the squirrel.
        Vector3 lungeStart = playerCamera.position;

        Vector3 lungeDirection =
            escapeEnd - lungeStart;

        lungeDirection.y = 0f;

        Vector3 lungeEnd = lungeStart;

        if (lungeDirection.sqrMagnitude > 0.001f)
        {
            lungeDirection.Normalize();

            // Move toward the squirrel's final position.
            lungeEnd = new Vector3(
                escapeEnd.x,
                lungeStart.y,
                escapeEnd.z
            ) - lungeDirection * lungeStopDistance;
        }

        elapsed = 0f;

        while (elapsed < lungeDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / Mathf.Max(lungeDuration, 0.01f));

            // Accelerate quickly into the lunge.
            float fastT = 1f - Mathf.Pow(1f - t, 3f);

            playerCamera.position = Vector3.Lerp(
                lungeStart, lungeEnd, fastT);

            // Keep looking at the squirrel.
            Vector3 lookDirection =
                squirrel.position - playerCamera.position;

            if (lookDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetLook =
                    Quaternion.LookRotation(lookDirection);

                playerCamera.rotation = Quaternion.Slerp(
                    playerCamera.rotation,
                    targetLook,
                    cameraLookSpeed * Time.deltaTime);
            }

            yield return null;
        }

        // Remember the lunge position and rotation.
        Vector3 lungeCameraPosition =
            playerCamera.localPosition;

        Quaternion lungeCameraRotation =
            playerCamera.localRotation;


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
                redVignette.alpha = GetRedFlash();
            }

            
            // Return camera from the lunge.
            playerCamera.localPosition = Vector3.Lerp(
                lungeCameraPosition,
                originalCameraPosition,
                t);

            playerCamera.localRotation = Quaternion.Slerp(
                lungeCameraRotation,
                originalCameraRotation,
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

        // Show blood splatter image immediately.
        if (bloodSplatterOverlay != null)
        {
            bloodSplatterOverlay.alpha = bloodSplatterMaxAlpha;
            StartCoroutine(FadeOutBloodSplatter());
        }

        // Hide the squirrel immediately after the crunch.
        squirrel.gameObject.SetActive(false);

        // Brief visual impact.
        if (redVignette != null)
            redVignette.alpha = 0.85f;

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
                Mathf.Sin(Time.time * 7f) * 0.015f,
                Mathf.Sin(Time.time * 9f) * 0.035f,
                Mathf.Sin(Time.time * 6f) * 0.01f
            );

            if (redVignette != null)
            {
                float fade = 1f - Mathf.Clamp01(elapsed / chewingDuration);

                redVignette.alpha = GetRedFlash() * fade;
            }

            yield return null;
        }

        playerCamera.localPosition =
            originalCameraPosition;
        playerCamera.localRotation =
            originalCameraRotation;

        if (redVignette != null)
            redVignette.alpha = 0f;

        if (audioSource != null)
            audioSource.Stop();

        if (playerController != null)
            playerController.enabled = true;

        if (encounterBackgroundAudio != null)
            StartCoroutine(FadeOutEncounterAudio());
        isPlaying = false;
    }
}

