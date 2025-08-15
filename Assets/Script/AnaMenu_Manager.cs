using UnityEngine;
using UnityEngine.SceneManagement;
using Murat;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Collections;
public class AnaMenu_Manager : MonoBehaviour
{
    BellekYonetim _BellekYonetim = new BellekYonetim();
    VeriYonetimi _VeriYonetim = new VeriYonetimi();
    ReklamYonetim _ReklamYonetim = new ReklamYonetim();
    public GameObject CikisPaneli;
    public List<ItemBilgileri> _Varsayilan_ItemBilgileri = new List<ItemBilgileri>();
    public List<DilVerileriAnaObje> _Varsayilan_DilVerileri = new List<DilVerileriAnaObje>();
    
    public AudioSource ButonSes;

    public List<DilVerileriAnaObje> _DilVerileriAnaObje = new List<DilVerileriAnaObje>();
    List<DilVerileriAnaObje> _DilOkunanVeriler = new List<DilVerileriAnaObje>();
    public TextMeshProUGUI[] TextObjeleri;
    public GameObject YuklemeEkrani;
    public Slider YuklemeSlider;
    void Start()
    {
        _BellekYonetim.KontrolEtVeTanimla();
        _VeriYonetim.ilkKurulumDosyaOlusturma(_Varsayilan_ItemBilgileri, _Varsayilan_DilVerileri);
        ButonSes.volume = _BellekYonetim.VeriOku_f("MenuFx");
        //_BellekYonetim.VeriKaydet_string("Dil", "EN");

        _VeriYonetim.Dil_Load();
        _DilOkunanVeriler = _VeriYonetim.DilVerileriListeyiAktar();
        _DilVerileriAnaObje.Add(_DilOkunanVeriler[0]);
         DilTercihiYonetimi();


    }
    void DilTercihiYonetimi()
    {
        if (_BellekYonetim.VeriOku_s("Dil") == "EN")
        {
            for (int i = 0; i < TextObjeleri.Length; i++)
            {
                TextObjeleri[i].text = _DilVerileriAnaObje[0]._DilVerileri_EN[i].Metin;
            }
        }
        else if (_BellekYonetim.VeriOku_s("Dil") == "TR")
        {
            for (int i = 0; i < TextObjeleri.Length; i++)
            {
                TextObjeleri[i].text = _DilVerileriAnaObje[0]._DilVerileri_TR[i].Metin;
            }
        }
        else
        {
            for (int i = 0; i < TextObjeleri.Length; i++)
            {
                TextObjeleri[i].text = _DilVerileriAnaObje[0]._DilVerileri_DE[i].Metin;
            }
        }
    }

    public void SahneYukle(int Index)
    {
        ButonSes.Play();
        SceneManager.LoadScene(Index);
    }

    public void Oyna()
    {
        ButonSes.Play();
        StartCoroutine(LoadAsync(_BellekYonetim.VeriOku_i("SonLevel")));

    }
    IEnumerator LoadAsync(int SceneIndex)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(SceneIndex);
        YuklemeEkrani.SetActive(true);
        while (operation != null && !operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / .9f);
            YuklemeSlider.value = progress;
            yield return null;
        }
    }
    public void CikisButonislem(string durum)
    {
        ButonSes.Play();
        if (durum == "Evet")
            Application.Quit();
        else if (durum == "cikis")
            CikisPaneli.SetActive(true);
        else
            CikisPaneli.SetActive(false);

    }
}
