using UnityEngine;
using UnityEngine.AI;

public class Alt_karakter : MonoBehaviour
{
    NavMeshAgent _Navmesh;
    public GameManager _Gamemanager;
    public GameObject Target;
    void Start()
    {
        _Navmesh = GetComponent<NavMeshAgent>();
    }

    private void LateUpdate()
    {
        _Navmesh.SetDestination(Target.transform.position);
    }

    Vector3 PozisyonVer()
    {
        return transform.position + Vector3.up * 1f;

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("telli_engel"))
        {
            _Gamemanager.YokOlmaEfektiOlustur(PozisyonVer());
            gameObject.SetActive(false);
        }
       else if (other.CompareTag("Testere"))
        {
            _Gamemanager.YokOlmaEfektiOlustur(PozisyonVer());
            gameObject.SetActive(false);
        }
        else if (other.CompareTag("PervaneIgneler"))
        {
            _Gamemanager.YokOlmaEfektiOlustur(PozisyonVer());
            gameObject.SetActive(false);
        }
        else if (other.CompareTag("Balyoz"))
        {
            _Gamemanager.YokOlmaEfektiOlustur(PozisyonVer(), true);
            gameObject.SetActive(false);
        }
        else if (other.CompareTag("Dusman"))
        {
            _Gamemanager.YokOlmaEfektiOlustur(PozisyonVer(),false,false);
            gameObject.SetActive(false);
        }
        else if (other.CompareTag("BosKarakter"))
        {
            _Gamemanager.Karakterler.Add(other.gameObject);
        }
    }
}
