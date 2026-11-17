
using UnityEngine;
using System.Collections;

public class WendigoEnding : MonoBehaviour
{
    [Header("Player")]
    public Transform player;
    public Camera playerCamera;
    public MonoBehaviour playerController;
    public MonoBehaviour headBob;
    public MonoBehaviour mouseLook;

    [Header("Wendigo")]
    public GameObject wendigoModel;
    public Transform faceTarget;
    public Transform eyeViewPoint;
    public AudioSource screamAudio;
    public AudioSource staringAudio;
    public AudioSource redFlashAudio;
    public ParticleSystem mouthBreath;

    [Header("Screen Flash")]
    public CanvasGroup redFlash;
    public float flashSpeed = 5f;
    public float flashAlpha = 0.5f;

    [Header("Camera Movement")]
    public float lookBehindAngle = 150f;
    public float shakeDuration = 2.5f;
    public float shakePosition = 0.12f;
    public float shakeRotation = 9f;
    public float settleDuration = 0.8f;
    public float revealDuration = 3f;

    [Header("Final Camera Orbit")]
    public float orbitDuration = 3f;
    public float orbitAngle = 70f;
    public float orbitPause = 0.6f;

    [Header("Camera Position")]
    public Transform scareCameraPoint;

    [Header("Wendigo Placement")]
    public float spawnDistance = 2.5f;
    public float spawnHeightOffset = 0f;

    [Header("Final Ending")]
    public CanvasGroup blackScreen;
    public AudioSource finalScream;
    public AudioSource creditsMusic;

    [Header("Rolling Credits")]
    public RectTransform creditsContent;
    public RectTransform creditsViewport;

    public float blackFadeDuration = 1.5f;
    public float screamDelay = 2f;
    public float creditsStartDelay = 1f;
    public float creditsScrollDuration = 25f;
    public float pauseAfterScream = 3f;
    public float finalScreamFadeDuration = 1f;

    private bool playerInZone;
    private bool endingStarted;
    private Vector3 entryDirection;
    private Coroutine flashRoutine;

    void Start()
    {
        if (redFlash != null)
            redFlash.alpha = 0f;

        if (wendigoModel != null)
            wendigoModel.SetActive(false);
        if (blackScreen != null)
            blackScreen.alpha = 0f;

        if (creditsContent != null)
            creditsContent.gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || endingStarted)
            return;

        playerInZone = true;

        entryDirection = player.forward;
        entryDirection.y = 0f;
        entryDirection.Normalize();

        flashRoutine = StartCoroutine(FlashRed());
        
        if (redFlashAudio != null)
        {
            redFlashAudio.Play();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player") || endingStarted)
            return;

        playerInZone = false;

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }

