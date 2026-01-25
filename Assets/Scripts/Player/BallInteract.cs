using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class BallInteract : MonoBehaviour
{
    [Header("Game Manager")]
    public GameManager gameManager; // Manager for the game logic
    public bool onLeft; // Whether the player is on the left court or not

    [Header("Ball Manager")]
    public BallManager ballManager; // Manager for the ball
    public float interactionRadius = 5f; // How far the player can interact with the ball

    private Rigidbody rb; // Rigidbody of the player
    private InputActionMap playerActionMap;
    private GameObject ball; // Ball in the game
    private Rigidbody ballRb; // Rigidbody of the ball
    private Vector3 bumpToLocation; // Where the ball will go after bumping
    private Vector3 setToLocation; // Where the ball will go after setting
    private Vector3 spikeToLocation; // Where the ball will go after spiking
    private Vector3 serveToLocation; // Where the ball will go after spiking
    private float spikeSpeed; // Speed of the ball when spiked
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Assign player rigidbody and spike speed
        rb = GetComponent<Rigidbody>();
        spikeSpeed = 10.0f;
        
        // Get the ball and its rigid body, log errors if either are missing
        ball = GameObject.FindGameObjectWithTag("Ball");
        if (ball != null)
        {
            ballRb = ball.GetComponent<Rigidbody>();
            if (ballRb == null)
            {
                Debug.LogError("Ball rigid body was not found for BallInteract!");
            }
        }
        else
        {
            Debug.LogError("Ball game object was not found for BallInteract!");
        }

        // Make sure ball manager was assigned in inspector
        if (ballManager == null)
        {
            Debug.LogError("Ball Manager was not set in inspector for BallInteract!");
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        // If the ball and its rigidbody exist, check the player state
        if (ball != null && ballRb != null)
        {
            CheckState();
        } 
    }

    // Check the state of player for legal input
    private void CheckState()
    {
        // If the player can legally hit the ball
        if (CanHit())
        {
            // Check the state of the game
            switch (gameManager.gameState)
            {
                // Ball was spiked or served
                case GameManager.GameState.Spiked: case GameManager.GameState.Served:
                    // If the player is near the ball and they press the bump button, bump the ball
                    if (IsPlayerNearBall() && InputSystem.actions.FindAction("Bump").WasPerformedThisFrame())
                    {
                        BumpBall();
                    }
                    break;
                // Ball was bumped
                case GameManager.GameState.Bumped:
                    // If the player is near the ball and they press the set button, set the ball
                    if (IsPlayerNearBall() && InputSystem.actions.FindAction("Set").WasPerformedThisFrame())
                    {
                        SetBall();
                    }
                    break;
                // Ball was set
                case GameManager.GameState.Set:
                    // If the player is near the ball and they press the spike button, spike the ball
                    if (IsPlayerNearBall() && InputSystem.actions.FindAction("Spike").WasPerformedThisFrame())
                    {
                        SpikeBall();
                    }
                    break;
                // If the point is about to start (ball needs to be served)
                case GameManager.GameState.PointStart:
                    // If this player is the one serving and they press the serve button, serve the ball
                    if (gameManager.server == gameObject && InputSystem.actions.FindAction("Serve").WasPerformedThisFrame())
                    {
                        ServeBall();
                    }
                    break;
            }
        }
    }

    // Checks to see if they player can legally hit the ball
    private bool CanHit()
    {
        // If this player just hit the ball, they cannot hit it again
        if (gameObject.Equals(gameManager.lastHit)) return false;

        // If the ball has been served by the other team, they can hit
        if (!gameManager.leftAttack.Equals(onLeft) && gameManager.gameState.Equals(GameManager.GameState.Served)) return true;

        // If it's the player's turn to serve, they can hit
        if (gameManager.leftAttack.Equals(onLeft) && gameManager.gameState.Equals(GameManager.GameState.PointStart)
            && gameManager.server == gameObject) return true;

        // If the ball has been served by a teammate, they cannot hit it
        if (gameManager.leftAttack.Equals(onLeft) && gameManager.gameState.Equals(GameManager.GameState.Served)) return false;

        // If the ball is on this side of the court and it has not been spiked yet, they can hit it
        if (gameManager.leftAttack.Equals(onLeft) && !gameManager.gameState.Equals(GameManager.GameState.Spiked)) return true;

        // If the ball is on the other side of the court and has been spiked, they can hit, else they cannot
        return !gameManager.leftAttack.Equals(onLeft) && gameManager.gameState.Equals(GameManager.GameState.Spiked);
    }

    // Checks if the player is near the ball
    private bool IsPlayerNearBall()
    {
        // Get the distance the player is from the ball, then return whether that distance is within the interaction radius
        float distance = Vector3.Distance(transform.position, ball.transform.position);
        return distance <= interactionRadius;
    }

    // Bump the ball
    private void BumpBall()
    {
        // Set bump to location to front of the right court
        bumpToLocation = new Vector3(2f, 0f, 0f);

        // If on left court, set to front of left court
        if (onLeft)
        {
            bumpToLocation *= -1;
        }
        
        // Set the ball's intial velocity and destination
        SetBallInitVelocity(ballRb, bumpToLocation, 5.0f);
        ballManager.goingTo = bumpToLocation;

        // Update game manager fields
        gameManager.gameState = GameManager.GameState.Bumped;
        gameManager.lastHit = gameObject;
        gameManager.leftAttack = onLeft;
    }

    // Set the ball
    private void SetBall()
    {
        // Set the setting location to middle of court as default
        setToLocation = bumpToLocation;

        // Get the direction value
        Vector2 dir = InputSystem.actions.FindAction("Direction").ReadValue<Vector2>();

        // If player wants to set towards top or bottom, update set to location
        if (dir.y < -0.64f)
        {
            setToLocation -= new Vector3(0, 0, 4); // Upper side of the court
        }
        else if (dir.y > 0.64f)
        {
            setToLocation += new Vector3(0, 0, 4); // Lower side of the court
        }

        // Set the ball's initial velocity and destination
        SetBallInitVelocity(ballRb, setToLocation, 5.0f);
        ballManager.goingTo = setToLocation;

        // Update game manager fields
        gameManager.gameState = GameManager.GameState.Set;
        gameManager.lastHit = gameObject;
    }

    // Spike the ball
    private void SpikeBall()
    {
        // Set the spiking location to middle-back of court on the rightside as default
        spikeToLocation = new Vector3(8, 0, 0);

        // If rightside is spiking, switch to spike towards leftside
        if (!onLeft)
        {
            spikeToLocation *= -1;
        }

        // Get the direction value
        Vector2 dir = InputSystem.actions.FindAction("Direction").ReadValue<Vector2>();

        // If player wants to spike towards top or bottom, update set to location
        if (dir.y < -0.64f)
        {
            spikeToLocation.z -= 4;
        }
        else if (dir.y > 0.64f)
        {
            spikeToLocation.z += 4;
        }

        // Set the ball's initial velocity and destination
        SetBallInitVelocity(ballRb, spikeToLocation, -1.0f);
        ballManager.goingTo = spikeToLocation;

        // Update game manager fields
        gameManager.gameState = GameManager.GameState.Spiked;
        gameManager.lastHit = gameObject;
    }

    // Serve the ball
    private void ServeBall()
    {
        // Set the serving location to middle-back of court on the rightside as default
        serveToLocation = new Vector3(8, 0, 0);

        // If rightside is spiking, switch to serve towards leftside
        if (!onLeft)
        {
            serveToLocation *= -1;
        }

        // Get the direction value
        Vector2 dir = InputSystem.actions.FindAction("Direction").ReadValue<Vector2>();

        // If player wants to set towards top or bottom, update set to location
        if (dir.y < -0.64f)
        {
            serveToLocation -= new Vector3(0, 0, 4);
        }
        else if (dir.y > 0.64f)
        {
            serveToLocation += new Vector3(0, 0, 4);
        }

        // Set the ball's initial velocity and destination
        SetBallInitVelocity(ballRb, serveToLocation, 6.0f);
        ballManager.goingTo = serveToLocation;

        // Update game manager fields
        gameManager.gameState = GameManager.GameState.Served;
        gameManager.lastHit = gameObject;
        gameManager.leftAttack = onLeft;
    }

    // Set the velocity of the ball after interacting with it
    private void SetBallInitVelocity(Rigidbody ballRb, Vector3 endLocation, float maxHeight)
    {
        // Bumping, setting, or serving
        if (maxHeight > ballRb.transform.position.y)
        {
            // If gravity is disabled, enable it
            if (!ballRb.useGravity)
            {
                ballRb.useGravity = true;
            }

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
        else // Spiking or blocking
        {
            // If gravity is enabled, disable it
            if (ballRb.useGravity)
            {
                ballRb.useGravity = false;
            }

            // Calculate the direction the ball will go in
            Vector3 initVel = endLocation - ballRb.transform.position;

            // Set speed of inital velocity
            initVel.Normalize();
            initVel *= spikeSpeed;

            // Set the ball's intial velocity
            ballRb.linearVelocity = initVel;
        }
    }
}
