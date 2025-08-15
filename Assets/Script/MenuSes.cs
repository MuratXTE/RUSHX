using UnityEngine;
using UnityEngine.SubsystemsImplementation;

public class MenuSes : MonoBehaviour
{
    private static GameObject instance;
    public AudioSource Ses;
    void Start()
    {
        Ses.volume = PlayerPrefs.GetFloat("MenuSes");
        DontDestroyOnLoad(gameObject);

        if (instance == null)
            instance = gameObject;
        else
            Destroy(gameObject);
    }
    void Update()
    {
        Ses.volume = PlayerPrefs.GetFloat("MenuSes");
    }
}
