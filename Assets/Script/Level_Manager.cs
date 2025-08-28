using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Murat;
using System.Collections.Generic;
using System.Collections;

public class Level_Manager : MonoBehaviour
{
    public Button[] Butonlar;
    public int Level;
    public Sprite KilitButon;

    BellekYonetim _BellekYonetim = new BellekYonetim();
    public AudioSource ButonSes;

    public List<DilVerileriAnaObje> _DilVerileriAnaObje = new List<DilVerileriAnaObje>();
    List<DilVerileriAnaObje> _DilOkunanVeriler = new List<DilVerileriAnaObje>();
    public Text[] TextObjeleri;
    VeriYonetimi _VeriYonetim = new VeriYonetimi();
    public GameObject YuklemeEkrani;
    public Slider YuklemeSlider;

    void Start()
    {
        _VeriYonetim.Dil_Load();
        _DilOkunanVeriler = _VeriYonetim.DilVerileriListeyiAktar();
        _DilVerileriAnaObje.Add(_DilOkunanVeriler[2]);
        DilTercihiYonetimi();

        ButonSes.volume = _BellekYonetim.VeriOku_f("MenuFx");

        int mevcutLevel = _BellekYonetim.VeriOku_i("SonLevel");
        int Index = 1;

        for (int i = 0; i < Butonlar.Length; i++)
        {
            if (Index <= mevcutLevel)
            {
                Butonlar[i].GetComponentInChildren<Text>().text = Index.ToString();
                int SahneIndex = Index; // artýk ofset yok!
                Butonlar[i].onClick.AddListener(() => SahneYukle(SahneIndex));
                Butonlar[i].interactable = true;
            }
            else
            {
                Butonlar[i].GetComponent<Image>().sprite = KilitButon;
                Butonlar[i].interactable = false; // Doðru kullaným
            }
            Index++;
        }
    }

    void DilTercihiYonetimi()
    {
        if (_BellekYonetim.VeriOku_s("Dil") == "EN")
        {
            for (int i = 0; i < TextObjeleri.Length; i++)
                TextObjeleri[i].text = _DilVerileriAnaObje[0]._DilVerileri_EN[i].Metin;
        }
        else if (_BellekYonetim.VeriOku_s("Dil") == "TR")
        {
            for (int i = 0; i < TextObjeleri.Length; i++)
                TextObjeleri[i].text = _DilVerileriAnaObje[0]._DilVerileri_TR[i].Metin;
        }
        else
        {
            for (int i = 0; i < TextObjeleri.Length; i++)
                TextObjeleri[i].text = _DilVerileriAnaObje[0]._DilVerileri_DE[i].Metin;
        }
    }

    public void SahneYukle(int Index)
    {
        ButonSes.Play();
        StartCoroutine(LoadAsync(Index));
    }

    IEnumerator LoadAsync(int SceneIndex)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(SceneIndex);
        YuklemeEkrani.SetActive(true);
        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / .9f);
            YuklemeSlider.value = progress;
            yield return null;
        }
    }

    public void GeriDon()
    {
        ButonSes.Play();
        SceneManager.LoadScene(0);
    }
}
