using UnityEngine;

public class EnemyFollowerMovement : MonoBehaviour
{
    private Transform playerTransform; // Reference to the player's transform
    private Rigidbody2D rb; // Reference to the enemy's Rigidbody2D
    private Vector2 moveDirection; // Direction towards the player
    private EnemyStatus enemyStatus; // Reference to the EnemyStatus script

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); // Get the Rigidbody2D component
        enemyStatus = GetComponent<EnemyStatus>(); // Get the EnemyStatus component
    }

    void Start()
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

    void Update()
    {
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position); // Calculate distance to player
            if (distanceToPlayer <= enemyStatus.StopDistance)
            {
                moveDirection = Vector2.zero; // Stop moving if within stop distance
            }
            else
            {
                moveDirection = (playerTransform.position - transform.position).normalized; // Calculate direction to player
            }
        }
    }
    
    void FixedUpdate()
    {
        if (playerTransform != null)
        {
            rb.MovePosition(rb.position + moveDirection * enemyStatus.MoveSpeed * Time.fixedDeltaTime); // Move the enemy towards the player
        }
    }
}
