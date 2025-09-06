using UnityEngine;

public class EnemySoldier : MonoBehaviour
{
    [HideInInspector]
    public EnemyArmy enemyArmy;
    [Header("Enemy Soldier Settings")]
    public float health = 1f;
    public bool canDie = true;
    public void Die()
    {
        if (enemyArmy != null)
        {
            enemyArmy.RemoveEnemySoldier(transform);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void TakeDamage(float damage)
    {
        if (!canDie) return;

        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }
}
