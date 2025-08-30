using UnityEngine;

public class EnemySoldier : MonoBehaviour
{
    [HideInInspector]
    public EnemyArmy enemyArmy;
    
    [Header("Enemy Soldier Settings")]
    public float health = 1f;
    public bool canDie = true;
    
    // Enemy soldiers don't need collision detection with obstacles
    // They only participate in combat with player soldiers
    
    public void Die()
    {
        Debug.Log("EnemySoldier.Die() called");
        
        if (enemyArmy != null)
        {
            Debug.Log("Calling enemyArmy.RemoveEnemySoldier()");
            enemyArmy.RemoveEnemySoldier(transform);
        }
        else
        {
            Debug.Log("No enemyArmy found, destroying enemy soldier directly");
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
