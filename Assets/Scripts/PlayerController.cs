
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 0.1f;
    public Transform playerCamera;

    [Header("Hunger Movement")]
    [Range(0.1f, 1f)]
    public float hungerSpeedMultiplier = 1f;

    private CharacterController controller;
    private float xRotation = 0f;
    private float verticalVelocity = 0f;

    // Used by the head bob script.
    public float HorizontalSpeed { get; private set; }

    void Start()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Keyboard.current == null)
            return;

        // WASD movement.
        float x = 0f;
        float z = 0f;

        if (Keyboard.current.aKey.isPressed) x -= 1f;
        if (Keyboard.current.dKey.isPressed) x += 1f;
        if (Keyboard.current.wKey.isPressed) z += 1f;
        if (Keyboard.current.sKey.isPressed) z -= 1f;

        // Prevent faster diagonal movement.
        Vector3 input = Vector3.ClampMagnitude(
            new Vector3(x, 0f, z), 1f
        );

        Vector3 horizontalMove =
            transform.TransformDirection(input) * moveSpeed;

        // Apply gravity.
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 movement =
        horizontalMove * hungerSpeedMultiplier;

        // Add gravity back into the movement.
        movement.y = verticalVelocity;

        // One CharacterController.Move call per frame.
        controller.Move(movement * Time.deltaTime);

        HorizontalSpeed = new Vector3(
            controller.velocity.x,
            0f,
            controller.velocity.z
        ).magnitude;

        // Mouse look.
        if (Mouse.current != null && playerCamera != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            float mouseX = mouseDelta.x * mouseSensitivity;
            float mouseY = mouseDelta.y * mouseSensitivity;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            playerCamera.localRotation =
                Quaternion.Euler(xRotation, 0f, 0f);

            transform.Rotate(Vector3.up * mouseX);
        }
    }
}
