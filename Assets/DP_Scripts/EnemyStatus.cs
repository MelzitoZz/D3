using UnityEngine;

public class EnemyStatus : MonoBehaviour
{
    [SerializeField]
    private float baseMaxHealth = 10f; // Base health of the enemy
    [SerializeField]
    private float baseMoveSpeed = 3f; // Base speed of the enemy movement
    [SerializeField]
    private float xpValue = 5f; // XP value given to the player upon defeat
    [SerializeField]
    private float stopDistance = 1f; // Distance to stop from the player

    private float currentHealth; // Current health of the enemy
    private float actualMoveSpeed; // Actual move speed after adjustments

    [SerializeField] private GameObject xpGemPrefab;

    public float MaxHealth { get; private set; } // Public getter for actual maximum health
    public float MoveSpeed { get { return actualMoveSpeed; } } // Public getter for movement speed
    public float StopDistance { get { return stopDistance; } } // Public getter for stop distance

    void Awake()
    {
        // currentHealth e MaxHealth serão definidos pelo spawner através de InitializeStats
        // Se este inimigo for instanciado sem um spawner (ex: arrastado para a cena), ele usará os valores base.
        if (MaxHealth == 0) // Verifica se o spawner já inicializou
        {
            MaxHealth = baseMaxHealth;
            currentHealth = baseMaxHealth;
            actualMoveSpeed = baseMoveSpeed;
        }
    }

    /// <summary>
    /// Inicializa as estatísticas do inimigo com base em um fator de dificuldade.
    /// </summary>
    /// <param name="difficultyMultiplier">Multiplicador para aplicar à saúde e velocidade.</param>
    public void InitializeStats(float difficultyMultiplier)
    {
        MaxHealth = baseMaxHealth * difficultyMultiplier;
        currentHealth = MaxHealth; // Inicializa a vida atual com a vida máxima ajustada

        actualMoveSpeed = baseMoveSpeed * difficultyMultiplier; // Opcional: aumentar velocidade também
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
        if (xpGemPrefab != null)
        {
            GameObject gem = Instantiate(xpGemPrefab, transform.position, Quaternion.identity); // Spawn XP gem at enemy's position

            XPGem gemScript = gem.GetComponent<XPGem>(); // Get the XPGem script component
            if (gemScript != null)
            {
                gemScript.xpValue = xpValue; // Set the XP value of the gem
            }
        }

        Destroy(gameObject); // Destroy the enemy game object
    }
}