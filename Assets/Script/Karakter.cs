using UnityEngine;

public class Karakter : MonoBehaviour
{
    public GameManager _GameManager;
    public Kamera _Kamera;
    public bool SonaGeldikmi;
    public GameObject Gidecegiyer;

    private void FixedUpdate()
    {
        if (!SonaGeldikmi)
            transform.Translate(Vector3.forward * 2f * Time.deltaTime);
    }

    void Update()
    {
        if (Time.timeScale != 0)
        {
            if (SonaGeldikmi)
            {
                float speed = 2f;
                transform.position = Vector3.MoveTowards(transform.position, Gidecegiyer.transform.position, speed * Time.deltaTime);
            }
            else
            {
                // ✅ Mobil dokunma kontrolü
                if (Input.touchCount > 0)
                {
                    Touch touch = Input.GetTouch(0);
                    if (touch.phase == TouchPhase.Moved)
                    {
                        float deltaX = touch.deltaPosition.x * 0.005f;
                        transform.position = new Vector3(
                            transform.position.x + deltaX,
                            transform.position.y,
                            transform.position.z
                        );
                    }
                }
                // ✅ Editor test için mouse kontrolü
                else if (Input.GetMouseButton(0))
                {
                    if (Input.GetAxis("Mouse X") < 0)
                    {
                        transform.position = Vector3.Lerp(
                            transform.position,
                            new Vector3(transform.position.x - 0.1f, transform.position.y, transform.position.z),
                            0.3f
                        );
                    }
                    if (Input.GetAxis("Mouse X") > 0)
                    {
                        transform.position = Vector3.Lerp(
                            transform.position,
                            new Vector3(transform.position.x + 0.1f, transform.position.y, transform.position.z),
                            0.3f
                        );
                    }
                }
            }
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
            _Kamera.SonaGeldikmi = true;
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
        if (collision.gameObject.CompareTag("Direk") || collision.gameObject.CompareTag("telli_engel") || collision.gameObject.CompareTag("PervaneIgneler"))
        {
            if (transform.position.x > 0)
                transform.position = new Vector3(transform.position.x - 0.2f, transform.position.y, transform.position.z);
            else
                transform.position = new Vector3(transform.position.x + 0.2f, transform.position.y, transform.position.z);
        }
    }
}
