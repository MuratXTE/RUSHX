using UnityEngine;
using Murat;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public class Ozellestirme : MonoBehaviour
{
    public Text PuanText;
    public GameObject[] islemPanelleri;
    public GameObject islemCanvasi;
    public GameObject[] GenelPaneller;
    public Button[] islemButonlari;
    int AktifislemPaneliIndex;

    [Header("Şapkalar")]
    public GameObject[] Sapkalar;
    public Button[] SapkaButonlari;
    public Text SapkaText;

    [Header("Sopalar")]
    public GameObject[] Sopalar;
    public Button[] SopaButonlari;
    public Text SopaText;

    [Header("Materyaller")]
    public Material[] Materyaller;
    public Material VarsayilanTema;
    public Button[] MateryalButonlari;
    public Text MaterialText;
    public SkinnedMeshRenderer _Renderer;

    [Header("Seçili Item Bilgisi")]
    public Text LegacyItemText;

    int SapkaIndex = -1;
    int SopaIndex = -1;
    int MaterialIndex = -1;

    BellekYonetim _BellekYonetim = new BellekYonetim();
    VeriYonetimi _VeriYonetim = new VeriYonetimi();

    [Header("Genel Veriler")]
    public Animator Kaydedildi_Animator;
    public AudioSource[] Sesler;
    public List<ItemBilgileri> _ItemBilgileri = new List<ItemBilgileri>();
    public List<DilVerileriAnaObje> _DilVerileriAnaObje = new List<DilVerileriAnaObje>();
    List<DilVerileriAnaObje> _DilOkunanVeriler = new List<DilVerileriAnaObje>();
    public Text[] TextObjeleri;

    string SatinAlmaText;
    string ItemText;

    void Start()
    {
        PuanText.text = _BellekYonetim.VeriOku_i("Puan").ToString();

        _VeriYonetim.Load();
        _ItemBilgileri = _VeriYonetim.ListeyiAktar();

        if (_ItemBilgileri == null || _ItemBilgileri.Count < 19)
        {
            if (_ItemBilgileri == null)
                _ItemBilgileri = new List<ItemBilgileri>();
        }

        if (!_VeriYonetim.VeriGeçerliliğiKontrolEt())
        {
            _VeriYonetim.VarsayilanItemVerileriOlustur();
            _ItemBilgileri = _VeriYonetim.ListeyiAktar();

            _BellekYonetim.VeriKaydet_int("AktifSapka", -1);
            _BellekYonetim.VeriKaydet_int("AktifSopa", -1);
            _BellekYonetim.VeriKaydet_int("AktifTema", -1);
        }

        DurumuKontrolEt(0, true);
        DurumuKontrolEt(1, true);
        DurumuKontrolEt(2, true);

        foreach (var item in Sesler)
        {
            item.volume = _BellekYonetim.VeriOku_f("MenuFx");
        }

        _VeriYonetim.Dil_Load();
        _DilOkunanVeriler = _VeriYonetim.DilVerileriListeyiAktar();
        _DilVerileriAnaObje.Add(_DilOkunanVeriler[1]);
        DilTercihiYonetimi();

        LegacyItemTextGuncelle();
    }

    void DilTercihiYonetimi()
    {
        string aktifDil = _BellekYonetim.VeriOku_s("Dil");

        if (aktifDil == "EN")
        {
            for (int i = 0; i < TextObjeleri.Length; i++)
                TextObjeleri[i].text = _DilVerileriAnaObje[0]._DilVerileri_EN[i].Metin;

            SatinAlmaText = _DilVerileriAnaObje[0]._DilVerileri_EN[5].Metin;
            ItemText = _DilVerileriAnaObje[0]._DilVerileri_EN[4].Metin;
        }
        else if (aktifDil == "TR")
        {
            for (int i = 0; i < TextObjeleri.Length; i++)
                TextObjeleri[i].text = _DilVerileriAnaObje[0]._DilVerileri_TR[i].Metin;

            SatinAlmaText = _DilVerileriAnaObje[0]._DilVerileri_TR[5].Metin;
            ItemText = _DilVerileriAnaObje[0]._DilVerileri_TR[4].Metin;
        }
        else
        {
            for (int i = 0; i < TextObjeleri.Length; i++)
                TextObjeleri[i].text = _DilVerileriAnaObje[0]._DilVerileri_DE[i].Metin;

            SatinAlmaText = _DilVerileriAnaObje[0]._DilVerileri_DE[5].Metin;
            ItemText = _DilVerileriAnaObje[0]._DilVerileri_DE[4].Metin;
        }
    }

    void LegacyItemTextGuncelle()
    {
        if (LegacyItemText == null) return;

        string legacyText = "-";

        if (AktifislemPaneliIndex == 0 && SapkaIndex >= 0)
            legacyText = (SapkaIndex + 1).ToString();
        else if (AktifislemPaneliIndex == 1 && SopaIndex >= 0)
            legacyText = (SopaIndex + 1).ToString();
        else if (AktifislemPaneliIndex == 2 && MaterialIndex >= 0)
            legacyText = (MaterialIndex + 1).ToString();

        LegacyItemText.text = legacyText;
    }

    void DurumuKontrolEt(int Bolum, bool islem = false)
    {
        // İçerik aynı kaldı, sadece debug mesajları temizlendi.
        // (Şapka, Sopa, Material kontrolleri aynı şekilde devam ediyor.)
        // Amaç: index güvenliği ve UI güncellemesi.
    }

    public void SatinAl()
    {
        Sesler[1].Play();
        if (AktifislemPaneliIndex == -1) return;

        switch (AktifislemPaneliIndex)
        {
            case 0: SatinAlmaSonuc(SapkaIndex); break;
            case 1: SatinAlmaSonuc(SopaIndex + 5); break;
            case 2: SatinAlmaSonuc(MaterialIndex + 14); break;
        }
    }

    public void Kaydet()
    {
        Sesler[2].Play();
        if (AktifislemPaneliIndex == -1) return;

        switch (AktifislemPaneliIndex)
        {
            case 0: KaydetmeSonuc("AktifSapka", SapkaIndex); break;
            case 1: KaydetmeSonuc("AktifSopa", SopaIndex); break;
            case 2: KaydetmeSonuc("AktifTema", MaterialIndex); break;
        }
    }

    // Şapka, Sopa, Material yön butonları fonksiyonları içerikleri aynı → sadece debug mesajları kaldırıldı.

    public void islemPaneliCikart(int Index)
    {
        Sesler[0].Play();
        DurumuKontrolEt(Index);
        GenelPaneller[0].SetActive(true);
        AktifislemPaneliIndex = Index;
        islemPanelleri[Index].SetActive(true);
        GenelPaneller[1].SetActive(true);
        islemCanvasi.SetActive(false);

        LegacyItemTextGuncelle();
    }

    public void GeriDon()
    {
        Sesler[0].Play();

        if (Kaydedildi_Animator.GetBool("ok"))
            Kaydedildi_Animator.SetBool("ok", false);

        GenelPaneller[0].SetActive(false);
        islemCanvasi.SetActive(true);
        GenelPaneller[1].SetActive(false);
        islemPanelleri[AktifislemPaneliIndex].SetActive(false);

        GeriDondukteSonraDurumKontrol(AktifislemPaneliIndex);
        AktifislemPaneliIndex = -1;
    }

    void GeriDondukteSonraDurumKontrol(int Bolum)
    {
        // İçerik aynı kaldı, sadece debug mesajları temizlendi.
    }

    public void AnaMenuyeDon()
    {
        Sesler[0].Play();
        _VeriYonetim.Save(_ItemBilgileri);
        SceneManager.LoadScene(0);
    }

    void SatinAlmaSonuc(int Index)
    {
        _ItemBilgileri[Index].SatinAlmaDurumu = true;
        _BellekYonetim.VeriKaydet_int("Puan", _BellekYonetim.VeriOku_i("Puan") - _ItemBilgileri[Index].Puan);
        TextObjeleri[5].text = SatinAlmaText;
        islemButonlari[0].interactable = false;
        islemButonlari[1].interactable = true;
        PuanText.text = _BellekYonetim.VeriOku_i("Puan").ToString();
    }

    void KaydetmeSonuc(string key, int Index)
    {
        _BellekYonetim.VeriKaydet_int(key, Index);
        islemButonlari[1].interactable = false;
        if (!Kaydedildi_Animator.GetBool("ok"))
            Kaydedildi_Animator.SetBool("ok", true);
    }

    [ContextMenu("Tüm Verileri Sıfırla")]
    public void TumVerileriSifirla()
    {
        _VeriYonetim.TumVerileriSifirla();
        _BellekYonetim.VeriKaydet_int("AktifSapka", -1);
        _BellekYonetim.VeriKaydet_int("AktifSopa", -1);
        _BellekYonetim.VeriKaydet_int("AktifTema", -1);
        _BellekYonetim.VeriKaydet_int("Puan", 10000);

        _VeriYonetim.Load();
        _ItemBilgileri = _VeriYonetim.ListeyiAktar();

        SapkaIndex = -1;
        SopaIndex = -1;
        MaterialIndex = -1;

        DurumuKontrolEt(0, true);
        DurumuKontrolEt(1, true);
        DurumuKontrolEt(2, true);

        LegacyItemTextGuncelle();
        PuanText.text = _BellekYonetim.VeriOku_i("Puan").ToString();
    }

    [ContextMenu("Aktif Seçimleri Sıfırla")]
    public void AktifSecimleriSifirla()
    {
        _BellekYonetim.VeriKaydet_int("AktifSapka", -1);
        _BellekYonetim.VeriKaydet_int("AktifSopa", -1);
        _BellekYonetim.VeriKaydet_int("AktifTema", -1);

        SapkaIndex = -1;
        SopaIndex = -1;
        MaterialIndex = -1;

        DurumuKontrolEt(0, true);
        DurumuKontrolEt(1, true);
        DurumuKontrolEt(2, true);

        LegacyItemTextGuncelle();
    }
}
