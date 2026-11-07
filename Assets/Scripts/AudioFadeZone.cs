
using UnityEngine;

public class AudioFadeZone : MonoBehaviour
{
    public BeginningSoundTrigger beginningSound;

    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() == null)
            return;

        if (hasTriggered)
            return;

        hasTriggered = true;

        if (beginningSound != null)
            beginningSound.FadeOutAudio();
    }
}

