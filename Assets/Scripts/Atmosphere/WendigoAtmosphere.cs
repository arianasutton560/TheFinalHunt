
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class WendigoAtmosphere : MonoBehaviour
{
    [Header("Main Lighting")]
    [SerializeField] private Light mainDirectionalLight;

    [Header("Transition")]
    [SerializeField, Min(0.1f)]
    private float transitionDuration = 90f;

    [SerializeField]
    private AnimationCurve transitionCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Cinematic Settings")]
    [SerializeField] private bool startAtNight = false;

    [Header("Day Fog")]
    [SerializeField] private Color dayFogColor =
        new Color(0.408f, 0.451f, 0.478f);

    [SerializeField] private float dayFogStart = 12f;
    [SerializeField] private float dayFogEnd = 75f;

    [Header("Night Fog")]
    [SerializeField] private Color nightFogColor =
        new Color(0.075f, 0.11f, 0.16f);

    [SerializeField] private float nightFogStart = 8f;
    [SerializeField] private float nightFogEnd = 45f;

    [Header("Day Lighting")]
    [SerializeField] private Color dayLightColor =
        new Color(1f, 0.9f, 0.78f);

    [SerializeField] private float dayLightIntensity = 1.2f;
    [SerializeField] private Vector3 dayLightRotation =
        new Vector3(35f, -30f, 0f);

    [SerializeField] private Color dayAmbientColor =
        new Color(0.38f, 0.42f, 0.46f);

    [Header("Night Lighting")]
    [SerializeField] private Color nightLightColor =
        new Color(0.45f, 0.6f, 1f);

    [SerializeField] private float nightLightIntensity = 0.12f;
    [SerializeField] private Vector3 nightLightRotation =
        new Vector3(65f, 20f, 0f);

    [SerializeField] private Color nightAmbientColor =
        new Color(0.035f, 0.055f, 0.09f);

    public bool IsNight { get; private set; }
    public bool IsTransitioning { get; private set; }

    private bool transitionStarted;

    private void Awake()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.ambientMode = AmbientMode.Flat;

        if (startAtNight)
        {
            ApplyAtmosphere(1f);
            IsNight = true;
            IsTransitioning = false;
            transitionStarted = true;
        }
        else
        {
            ApplyAtmosphere(0f);
        }
    }

    public void StartNightTransition()
    {
        // This transition can only start once.
        if (transitionStarted)
            return;

        transitionStarted = true;
        StartCoroutine(TransitionToNight());
    }

    private IEnumerator TransitionToNight()
    {
        IsTransitioning = true;

        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(
                elapsed / transitionDuration
            );

            float smoothProgress =
                Mathf.Clamp01(transitionCurve.Evaluate(progress));

            ApplyAtmosphere(smoothProgress);

            yield return null;
        }

        ApplyAtmosphere(1f);

        IsNight = true;
        IsTransitioning = false;
    }

    private void ApplyAtmosphere(float t)
    {
        // Fog becomes colder and closer at night.
        RenderSettings.fogColor =
            Color.Lerp(dayFogColor, nightFogColor, t);

        RenderSettings.fogStartDistance =
            Mathf.Lerp(dayFogStart, nightFogStart, t);

        RenderSettings.fogEndDistance =
            Mathf.Lerp(dayFogEnd, nightFogEnd, t);

        // Reduce ambient forest illumination.
        RenderSettings.ambientLight =
            Color.Lerp(dayAmbientColor, nightAmbientColor, t);

        // Change the directional light into moonlight.
        if (mainDirectionalLight != null)
        {
            mainDirectionalLight.color =
                Color.Lerp(dayLightColor, nightLightColor, t);

            mainDirectionalLight.intensity =
                Mathf.Lerp(
                    dayLightIntensity,
                    nightLightIntensity,
                    t
                );

            Quaternion dayRotation =
                Quaternion.Euler(dayLightRotation);

            Quaternion nightRotation =
                Quaternion.Euler(nightLightRotation);

            mainDirectionalLight.transform.rotation =
                Quaternion.Slerp(dayRotation, nightRotation, t);
        }
    }
}
