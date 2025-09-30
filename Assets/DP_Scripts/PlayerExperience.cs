using UnityEngine;

public class PlayerExperience : MonoBehaviour
{
    public float currentXP = 0f; // Current experience points of the player
    public float xpToNextLevel = 100f; // XP required to reach the next level
    public int playerLevel = 1; // Current level of the player
    public float xpGrowthRate = 1.5f; // Growth rate for XP required per level

    public void AddExperience(float amount)
    {
        currentXP += amount; // Add the gained XP to current XP
        Debug.Log("Gained " + amount + " XP. Current XP: " + currentXP);

        // Check if the player has enough XP to level up
        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel; // Subtract the required XP for leveling up
            LevelUp();
        }
    }
    
    public void LevelUp()
    {
        playerLevel++; // Increase the player's level
        xpToNextLevel *= xpGrowthRate; // Increase the XP required for the next level
        Debug.Log("Level up! New level: " + playerLevel);
    }
}
