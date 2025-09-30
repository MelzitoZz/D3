using UnityEngine;
using System.Collections;

public class XPGem : MonoBehaviour
{
    public float xpValue = 5f; // XP value given to the player upon defeat
    public float moveSpeed = 12f; // Speed at which the gem moves towards the player
    public float collectionDistance = 0.5f; // Radius within which the gem detects the player
    private Transform playerTransform; // Reference to the player's transform
    private bool isFollowingPlayer = false; // Flag to check if the gem is following the player

    void Start()
    {
        CheckIfAlreadyInCollectorRange(); // Check if the gem is already within the player's collector range at spawn
    }

    void Update()
    {
        if (isFollowingPlayer && playerTransform != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime); // Move towards the player

            if (Vector2.Distance(transform.position, playerTransform.position) < collectionDistance) // Check if within collection distance
            {
                CollectXP(); // Collect XP and destroy the gem
            }
            ;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerCollector"))
        {
            StartFollowingPlayer(other.transform); // Start following the player when entering the collector range
        }
    }

    private void CollectXP()
    {
        if (playerTransform == null) return;

        PlayerExperience playerXP = playerTransform.GetComponentInParent<PlayerExperience>(); // Get the PlayerExperience component from the player
        if (playerXP != null)
        {
            playerXP.AddExperience(xpValue); // Add XP to the player
            Destroy(gameObject); // Destroy the gem
        }
    }

    void StartFollowingPlayer(Transform player)
    {
        playerTransform = player;
        isFollowingPlayer = true;
    }

    private void CheckIfAlreadyInCollectorRange()
    {
        GameObject playerCollector = GameObject.FindGameObjectWithTag("PlayerCollector"); // Find the player collector object
        if (playerCollector == null) return;

        CircleCollider2D collectorCollider = playerCollector.GetComponent<CircleCollider2D>(); // Get the collector's collider
        if (collectorCollider == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerCollector.transform.position); // Calculate distance to the player collector

        if (distanceToPlayer < collectorCollider.radius)
        {
            StartFollowingPlayer(playerCollector.transform); // Start following the player if already within range
        }

    }
}
