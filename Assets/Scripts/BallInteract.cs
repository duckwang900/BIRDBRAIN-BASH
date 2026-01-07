using System;
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
    private InputAction bumpAction;
    private InputAction setAction;
    private InputAction directionAction; // Which direction the player will perform an action (no plans to use this for bumping atm)
    private Transform playerTransform;
    private GameObject ball;
    private Vector3 bumpToLocation; // Where the ball will go after bumping
    private Vector3 setToLocation; // Where the ball will go after setting
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerTransform = transform;
        bumpToLocation = new UnityEngine.Vector3(1f, 0, 0);
        
        ball = GameObject.FindGameObjectWithTag("Ball");
        
        if (inputActionAsset != null)
        {
            playerActionMap = inputActionAsset.FindActionMap("Player");
            if (playerActionMap != null)
            {
                bumpAction = playerActionMap.FindAction("Bump");
                if (bumpAction != null)
                {
                    bumpAction.performed += OnInteract;
                }
                else
                {
                    Debug.LogError("Bump action not found!");
                }

                setAction = playerActionMap.FindAction("Set");
                if (setAction == null)
                {
                    Debug.LogError("Set action not found!");
                }

                directionAction = playerActionMap.FindAction("Direction");
                if (directionAction == null)
                {
                    Debug.LogError("Direction action not found!");
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
        if (bumpAction != null)
        {
            bumpAction.performed -= OnInteract;
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
                BumpBall(ballRb);
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
        if (bumpAction != null && bumpAction.IsPressed())
        {
            OnInteractFallback();
        }
        else if (setAction != null && setAction.IsPressed())
        {
            SetBall();
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
                BumpBall(ballRb);
            }
            else
            {
                Debug.LogWarning("Ball has no Rigidbody component!");
            }
        }
    }

    private void BumpBall(Rigidbody ballRb)
    {
        // Set the ball's intial velocity
        SetBallInitVelocity(ballRb, bumpToLocation, 5.0f);
    }

    private void SetBall()
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
                // Set the setting location to middle of court as default
                setToLocation = bumpToLocation;

                // Get the direction value
                Vector2 dir = directionAction.ReadValue<Vector2>();

                // If player wants to set towards top or bottom, update set to location
                if (dir.y < -0.64f)
                {
                    setToLocation -= new Vector3(0, 0, 4);
                }
                else if (dir.y > 0.64f)
                {
                    setToLocation += new Vector3(0, 0, 4);
                }

                // Set the ball's initial velocity
                SetBallInitVelocity(ballRb, setToLocation, 6.0f);
            }
            else
            {
                Debug.LogWarning("Ball has no Rigidbody component!");
            }
        } 
    }

    private void SetBallInitVelocity(Rigidbody ballRb, Vector3 endLocation, float maxHeight)
    {
        // Case where the ball needs to go up
        if (maxHeight > ballRb.transform.position.y)
        {
            // Calculate the velocity in the y direction for the ball to reach a height of 5 given its current y component
            float gravity = MathF.Abs(Physics.gravity.y);
            float vyInit = MathF.Sqrt(2 * gravity * (maxHeight - ballRb.transform.position.y));

            // Calculate time the ball will be in the air
            float vyFinal = MathF.Sqrt(10 * gravity);
            float t1 = vyInit / gravity;
            float t2 = vyFinal / gravity;
            float t = t1 + t2; 

            // Calculate the x and z velocities of the ball
            float vx = (endLocation.x - ballRb.transform.position.x) / t;
            float vz = (endLocation.z - ballRb.transform.position.z) / t;

            // Set the ball's intial velocity
            ballRb.linearVelocity = new Vector3(vx, vyInit, vz);
        }
    }
}
