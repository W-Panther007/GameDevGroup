using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // Import the Input System

public class PlayerMovement : MonoBehaviour
{
    private Vector2 PlayerMovementInput; // Input for moving the player
    private Vector2 PlayerMouseInput; // Input for looking around
    private float xRot; // Vertical rotation for the camera

    [SerializeField] public Transform PlayerCamera; // Reference to the player camera
    [SerializeField] private CharacterController Controller; // Reference to the character controller
    [Space]
    [SerializeField] private float Speed = 5f; // Movement speed
    [SerializeField] private float Sensitivity = 5000f; // Extremely high mouse sensitivity

    // Reference to the Input Action Asset
    private PlayerInputActions inputActions;

    private void Awake()
    {
        // Initialize the Input Action asset
        inputActions = new PlayerInputActions();

        // Enable the action map
        inputActions.Player.Enable();

        // Set up the callbacks for movement
        inputActions.Player.Move.performed += ctx => PlayerMovementInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => PlayerMovementInput = Vector2.zero; // Stop movement when keys are released

        // Set up the callbacks for looking around
        inputActions.Player.Look.performed += ctx => PlayerMouseInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => PlayerMouseInput = Vector2.zero; // Stop looking when the mouse stops moving
    }

    private void Update()
    {
        MovePlayer();
        MovePlayerCamera();
    }

    private void MovePlayer()
    {
        // Convert 2D movement input to 3D
        Vector3 MoveVector = new Vector3(PlayerMovementInput.x, 0f, PlayerMovementInput.y);
        MoveVector = transform.TransformDirection(MoveVector); // Transform relative to the player's direction

        // Move the player using the CharacterController
        Controller.Move(MoveVector * Speed * Time.deltaTime);
    }

    private void MovePlayerCamera()
    {
        // Adjust camera rotation based on mouse input with an additional multiplier
        float lookMultiplier = 10f; // Adjust this value to increase the speed further
        xRot -= PlayerMouseInput.y * Sensitivity * lookMultiplier * Time.deltaTime; // Invert vertical rotation
        xRot = Mathf.Clamp(xRot, -90f, 90f); // Clamping the vertical rotation to avoid flipping

        // Apply horizontal rotation to the player
        transform.Rotate(0f, PlayerMouseInput.x * Sensitivity * lookMultiplier * Time.deltaTime, 0f);
        // Apply vertical rotation to the camera
        PlayerCamera.transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);
    }

    private void OnDisable()
    {
        // Disable the input actions when the object is disabled
        inputActions.Player.Disable();
    }
}
