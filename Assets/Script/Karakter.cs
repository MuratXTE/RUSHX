using UnityEngine;

public class Karakter : MonoBehaviour
{
    public GameManager _GameManager;
    public Kamera _Kamera;
    public bool SonaGeldikmi;
    public GameObject Gidecegiyer;
    private void FixedUpdate()
    {
        if(!SonaGeldikmi)
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
                if (Input.GetKey(KeyCode.Mouse0))
                {
                    if (Input.GetAxis("Mouse X") < 0)
                    {
                        transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x - .1f, transform.position.y,
                            transform.position.z), .3f);
                    }
                    if (Input.GetAxis("Mouse X") > 0)
                    {
                        transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x + .1f, transform.position.y,
                            transform.position.z), .3f);
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
            _GameManager.AdamYonetimi(other.tag,sayi, other.transform);
        }
        else if (other.CompareTag("Sontetikleyici"))
        {
            _Kamera.SonaGeldikmi = true;
            _GameManager.DusmanlariTetikle();
            SonaGeldikmi = true;
        }
        else if (other.CompareTag("BosKarakter"))
        {
            _GameManager.Karakterler.Add(other.gameObject);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Direk")|| collision.gameObject.CompareTag("telli_engel")|| collision.gameObject.CompareTag("PervaneIgneler"))
        {
            if (transform.position.x > 0)
                transform.position = new Vector3(transform.position.x - .2f, transform.position.y, transform.position.z);
            else
                transform.position = new Vector3(transform.position.x + .2f, transform.position.y, transform.position.z);
        }
    }
}
