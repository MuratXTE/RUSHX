using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Murat;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class AyarlarManager : MonoBehaviour
{
    public AudioSource ButonSes;
    public Slider MenuSes;
    public Slider MenuFx;
    public Slider OyunSes;

    BellekYonetim _BellekYonetim = new BellekYonetim();
    VeriYonetimi _VeriYonetim = new VeriYonetimi();

    public List<DilVerileriAnaObje> _DilVerileriAnaObje = new List<DilVerileriAnaObje>();
    List<DilVerileriAnaObje> _DilOkunanVeriler = new List<DilVerileriAnaObje>();
    public TextMeshProUGUI[] TextObjeleri;

    [Header("--------------DÝL TERCÝHÝ OBJELERÝ-------")]
    public TextMeshProUGUI DilText;
    public Button[] DilButonlari;
    int AktifDilIndex = 0;

    void Start()
    {
        ButonSes.volume = _BellekYonetim.VeriOku_f("MenuFx");

        MenuSes.value = _BellekYonetim.VeriOku_f("MenuSes");
        MenuFx.value = _BellekYonetim.VeriOku_f("MenuFx");
        OyunSes.value = _BellekYonetim.VeriOku_f("OyunSes");

        _VeriYonetim.Dil_Load();
        _DilOkunanVeriler = _VeriYonetim.DilVerileriListeyiAktar();

        if (_DilOkunanVeriler != null && _DilOkunanVeriler.Count > 4)
        {
            _DilVerileriAnaObje.Add(_DilOkunanVeriler[4]);
        }
        else
        {
            Debug.LogError("Dil verileri yüklenemedi!");
            return;
        }

        DilTercihiYonetimi();
        DilDurumunuKontrolEt();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
            StartCoroutine(GuvenliKaydet());
    }

    private IEnumerator GuvenliKaydet()
    {
        yield return null; // frame boþalt
        PlayerPrefs.Save();
    }

    void DilTercihiYonetimi()
    {
        if (_DilVerileriAnaObje == null || _DilVerileriAnaObje.Count == 0)
        {
            Debug.LogError("Dil verileri bulunamadý!");
            return;
        }

        string aktifDil = _BellekYonetim.VeriOku_s("Dil");

        if (aktifDil == "EN")
        {
            for (int i = 0; i < TextObjeleri.Length; i++)
                if (i < _DilVerileriAnaObje[0]._DilVerileri_EN.Count)
                    TextObjeleri[i].text = _DilVerileriAnaObje[0]._DilVerileri_EN[i].Metin;
        }
        else if (aktifDil == "TR")
        {
            for (int i = 0; i < TextObjeleri.Length; i++)
                if (i < _DilVerileriAnaObje[0]._DilVerileri_TR.Count)
                    TextObjeleri[i].text = _DilVerileriAnaObje[0]._DilVerileri_TR[i].Metin;
        }
        else // DE
        {
            for (int i = 0; i < TextObjeleri.Length; i++)
                if (i < _DilVerileriAnaObje[0]._DilVerileri_DE.Count)
                    TextObjeleri[i].text = _DilVerileriAnaObje[0]._DilVerileri_DE[i].Metin;
        }
    }

    public void SesAyarla(string HangiAyar)
    {
        switch (HangiAyar)
        {
            case "menuses":
                _BellekYonetim.VeriKaydet_float("MenuSes", MenuSes.value);
                break;
            case "menufx":
                _BellekYonetim.VeriKaydet_float("MenuFx", MenuFx.value);
                ButonSes.volume = MenuFx.value;
                break;
            case "oyunses":
                _BellekYonetim.VeriKaydet_float("OyunSes", OyunSes.value);
                break;
        }
    }

    public void GeriDon()
    {
        ButonSes.Play();
        StartCoroutine(LoadSceneSafe(0));
    }

    IEnumerator LoadSceneSafe(int index)
    {
        yield return null; // 1 frame bekle
        SceneManager.LoadScene(index);
    }

    void DilDurumunuKontrolEt()
    {
        string aktifDil = _BellekYonetim.VeriOku_s("Dil");

        if (DilButonlari == null || DilButonlari.Length < 2)
        {
            Debug.LogError("DilButonlari array'i düzgün ayarlanmamýþ!");
            return;
        }

        if (aktifDil == "EN")
        {
            AktifDilIndex = 0;
            DilText.text = "ENGLISH";
            DilButonlari[0].interactable = false;
            DilButonlari[1].interactable = true;
        }
        else if (aktifDil == "TR")
        {
            AktifDilIndex = 1;
            DilText.text = "TÜRKÇE";
            DilButonlari[0].interactable = true;
            DilButonlari[1].interactable = true;
        }
        else // DE
        {
            AktifDilIndex = 2;
            DilText.text = "DEUTSCH";
            DilButonlari[0].interactable = true;
            DilButonlari[1].interactable = false;
        }
    }

    public void DilDegistir(string Yon)
    {
        if (Yon == "ileri")
        {
            AktifDilIndex++;
            if (AktifDilIndex > 2) AktifDilIndex = 0;
        }
        else if (Yon == "geri")
        {
            AktifDilIndex--;
            if (AktifDilIndex < 0) AktifDilIndex = 2;
        }

        switch (AktifDilIndex)
        {
            case 0:
                DilText.text = "ENGLISH";
                if (DilButonlari.Length >= 2)
                {
                    DilButonlari[0].interactable = false;
                    DilButonlari[1].interactable = true;
                }
                _BellekYonetim.VeriKaydet_string("Dil", "EN");
                break;

            case 1:
                DilText.text = "TÜRKÇE";
                if (DilButonlari.Length >= 2)
                {
                    DilButonlari[0].interactable = true;
                    DilButonlari[1].interactable = true;
                }
                _BellekYonetim.VeriKaydet_string("Dil", "TR");
                break;

            case 2:
                DilText.text = "DEUTSCH";
                if (DilButonlari.Length >= 2)
                {
                    DilButonlari[0].interactable = true;
                    DilButonlari[1].interactable = false;
                }
                _BellekYonetim.VeriKaydet_string("Dil", "DE");
                break;
        }

        DilTercihiYonetimi();
        ButonSes.Play();
    }
}
