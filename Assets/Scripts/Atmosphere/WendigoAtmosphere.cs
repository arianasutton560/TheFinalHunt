
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

    [Header("Night Sky")]
    [SerializeField] private float nightSkyExposure = 0f;

    private Material runtimeSkybox;
    private float daySkyExposure = 1f;

    [Header("Snowstorm Fog")]
    [SerializeField] private SnowstormZone snowstormZone;

    [SerializeField] private float stormFogStart = 2f;
    [SerializeField] private float stormFogEnd = 18f;

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
    private float currentAtmosphereTime = 0f;

    private void Awake()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.ambientMode = AmbientMode.Flat;

        if (RenderSettings.skybox != null)
        {
            runtimeSkybox = new Material(RenderSettings.skybox);
            RenderSettings.skybox = runtimeSkybox;

            if (runtimeSkybox.HasProperty("_Exposure"))
            {
                daySkyExposure =
                    runtimeSkybox.GetFloat("_Exposure");
            }
        }
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

    private void LateUpdate()
    {
        // Update fog continuously so the
        // snowstorm can build and fade.
        ApplyAtmosphere(currentAtmosphereTime);
    }

    private void ApplyAtmosphere(float t)
    {
        currentAtmosphereTime = t;
        // Calculate the normal day/night fog.
        Color normalFogColor =
            Color.Lerp(dayFogColor, nightFogColor, t);

        float normalFogStart =
            Mathf.Lerp(dayFogStart, nightFogStart, t);

        float normalFogEnd =
            Mathf.Lerp(dayFogEnd, nightFogEnd, t);

        // Read the current snowstorm intensity.
        float stormStrength = 0f;

        if (snowstormZone != null)
            stormStrength = snowstormZone.StormIntensity;

            if (runtimeSkybox != null &&
                runtimeSkybox.HasProperty("_Exposure"))
            {
                float skyBrightness = Mathf.Lerp(
                    daySkyExposure,
                    nightSkyExposure,
                    t);

                runtimeSkybox.SetFloat("_Exposure", skyBrightness);
            }

        // Make fog thicker as the storm grows.
        RenderSettings.fogColor = normalFogColor;

        RenderSettings.fogStartDistance =
            Mathf.Lerp(
                normalFogStart,
                stormFogStart,
                stormStrength);

        RenderSettings.fogEndDistance =
            Mathf.Lerp(
                normalFogEnd,
                stormFogEnd,
                stormStrength);

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
