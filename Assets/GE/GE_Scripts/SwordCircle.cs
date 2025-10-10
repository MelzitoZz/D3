using UnityEngine;
using System.Collections;

public class SwordCircle : MonoBehaviour
{
    public GameObject spinningSwordPrefab;
    public float damage = 1f;
    public float duration = 3f;
    public float cooldown = 4f;
    public int spins = 5;
    public float radius = 2f;
    public int swordCount = 5;
    public int level = 1;
    public LayerMask enemyLayer;

    private bool onCooldown;

    public void Activate()
    {
        if (!onCooldown)
        {
            StartCoroutine(SpinRoutine());
        }
    }

    IEnumerator SpinRoutine()
    {
        onCooldown = true;
        // Instanciar espadas girando
        GameObject[] swords = new GameObject[swordCount];
        for (int i = 0; i < swordCount; i++)
        {
            float angle = i * Mathf.PI * 2 / swordCount;
            Vector3 pos = transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            swords[i] = Instantiate(spinningSwordPrefab, pos, Quaternion.identity, transform);
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            // Fazer as espadas girarem
            for (int i = 0; i < swordCount; i++)
            {
                float angle = i * Mathf.PI * 2 / swordCount + elapsed / duration * spins * Mathf.PI * 2;
                swords[i].transform.localPosition = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            }

            // Dano em área
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);
            foreach (var hit in hits)
            {
                hit.GetComponent<Enemy>()?.TakeDamage(damage);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Destruir espadas visuais
        foreach (var sword in swords)
            Destroy(sword);

        yield return new WaitForSeconds(cooldown);
        onCooldown = false;
    }

    public void Upgrade()
    {
        level++;
        damage += 1;
        if (level % 3 == 0)
        {
            spins++;
            swordCount++;
        }
    }
}