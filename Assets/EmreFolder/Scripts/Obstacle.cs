using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [Header("Obstacle Settings")]
    public float damage = 1f;
    public bool killsPlayer = false;
    public bool killsSoldiers = true;
    public bool destroyOnHit = false;
    
    [Header("Effects")]
    public GameObject hitEffect;
    public AudioClip hitSound;
    
    private AudioSource audioSource;
    
    void Start()
    {
        // Ensure obstacle has the Obstacle tag
        if (!CompareTag("Obstacle"))
        {
            tag = "Obstacle";
        }
        
        audioSource = GetComponent<AudioSource>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        HandleCollision(other.gameObject);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.gameObject);
    }
    
    void HandleCollision(GameObject hitObject)
    {
        // Check if it's a soldier
        ArmySoldier soldier = hitObject.GetComponent<ArmySoldier>();
        if (soldier != null && killsSoldiers)
        {
            // Play effects
            PlayHitEffects(hitObject.transform.position);
            
            // Damage or kill the soldier
            soldier.TakeDamage(damage);
            
            // Destroy obstacle if set to
            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
            return;
        }
        
        // Check if it's the player
        PlayerController player = hitObject.GetComponent<PlayerController>();
        if (player != null && killsPlayer)
        {
            // Play effects
            PlayHitEffects(hitObject.transform.position);
            
            // Handle player death (you can expand this)
            HandlePlayerDeath(player);
            
            // Destroy obstacle if set to
            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }
    }
    
    void PlayHitEffects(Vector3 position)
    {
        // Play hit effect
        if (hitEffect != null)
        {
            Instantiate(hitEffect, position, Quaternion.identity);
        }
        
        // Play hit sound
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
    }
    
    void HandlePlayerDeath(PlayerController player)
    {
        // Basic player death handling
        // You can expand this to restart level, show game over screen, etc.
        Debug.Log("Player hit obstacle!");
        
        // For now, just reset player position
        player.ResetPosition();
        
        // Optionally clear the army
        ArmyManager armyManager = player.GetComponent<ArmyManager>();
        if (armyManager != null)
        {
            armyManager.ClearArmy();
        }
    }
}
