using System.Collections.Generic;
using UnityEngine;
using Murat;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static int AnlikKarakterSayisi = 1;
    public List<GameObject> Karakterler;

    [Header("LEVEL VERİLERİ")]
    public GameObject _AnaKarakter;
    public bool OyunBittimi;
    bool SonaGeldikmi;

    [Header("----------------------------SAPKALAR")]
    public GameObject[] Sapkalar;
    [Header("----------------------------SOPALAR")]
    public GameObject[] Sopalar;
    [Header("----------------------------MATERYALLER")]
    public Material[] Materyaller;
    public SkinnedMeshRenderer _Renderer;
    public Material VarsayilanTema;

    Matematiksel_islemler _Matematiksel_islemler = new Matematiksel_islemler();
    BellekYonetim _BellekYonetim = new BellekYonetim();
    VeriYonetimi _VeriYonetim = new VeriYonetimi();
    ReklamManager _ReklamManager = new ReklamManager();
    UnityEngine.SceneManagement.Scene _Scene;

    [Header("----------------------------GENEL VERİLERİ")]
    public AudioSource[] Sesler;
    public GameObject[] islemPanelleri;
    public Slider OyunSesiAyar;
    public List<DilVerileriAnaObje> _DilVerileriAnaObje = new List<DilVerileriAnaObje>();
    List<DilVerileriAnaObje> _DilOkunanVeriler = new List<DilVerileriAnaObje>();
    public TextMeshProUGUI[] TextObjeleri;

    [Header("----------------------------LOADING VERİLERİ")]
    public GameObject YuklemeEkrani;
    public Slider YuklemeSlider;

    private void Awake()
    {
        Sesler[0].volume = _BellekYonetim.VeriOku_f("OyunSes");
        OyunSesiAyar.value = _BellekYonetim.VeriOku_f("OyunSes");
        Sesler[1].volume = _BellekYonetim.VeriOku_f("MenuFx");
        Destroy(GameObject.FindWithTag("MenuSes"));
        ItemleriKontrolEt();
    }

    void Start()
    {
        _Scene = SceneManager.GetActiveScene();
        _VeriYonetim.Dil_Load();
        _DilOkunanVeriler = _VeriYonetim.DilVerileriListeyiAktar();
        _DilVerileriAnaObje.Add(_DilOkunanVeriler[5]);
        DilTercihiYonetimi();

        // ✅ sadece tek reklam çağrısı
        Object.FindFirstObjectByType<ReklamManager>().GecisReklamiGoster();
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

    void SavasDurumu()
    {
        if (SonaGeldikmi && !OyunBittimi)
        {
            if (AnlikKarakterSayisi <= 1)
            {
                OyunBittimi = true;

                foreach (var item in Karakterler)
                {
                    if (item.activeInHierarchy)
                        item.GetComponent<Animator>().SetBool("Saldir", false);
                }

                _AnaKarakter.GetComponent<Animator>().SetBool("Saldir", false);

                _ReklamManager.GecisReklamiGoster();

                islemPanelleri[3].SetActive(true); // kaybettin paneli
            }
            else
            {
                OyunBittimi = true;

                // Puan + level kaydı
                if (_Scene.buildIndex == _BellekYonetim.VeriOku_i("SonLevel"))
                {
                    if (AnlikKarakterSayisi > 5)
                        _BellekYonetim.VeriKaydet_int("Puan", _BellekYonetim.VeriOku_i("Puan") + 600);
                    else
                        _BellekYonetim.VeriKaydet_int("Puan", _BellekYonetim.VeriOku_i("Puan") + 100);

                    _BellekYonetim.VeriKaydet_int("SonLevel", _BellekYonetim.VeriOku_i("SonLevel") + 1);
                }

                islemPanelleri[2].SetActive(true); // kazandın paneli
            }
        }
    }

    public void AdamYonetimi(string islemturu, int GelenSayi, Transform Pozisyon)
    {
        switch (islemturu)
        {
            case "Carpma":
                _Matematiksel_islemler.Carpma(GelenSayi, Karakterler, Pozisyon, null);
                break;
            case "Toplama":
                _Matematiksel_islemler.Toplama(GelenSayi, Karakterler, Pozisyon, null);
                break;
            case "Cikartma":
                _Matematiksel_islemler.Cikartma(GelenSayi, Karakterler, null);
                break;
            case "Bolme":
                _Matematiksel_islemler.Bolme(GelenSayi, Karakterler, null);
                break;
        }
    }

    public void ItemleriKontrolEt()
    {
        if (_BellekYonetim.VeriOku_i("AktifSapka") != -1)
        {
            int sapkaIndex = _BellekYonetim.VeriOku_i("AktifSapka");
            if (sapkaIndex < Sapkalar.Length)
                Sapkalar[sapkaIndex].SetActive(true);
        }

        if (_BellekYonetim.VeriOku_i("AktifSopa") != -1)
        {
            int sopaIndex = _BellekYonetim.VeriOku_i("AktifSopa");
            if (sopaIndex < Sopalar.Length)
                Sopalar[sopaIndex].SetActive(true);
        }

        if (_BellekYonetim.VeriOku_i("AktifTema") != -1)
        {
            int temaIndex = _BellekYonetim.VeriOku_i("AktifTema");
            if (temaIndex < Materyaller.Length)
            {
                Material[] mats = _Renderer.materials;
                mats[0] = Materyaller[temaIndex];
                _Renderer.materials = mats;
            }
        }
        else
        {
            Material[] mats = _Renderer.materials;
            mats[0] = VarsayilanTema;
            _Renderer.materials = mats;
        }
    }

    public void CikisButonislem(string durum)
    {
        Sesler[1].Play();
        Time.timeScale = 0;

        if (durum == "durdur")
            islemPanelleri[0].SetActive(true);
        else if (durum == "devamet")
        {
            islemPanelleri[0].SetActive(false);
            Time.timeScale = 1;
        }
        else if (durum == "tekrar")
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            Time.timeScale = 1;
        }
        else if (durum == "Anasayfa")
        {
            SceneManager.LoadScene(0);
            Time.timeScale = 1;
        }
    }

    public void Ayarlar(string durum)
    {
        Sesler[1].Play();
        if (durum == "ayarla")
        {
            islemPanelleri[1].SetActive(true);
            Time.timeScale = 0;
        }
        else
        {
            islemPanelleri[1].SetActive(false);
            Time.timeScale = 1;
        }
    }

    public void SesiAyarla()
    {
        _BellekYonetim.VeriKaydet_float("OyunSes", OyunSesiAyar.value);
        Sesler[0].volume = OyunSesiAyar.value;
    }

    public void SonrakiLevel()
    {
        StartCoroutine(LoadAsync(_Scene.buildIndex + 1));
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

    public void OdulluReklam()
    {
        _ReklamManager.OdulluReklamGoster();
    }
}