        if (redFlash != null)
            redFlash.alpha = 0f;
        if (redFlashAudio != null)
        {
            redFlashAudio.Stop();
        }
    }

    void Update()
    {
        if (!playerInZone || endingStarted)
            return;

        Vector3 lookDirection = playerCamera.transform.forward;
        lookDirection.y = 0f;
        lookDirection.Normalize();

        float angle = Vector3.Angle(
            entryDirection,
            lookDirection
        );

        if (angle >= lookBehindAngle)
        {
            endingStarted = true;
            StartCoroutine(PlayEnding());
        }
    }

    IEnumerator FlashRed()
    {
        float flashTime = 0f;

        while (true)
        {
            flashTime += Time.deltaTime * flashSpeed;

            if (redFlash != null)
            {
                float pulse = (Mathf.Sin(flashTime) + 1f) * 0.5f;
                redFlash.alpha = pulse * flashAlpha;
            }

            yield return null;
        }
    }
    IEnumerator FadeInStaringAudio()
    {
        float targetVolume = staringAudio.volume;
        staringAudio.volume = 0f;
        staringAudio.Play();

        float time = 0f;
        float fadeDuration = 2f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            staringAudio.volume = Mathf.Lerp(
                0f,
                targetVolume,
                time / fadeDuration
            );

            yield return null;
        }

        staringAudio.volume = targetVolume;
    }
    IEnumerator ShowEndingCredits()
    {
        // Stop the previous ending sounds.
        if (redFlashAudio != null)
            redFlashAudio.Stop();

        if (staringAudio != null)
            staringAudio.Stop();

        if (screamAudio != null)
            screamAudio.Stop();

        // Fade the screen to black.
        float time = 0f;

        while (time < blackFadeDuration)
        {
            time += Time.deltaTime;

            float progress = Mathf.Clamp01(
                time / blackFadeDuration
            );

            blackScreen.alpha = Mathf.Lerp(0f, 1f, progress);

            yield return null;
        }

        blackScreen.alpha = 1f;

        // Wait two seconds in complete darkness.
        yield return new WaitForSeconds(screamDelay);

        // Play the final scream.
        if (finalScream != null && finalScream.clip != null)
        {
            finalScream.loop = false;
            float screamVolume = finalScream.volume;
            finalScream.Play();

            // Fade the scream during its final seconds.
            float fadeDuration = Mathf.Min(
                finalScreamFadeDuration,
                finalScream.clip.length
            );
            float fadeStartTime = finalScream.clip.length - fadeDuration;

            while (finalScream.isPlaying && finalScream.time < fadeStartTime)
            {
                yield return null;
            }

            float fadeTime = 0f;
            while (finalScream.isPlaying && fadeTime < fadeDuration)
            {
                fadeTime += Time.deltaTime;
                finalScream.volume = Mathf.Lerp(
                    screamVolume,
                    0f,
                    fadeTime / fadeDuration
                );
                yield return null;
            }

            finalScream.volume = 0f;
            finalScream.Stop();
            finalScream.volume = screamVolume;
        }

        // Pause in complete silence.
        yield return new WaitForSeconds(pauseAfterScream);

        // Start the credits music.
        if (creditsMusic != null)
        {
            creditsMusic.Play();
        }

        // Prepare the scrolling text.
        creditsContent.gameObject.SetActive(true);

        Canvas.ForceUpdateCanvases();

        float screenHeight = creditsViewport.rect.height;
        float contentHeight = creditsContent.rect.height;

        float startY = -(screenHeight + contentHeight) * 0.5f;
        float endY = (screenHeight + contentHeight) * 0.5f;

        creditsContent.anchoredPosition =
            new Vector2(0f, startY);

        time = 0f;

        // Scroll the credits upward.
        while (time < creditsScrollDuration)
        {
            time += Time.deltaTime;

            float progress = Mathf.Clamp01(
                time / creditsScrollDuration
            );

            float y = Mathf.Lerp(startY, endY, progress);

            creditsContent.anchoredPosition =
                new Vector2(0f, y);

            yield return null;
        }

        // Keep the screen black when credits finish.
        creditsContent.gameObject.SetActive(false);

        Debug.Log("Credits finished");
    }

    IEnumerator PlayEnding()
    {
        // Stop normal player and camera movement.
        if (playerController != null)
            playerController.enabled = false;

        if (headBob != null)
            headBob.enabled = false;

        if (mouseLook != null)
            mouseLook.enabled = false;

        // Reveal the Wendigo.
        Vector3 behindPlayer = -entryDirection;

        Vector3 spawnPosition = player.position
            + behindPlayer * spawnDistance;

        spawnPosition.y = player.position.y + spawnHeightOffset;

        wendigoModel.transform.position = spawnPosition;

        Vector3 faceDirection = player.position - spawnPosition;
        faceDirection.y = 0f;

        wendigoModel.transform.rotation =
            Quaternion.LookRotation(faceDirection);

        wendigoModel.SetActive(true);

        if (mouthBreath != null)
            mouthBreath.Play();

        if (screamAudio != null)
            screamAudio.Play();

        Transform cam = playerCamera.transform;

        Vector3 startPosition = cam.position;
        Quaternion startRotation = cam.rotation;

        // Frantic camera movement during the scream.
        float time = 0f;

        while (time < shakeDuration)
        {
            time += Time.deltaTime;

            float progress = Mathf.Clamp01(time / shakeDuration);
            float strength = 1f - progress * progress;

            Vector3 offset = Random.insideUnitSphere
                * shakePosition * strength;

            Vector3 direction = faceTarget.position
                - (startPosition + offset);

            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            Vector3 randomRotation = new Vector3(
                Random.Range(-shakeRotation, shakeRotation),
                Random.Range(-shakeRotation, shakeRotation),
                Random.Range(-shakeRotation, shakeRotation)
            ) * strength;

            cam.position = startPosition + offset;

            cam.rotation = targetRotation *
                Quaternion.Euler(randomRotation);

            yield return null;
        }

        
        // Move the camera to the planned position.
        time = 0f;

        Vector3 settleStartPosition = cam.position;
        Quaternion settleStartRotation = cam.rotation;

        Vector3 targetPosition = scareCameraPoint.position;

        Quaternion settleTargetRotation = Quaternion.LookRotation(
            faceTarget.position - targetPosition,
            Vector3.up
        );

        while (time < settleDuration)
        {
            time += Time.deltaTime;

            float progress = Mathf.Clamp01(
                time / settleDuration
            );

            progress = Mathf.SmoothStep(0f, 1f, progress);

            cam.position = Vector3.Lerp(
                settleStartPosition,
                targetPosition,
                progress
            );

            cam.rotation = Quaternion.Slerp(
                settleStartRotation,
                settleTargetRotation,
                progress
            );

            yield return null;
        }

        cam.SetPositionAndRotation(
            targetPosition,
            settleTargetRotation
        );


        // Stop the red flashing.
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }

        if (redFlash != null)
            redFlash.alpha = 0f;
        
        // Begin creepy background audio while the Wendigo stares.
        if (staringAudio != null)
        {
            StartCoroutine(FadeInStaringAudio());
        }

        yield return new WaitForSeconds(0.7f);

        // Move the camera into the Wendigo's eyes.
        
        // Slowly orbit around the Wendigo's head.
        time = 0f;

        Vector3 center = faceTarget.position;
        Vector3 startingOffset = cam.position - center;

        while (time < orbitDuration)
        {
            time += Time.deltaTime;

            float progress = Mathf.Clamp01(time / orbitDuration);
            progress = Mathf.SmoothStep(0f, 1f, progress);

            float angle = Mathf.Lerp(0f, orbitAngle, progress);

            Quaternion orbitRotation =
                Quaternion.AngleAxis(angle, Vector3.up);

            cam.position = center + orbitRotation * startingOffset;

            Vector3 lookDirection = center - cam.position;

            cam.rotation = Quaternion.LookRotation(
                lookDirection, Vector3.up
            );

            yield return null;
        }

        // Pause at the side of the creature.
        yield return new WaitForSeconds(orbitPause);

        // Slowly transition into the Wendigo's eyes.
        Vector3 eyeStartPosition = cam.position;
        Quaternion eyeStartRotation = cam.rotation;

        time = 0f;

        while (time < revealDuration)
        {
            time += Time.deltaTime;

            float progress = Mathf.Clamp01(time / revealDuration);
            progress = Mathf.SmoothStep(0f, 1f, progress);

            cam.position = Vector3.Lerp(
                eyeStartPosition,
                eyeViewPoint.position,
                progress
            );

            cam.rotation = Quaternion.Slerp(
                eyeStartRotation,
                eyeViewPoint.rotation,
                progress
            );

            yield return null;
        }

        cam.SetPositionAndRotation(
            eyeViewPoint.position,
            eyeViewPoint.rotation
        );

        // Stop the red screen sound at the end.
        if (redFlashAudio != null)
        {
            redFlashAudio.Stop();
        }

        // Transition to the black screen and credits.
        yield return StartCoroutine(ShowEndingCredits());

        Debug.Log("Wendigo ending complete");
    }
    
}

