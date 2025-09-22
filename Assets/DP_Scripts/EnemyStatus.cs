using UnityEngine;

public class EnemyStatus : MonoBehaviour
{
    [SerializeField]
    private float maxHealth = 10f; // Maximum health of the enemy

    [SerializeField]
    private float moveSpeed = 3f; // Speed of the enemy movement

    [SerializeField]
    private float stopDistance = 1f; // Distance to stop from the player

    private float currentHealth; // Current health of the enemy

    public float MaxHealth { get { return maxHealth; } } // Public getter for maximum health
    public float MoveSpeed { get { return moveSpeed; } } // Public getter for movement speed
    public float StopDistance { get { return stopDistance; } } // Public getter for movement speed

    void Awake()
    {
        currentHealth = maxHealth; // Initialize current health to maximum health
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage; // Reduce current health by damage amount
        if (currentHealth <= 0)
        {
            Die(); // Call Die method if health drops to zero or below
        }
    }

    public void Die()
    {
        Destroy(gameObject); // Destroy the enemy game object
    }
}
