using UnityEngine;

public class PrimarySword : MonoBehaviour
{
    public GameObject swordConePrefab;
    public float damage = 1f;
    public float duration = 0.5f;
    public float interval = 1f;
    public float distance = 1.5f;
    public int level = 1;
    public LayerMask enemyLayer;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= interval)
        {
            Activate();
            timer = 0f;
        }
    }

    public void Activate()
    {
        // Instancia o cone visual
        Vector3 spawnPos = transform.position + transform.right * (distance / 2f);
        Quaternion rot = Quaternion.Euler(0, 0, GetPlayerRotation());
        GameObject cone = Instantiate(swordConePrefab, spawnPos, rot);
        cone.transform.localScale = new Vector3(distance, distance, 1);
        // Destroi após a duração
        Destroy(cone, duration);

        // Detecta inimigos no cone usando OverlapCircleAll e ângulo
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, distance, enemyLayer);
        foreach (var hit in hits)
        {
            Vector2 dirToEnemy = (hit.transform.position - transform.position).normalized;
            float angle = Vector2.Angle(transform.right, dirToEnemy);
            if (angle <= 45)
            {
                hit.GetComponent<Enemy>()?.TakeDamage(damage);
            }
        }
    }

    float GetPlayerRotation()
    {
        // Retorne o ângulo do player, ajuste se necessário para seu sistema
        return transform.eulerAngles.z;
    }

    public void Upgrade()
    {
        level++;
        damage += 1;
        distance += 0.25f;
    }
}