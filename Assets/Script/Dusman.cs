using UnityEngine;
using UnityEngine.AI;

public class Dusman : MonoBehaviour
{
    public GameObject Saldiri_Hedefi;
    public NavMeshAgent _NavMesh;
    public Animator _Animator;
    public GameManager _Gamemanager;
    bool Saldiri_Basladimi;
   
    public void AnimasyonTetikle()
    {
        _Animator.SetBool("Saldir", true);
        Saldiri_Basladimi = true;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(Saldiri_Basladimi)
        _NavMesh.SetDestination(Saldiri_Hedefi.transform.position);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("AltKarakterler"))
        {
            Vector3 yeniPoz = transform.position + Vector3.up * 1f;
            _Gamemanager.YokOlmaEfektiOlustur(yeniPoz,false,true);
            gameObject.SetActive(false);
        }
    }
}
