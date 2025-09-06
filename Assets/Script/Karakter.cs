using UnityEngine;

public class Karakter : MonoBehaviour
{
    public GameManager _GameManager;
    public bool SonaGeldikmi;
    public GameObject Gidecegiyer;

    [Header("Hareket Ayarları")]
    public float ileriHiz = 2f;
    public float yanHiz = 0.005f;
    public float bitisHiz = 2f;

    void Update()
    {
        if (Time.timeScale == 0) return;

        if (!SonaGeldikmi)
        {
            // İleri hareket
            transform.Translate(Vector3.forward * ileriHiz * Time.deltaTime);

            // Dokunmatik kontrol
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Moved)
                {
                    float deltaX = touch.deltaPosition.x * yanHiz;
                    transform.position += new Vector3(deltaX, 0, 0);
                }
            }
            // Editor mouse kontrolü
            else if (Input.GetMouseButton(0))
            {
                float deltaX = Input.GetAxis("Mouse X") * 0.1f;
                transform.position = Vector3.Lerp(
                    transform.position,
                    new Vector3(transform.position.x + deltaX, transform.position.y, transform.position.z),
                    0.3f
                );
            }
        }
        else
        {
            // Bitiş pozisyonuna doğru git
            if (Gidecegiyer != null)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    Gidecegiyer.transform.position,
                    bitisHiz * Time.deltaTime
                );
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Carpma") || other.CompareTag("Toplama") ||
            other.CompareTag("Cikartma") || other.CompareTag("Bolme"))
        {
            if (int.TryParse(other.name, out int sayi))
            {
                _GameManager.AdamYonetimi(other.tag, sayi, other.transform);
            }
            else
            {
                Debug.LogWarning($"Objenin ismi sayı değil: {other.name}");
            }
        }
        else if (other.CompareTag("Sontetikleyici"))
        {
            SonaGeldikmi = true;
        }
        else if (other.CompareTag("BosKarakter"))
        {
            if (!_GameManager.Karakterler.Contains(other.gameObject))
                _GameManager.Karakterler.Add(other.gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Direk") ||
            collision.gameObject.CompareTag("telli_engel") ||
            collision.gameObject.CompareTag("PervaneIgneler"))
        {
            // Çarpışmada küçük yan kayma efekti
            float offset = transform.position.x > 0 ? -0.2f : 0.2f;
            Vector3 yeniPoz = new Vector3(transform.position.x + offset, transform.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, yeniPoz, 0.5f);
        }
    }
}
