

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

    [Header("Terrain Following")]
    public LayerMask groundLayer;
    public float raycastHeight = 3f;
    public float raycastDistance = 12f;
    public float groundOffset = 0.08f;
   
    [Header("Final Encounter")]

    public CanvasGroup fadeScreen;
    public PlayerController playerController;

    public float fadeDuration = 0.5f;
    public float eatingDuration = 3f;
    public SquirrelAttackCinematic finalAttack;
    private bool finalEncounterComplete;


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
                if (currentBush == bushes.Count - 1)
                {
                    if (finalAttack != null)
                    {
                        finalEncounterComplete = true;
                        isMoving = true;

                        if (interactionPrompt != null)
                            interactionPrompt.SetActive(false);

                        StartCoroutine(StopRustling(currentBush));
                        finalAttack.PlayCinematic();
                    }
                }
                else
                {
                    StartCoroutine(MoveToNextBush());
                }
            }

        }
    }

    
    Vector3 FollowGround(Vector3 position)
    {
        Vector3 rayStart =
            position + Vector3.up * raycastHeight;

        RaycastHit hit;

        if (Physics.Raycast(
            rayStart,
            Vector3.down,
            out hit,
            raycastHeight + raycastDistance,
            groundLayer,
            QueryTriggerInteraction.Ignore))
        {
            position.y = hit.point.y + groundOffset;
        }

        return position;
    }

    
    bool CanInteract()
    {
        if (isMoving || finalEncounterComplete ||
            player == null || bushes.Count == 0)
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
        // Move to the next bush in the Inspector list.
        int nextBush = currentBush + 1;

        // Stop at the final bush.
        if (nextBush >= bushes.Count)
            return currentBush;

        return nextBush;
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

            // Keep the squirrel on the snowy ground.
            position = FollowGround(position);

            squirrel.position = position;

            yield return null;
        }

        squirrel.position = FollowGround(destination);

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
