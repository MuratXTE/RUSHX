using UnityEngine;

public class FireObstacle : MonoBehaviour
{
    [Header("Fire Settings")]
    public float damage = 1f;
    public bool killsPlayer = false;
    public bool killsSoldiers = true;

    [Header("Effects")]
    public ParticleSystem fireParticle;
    public AudioClip hitSound;

    private void Start()
    {
        if (fireParticle == null)
            fireParticle = GetComponent<ParticleSystem>();
    }

    private void OnTriggerEnter(Collider other) => HandleCollision(other.gameObject);
    private void OnCollisionEnter(Collision collision) => HandleCollision(collision.gameObject);

    private void HandleCollision(GameObject hitObject)
    {
        var soldier = hitObject.GetComponent<ArmySoldier>();
        if (soldier != null && killsSoldiers)
        {
            soldier.TakeDamage(damage);
            PlayEffects(hitObject.transform.position);
            return;
        }

        var player = hitObject.GetComponent<PlayerController>();
        if (player != null && killsPlayer)
        {
            player.ResetPosition();
            PlayEffects(hitObject.transform.position);
        }
    }

    private void PlayEffects(Vector3 pos)
    {
        if (hitSound != null)
            AudioSource.PlayClipAtPoint(hitSound, pos, 1f);

        if (fireParticle != null)
            fireParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        Destroy(gameObject, 2f);
    }
}
