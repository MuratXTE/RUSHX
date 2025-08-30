using UnityEngine;
using System.Collections;

public class DragonFire : MonoBehaviour
{
    [Header("Ateş Ayarları")]
    public ParticleSystem fireBreath;     // Ateş partikül sistemi
    public AudioSource fireSound;         // Ateş sesi
    public Collider fireCollider;         // Hasar alanı (Box Collider / Capsule Collider)
    public float fireDuration = 2f;       // Ateşin aktif kalma süresi
    public float fireCooldown = 7f;       // Kaç saniyede bir ateş püskürsün

    private bool isBreathing = false;

    void Start()
    {
        fireCollider.enabled = false; // başta kapalı
        StartCoroutine(AutoBreath()); // otomatik ateş döngüsü başlat
    }

    IEnumerator AutoBreath()
    {
        while (true)
        {
            yield return new WaitForSeconds(fireCooldown); // 5 saniye bekle
            StartCoroutine(BreatheFire());
        }
    }

    IEnumerator BreatheFire()
    {
        isBreathing = true;

        if (fireBreath != null) fireBreath.Play();  // particle başlat
        if (fireSound != null) fireSound.Play();    // sesi çal
        if (fireCollider != null) fireCollider.enabled = true; // hasar alanı aç

        yield return new WaitForSeconds(fireDuration);

        if (fireBreath != null) fireBreath.Stop();
        if (fireCollider != null) fireCollider.enabled = false;

        isBreathing = false;
    }
    private void OnTriggerStay(Collider other)
    {
        if (!isBreathing) return; // sadece ateş püskürürken

        // Düşman Asker
        if (other.TryGetComponent<EnemySoldier>(out var enemy))
        {
            enemy.Die();
        }
        // Oyuncu Askeri
        else if (other.TryGetComponent<ArmySoldier>(out var ally))
        {
            ally.Die();
        }
        // Pickup Asker
        else if (other.TryGetComponent<PickupSoldier>(out var pickup))
        {
            Destroy(pickup.gameObject);
        }
    }
}
