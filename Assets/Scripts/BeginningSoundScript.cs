
using System.Collections;
using UnityEngine;

public class BeginningSoundTrigger : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource beginningAudio;

    [Header("Fade Out")]
    public float fadeDuration = 5f;

    private bool hasPlayed = false;
    private bool isFading = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() == null)
            return;

        if (hasPlayed)
            return;

        hasPlayed = true;

        if (beginningAudio != null)
            beginningAudio.Play();
    }

    public void FadeOutAudio()
    {
        if (beginningAudio == null || isFading)
            return;

        isFading = true;
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        float startingVolume = beginningAudio.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / Mathf.Max(fadeDuration, 0.01f));

            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            beginningAudio.volume = Mathf.Lerp(
                startingVolume, 0f, smoothT);

            yield return null;
        }

        beginningAudio.volume = 0f;
        beginningAudio.Stop();
    }
}

