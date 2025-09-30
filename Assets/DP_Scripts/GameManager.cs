using System.Collections;
using UnityEngine;

// Importante esse script não é o script real de GameManager é somente para eu testar o jogo.

public class GameManager : MonoBehaviour
{
    [Header("Auto Damage Test (dev only)")]
    [Tooltip("Habilita aplicar dano automaticamente a todos os inimigos na cena para testes")]
    public bool enableAutoDamage = true;

    [Tooltip("Dano aplicado a cada inimigo por tick")]
    public float damagePerTick = 2f;

    [Tooltip("Intervalo entre ticks em segundos")]
    public float tickInterval = 1f;

    private Coroutine autoDamageCoroutine;

    void Start()
    {
        if (enableAutoDamage)
        {
            autoDamageCoroutine = StartCoroutine(AutoDamageAllEnemies()); // Start the automatic damage coroutine
        }
    }

    private IEnumerator AutoDamageAllEnemies()
    {
        while (true)
        {
            EnemyStatus[] enemies = Object.FindObjectsByType<EnemyStatus>(FindObjectsSortMode.None); // Search for all active EnemyStatus in the scene

            //if (enemies == null || enemies.Length == 0)
            //{
                //autoDamageCoroutine = null; // No more enemies: stop the coroutine
                //yield break;
            //}

            // Apply damage to each enemy
            foreach (var e in enemies)
            {
                if (e != null)
                {
                    e.TakeDamage(damagePerTick);
                }
            }

            yield return new WaitForSeconds(tickInterval); // Wait for the next tick
        }
    }

    public void StartAutoDamage()
    {
        if (autoDamageCoroutine == null)
            autoDamageCoroutine = StartCoroutine(AutoDamageAllEnemies()); // Start the automatic damage coroutine
    }

    public void StopAutoDamage()
    {
        if (autoDamageCoroutine != null)
        {
            StopCoroutine(autoDamageCoroutine); // Stop the coroutine
            autoDamageCoroutine = null;
        }
    }
}
