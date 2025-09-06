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

    [Header("Combat Settings")]
    [Tooltip("Is this soldier currently moving to combat? Makes them temporarily invulnerable to obstacles.")]
    public bool isInCombatMovement = false;

    [Header("Animation Settings")]
    public float damageAnimationDuration = 0.2f;
    public Color damageColor = Color.red;

    [Header("Death Effects")]
    public GameObject deathParticleEffect;
    public bool useArmyManagerParticle = true;

    [Header("----------------------------SOLDIER ITEMS")]
    public GameObject[] Sapkalar;
    public GameObject[] Sopalar;
    public Material[] Materyaller;
    public SkinnedMeshRenderer _Renderer;
    public Material VarsayilanTema;

    private BellekYonetim _BellekYonetim = new BellekYonetim();

    private void Start()
    {
        ApplyItemsToSoldier();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle") && canDie && !isInCombatMovement)
            Die();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle") && canDie && !isInCombatMovement)
            Die();
    }

    public void TakeDamage(float damage)
    {
        if (!canDie) return;

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Color originalColor = renderer.material.color;
            renderer.material.color = damageColor;
            DOTween.To(() => renderer.material.color, x => renderer.material.color = x, originalColor, damageAnimationDuration);
        }

        transform.DOShakePosition(damageAnimationDuration, 0.1f, 10, 90, false, true);
        transform.DOPunchScale(Vector3.one * 0.1f, damageAnimationDuration, 1, 0.5f);

        health -= damage;
        if (health <= 0)
            Die();
    }

    public void Die()
    {
        if (!useArmyManagerParticle && deathParticleEffect != null)
        {
            GameObject deathEffect = Instantiate(deathParticleEffect, transform.position, Quaternion.identity);
            Destroy(deathEffect, 3f);
        }

        if (armyManager != null)
        {
            armyManager.RemoveSoldier(transform);
        }
        else
        {
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
        // Sapka
        int sapkaIndex = _BellekYonetim.VeriOku_i("AktifSapka");
        if (sapkaIndex != -1 && Sapkalar != null && sapkaIndex < Sapkalar.Length)
        {
            foreach (var sapka in Sapkalar)
            {
                if (sapka != null) sapka.SetActive(false);
            }
            Sapkalar[sapkaIndex].SetActive(true);
        }

        // Sopa
        int sopaIndex = _BellekYonetim.VeriOku_i("AktifSopa");
        if (sopaIndex != -1 && Sopalar != null && sopaIndex < Sopalar.Length)
        {
            foreach (var sopa in Sopalar)
            {
                if (sopa != null) sopa.SetActive(false);
            }
            Sopalar[sopaIndex].SetActive(true);
        }

        // Tema (Materyal)
        int temaIndex = _BellekYonetim.VeriOku_i("AktifTema");
        if (temaIndex != -1 && Materyaller != null && temaIndex < Materyaller.Length && _Renderer != null)
        {
            Material[] mats = _Renderer.materials;
            mats[0] = Materyaller[temaIndex];
            _Renderer.materials = mats;
        }
        else
        {
            if (_Renderer != null && VarsayilanTema != null)
            {
                Material[] mats = _Renderer.materials;
                mats[0] = VarsayilanTema;
                _Renderer.materials = mats;
            }
        }
    }
}
