using UnityEngine;
using UnityEngine.InputSystem;

public class BallInteract : MonoBehaviour
{
    [Header("Ball Interaction Settings")]
    public float upwardForce = 500f;
    public float interactionRadius = 5f;
    
    [Header("Input Settings")]
    public InputActionAsset inputActionAsset;
    
    private Rigidbody rb;
    private InputActionMap playerActionMap;
    private InputAction interactAction;
    private Transform playerTransform;
    private GameObject ball;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerTransform = transform;
        
        ball = GameObject.FindGameObjectWithTag("Ball");
        
        if (inputActionAsset != null)
        {
            playerActionMap = inputActionAsset.FindActionMap("Player");
            if (playerActionMap != null)
            {
                interactAction = playerActionMap.FindAction("Interact");
                if (interactAction != null)
                {
                    interactAction.performed += OnInteract;
                }
                else
                {
                    Debug.LogError("Interact action not found!");
                }
            }
            else
            {
                Debug.LogError("Player action map not found!");
            }
        }
        else
        {
            Debug.LogError("Input Action Asset not assigned!");
        }
    }

    void OnEnable()
    {
        if (playerActionMap != null)
        {
            playerActionMap.Enable();
        }
    }

    void OnDisable()
    {
        if (playerActionMap != null)
        {
            playerActionMap.Disable();
        }
    }

    void OnDestroy()
    {
        if (interactAction != null)
        {
            interactAction.performed -= OnInteract;
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (ball == null)
        {
            Debug.LogWarning("Ball is null!");
            return;
        }
        bool nearBall = IsPlayerNearBall();
        if (nearBall)
        {
            Rigidbody ballRb = ball.GetComponent<Rigidbody>();
            if (ballRb != null)
            {
                ballRb.AddForce(Vector3.up * upwardForce);
            }
            else
            {
                Debug.LogWarning("Ball has no Rigidbody component!");
            }
        }
    }

    private bool IsPlayerNearBall()
    {
        if (ball == null) return false;
        
        float distance = Vector3.Distance(playerTransform.position, ball.transform.position);
        return distance <= interactionRadius;
    }
    
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log("E key detected via Keyboard.current!");
            OnInteractFallback();
        }
    }
    
    private void OnInteractFallback()
    {
        Debug.Log("Fallback interact triggered!");
        
        if (ball == null)
        {
            Debug.LogWarning("Ball is null!");
            return;
        }
        
        bool nearBall = IsPlayerNearBall();
        Debug.Log($"Player near ball: {nearBall}, Distance: {Vector3.Distance(playerTransform.position, ball.transform.position)}");
        
        if (nearBall)
        {
            Rigidbody ballRb = ball.GetComponent<Rigidbody>();
            if (ballRb != null)
            {
                ballRb.AddForce(Vector3.up * upwardForce);
            }
            else
            {
                Debug.LogWarning("Ball has no Rigidbody component!");
            }
        }
    }
}
