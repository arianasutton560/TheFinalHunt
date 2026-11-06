
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerVisionDistortion : MonoBehaviour
{
    [Header("Transition")]
    public float fadeInDuration = 3f;
    public float fadeOutDuration = 4f;

    [Header("Camera Distortion")]
    public float maxTilt = 8f;
    public float sideSway = 0.07f;
    public float swaySpeed = 1.4f;
    public float verticalSway = 0.025f;

    
    [Header("Visual Vertigo")]
    public Camera playerCamera;
    public float vertigoZoom = 5f;
    public float vertigoSpeed = 0.8f;

    [Header("Disoriented Filter")]
    public Volume disorientedVolume;

    [Header("Movement Slowdown")]
    public PlayerController playerController;
    [Range(0.1f, 1f)]
    public float slowedSpeedMultiplier = 0.45f;

    private float normalMovementSpeed;

    private float originalFOV;


    private float intensity = 0f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private HashSet<DistortionZone> activeZones =
        new HashSet<DistortionZone>();

    void Start()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;

        if (playerCamera != null)
            originalFOV = playerCamera.fieldOfView;

        intensity = 0f;

        if (disorientedVolume != null)
            disorientedVolume.weight = 0f;

        if (playerController != null)
            normalMovementSpeed = playerController.moveSpeed;
    }

    public void EnterZone(DistortionZone zone)
    {
        activeZones.Add(zone);
    }

    public void ExitZone(DistortionZone zone)
    {
        activeZones.Remove(zone);
    }

    void Update()
    {
        float target = activeZones.Count > 0 ? 1f : 0f;

        float duration = target > intensity
            ? fadeInDuration
            : fadeOutDuration;

        intensity = Mathf.MoveTowards(
            intensity,
            target,
            Time.deltaTime / Mathf.Max(duration, 0.01f));

        if (disorientedVolume != null){
            float filterStrength = Mathf.SmoothStep(0f, 1f, intensity);
            disorientedVolume.weight = filterStrength;
        }
        
        if (playerController != null)
        {
            float slowdownStrength =
                Mathf.SmoothStep(0f, 1f, intensity);

            playerController.moveSpeed = Mathf.Lerp(
                normalMovementSpeed,
                normalMovementSpeed * slowedSpeedMultiplier,
                slowdownStrength
            );
        }
    }

    void LateUpdate()
    {
        // Smooth entry and exit.
        float strength = Mathf.SmoothStep(
            0f, 1f, intensity);

        float sway = Time.time * swaySpeed;

        // Combine different movements to make
        // the motion feel less predictable.
        float tilt =
            Mathf.Sin(sway) * maxTilt +
            Mathf.Sin(sway * 0.53f) * maxTilt * 0.35f;

        float horizontal =
            Mathf.Sin(sway * 0.7f) * sideSway;

        float vertical =
            Mathf.Sin(sway * 1.3f) * verticalSway;

        float pitch =
            Mathf.Sin(sway * 0.6f) * 1.5f;

        transform.localPosition =
            originalPosition +
            new Vector3(
                horizontal * strength,
                vertical * strength,
                0f);

        transform.localRotation =
            originalRotation *
            Quaternion.Euler(
                pitch * strength,
                0f,
                tilt * strength);

        // Slowly expand and contract the player's vision.
        if (playerCamera != null)
        {
            float zoom =
                Mathf.Sin(Time.time * vertigoSpeed)
                * vertigoZoom;

            playerCamera.fieldOfView =
                originalFOV + zoom * strength;
        }
    }
    void OnDisable()
    {
        if (playerController != null)
            playerController.moveSpeed = normalMovementSpeed;
    }
}

