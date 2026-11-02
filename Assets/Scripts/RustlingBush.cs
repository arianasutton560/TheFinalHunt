

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RustlingBush : MonoBehaviour
{
    [System.Serializable]
    public class BushSpot
    {
        public Transform bushRoot;
        public Transform bushModel;
        public Transform squirrelHidePoint;
        public AudioSource rustlingAudio;

        [HideInInspector] public Quaternion originalRotation;
    }

    [Header("References")]
    public Transform player;
    public Transform squirrel;
    public Animator squirrelAnimator;
    public GameObject interactionPrompt;

    [Header("Bushes")]
    public List<BushSpot> bushes = new List<BushSpot>();
    public int startingBush = 0;

    [Header("Interaction")]
    public float interactionDistance = 6f;

    [Header("Bush Movement")]
    public float shakeAmount = 4f;
    public float shakeSpeed = 12f;
    public float stopDuration = 1.5f;

    [Header("Squirrel Movement")]
    public float squirrelSpeed = 5f;
    public float jumpHeight = 0.25f;
    public float groundOffset = 0f;

    [Header("Sound")]
    [Range(0f, 1f)]
    public float rustlingVolume = 0.7f;

    private int currentBush;
    private int lastBush = -1;
    private bool isMoving;
    private float shakeStrength;

    // Allows the torch script to give the bush priority.
    public static RustlingBush Instance;

    public static bool IsPlayerNearBush
    {
        get
        {
            if (Instance == null)
                return false;

            return Instance.CanInteract();
        }
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (bushes.Count < 2)
        {
            Debug.LogWarning("Assign at least two bushes.");
            enabled = false;
            return;
        }

        currentBush = Mathf.Clamp(
            startingBush, 0, bushes.Count - 1);

        shakeStrength = shakeAmount;

        foreach (BushSpot bush in bushes)
        {
            if (bush.bushModel != null)
                bush.originalRotation =
                    bush.bushModel.localRotation;

            if (bush.rustlingAudio != null)
            {
                bush.rustlingAudio.Stop();
                bush.rustlingAudio.loop = true;
                bush.rustlingAudio.volume = rustlingVolume;
            }
        }

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        if (squirrel != null &&
            bushes[currentBush].squirrelHidePoint != null)
        {
            squirrel.position =
                bushes[currentBush].squirrelHidePoint.position;
        }

        StartRustling(currentBush);
    }

    void Update()
    {
        if (bushes.Count < 2)
            return;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(CanInteract());

        if (!isMoving)
        {
            ShakeBush(currentBush, shakeStrength);

            if (CanInteract() &&
                Keyboard.current != null &&
                Keyboard.current.eKey.wasPressedThisFrame)
            {
                StartCoroutine(MoveToNextBush());
            }
        }
    }

    bool CanInteract()
    {
        if (isMoving || player == null || bushes.Count < 2)
            return false;

        Transform root = bushes[currentBush].bushRoot;

        if (root == null)
            return false;

        return Vector3.Distance(
            player.position, root.position)
            <= interactionDistance;
    }

    void ShakeBush(int index, float strength)
    {
        BushSpot bush = bushes[index];

        if (bush.bushModel == null)
            return;

        float shake =
            Mathf.Sin(Time.time * shakeSpeed) * strength;

        bush.bushModel.localRotation =
            bush.originalRotation *
            Quaternion.Euler(0f, 0f, shake);
    }

    void StartRustling(int index)
    {
        AudioSource audio = bushes[index].rustlingAudio;

        if (audio != null)
        {
            audio.volume = rustlingVolume;
            audio.Play();
        }
    }

    int ChooseNextBush()
    {
        List<int> choices = new List<int>();

        for (int i = 0; i < bushes.Count; i++)
        {
            if (i != currentBush && i != lastBush &&
                bushes[i].squirrelHidePoint != null)
            {
                choices.Add(i);
            }
        }

        // Allow the previous bush if no other choice exists.
        if (choices.Count == 0)
        {
            for (int i = 0; i < bushes.Count; i++)
            {
                if (i != currentBush &&
                    bushes[i].squirrelHidePoint != null)
                    choices.Add(i);
            }
        }

        if (choices.Count == 0)
            return currentBush;

        return choices[Random.Range(0, choices.Count)];
    }

    IEnumerator MoveToNextBush()
    {
        int nextBush = ChooseNextBush();

        if (nextBush == currentBush || squirrel == null)
            yield break;

        isMoving = true;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        // Start squirrel running.
        if (squirrelAnimator != null)
            squirrelAnimator.SetTrigger("Run");

        Vector3 destination =
            bushes[nextBush].squirrelHidePoint.position;

        // Gradually stop the old bush shaking and rustling.
        StartCoroutine(StopRustling(currentBush));

        Vector3 start = squirrel.position;
        Vector3 direction = destination - start;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
            squirrel.rotation =
                Quaternion.LookRotation(direction);

        float distance = Vector3.Distance(start, destination);
        float duration = distance / Mathf.Max(squirrelSpeed, 0.1f);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            Vector3 position =
                Vector3.Lerp(start, destination, t);

            // Small running hops.
            position.y += Mathf.Abs(
                Mathf.Sin(t * distance * 5f)) * jumpHeight;

            squirrel.position = position;

            yield return null;
        }

        squirrel.position = destination;

        // Return squirrel to its idle state.
        if (squirrelAnimator != null)
            squirrelAnimator.Play("SquirrelIdle");

        lastBush = currentBush;
        currentBush = nextBush;

        shakeStrength = shakeAmount;
        StartRustling(currentBush);

        isMoving = false;
    }

    IEnumerator StopRustling(int index)
    {
        BushSpot bush = bushes[index];

        float elapsed = 0f;
        float startVolume = bush.rustlingAudio != null
            ? bush.rustlingAudio.volume : 0f;

        while (elapsed < stopDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / stopDuration);

            float strength = Mathf.Lerp(shakeAmount, 0f, t);

            ShakeBush(index, strength);

            if (bush.rustlingAudio != null)
                bush.rustlingAudio.volume =
                    Mathf.Lerp(startVolume, 0f, t);

            yield return null;
        }

        if (bush.bushModel != null)
            bush.bushModel.localRotation =
                bush.originalRotation;

        if (bush.rustlingAudio != null)
            bush.rustlingAudio.Stop();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
