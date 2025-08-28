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
        if (other.CompareTag("Obstacle") && canDie)
            Die();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle") && canDie)
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
        if (_BellekYonetim.VeriOku_i("AktifSapka") != -1)
        {
            int sapkaIndex = _BellekYonetim.VeriOku_i("AktifSapka");
            if (Sapkalar != null && sapkaIndex < Sapkalar.Length)
            {
                foreach (var sapka in Sapkalar)
                {
                    if (sapka != null) sapka.SetActive(false);
                }
                Sapkalar[sapkaIndex].SetActive(true);
            }
        }

        // Sopa
        if (_BellekYonetim.VeriOku_i("AktifSopa") != -1)
        {
            int sopaIndex = _BellekYonetim.VeriOku_i("AktifSopa");
            if (Sopalar != null && sopaIndex < Sopalar.Length)
            {
                foreach (var sopa in Sopalar)
                {
                    if (sopa != null) sopa.SetActive(false);
                }
                Sopalar[sopaIndex].SetActive(true);
            }
        }

        // Tema (Materyal)
        if (_BellekYonetim.VeriOku_i("AktifTema") != -1)
        {
            int temaIndex = _BellekYonetim.VeriOku_i("AktifTema");
            if (Materyaller != null && temaIndex < Materyaller.Length && _Renderer != null)
            {
                Material[] mats = _Renderer.materials;
                mats[0] = Materyaller[temaIndex];
                _Renderer.materials = mats;
            }
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
