using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class BallInteract : MonoBehaviour
{
    [Header("Game Manager")]
    public GameManager gameManager;
    public bool onLeft;

    [Header("Ball Interaction Settings")]
    public float upwardForce = 500f;
    public float interactionRadius = 5f;
    
    [Header("Input Settings")]
    public InputActionAsset inputActionAsset;
    
    private Rigidbody rb;
    private InputActionMap playerActionMap;
    private InputAction bumpAction;
    private InputAction setAction;
    private InputAction spikeAction;
    private InputAction directionAction; // Which direction the player will perform an action (no plans to use this for bumping atm)
    private Transform playerTransform;
    private GameObject ball;
    private Vector3 bumpToLocation; // Where the ball will go after bumping
    private Vector3 setToLocation; // Where the ball will go after setting
    private Vector3 spikeToLocation; // Where the ball will go after spiking
    private float spikeSpeed; // Speed of the ball when spiked
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerTransform = transform;
        spikeSpeed = 10.0f;
        
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

                spikeAction = playerActionMap.FindAction("Spike");
                if (spikeAction == null)
                {
                    Debug.LogError("Spike action not found!");
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
        else if (spikeAction != null && spikeAction.IsPressed())
        {
            SpikeBall();
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
        // Set bump to location to front middle of whatever side of the court is bumping
        bumpToLocation = new Vector3(2f, 0f, 0f);
        if (ballRb.transform.position.x < 0)
        {
            bumpToLocation *= -1;
        }
        
        // Set the ball's intial velocity
        SetBallInitVelocity(ballRb, bumpToLocation, 5.0f);

        // Update game manager fields
        gameManager.gameState = GameManager.GameState.Bumped;
        gameManager.lastHit = gameObject;
        gameManager.leftAttack = onLeft;
    }

    private void SpikeBall()
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
                // Set the spiking location to middle-back of court on the rightside as default
                spikeToLocation = new Vector3(8, 0, 0);

                // If rightside is spiking, switch to spike towards leftside
                if (ballRb.transform.position.x > 0)
                {
                    spikeToLocation *= -1;
                }

                // Get the direction value
                Vector2 dir = directionAction.ReadValue<Vector2>();

                // If player wants to spike towards top or bottom, update set to location
                if (dir.y < -0.64f)
                {
                    spikeToLocation.z -= 4;
                }
                else if (dir.y > 0.64f)
                {
                    spikeToLocation.z += 4;
                }

                // Set the ball's initial velocity
                SetBallInitVelocity(ballRb, spikeToLocation, -1.0f);

                // Update game manager fields
                gameManager.gameState = GameManager.GameState.Spiked;
                gameManager.lastHit = gameObject;
            }
            else
            {
                Debug.LogWarning("Ball has no Rigidbody component!");
            }
        } 
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

                // Update game manager fields
                gameManager.gameState = GameManager.GameState.Set;
                gameManager.lastHit = gameObject;
            }
            else
            {
                Debug.LogWarning("Ball has no Rigidbody component!");
            }
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
}
