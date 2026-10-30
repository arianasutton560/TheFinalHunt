
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NightTransitionTrigger : MonoBehaviour
{
    [SerializeField]
    private WendigoAtmosphere atmosphere;

    [SerializeField]
    private string playerTag = "Player";

    private void Reset()
    {
        Collider trigger = GetComponent<Collider>();
        trigger.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only respond to the player.
        if (!other.CompareTag(playerTag))
            return;

        if (atmosphere == null)
        {
            Debug.LogWarning(
                "Night trigger has no atmosphere controller!"
            );
            return;
        }

        atmosphere.StartNightTransition();
    }
}

