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
        ArmySoldier soldier = hitObject.GetComponent<ArmySoldier>();
        if (soldier != null && killsSoldiers)
        {
            PlayHitEffects(hitObject.transform.position);
            soldier.TakeDamage(damage);
            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
            return;
        }
        PlayerController player = hitObject.GetComponent<PlayerController>();
        if (player != null && killsPlayer)
        {
            PlayHitEffects(hitObject.transform.position);
            HandlePlayerDeath(player);
            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }
    }
    void PlayHitEffects(Vector3 position)
    {
        if (hitEffect != null)
        {
            Instantiate(hitEffect, position, Quaternion.identity);
        }
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
    }
    void HandlePlayerDeath(PlayerController player)
    { player.ResetPosition();
        ArmyManager armyManager = player.GetComponent<ArmyManager>();
        if (armyManager != null)
        {
            armyManager.ClearArmy();
        }
    }
}
