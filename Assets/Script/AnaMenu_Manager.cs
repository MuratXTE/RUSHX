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
    ReklamManager _ReklamManager = new ReklamManager();
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

        _VeriYonetim.Dil_Load();
        _DilOkunanVeriler = _VeriYonetim.DilVerileriListeyiAktar();
        _DilVerileriAnaObje.Add(_DilOkunanVeriler[0]);
        DilTercihiYonetimi();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
            StartCoroutine(GuvenliKaydet());
    }

    private IEnumerator GuvenliKaydet()
    {
        yield return null; // bir frame bekle
        PlayerPrefs.Save();
    }

    void DilTercihiYonetimi()
    {
        string aktifDil = _BellekYonetim.VeriOku_s("Dil");
        for (int i = 0; i < TextObjeleri.Length; i++)
        {
            if (aktifDil == "EN")
                TextObjeleri[i].text = _DilVerileriAnaObje[0]._DilVerileri_EN[i].Metin;
            else if (aktifDil == "TR")
                TextObjeleri[i].text = _DilVerileriAnaObje[0]._DilVerileri_TR[i].Metin;
            else
                TextObjeleri[i].text = _DilVerileriAnaObje[0]._DilVerileri_DE[i].Metin;
        }
    }

    public void SahneYukle(int Index)
    {
        ButonSes.Play();
        StartCoroutine(LoadSceneSafe(Index));
    }

    public void Oyna()
    {
        ButonSes.Play();
        int sonLevel = _BellekYonetim.VeriOku_i("SonLevel");
        if (sonLevel <= 0) sonLevel = 1; // fallback
        StartCoroutine(LoadAsync(sonLevel));
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

    IEnumerator LoadSceneSafe(int index)
    {
        yield return null; // 1 frame boþalt
        SceneManager.LoadScene(index);
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
