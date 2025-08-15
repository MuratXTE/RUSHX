using UnityEngine;
using DG.Tweening;

public class ArmySoldier : MonoBehaviour
{
    [HideInInspector]
    public ArmyManager armyManager;
    
    [Header("Soldier Settings")]
    public float health = 1f;
    public bool canDie = true;
    
    [Header("Animation Settings")]
    public float damageAnimationDuration = 0.2f;
    public Color damageColor = Color.red;
    
    [Header("Death Effects")]
    public GameObject deathParticleEffect; // Individual soldier death particle
    public bool useArmyManagerParticle = true; // Use ArmyManager's particle instead
    
    private void OnTriggerEnter(Collider other)
    {
        // Handle obstacles that can kill soldiers
        if (other.CompareTag("Obstacle") && canDie)
        {
            Die();
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // Handle obstacles that can kill soldiers
        if (collision.gameObject.CompareTag("Obstacle") && canDie)
        {
            Die();
        }
    }
    
    public void TakeDamage(float damage)
    {
        if (!canDie) return;
        
        // Damage animation - flash red and shake
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Color originalColor = renderer.material.color;
            renderer.material.color = damageColor;
            DOTween.To(() => renderer.material.color, x => renderer.material.color = x, originalColor, damageAnimationDuration);
        }
        
        // Shake effect on damage
        transform.DOShakePosition(damageAnimationDuration, 0.1f, 10, 90, false, true);
        transform.DOPunchScale(Vector3.one * 0.1f, damageAnimationDuration, 1, 0.5f);
        
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }
    
    public void Die()
    {
        Debug.Log("ArmySoldier.Die() called");
        
        // Spawn death particle if we have one and not using army manager particle
        if (!useArmyManagerParticle && deathParticleEffect != null)
        {
            GameObject deathEffect = Instantiate(deathParticleEffect, transform.position, Quaternion.identity);
            Destroy(deathEffect, 3f); // Auto-destroy after 3 seconds
        }
        
        if (armyManager != null)
        {
            Debug.Log("Calling armyManager.RemoveSoldier()");
            armyManager.RemoveSoldier(transform);
        }
        else
        {
            Debug.Log("No armyManager found, destroying soldier directly");
            
            // If no army manager, still spawn particle effect
            if (deathParticleEffect != null)
            {
                GameObject deathEffect = Instantiate(deathParticleEffect, transform.position, Quaternion.identity);
                Destroy(deathEffect, 3f);
            }
            
            Destroy(gameObject);
        }
    }
}
