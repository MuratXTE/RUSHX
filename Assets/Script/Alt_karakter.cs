using UnityEngine;
using UnityEngine.AI;

public class Alt_karakter : MonoBehaviour
{
    private NavMeshAgent _Navmesh;
    public GameManager _Gamemanager;
    public GameObject Target;

    void Start()
    {
        _Navmesh = GetComponent<NavMeshAgent>();
        if (_Navmesh != null)
        {
            // Hedefi 0.2 sn’de bir güncelle (performans için iyi)
            InvokeRepeating(nameof(UpdateDestination), 0f, 0.2f);
        }
        else
        {
            Debug.LogError($"NavMeshAgent bulunamadý: {gameObject.name}");
        }
    }

    void UpdateDestination()
    {
        if (Target != null && _Navmesh != null && _Navmesh.enabled && _Navmesh.isOnNavMesh)
        {
            _Navmesh.SetDestination(Target.transform.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("telli_engel") ||
            other.CompareTag("Testere") ||
            other.CompareTag("PervaneIgneler") ||
            other.CompareTag("Balyoz"))
        {
            if (GameManager.AnlikKarakterSayisi > 0)
                GameManager.AnlikKarakterSayisi--;

            gameObject.SetActive(false);
        }
        else if (other.CompareTag("BosKarakter"))
        {
            if (_Gamemanager != null && !_Gamemanager.Karakterler.Contains(other.gameObject))
                _Gamemanager.Karakterler.Add(other.gameObject);
        }
    }
}
