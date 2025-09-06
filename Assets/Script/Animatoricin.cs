using UnityEngine;

public class Animatoricin : MonoBehaviour
{
    [Header("Animator Bileþeni")]
    public Animator _Animator;
    public void KendiniPasiflestir()
    {
        if (_Animator != null)
        {
            _Animator.SetBool("ok", false);
        }
        else
        {
            Debug.LogWarning($"Animator atanmadý! Oyun objesi: {gameObject.name}");
        }
    }
}
