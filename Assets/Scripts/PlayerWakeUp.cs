
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerWakeUp : MonoBehaviour
{
    [Header("References")]
    public Transform playerCamera;
    public MonoBehaviour playerController;
    public Image darkOverlay;

    [Header("Wake Up Settings")]
    public float wakeDuration = 4f;
    public float groundHeight = 0.2f;
    public float startTilt = 25f;
    public float startingDarkness = 0.65f;

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

        Vector3 groundPosition = standingPosition;
        groundPosition.y = groundHeight;

        Quaternion groundRotation = standingRotation *
            Quaternion.Euler(10f, 0f, startTilt);

        playerCamera.localPosition = groundPosition;
        playerCamera.localRotation = groundRotation;

        Color overlayColor = darkOverlay.color;
        overlayColor.a = startingDarkness;
        darkOverlay.color = overlayColor;

        float elapsed = 0f;

        while (elapsed < wakeDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / wakeDuration);
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            playerCamera.localPosition = Vector3.Lerp(
                groundPosition, standingPosition, smooth);

            playerCamera.localRotation = Quaternion.Slerp(
                groundRotation, standingRotation, smooth);

            overlayColor.a = Mathf.Lerp(
                startingDarkness, 0f, smooth);

            darkOverlay.color = overlayColor;

            yield return null;
        }

        playerCamera.localPosition = standingPosition;
        playerCamera.localRotation = standingRotation;

        overlayColor.a = 0f;
        darkOverlay.color = overlayColor;

        if (playerController != null)
            playerController.enabled = true;
    }
}

