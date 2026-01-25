using UnityEngine;
using UnityEngine.InputSystem;

public class PenguinScript : MonoBehaviour
{
    [Header("Dash Ability")]
    public float dashCooldown = 5.0f; // How long it takes to use the dash again
    public float dashDuration = 1.0f; // How long the dash will last
    public float dashSpeed = 10.0f; // Forward movement speed during dash
    public float rotationSpeed = 8.0f; // How fast penguin rotates
    
    [HideInInspector] public bool isDashing = false; // If the penguin is dashing
    private bool isReturningUpright = false; // Ensure penguin returns to upright after dash
    private float dashTimer = 0.0f; // Tracker for the cooldown
    private float cooldownTimer = 0.0f; // Tracker for the next dash
    private CharacterMovement characterMovement; // Track movement from character movement script
    private Rigidbody rb; // Rigidbody of the penguin

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get the rigidbody and the character movement components from the penguin
        rb = GetComponent<Rigidbody>();
        characterMovement = GetComponent<CharacterMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        // Handle dash input using Input System
        InputAction dash = InputSystem.actions.FindAction("Dash");
        bool dashPressed = (dash != null && dash.WasPressedThisFrame()) || 
                          (dash == null && Keyboard.current?.spaceKey.wasPressedThisFrame == true);
        
        if (dashPressed && cooldownTimer <= 0 && !isDashing)
        {
            StartDash();
        }

        // Update timers
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0) EndDash();
        }

        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
        
        // Handle returning to upright position
        if (isReturningUpright && characterMovement != null)
        {
            float angleFromUpright = Vector3.Angle(transform.up, Vector3.up);
            if (angleFromUpright < 5.0f)
            {
                isReturningUpright = false;
                characterMovement.overrideRotation = false;
            }
        }
    }

    // Start the dash for the penguin
    void StartDash()
    {
        // Mark the penguin as dashing and start the dash timer
        isDashing = true;
        dashTimer = dashDuration;
        
        // Apply forward force in the direction penguin is currently facing
        rb.AddForce(transform.forward * dashSpeed, ForceMode.Impulse);
        
        // Override CharacterMovement rotation to do belly slide
        if (characterMovement != null)
        {
            characterMovement.overrideRotation = true;
            characterMovement.targetRotation = transform.rotation * Quaternion.Euler(90, 0, 0);
        }
    }

    // End the dash for the penguin
    void EndDash()
    {
        // Mark the penguin as not dashing and start the cooldown timer
        isDashing = false;
        cooldownTimer = dashCooldown;
        
        // Start transition back to upright position
        if (characterMovement != null)
        {
            Vector3 currentEuler = transform.eulerAngles;
            Quaternion uprightRotation = Quaternion.Euler(0, currentEuler.y, 0);
            characterMovement.targetRotation = uprightRotation;
            isReturningUpright = true;
        }
    }
}
