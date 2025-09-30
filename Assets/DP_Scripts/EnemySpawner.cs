using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemyType
    {
        public GameObject enemyPrefab;
        [Range(0, 100)]
        public int spawnChance = 50; // Spawn chance for this enemy (0-100%)
    }

    [System.Serializable]
    public class SpawnWave
    {
        [Header("Wave Settings")]
        public float waveStartTime = 0f; // Time in seconds when this wave starts
        public float spawnInterval = 3f; // Overrides global spawn interval for this wave
        public List<EnemyType> waveEnemyTypes; // Specific enemy types and chances for this wave
    }

    [Header("Spawner Settings")]
    public List<SpawnWave> spawnWaves; // List of different spawn waves, ordered by waveStartTime
    public float distanceToSpawn = 15f; // Distance from the camera to spawn enemies
    
    [Header("Difficulty Settings")]
    public float initialDifficultyMultiplier = 1f; // Initial multiplier for enemy stats
    public float difficultyIncreaseRate = 0.005f; // How much the difficulty multiplier increases per second
    // Ex: 0.02f means after 50 seconds, the multiplier will be 2.0

    private float timer;
    private Camera mainCamera;
    private int totalSpawnChance;
    private int currentWaveIndex = -1; // -1 indicates no wave active yet
    private float gameTime = 0f; // Tracks total game time for wave progression

    void Start()
    {
        mainCamera = Camera.main;
        timer = 0f; // Start timer to check for first wave immediately

        // Ensure waves are sorted by start time
        spawnWaves.Sort((a, b) => a.waveStartTime.CompareTo(b.waveStartTime));

        // Initialize with the first wave's settings if available
        if (spawnWaves.Count > 0)
        {
            UpdateWave(0);
        }
    }

    void Update()
    {
        gameTime += Time.deltaTime; // Increment total game time

        // Check for wave transitions
        if (currentWaveIndex + 1 < spawnWaves.Count && gameTime >= spawnWaves[currentWaveIndex + 1].waveStartTime)
        {
            UpdateWave(currentWaveIndex + 1);
        }

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            SpawnEnemy();
            // Use the current wave's spawn interval
            timer = spawnWaves[currentWaveIndex].spawnInterval;
        }
    }

    void UpdateWave(int newWaveIndex)
    {
        if (newWaveIndex >= spawnWaves.Count) return; // No more waves

        currentWaveIndex = newWaveIndex;
        Debug.Log($"Transitioning to Wave {currentWaveIndex + 1} at game time {gameTime:F2}");

        // Recalculate total spawn chance for the new wave's enemy types
        totalSpawnChance = 0;
        foreach (EnemyType enemy in spawnWaves[currentWaveIndex].waveEnemyTypes)
        {
            totalSpawnChance += enemy.spawnChance;
        }

        if (totalSpawnChance == 0 && spawnWaves[currentWaveIndex].waveEnemyTypes.Count > 0)
        {
            Debug.LogWarning($"Wave {currentWaveIndex + 1}: The sum of enemy spawn chances is 0. No enemies will be spawned. Check 'Spawn Chance' settings.");
        }

        // Reset timer based on the new wave's interval
        timer = spawnWaves[currentWaveIndex].spawnInterval;
    }

    void SpawnEnemy()
    {
        if (currentWaveIndex == -1 || spawnWaves[currentWaveIndex].waveEnemyTypes.Count == 0 || totalSpawnChance == 0)
        {
            return;
        }

        GameObject selectedEnemyPrefab = GetRandomEnemyPrefab();

        if (selectedEnemyPrefab != null)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();
            GameObject newEnemy = Instantiate(selectedEnemyPrefab, spawnPosition, Quaternion.identity);

            EnemyStatus enemyStatus = newEnemy.GetComponent<EnemyStatus>();
            if (enemyStatus != null)
            {
                // Calculate current difficulty multiplier based on game time
                float currentDifficultyMultiplier = initialDifficultyMultiplier + (gameTime * difficultyIncreaseRate);
                enemyStatus.InitializeStats(currentDifficultyMultiplier);
                Debug.Log($"Spawned enemy with difficulty multiplier: {currentDifficultyMultiplier:F2}. Base Health: {enemyStatus.MaxHealth / currentDifficultyMultiplier:F1}, Actual Health: {enemyStatus.MaxHealth:F1}");
            }
            else
            {
                Debug.LogWarning($"Enemy prefab {selectedEnemyPrefab.name} is missing an EnemyStatus component!");
            }
        }
    }

    private GameObject GetRandomEnemyPrefab()
    {
        int randomChance = Random.Range(0, totalSpawnChance);
        int currentChanceSum = 0;

        foreach (EnemyType enemy in spawnWaves[currentWaveIndex].waveEnemyTypes)
        {
            currentChanceSum += enemy.spawnChance;
            if (randomChance < currentChanceSum)
            {
                return enemy.enemyPrefab;
            }
        }
        return null;
    }

    private Vector3 GetRandomSpawnPosition()
    {
        int side = Random.Range(0, 4);
        Vector2 viewportPosition = Vector2.zero;

        switch (side)
        {
            case 0: // Top
                viewportPosition = new Vector2(Random.Range(0f, 1f), 1.1f);
                break;
            case 1: // Bottom
                viewportPosition = new Vector2(Random.Range(0f, 1f), -0.1f);
                break;
            case 2: // Right
                viewportPosition = new Vector2(1.1f, Random.Range(0f, 1f));
                break;
            case 3: // Left
                viewportPosition = new Vector2(-0.1f, Random.Range(0f, 1f));
                break;
        }

        Vector3 worldPosition = mainCamera.ViewportToWorldPoint(new Vector3(viewportPosition.x, viewportPosition.y, mainCamera.nearClipPlane));
        worldPosition.z = 0f;
        return worldPosition;
    }
}