using UnityEngine;

public class Karakter : MonoBehaviour
{
    public GameManager _GameManager;
    public bool SonaGeldikmi;
    public GameObject Gidecegiyer;

    private void FixedUpdate()
    {
        if (!SonaGeldikmi)
            transform.Translate(Vector3.forward * 2f * Time.deltaTime);
    }

    void Update()
    {
        if (Time.timeScale == 0) return;

        if (SonaGeldikmi)
        {
            float speed = 2f;
            transform.position = Vector3.MoveTowards(transform.position, Gidecegiyer.transform.position, speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Carpma") || other.CompareTag("Toplama") || other.CompareTag("Cikartma") || other.CompareTag("Bolme"))
        {
            int sayi = int.Parse(other.name);
            _GameManager.AdamYonetimi(other.tag, sayi, other.transform);
        }
        else if (other.CompareTag("Sontetikleyici"))
        {
            SonaGeldikmi = true;
        }
        else if (other.CompareTag("BosKarakter"))
        {
            if (!_GameManager.Karakterler.Contains(other.gameObject)) // tekrar eklemeyi engelle
                _GameManager.Karakterler.Add(other.gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Direk") ||
            collision.gameObject.CompareTag("telli_engel") ||
            collision.gameObject.CompareTag("PervaneIgneler"))
        {
            float offset = transform.position.x > 0 ? -0.2f : 0.2f;
            transform.position = new Vector3(transform.position.x + offset, transform.position.y, transform.position.z);
        }
    }
}
