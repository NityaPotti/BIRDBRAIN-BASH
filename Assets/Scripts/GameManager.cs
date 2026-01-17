using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [Header("Left Team")]
    public GameObject leftPlayer1; // First player on left side
    public GameObject leftPlayer2; // Second player on left side

    [Header("Right Team")]
    public GameObject rightPlayer1; // First player on right side
    public GameObject rightPlayer2; // Second player on right side

    [Header("Game Manager Stuff")]
    public GameState gameState; // State of the match
    public GameObject lastHit; // Player that had the last hit
    public bool leftAttack; // If left is attacking
    public GameObject ball; // Ball object

    private Vector3 leftPlayer1Origin;
    private Vector3 leftPlayer2Origin;
    private Vector3 rightPlayer1Origin;
    private Vector3 rightPlayer2Origin;
    private Vector3 ballOrigin;

    public enum GameState
    {
        Bumped,
        Set,
        Spiked,
        Blocked
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Set the game state to bumping
        gameState = GameState.Spiked;

        // Set the last hit to null
        lastHit = null; 

        // Get all starting positions of players and ball
        if (leftPlayer1Origin != null)
        {
            leftPlayer1Origin = leftPlayer1.transform.position;
        }
        else
        {
            Debug.LogError("Left player 1 origin not set in inspector for Game Manager!");
        }

        if (leftPlayer2Origin != null)
        {
            leftPlayer2Origin = leftPlayer2.transform.position;
        }
        else
        {
            Debug.LogError("Left player 2 origin not set in inspector for Game Manager!");
        }

        if (rightPlayer1Origin != null)
        {
            rightPlayer1Origin = rightPlayer1.transform.position;
        }
        else
        {
            Debug.LogError("Righ player 1 origin not set in inspector for Game Manager!");
        }

        if (rightPlayer2Origin != null)
        {
            rightPlayer2Origin = rightPlayer2.transform.position;
        }
        else
        {
            Debug.LogError("Right player 2 origin not set in inspector for Game Manager!");
        }

        if (ballOrigin != null)
        {
            ballOrigin = ball.transform.position;
        }
        else
        {
            Debug.LogError("Ball origin not set in inspector for Game Manager!");
        }
    }

    void OnReset()
    {
        // Reset all positions and velocities for all players and ball
        leftPlayer1.transform.position = leftPlayer1Origin;
        leftPlayer2.transform.position = leftPlayer2Origin;
        rightPlayer1.transform.position = rightPlayer1Origin;
        rightPlayer2.transform.position = rightPlayer2Origin;
        ball.transform.position = ballOrigin;

        leftPlayer1.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        leftPlayer2.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        rightPlayer1.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        rightPlayer2.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        ball.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;

        // Enable gravity for the ball
        ball.GetComponent<Rigidbody>().useGravity = true;

        // Reset the game manager fields
        gameState = GameState.Spiked;
        lastHit = null;
        leftAttack = false;
    }
}
