
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System.Collections;

public class PlayerWakeUp : MonoBehaviour
{
    [Header("References")]
    public Transform playerCamera;
    public MonoBehaviour playerController;
    public Image darkOverlay;
    public Volume wakeUpVolume;
    public RealisticHeadBob headBob;

    [Header("Timing")]
    public float groundDuration = 3f;
    public float headLiftDuration = 1.5f;
    public float riseDuration = 3.5f;

    [Header("Camera")]
    public float groundHeight = 0.2f;
    public float startTilt = 65f;
    public float headLiftAmount = 0.12f;
    public float startPitch = 10f;

    [Header("Vision")]
    public float startingDarkness = 0.8f;

    private Vector3 standingPosition;
    private Quaternion standingRotation;
    

    void Start()
    {
        standingPosition = playerCamera.localPosition;
        standingRotation = playerCamera.localRotation;

        StartCoroutine(WakeUp());
    }

    IEnumerator WakeUp()
    {
        if (playerController != null)
            playerController.enabled = false;
        if (headBob != null)
            headBob.enabled = false;

        Vector3 groundPosition = standingPosition;
        groundPosition.y = groundHeight;

        Quaternion groundRotation = standingRotation *
            Quaternion.Euler(startPitch, 0f, startTilt);

        Vector3 liftedPosition = groundPosition +
            Vector3.up * headLiftAmount;

        Quaternion liftedRotation = standingRotation *
            Quaternion.Euler(5f, 0f, startTilt - 12f);

        playerCamera.localPosition = groundPosition;
        playerCamera.localRotation = groundRotation;

        Color overlayColor = darkOverlay.color;
        overlayColor.a = startingDarkness;
        darkOverlay.color = overlayColor;

        if (wakeUpVolume != null)
            wakeUpVolume.weight = 1f;

        float elapsed = 0f;

        // Phase 1: Regain consciousness
        while (elapsed < groundDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / groundDuration);
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            overlayColor.a = Mathf.Lerp(
                startingDarkness, 0.25f, smooth);

            darkOverlay.color = overlayColor;

            if (wakeUpVolume != null)
                wakeUpVolume.weight = Mathf.Lerp(
                    1f, 0.15f, smooth);

            yield return null;
        }

        elapsed = 0f;

        // Phase 2: Slightly lift the head
        while (elapsed < headLiftDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / headLiftDuration);
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            playerCamera.localPosition = Vector3.Lerp(
                groundPosition, liftedPosition, smooth);

            playerCamera.localRotation = Quaternion.Slerp(
                groundRotation, liftedRotation, smooth);

            yield return null;
        }

        elapsed = 0f;

        // Phase 3: Slowly stand up
        while (elapsed < riseDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / riseDuration);

            // Camera height and rotation move at different speeds
            float heightSmooth = Mathf.SmoothStep(0f, 1f, t);

            float rotationT = Mathf.Clamp01(
                (t - 0.1f) / 0.9f);

            float rotationSmooth = Mathf.SmoothStep(
                0f, 1f, rotationT);

            playerCamera.localPosition = Vector3.Lerp(
                liftedPosition, standingPosition, heightSmooth);

            playerCamera.localRotation = Quaternion.Slerp(
                liftedRotation, standingRotation, rotationSmooth);

            overlayColor.a = Mathf.Lerp(
                0.25f, 0f, heightSmooth);

            darkOverlay.color = overlayColor;

            if (wakeUpVolume != null)
                wakeUpVolume.weight = Mathf.Lerp(
                    0.15f, 0f, heightSmooth);

            yield return null;
        }

        playerCamera.localPosition = standingPosition;
        playerCamera.localRotation = standingRotation;

        overlayColor.a = 0f;
        darkOverlay.color = overlayColor;

        if (wakeUpVolume != null)
            wakeUpVolume.weight = 0f;

        if (playerController != null)
            playerController.enabled = true;
        if (headBob != null)
            headBob.enabled = true;
    }
}
