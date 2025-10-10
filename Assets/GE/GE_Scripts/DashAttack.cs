using UnityEngine;
using System.Collections;

public class DashAttack : MonoBehaviour
{
    public GameObject dashHitboxPrefab;
    public float damage = 1f;
    public float dashDistance = 3f;
    public float duration = 0.5f;
    public float cooldown = 6f;
    public int level = 1;
    public LayerMask enemyLayer;

    private bool onCooldown;
    private Rigidbody2D rb;

    void Awake() { rb = GetComponent<Rigidbody2D>(); }

    public void Activate()
    {
        if (!onCooldown)
            StartCoroutine(DashRoutine());
    }

    IEnumerator DashRoutine()
    {
        onCooldown = true;
        Vector2 dashDir = transform.right;
        float dashSpeed = dashDistance / duration;
        float startTime = Time.time;

        // Instanciar hitbox visual da estocada
        GameObject dashHitbox = Instantiate(
            dashHitboxPrefab, 
            transform.position + (Vector3)dashDir * (dashDistance/2f), 
            Quaternion.identity);
        dashHitbox.transform.right = dashDir;
        dashHitbox.transform.localScale = new Vector3(dashDistance, 1, 1);
        Destroy(dashHitbox, duration);

        while (Time.time < startTime + duration)
        {
            rb.linearVelocity = dashDir * dashSpeed;
            Collider2D[] hits = Physics2D.OverlapBoxAll(
                transform.position + (Vector3)dashDir * (dashDistance/2f), 
                new Vector2(dashDistance, 1f), 
                0, enemyLayer);
            foreach (var hit in hits)
                hit.GetComponent<Enemy>()?.TakeDamage(damage);
            yield return null;
        }
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(cooldown);
        onCooldown = false;
    }

    public void Upgrade()
    {
        level++;
        if (level % 2 == 0) damage += 1;
        else dashDistance += 0.5f;
    }
}