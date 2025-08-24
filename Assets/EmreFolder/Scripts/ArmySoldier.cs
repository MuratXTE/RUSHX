using UnityEngine;
using DG.Tweening;
using Murat;

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
    
    [Header("----------------------------SOLDIER ITEMS")]
    public GameObject[] Sapkalar;
    public GameObject[] Sopalar;
    public Material[] Materyaller;
    public SkinnedMeshRenderer _Renderer;
    public Material VarsayilanTema;
    
    private BellekYonetim _BellekYonetim = new BellekYonetim();
    
    private void Start()
    {
        // Apply items to this soldier when it spawns
        ApplyItemsToSoldier();
    }
    
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
    
    public void ApplyItemsToSoldier()
    {
        // Apply the same items that the player has active
        Debug.Log($"Applying items to soldier: {gameObject.name}");
        
        // Apply Sapka (Hat)
        if (_BellekYonetim.VeriOku_i("AktifSapka") != -1)
        {
            int sapkaIndex = _BellekYonetim.VeriOku_i("AktifSapka");
            if (Sapkalar != null && sapkaIndex < Sapkalar.Length)
            {
                // Deactivate all hats first
                foreach (var sapka in Sapkalar)
                {
                    if (sapka != null) sapka.SetActive(false);
                }
                // Activate the selected hat
                Sapkalar[sapkaIndex].SetActive(true);
                Debug.Log($"Soldier hat activated: {sapkaIndex}");
            }
        }
        
        // Apply Sopa (Weapon)
        if (_BellekYonetim.VeriOku_i("AktifSopa") != -1)
        {
            int sopaIndex = _BellekYonetim.VeriOku_i("AktifSopa");
            if (Sopalar != null && sopaIndex < Sopalar.Length)
            {
                // Deactivate all weapons first
                foreach (var sopa in Sopalar)
                {
                    if (sopa != null) sopa.SetActive(false);
                }
                // Activate the selected weapon
                Sopalar[sopaIndex].SetActive(true);
                Debug.Log($"Soldier weapon activated: {sopaIndex}");
            }
        }
        
        // Apply Material (Theme)
        if (_BellekYonetim.VeriOku_i("AktifTema") != -1)
        {
            int temaIndex = _BellekYonetim.VeriOku_i("AktifTema");
            if (Materyaller != null && temaIndex < Materyaller.Length && _Renderer != null)
            {
                Material[] mats = _Renderer.materials;
                mats[0] = Materyaller[temaIndex];
                _Renderer.materials = mats;
                Debug.Log($"Soldier material activated: {temaIndex}");
            }
        }
        else
        {
            // Use default material if no theme is selected
            if (_Renderer != null && VarsayilanTema != null)
            {
                Material[] mats = _Renderer.materials;
                mats[0] = VarsayilanTema;
                _Renderer.materials = mats;
                Debug.Log("Soldier using default material");
            }
        }
    }
}
