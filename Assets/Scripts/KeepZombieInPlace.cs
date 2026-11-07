
using UnityEngine;

public class KeepZombieInPlace : MonoBehaviour
{
    public Transform hips;

    private Vector3 startingPosition;
    private bool initialized = false;

    void LateUpdate()
    {
        if (hips == null)
            return;

        if (!initialized)
        {
            startingPosition = hips.localPosition;
            initialized = true;
        }

        Vector3 position = hips.localPosition;

        // Prevent the hips from traveling horizontally.
        position.x = startingPosition.x;
        position.z = startingPosition.z;

        // Keep vertical bouncing from the running animation.
        hips.localPosition = position;
    }
}

