
using System.Collections;
using UnityEngine;

public class TreeMovementTrigger : MonoBehaviour
{
    [Header("Trees")]
    public Transform[] trees;

    [Header("Tree Movement")]
    public float moveDuration = 2f;
    public float tiltAmount = 8f;
    public float shakeSpeed = 5f;

        
    [Header("Sound")]
    public AudioSource treeMovementSound;
    public AudioSource treeSound;


    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() == null)
            return;

        if (hasTriggered)
            return;

        hasTriggered = true;
        StartCoroutine(MoveTrees());
    }

    IEnumerator MoveTrees()
    {
        if (treeMovementSound != null)
        {
            treeMovementSound.loop = true;
            treeMovementSound.Play();
        }
        Quaternion[] originalRotations =
            new Quaternion[trees.Length];

        // Save each tree's original rotation.
        for (int i = 0; i < trees.Length; i++)
        {
            if (trees[i] != null)
                originalRotations[i] = trees[i].localRotation;
        }

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / Mathf.Max(moveDuration, 0.01f));

            // Gradually increase then decrease movement.
            float strength = Mathf.Sin(t * Mathf.PI);

            for (int i = 0; i < trees.Length; i++)
            {
                if (trees[i] == null)
                    continue;

                float tilt = Mathf.Sin(
                    Time.time * shakeSpeed + i
                ) * tiltAmount * strength;

                trees[i].localRotation =
                    originalRotations[i] *
                    Quaternion.Euler(tilt, 0f, 0f);
            }

            yield return null;
        }

        // Return trees to their original rotation.
        for (int i = 0; i < trees.Length; i++)
        {
            if (trees[i] != null)
                trees[i].localRotation =
                    originalRotations[i];
        }

        // Stop the sound of moving trees.
        if (treeMovementSound != null)
            treeMovementSound.Stop();

        // Play the sound after the trees stop.
        if (treeSound != null)
            treeSound.Play();
    }
}

