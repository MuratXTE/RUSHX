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
        InvokeRepeating(nameof(UpdateDestination), 0f, 0.2f); // 0.2 sn’de bir hedef güncelle
    }

    void UpdateDestination()
    {
        if (Target != null && _Navmesh.enabled)
        {
            _Navmesh.SetDestination(Target.transform.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("telli_engel") || other.CompareTag("Testere") || other.CompareTag("PervaneIgneler"))
        {
            // Artýk sadece karakteri azaltýyoruz
            GameManager.AnlikKarakterSayisi--;
            gameObject.SetActive(false);
        }
        else if (other.CompareTag("Balyoz"))
        {
            GameManager.AnlikKarakterSayisi--;
            gameObject.SetActive(false);
        }
        else if (other.CompareTag("BosKarakter"))
        {
            if (!_Gamemanager.Karakterler.Contains(other.gameObject))
                _Gamemanager.Karakterler.Add(other.gameObject);
        }
    }
}
