
using UnityEngine;

public class RunningPersonTrigger : MonoBehaviour
{
    [Header("Character")]
    public Transform runningPerson;
    public Transform destination;

    [Header("Movement")]
    public float runSpeed = 5f;
    public float turnSpeed = 10f;

    private bool hasTriggered = false;
    private bool isRunning = false;

    void Start()
    {
        if (runningPerson != null)
            runningPerson.gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() == null)
            return;

        if (hasTriggered)
            return;

        hasTriggered = true;

        if (runningPerson != null && destination != null)
        {
            runningPerson.gameObject.SetActive(true);
            isRunning = true;
        }
    }

    void Update()
    {
        if (!isRunning || runningPerson == null ||
            destination == null)
            return;

        Vector3 target = destination.position;

        // Move the character toward the destination.
        runningPerson.position = Vector3.MoveTowards(
            runningPerson.position,
            target,
            runSpeed * Time.deltaTime
        );

        // Face the direction of travel.
        Vector3 direction =
            target - runningPerson.position;

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            runningPerson.rotation = Quaternion.Slerp(
                runningPerson.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
        }

        // Disappear when reaching the destination.
        if (Vector3.Distance(
            runningPerson.position,
            target) < 0.1f)
        {
            isRunning = false;
            runningPerson.gameObject.SetActive(false);
        }
    }
}

