using UnityEngine;
using System.Collections;

public class DefensiveStance : MonoBehaviour
{
    public GameObject defensiveCirclePrefab;
    public float damage = 2f;
    public float radius = 1f;
    public float duration = 2f;
    public float cooldown = 8f;
    public int level = 1;
    public float slowFactor = 0.5f;
    public LayerMask enemyLayer;

    private bool onCooldown;
    private Personagem playerCharacter;

    void Awake()
    {
        playerCharacter = GetComponent<Personagem>();
    }

    public void Activate()
    {
        if (!onCooldown)
            StartCoroutine(DefensiveRoutine());
    }

    IEnumerator DefensiveRoutine()
    {
        onCooldown = true;

        GameObject circle = Instantiate(defensiveCirclePrefab, transform.position, Quaternion.identity, transform);
        circle.transform.localScale = new Vector3(radius * 2, radius * 2, 1);
        Destroy(circle, duration);

        float startTime = Time.time;
        while (Time.time < startTime + duration)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);
            foreach (var hit in hits)
            {
                hit.GetComponent<Enemy>()?.TakeDamage(damage);
                Vector2 dir = (hit.transform.position - transform.position).normalized;
                hit.GetComponent<Rigidbody2D>()?.AddForce(dir * 10f, ForceMode2D.Impulse);
            }
            yield return null;
        }

        yield return new WaitForSeconds(cooldown);
        onCooldown = false;
    }

    public void Upgrade()
    {
        level++;
        if (level % 2 == 0) damage += 1;
        else radius += 0.2f;
    }
}