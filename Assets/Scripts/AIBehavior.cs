using UnityEngine;
using System;

public class AIBehavior : MonoBehaviour
{
    [Header("Ball Interaction Settings")]
    public float interactionRadius = 5f;

    [Header("Movement Attributes")]
    public float maxGroundSpeed = 1.0f; // Max speed that the character can move on the ground
    public float maxAirSpeed = 1.0f; // Max speed that the character can move in the air
    public float jumpForce = 1.0f; // Force the character uses to jump 

    private float directionChangeWeight = 15f; // How quickly the character can change direction
    private bool grounded = false; // If the character is touching the ground
    private GameObject ball;
    private AIState currState;
    private Rigidbody ballRb;
    private Vector3 bumpToLocation; // Where the ball will go after bumping
    private Vector3 setToLocation; // Where the ball will go after setting
    private Vector3 spikeToLocation; // Where the ball will go after spiking
    private float spikeSpeed; // Speed of the ball when spiked

    private enum AIState
    {
        Waiting,
        Bumping,
        Setting,
        Spiking
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ball = GameObject.FindGameObjectWithTag("Ball");
        if (ball != null)
        {
            ballRb = ball.GetComponent<Rigidbody>();
        }
        else
        {
            Debug.LogError("Ball game object was not found for AIBehavior!");
        }

        currState = AIState.Waiting;
        spikeSpeed = 10f;
    }

    // Update is called once per frame
    void Update()
    {
        if (ball != null)
        {
            CheckState();
        }
    }

    private void CheckState()
    {
        switch (currState)
        {
            case AIState.Waiting:
                if (ball.transform.position.x * transform.position.x >= 0 && grounded)
                {
                    currState = AIState.Bumping;
                }
                break;
            case AIState.Bumping:
                if (IsAINearBall() && ballRb.linearVelocity.y < 0)
                {
                    BumpBall();
                    currState = AIState.Setting;
                }
                else
                {
                    MoveAI();
                }
                break;
            case AIState.Setting:
                if (IsAINearBall() && ballRb.linearVelocity.y < 0)
                {
                    SetBall();
                    currState = AIState.Spiking;
                }
                else
                {
                    MoveAI();
                }
                break;
            case AIState.Spiking:
                if (ballRb.linearVelocity.y < 0)
                {
                    if (grounded)
                    {
                        GetComponent<Rigidbody>().linearVelocity += new Vector3(0, jumpForce, 0);
                        grounded = false;
                    }
                    else if (IsAINearBall())
                    {
                        SpikeBall();
                        currState = AIState.Waiting;
                    }
                }
                break;
        }
    }

    private bool IsAINearBall()
    {
        float distance = Vector3.Distance(transform.position, ball.transform.position);
        return distance <= interactionRadius;
    }
    
    private void MoveAI()
    {
        // Get the AI's rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();

        // Get the direction the AI needs to move in
        float dx = ball.transform.position.x - transform.position.x;
        float dz = ball.transform.position.z - transform.position.z;
        Vector2 dir = new Vector2(dx, dz);

        // Update the current direction and speed of the character based on player input
        if (!dir.Equals(Vector2.zero))
        {
            // Calculate new velocity, ensure it doesn't exceed max ground or air speed, then assign the velocity
            Vector2 newVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z) + dir * Time.fixedDeltaTime * directionChangeWeight;

            if (grounded && newVelocity.magnitude > maxGroundSpeed)
            {
                newVelocity.Normalize();
                newVelocity *= maxGroundSpeed;
            }
            else if (!grounded && newVelocity.magnitude > maxAirSpeed)
            {
                newVelocity.Normalize();
                newVelocity *= maxAirSpeed;
            }

            rb.linearVelocity = new Vector3(newVelocity.x, rb.linearVelocity.y, newVelocity.y);
        }
    }

    private void BumpBall()
    {
        // Set bump to location to front middle of whatever side of the court is bumping
        bumpToLocation = new Vector3(1f, 0f, 0f);
        if (ballRb.transform.position.x < 0)
        {
            bumpToLocation *= -1;
        }
        
        // Set the ball's intial velocity
        SetBallInitVelocity(ballRb, bumpToLocation, 5.0f);
    }

    private void SetBall()
    {        
        if (ballRb != null)
        {
            // Set the setting location to middle of court as default
            setToLocation = bumpToLocation;

            // Set the ball's initial velocity
            SetBallInitVelocity(ballRb, setToLocation, 6.0f);
        }
        else
        {
            Debug.LogWarning("Ball has no Rigidbody component!");
        }
    }
    private void SpikeBall()
    {
        if (ballRb != null)
        {
            // Set the spiking location to middle-back of court on the rightside as default
            spikeToLocation = new Vector3(8, 0, 0);

            // If rightside is spiking, switch to spike towards leftside
            if (ballRb.transform.position.x > 0)
            {
                spikeToLocation *= -1;
            }

            // Set the ball's initial velocity
            SetBallInitVelocity(ballRb, spikeToLocation, -1.0f);
        }
        else
        {
            Debug.LogWarning("Ball has no Rigidbody component!");
        }
    }

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

    // Calls whenever the character collides with another collider or rigidbody
    void OnCollisionEnter(Collision other)
    {
        // If the character collides with the court, it is now grounded
        if (other.gameObject.layer == 6)
        {
            grounded = true;
            Debug.Log("AI has landed.");
        }
    }

    // Calls whenever the character stops colliding with another collider or rigidbody
    void OnCollisionExit(Collision other)
    {
        // If the character stops colliding with the court, it is no longer grounded
        if (other.gameObject.layer == 6)
        {
            grounded = false;
            Debug.Log("AI has jumped.");
        }
    }
}
