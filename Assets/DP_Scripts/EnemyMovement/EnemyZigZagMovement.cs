using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyStatus))]
public class EnemyZigZagMovement : MonoBehaviour
{
    [Header("ZigZag Movement Settings")]
    public float zigZagSpeed = 5f; // Speed of the zigzag movement
    public float zigZapMagnitude = 2f; // Magnitude of the zigzag oscillation
    
    private Transform playerTransform; // Reference to the player's transform
    private Rigidbody2D rb; // Reference to the enemy's Rigidbody2D
    private EnemyStatus enemyStatus; // Reference to the EnemyStatus script

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); // Get the Rigidbody2D component
        enemyStatus = GetComponent<EnemyStatus>(); // Get the EnemyStatus component
    }

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player"); // Find the player by tag
        if (playerObject != null)
        {
            playerTransform = playerObject.transform; // Get the player's transform
        }
        else
        {
            Debug.LogError("Player object not found. Please ensure the player has the 'Player' tag.");
            this.enabled = false; // Disable this script if player is not found
        }
    }

    private void FixedUpdate()
    {
        if (playerTransform == null)
        {
            rb.linearVelocity = Vector2.zero; // Stop movement if playerTransform is not set
            return; // Exit if playerTransform is not set
        }

        Vector2 forwardDirection = (playerTransform.position - transform.position).normalized; // Direction towards the player
        Vector2 perpendicularDirection = new Vector2(-forwardDirection.y, forwardDirection.x); // Perpendicular direction for zigzag

        float sineWave = Mathf.Sin(Time.time * zigZagSpeed); // Calculate sine wave for zigzag effect
        Vector2 finalDirection = (forwardDirection + perpendicularDirection * sineWave * zigZapMagnitude).normalized; // Combine forward and zigzag directions
        rb.linearVelocity = finalDirection * enemyStatus.MoveSpeed; // Apply velocity to the Rigidbody2D
    }
}
