
using UnityEngine;
using Murat;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.Windows;
using File = System.IO.File;
using NUnit.Framework;
using System.Collections.Generic;
using System;
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
    [Header("----------------------------SAPKALAR")]
    public GameObject[] Sapkalar;
    public Button[] SapkaButonlari;
    public Text SapkaText;
    [Header("----------------------------SOPALAR")]
    public GameObject[] Sopalar;
    public Button[] SopaButonlari;
    public Text SopaText;
    [Header("----------------------------MATER�AL")]
    public Material[] Materyaller;
    public Material VarsayilanTema;
    public Button[] MateryalButonlari;
    public Text MaterialText;
    public SkinnedMeshRenderer _Renderer;
    [Header("----------------------------LEGACY")]
    public Text LegacyItemText; // Güncel seçili item'i gösterir


    int SapkaIndex = -1;
    int SopaIndex = -1;
    int MaterialIndex = -1;

    BellekYonetim _BellekYonetim = new BellekYonetim();
    VeriYonetimi _VeriYonetim = new VeriYonetimi();
    [Header("----------------------------GENEL VERILER")]
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
        //_BellekYonetim.VeriKaydet_string("Dil", "TR");


        _VeriYonetim.Load();
        _ItemBilgileri = _VeriYonetim.ListeyiAktar();

        // ✅ KRİTİK GÜVENLİK KONTROLÜ
        if (_ItemBilgileri == null || _ItemBilgileri.Count < 19)
        {
            Debug.LogError($"❌ UYARI: ItemBilgileri yetersiz! Count: {(_ItemBilgileri?.Count ?? 0)}. Minimum 19 gerekli.");
            Debug.LogError("Gereksinimler: 5 Şapka (0-4) + 9 Sopa (5-13) + 5 Material (14-18) = 19 item");
            
            // Güvenlik için boş list oluştur
            if (_ItemBilgileri == null)
                _ItemBilgileri = new List<ItemBilgileri>();
        }
        
        // ✅ EXTRA: Veri geçerliliğini kontrol et
        if (!_VeriYonetim.VeriGeçerliliğiKontrolEt())
        {
            Debug.LogError("❌ Veri geçerliliği kontrolü başarısız! Varsayılan veriler yeniden oluşturuluyor.");
            
            // Veri bozuksa yeniden oluştur ve yükle
            _VeriYonetim.VarsayilanItemVerileriOlustur();
            _ItemBilgileri = _VeriYonetim.ListeyiAktar();
            
            // Tüm active index'leri sıfırla (güvenlik için)
            _BellekYonetim.VeriKaydet_int("AktifSapka", -1);
            _BellekYonetim.VeriKaydet_int("AktifSopa", -1);
            _BellekYonetim.VeriKaydet_int("AktifTema", -1);
            
            Debug.Log("✅ Varsayılan veriler yeniden oluşturuldu ve active durumlar sıfırlandı.");
        }

        //_BellekYonetim.VeriKaydet_int("Puan", 10000);

        // Debug bilgileri - array boyutlarını kontrol et
        Debug.Log($"Sapkalar array size: {Sapkalar.Length}");
        Debug.Log($"Sopalar array size: {Sopalar.Length}");
        Debug.Log($"Materyaller array size: {Materyaller.Length}");
        Debug.Log($"ItemBilgileri count: {_ItemBilgileri.Count}");
        Debug.Log($"AktifSapka value: {_BellekYonetim.VeriOku_i("AktifSapka")}");
        Debug.Log($"AktifSopa value: {_BellekYonetim.VeriOku_i("AktifSopa")}");
        Debug.Log($"AktifTema value: {_BellekYonetim.VeriOku_i("AktifTema")}");

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
        
        // Legacy text'i başlangıçta güncelle
        LegacyItemTextGuncelle();
    }
    void DilTercihiYonetimi()
    {

        if (_BellekYonetim.VeriOku_s("Dil") == "EN")
        {
            for (int i = 0; i < TextObjeleri.Length; i++)
            {
                TextObjeleri[i].text = _DilVerileriAnaObje[0]._DilVerileri_EN[i].Metin;
            }
            SatinAlmaText = _DilVerileriAnaObje[0]._DilVerileri_EN[5].Metin;
            ItemText = _DilVerileriAnaObje[0]._DilVerileri_EN[4].Metin;
        }
        else if (_BellekYonetim.VeriOku_s("Dil") == "TR")
        {
            for (int i = 0; i < TextObjeleri.Length; i++)
            {
                TextObjeleri[i].text = _DilVerileriAnaObje[0]._DilVerileri_TR[i].Metin;
            }
            SatinAlmaText = _DilVerileriAnaObje[0]._DilVerileri_TR[5].Metin;
            ItemText = _DilVerileriAnaObje[0]._DilVerileri_TR[4].Metin;

        }
        else
        {
            for (int i = 0; i < TextObjeleri.Length; i++)
            {
                TextObjeleri[i].text = _DilVerileriAnaObje[0]._DilVerileri_DE[i].Metin;
            }
            SatinAlmaText = _DilVerileriAnaObje[0]._DilVerileri_DE[5].Metin;
            ItemText = _DilVerileriAnaObje[0]._DilVerileri_DE[4].Metin;

        }
    }
    
    // Legacy text'i güncel seçili item'a göre günceller
    void LegacyItemTextGuncelle()
    {
        if (LegacyItemText == null) return;
        
        string legacyText = "";
        
        // Şu anda hangi panel aktif?
        if (AktifislemPaneliIndex == 0) // Şapka paneli
        {
            if (SapkaIndex >= 0)
                legacyText = (SapkaIndex + 1).ToString();
            else
                legacyText = "-";
        }
        else if (AktifislemPaneliIndex == 1) // Sopa paneli
        {
            if (SopaIndex >= 0)
                legacyText = (SopaIndex + 1).ToString();
            else
                legacyText = "-";
        }
        else if (AktifislemPaneliIndex == 2) // Material paneli
        {
            if (MaterialIndex >= 0)
                legacyText = (MaterialIndex + 1).ToString();
            else
                legacyText = "-";
        }
        else
        {
            legacyText = "-";
        }
        
        LegacyItemText.text = legacyText;
    }
    
    void DurumuKontrolEt(int Bolum, bool islem = false)
    {
        if (Bolum == 0)
        {
            #region
            if (_BellekYonetim.VeriOku_i("AktifSapka") == -1)
            {

                foreach (var item in Sapkalar)
                {
                    item.SetActive(false);
                }
                TextObjeleri[5].text = SatinAlmaText;
                islemButonlari[0].interactable = false;
                islemButonlari[1].interactable = false;

                if (!islem)
                {
                    SapkaIndex = -1;
                    SapkaText.text = "0";
                }
            }
            else
            {
                foreach (var item in Sapkalar)
                {
                    item.SetActive(false);
                }

                SapkaIndex = _BellekYonetim.VeriOku_i("AktifSapka");
                
                // Güvenlik kontrolü: SapkaIndex sınırlar içinde mi?
                if (SapkaIndex >= 0 && SapkaIndex < Sapkalar.Length)
                {
                    Sapkalar[SapkaIndex].SetActive(true);

                    // İkinci güvenlik kontrolü: ItemBilgileri array'i için
                    if (SapkaIndex >= 0 && SapkaIndex < _ItemBilgileri.Count)
                    {
                        // Basit numara gösterimi
                        SapkaText.text = (SapkaIndex + 1).ToString();
                    }
                    else
                    {
                        SapkaText.text = "Item Bulunamadı";
                        Debug.LogError($"ItemBilgileri index out of range: {SapkaIndex}. Array size: {_ItemBilgileri.Count}");
                    }
                    
                    TextObjeleri[5].text = SatinAlmaText;
                    islemButonlari[0].interactable = false;
                    islemButonlari[1].interactable = true;
                }
                else
                {
                    Debug.LogError($"Sapka index out of range: {SapkaIndex}. Array size: {Sapkalar.Length}");
                    // Hatalı index durumunda sıfırla
                    _BellekYonetim.VeriKaydet_int("AktifSapka", -1);
                    SapkaIndex = -1;
                    SapkaText.text = "0";
                    TextObjeleri[5].text = SatinAlmaText;
                    islemButonlari[0].interactable = false;
                    islemButonlari[1].interactable = false;
                }
            }
            #endregion
        }
        else if (Bolum == 1)
        {
            #region
            if (_BellekYonetim.VeriOku_i("AktifSopa") == -1)
            {

                foreach (var item in Sopalar)
                {
                    item.SetActive(false);
                }
                islemButonlari[0].interactable = false;
                islemButonlari[1].interactable = false;
                TextObjeleri[5].text = SatinAlmaText;
                if (!islem)
                {
                    SopaIndex = -1;
                    SopaText.text = "0";
                }
                else
                {
                    SopaText.text = "0";
                }
            }
            else
            {
                foreach (var item in Sopalar)
                {
                    item.SetActive(false);
                }

                SopaIndex = _BellekYonetim.VeriOku_i("AktifSopa");
                
                // Güvenlik kontrolü: SopaIndex sınırlar içinde mi?
                if (SopaIndex >= 0 && SopaIndex < Sopalar.Length)
                {
                    Sopalar[SopaIndex].SetActive(true);

                    // İkinci güvenlik kontrolü: ItemBilgileri array'i için
                    int itemIndex = SopaIndex + 5;
                    if (itemIndex >= 0 && itemIndex < _ItemBilgileri.Count)
                    {
                        // Basit numara gösterimi
                        SopaText.text = (SopaIndex + 1).ToString();
                    }
                    else
                    {
                        SopaText.text = "Item Bulunamadı";
                        Debug.LogError($"ItemBilgileri index out of range: {itemIndex}. Array size: {_ItemBilgileri.Count}");
                    }
                    
                    TextObjeleri[5].text = SatinAlmaText;
                    islemButonlari[0].interactable = false;
                    islemButonlari[1].interactable = true;
                }
                else
                {
                    Debug.LogError($"Sopa index out of range: {SopaIndex}. Array size: {Sopalar.Length}");
                    // Hatalı index durumunda sıfırla
                    _BellekYonetim.VeriKaydet_int("AktifSopa", -1);
                    SopaIndex = -1;
                    SopaText.text = "0";
                    TextObjeleri[5].text = SatinAlmaText;
                    islemButonlari[0].interactable = false;
                    islemButonlari[1].interactable = false;
                }
            }
            #endregion
        }
        else
        {
            if (_BellekYonetim.VeriOku_i("AktifTema") == -1)
            {
                if (!islem)
                {
                    TextObjeleri[5].text = SatinAlmaText;
                    MaterialIndex = -1;
                    MaterialText.text = "0";
                    islemButonlari[0].interactable = false;
                    islemButonlari[1].interactable = false;
                }
                else
                {
                    Material[] mats = _Renderer.materials;
                    mats[0] = VarsayilanTema;
                    _Renderer.materials = mats;
                    MaterialText.text = "0";
                    TextObjeleri[5].text = SatinAlmaText;
                }
            }
            else
            {
                MaterialIndex = _BellekYonetim.VeriOku_i("AktifTema");
                
                // Güvenlik kontrolü: MaterialIndex sınırlar içinde mi?
                if (MaterialIndex >= 0 && MaterialIndex < Materyaller.Length)
                {
                    Material[] mats = _Renderer.materials;
                    mats[0] = Materyaller[MaterialIndex];
                    _Renderer.materials = mats;

                    // İkinci güvenlik kontrolü: ItemBilgileri array'i için
                    int itemIndex = MaterialIndex + 14;
                    if (itemIndex >= 0 && itemIndex < _ItemBilgileri.Count)
                    {
                        // Basit numara gösterimi
                        MaterialText.text = (MaterialIndex + 1).ToString();
                    }
                    else
                    {
                        MaterialText.text = "Material Bulunamadı";
                        Debug.LogError($"ItemBilgileri index out of range: {itemIndex}. Array size: {_ItemBilgileri.Count}");
                    }
                    
                    TextObjeleri[5].text = SatinAlmaText;
                    islemButonlari[0].interactable = false;
                    islemButonlari[1].interactable = true;
                }
                else
                {
                    Debug.LogError($"Material index out of range: {MaterialIndex}. Array size: {Materyaller.Length}");
                    // Hatalı index durumunda sıfırla ve varsayılan materyali kullan
                    _BellekYonetim.VeriKaydet_int("AktifTema", -1);
                    MaterialIndex = -1;
                    Material[] mats = _Renderer.materials;
                    mats[0] = VarsayilanTema;
                    _Renderer.materials = mats;
                    MaterialText.text = "0";
                    TextObjeleri[5].text = SatinAlmaText;
                    islemButonlari[0].interactable = false;
                    islemButonlari[1].interactable = false;
                }
            }
        }
        
        // Durum kontrol edildiğinde legacy text'i güncelle
        LegacyItemTextGuncelle();
    }
    public void SatinAl()
    {
        Sesler[1].Play();
        if (AktifislemPaneliIndex != -1)
        {
            switch (AktifislemPaneliIndex)
            {
                case 0:
                    SatinAlmaSonuc(SapkaIndex);
                    break;
                case 1:
                    int Index = SopaIndex + 5;
                    SatinAlmaSonuc(Index);
                    break;
                case 2:
                    int Index2 = MaterialIndex + 14;
                    SatinAlmaSonuc(Index2);
                    break;
            }
        }

    }
    public void Kaydet()
    {
        Sesler[2].Play();
        if (AktifislemPaneliIndex != -1)
        {
            switch (AktifislemPaneliIndex)
            {
                case 0:
                    KaydetmeSonuc("AktifSapka", SapkaIndex);
                    break;
                case 1:
                    KaydetmeSonuc("AktifSopa", SopaIndex);
                    break;
                case 2:
                    KaydetmeSonuc("AktifTema", MaterialIndex);
                    break;
            }
        }
    }
    public void Sapka_Yonbutonlari(string islem)
    {
        Sesler[0].Play();
        if (islem == "ileri")
        {
            if (SapkaIndex == -1)
            {
                SapkaIndex = 0;
                
                // Legacy text'i güncelle
                LegacyItemTextGuncelle();
                // Güvenlik kontrolü: İlk şapka erişimi
                if (SapkaIndex < Sapkalar.Length && SapkaIndex < _ItemBilgileri.Count)
                {
                    Sapkalar[SapkaIndex].SetActive(true);
                    
                    // Basit numara gösterimi
                    SapkaText.text = (SapkaIndex + 1).ToString();

                    // Debug: İlk şapka durumu (Index 0 problemi için)
                    Debug.Log($"🎩 İLK Şapka Index {SapkaIndex}: SatinAlmaDurumu = {_ItemBilgileri[SapkaIndex].SatinAlmaDurumu}, Puan = {_ItemBilgileri[SapkaIndex].Puan}");

                    if (!_ItemBilgileri[SapkaIndex].SatinAlmaDurumu)
                    {
                        TextObjeleri[5].text = _ItemBilgileri[SapkaIndex].Puan + " - " + SatinAlmaText;
                        islemButonlari[1].interactable = false;

                        if (_BellekYonetim.VeriOku_i("Puan") < _ItemBilgileri[SapkaIndex].Puan)
                            islemButonlari[0].interactable = false;
                        else
                            islemButonlari[0].interactable = true;
                    }
                    else
                    {
                        TextObjeleri[5].text = SatinAlmaText;
                        islemButonlari[0].interactable = false;
                        islemButonlari[1].interactable = true;
                    }
                }
                else
                {
                    Debug.LogError($"İlk şapka erişiminde hata: Sapkalar.Length={Sapkalar.Length}, ItemBilgileri.Count={_ItemBilgileri.Count}");
                    SapkaIndex = -1;
                }
            }
            else
            {
                Sapkalar[SapkaIndex].SetActive(false);
                SapkaIndex++;
                
                // Legacy text'i güncelle
                LegacyItemTextGuncelle();
                // Güvenlik kontrolü: SapkaIndex sınırlar içinde mi?
                if (SapkaIndex >= 0 && SapkaIndex < Sapkalar.Length && SapkaIndex < _ItemBilgileri.Count)
                {
                    Sapkalar[SapkaIndex].SetActive(true);
                    
                    // Basit numara gösterimi
                    SapkaText.text = (SapkaIndex + 1).ToString();

                    if (!_ItemBilgileri[SapkaIndex].SatinAlmaDurumu)
                    {
                        TextObjeleri[5].text = _ItemBilgileri[SapkaIndex].Puan + " - " + SatinAlmaText;
                        islemButonlari[1].interactable = false;

                        if (_BellekYonetim.VeriOku_i("Puan") < _ItemBilgileri[SapkaIndex].Puan)
                            islemButonlari[0].interactable = false;
                        else
                            islemButonlari[0].interactable = true;
                    }
                    else
                    {
                        TextObjeleri[5].text = SatinAlmaText;
                        islemButonlari[0].interactable = false;
                        islemButonlari[1].interactable = true;
                    }
                }
                else
                {
                    // Sınırların dışına çıkıldığında bir önceki geçerli index'e geri dön
                    SapkaIndex--;
                    // Legacy text'i güncelle
                    LegacyItemTextGuncelle();
                    Debug.LogWarning($"Sapka index sınırları aştı: {SapkaIndex + 1}. Maksimum: {Math.Min(Sapkalar.Length, _ItemBilgileri.Count) - 1}");
                }
            }

            //-----------------------------------------------------

            if (SapkaIndex == Sapkalar.Length - 1)
                SapkaButonlari[1].interactable = false;
            else
                SapkaButonlari[1].interactable = true;

            if (SapkaIndex != -1)
                SapkaButonlari[0].interactable = true;
        }
        else
        {
            if (SapkaIndex != -1)
            {
                Sapkalar[SapkaIndex].SetActive(false);
                SapkaIndex--;
                // Legacy text'i güncelle
                LegacyItemTextGuncelle();
                if (SapkaIndex != -1)
                {
                    // Güvenlik kontrolü: SapkaIndex sınırlar içinde mi?
                    if (SapkaIndex >= 0 && SapkaIndex < Sapkalar.Length && SapkaIndex < _ItemBilgileri.Count)
                    {
                        Sapkalar[SapkaIndex].SetActive(true);
                        SapkaButonlari[0].interactable = true;
                        
                        // Basit numara gösterimi
                        SapkaText.text = (SapkaIndex + 1).ToString();

                        if (!_ItemBilgileri[SapkaIndex].SatinAlmaDurumu)
                        {
                            TextObjeleri[5].text = _ItemBilgileri[SapkaIndex].Puan + " - " + SatinAlmaText;
                            islemButonlari[1].interactable = false;

                            if (_BellekYonetim.VeriOku_i("Puan") < _ItemBilgileri[SapkaIndex].Puan)
                                islemButonlari[0].interactable = false;
                            else
                                islemButonlari[0].interactable = true;
                        }
                        else
                        {
                            TextObjeleri[5].text = SatinAlmaText;
                            islemButonlari[0].interactable = false;
                            islemButonlari[1].interactable = true;
                        }
                    }
                    else
                    {
                        Debug.LogError($"Geri yönde şapka index hatası: {SapkaIndex}. Sapkalar.Length={Sapkalar.Length}, ItemBilgileri.Count={_ItemBilgileri.Count}");
                        SapkaIndex = -1;
                        SapkaButonlari[0].interactable = false;
                        SapkaText.text = "0";
                        TextObjeleri[5].text = SatinAlmaText;
                        islemButonlari[0].interactable = false;
                    }
                }
                else
                {
                    SapkaButonlari[0].interactable = false;
                    SapkaText.text = "0";
                    TextObjeleri[5].text = SatinAlmaText;
                    islemButonlari[0].interactable = false;
                }

            }
            else
            {
                SapkaButonlari[0].interactable = false;
                SapkaText.text = "0";
                TextObjeleri[5].text = SatinAlmaText;
                islemButonlari[0].interactable = false;
            }
            //-----------------------------------------------------
            if (SapkaIndex != Sapkalar.Length - 1)
                SapkaButonlari[1].interactable = true;
                
            // Legacy text'i güncelle
            LegacyItemTextGuncelle();
        }
    }
    public void Sopa_Yonbutonlari(string islem)
    {
        Sesler[0].Play();
        if (islem == "ileri")
        {
            if (SopaIndex == -1)
            {
                SopaIndex = 0;
                
                // Legacy text'i güncelle
                LegacyItemTextGuncelle();
                // Güvenlik kontrolü: İlk sopa erişimi
                if (SopaIndex < Sopalar.Length && (SopaIndex + 5) < _ItemBilgileri.Count)
                {
                    Sopalar[SopaIndex].SetActive(true);
                    
                    // Basit numara gösterimi
                    SopaText.text = (SopaIndex + 1).ToString();

                    // Debug: İlk sopa durumu (Index 0 problemi için)
                    Debug.Log($"⚔️ İLK Sopa Index {SopaIndex} (ItemIndex {SopaIndex + 5}): SatinAlmaDurumu = {_ItemBilgileri[SopaIndex + 5].SatinAlmaDurumu}, Puan = {_ItemBilgileri[SopaIndex + 5].Puan}");

                    if (!_ItemBilgileri[SopaIndex + 5].SatinAlmaDurumu)
                    {
                        TextObjeleri[5].text = _ItemBilgileri[SopaIndex + 5].Puan + " - " + SatinAlmaText;

                        islemButonlari[1].interactable = false;
                        if (_BellekYonetim.VeriOku_i("Puan") < _ItemBilgileri[SopaIndex + 5].Puan)
                            islemButonlari[0].interactable = false;
                        else
                            islemButonlari[0].interactable = true;
                    }
                    else
                    {
                        TextObjeleri[5].text = SatinAlmaText;
                        islemButonlari[0].interactable = false;
                        islemButonlari[1].interactable = true;
                    }
                }
                else
                {
                    Debug.LogError($"İlk sopa erişiminde hata: Sopalar.Length={Sopalar.Length}, ItemBilgileri.Count={_ItemBilgileri.Count}");
                    SopaIndex = -1;
                }
            }
            else
            {
                Sopalar[SopaIndex].SetActive(false);
                SopaIndex++;
                
                // Legacy text'i güncelle
                LegacyItemTextGuncelle();
                // Güvenlik kontrolü: SopaIndex sınırlar içinde mi?
                if (SopaIndex >= 0 && SopaIndex < Sopalar.Length && (SopaIndex + 5) < _ItemBilgileri.Count)
                {
                    Sopalar[SopaIndex].SetActive(true);
                    
                    // Basit numara gösterimi
                    SopaText.text = (SopaIndex + 1).ToString();

                    if (!_ItemBilgileri[SopaIndex + 5].SatinAlmaDurumu)
                    {
                        TextObjeleri[5].text = _ItemBilgileri[SopaIndex + 5].Puan + " - " + SatinAlmaText;
                        islemButonlari[1].interactable = false;
                        if (_BellekYonetim.VeriOku_i("Puan") < _ItemBilgileri[SopaIndex + 5].Puan)
                            islemButonlari[0].interactable = false;
                        else
                            islemButonlari[0].interactable = true;
                    }
                    else
                    {
                        TextObjeleri[5].text = SatinAlmaText;
                        islemButonlari[0].interactable = false;
                        islemButonlari[1].interactable = true;
                    }
                }
                else
                {
                    // Sınırların dışına çıkıldığında bir önceki geçerli index'e geri dön
                    SopaIndex--;
                    // Legacy text'i güncelle
                    LegacyItemTextGuncelle();
                    Debug.LogWarning($"Sopa index sınırları aştı: {SopaIndex + 1}. Maksimum sopa: {Sopalar.Length - 1}, Maksimum item: {(_ItemBilgileri.Count - 6)}");
                }
            }


            //-----------------------------------------------------


            if (SopaIndex == Sopalar.Length - 1)
                SopaButonlari[1].interactable = false;
            else
                SopaButonlari[1].interactable = true;

            if (SopaIndex != -1)
                SopaButonlari[0].interactable = true;
        }
        else
        {
            if (SopaIndex != -1)
            {
                Sopalar[SopaIndex].SetActive(false);
                SopaIndex--;
                // Legacy text'i güncelle
                LegacyItemTextGuncelle();
                if (SopaIndex != -1)
                {
                    // Güvenlik kontrolü: SopaIndex sınırlar içinde mi?
                    if (SopaIndex >= 0 && SopaIndex < Sopalar.Length && (SopaIndex + 5) < _ItemBilgileri.Count)
                    {
                        Sopalar[SopaIndex].SetActive(true);
                        SopaButonlari[0].interactable = true;
                        
                        // Basit numara gösterimi
                        SopaText.text = (SopaIndex + 1).ToString();

                        if (!_ItemBilgileri[SopaIndex + 5].SatinAlmaDurumu)
                        {
                            TextObjeleri[5].text = _ItemBilgileri[SopaIndex + 5].Puan + " - " + SatinAlmaText;
                            islemButonlari[1].interactable = false;
                            if (_BellekYonetim.VeriOku_i("Puan") < _ItemBilgileri[SopaIndex + 5].Puan)
                                islemButonlari[0].interactable = false;
                            else
                                islemButonlari[0].interactable = true;
                        }
                        else
                        {
                            TextObjeleri[5].text = SatinAlmaText;
                            islemButonlari[0].interactable = false;
                            islemButonlari[1].interactable = true;
                        }
                    }
                    else
                    {
                        Debug.LogError($"Geri yönde sopa index hatası: {SopaIndex}. Sopalar.Length={Sopalar.Length}, ItemBilgileri gerekli index={SopaIndex + 5}, Count={_ItemBilgileri.Count}");
                        SopaIndex = -1;
                        SopaButonlari[0].interactable = false;
                        SopaText.text = "0";
                        TextObjeleri[5].text = SatinAlmaText;
                        islemButonlari[0].interactable = false;
                    }
                }
                else
                {
                    SopaButonlari[0].interactable = false;
                    SopaText.text = "0";
                    TextObjeleri[5].text = SatinAlmaText;
                    islemButonlari[0].interactable = false;
                }

            }
            else
            {
                SopaButonlari[0].interactable = false;
                SopaText.text = "0";
            }
            //-----------------------------------------------------
            if (SopaIndex != Sopalar.Length - 1)
                SopaButonlari[1].interactable = true;
                
            // Legacy text'i güncelle
            LegacyItemTextGuncelle();
        }
    }
    public void Material_Yonbutonlari(string islem)
    {
        Sesler[0].Play();
        if (islem == "ileri")
        {
            if (MaterialIndex == -1)
            {
                MaterialIndex = 0;
                
                // Legacy text'i güncelle
                LegacyItemTextGuncelle();
                // Güvenlik kontrolü: İlk material erişimi
                if (MaterialIndex < Materyaller.Length && (MaterialIndex + 14) < _ItemBilgileri.Count)
                {
                    Material[] mats = _Renderer.materials;
                    mats[0] = Materyaller[MaterialIndex];
                    _Renderer.materials = mats;
                    
                    // Basit numara gösterimi
                    MaterialText.text = (MaterialIndex + 1).ToString();

                    if (!_ItemBilgileri[MaterialIndex + 14].SatinAlmaDurumu)
                    {
                        TextObjeleri[5].text = _ItemBilgileri[MaterialIndex + 14].Puan + " - " + SatinAlmaText;
                        islemButonlari[1].interactable = false;
                        if (_BellekYonetim.VeriOku_i("Puan") < _ItemBilgileri[MaterialIndex + 14].Puan)
                            islemButonlari[0].interactable = false;
                        else
                            islemButonlari[0].interactable = true;
                    }
                    else
                    {
                        TextObjeleri[5].text = SatinAlmaText;
                        islemButonlari[0].interactable = false;
                        islemButonlari[1].interactable = true;
                    }
                }
                else
                {
                    Debug.LogError($"İlk material erişiminde hata: Materyaller.Length={Materyaller.Length}, ItemBilgileri.Count={_ItemBilgileri.Count}");
                    MaterialIndex = -1;
                }
            }
            else
            {
                MaterialIndex++;
                
                // Legacy text'i güncelle
                LegacyItemTextGuncelle();
                // Güvenlik kontrolü: MaterialIndex sınırlar içinde mi?
                if (MaterialIndex >= 0 && MaterialIndex < Materyaller.Length && (MaterialIndex + 14) < _ItemBilgileri.Count)
                {
                    Material[] mats = _Renderer.materials;
                    mats[0] = Materyaller[MaterialIndex];
                    _Renderer.materials = mats;
                    
                    // Basit numara gösterimi
                    MaterialText.text = (MaterialIndex + 1).ToString();

                    if (!_ItemBilgileri[MaterialIndex + 14].SatinAlmaDurumu)
                    {
                        TextObjeleri[5].text = _ItemBilgileri[MaterialIndex + 14].Puan + " - " + SatinAlmaText;
                        islemButonlari[1].interactable = false;
                        if (_BellekYonetim.VeriOku_i("Puan") < _ItemBilgileri[MaterialIndex + 14].Puan)
                            islemButonlari[0].interactable = false;
                        else
                            islemButonlari[0].interactable = true;
                    }
                    else
                    {
                        TextObjeleri[5].text = SatinAlmaText;
                        islemButonlari[0].interactable = false;
                        islemButonlari[1].interactable = true;
                    }
                }
                else
                {
                    // Sınırların dışına çıkıldığında bir önceki geçerli index'e geri dön
                    MaterialIndex--;
                    // Legacy text'i güncelle
                    LegacyItemTextGuncelle();
                    Debug.LogWarning($"Material index sınırları aştı: {MaterialIndex + 1}. Maksimum material: {Materyaller.Length - 1}, Maksimum item: {(_ItemBilgileri.Count - 15)}");
                }
            }
            //-----------------------------------------------------


            if (MaterialIndex == Materyaller.Length - 1)
                MateryalButonlari[1].interactable = false;
            else
                MateryalButonlari[1].interactable = true;

            if (MaterialIndex != -1)
                MateryalButonlari[0].interactable = true;
        }
        else
        {
            if (MaterialIndex != -1)
            {
                MaterialIndex--;
                // Legacy text'i güncelle
                LegacyItemTextGuncelle();
                if (MaterialIndex != -1)
                {
                    // Güvenlik kontrolü: MaterialIndex sınırlar içinde mi?
                    if (MaterialIndex >= 0 && MaterialIndex < Materyaller.Length && (MaterialIndex + 14) < _ItemBilgileri.Count)
                    {
                        Material[] mats = _Renderer.materials;
                        mats[0] = Materyaller[MaterialIndex];
                        _Renderer.materials = mats;
                        MateryalButonlari[0].interactable = true;
                        
                        // Basit numara gösterimi
                        MaterialText.text = (MaterialIndex + 1).ToString();

                        if (!_ItemBilgileri[MaterialIndex + 14].SatinAlmaDurumu)
                        {
                            TextObjeleri[5].text = _ItemBilgileri[MaterialIndex + 14].Puan + " - " + SatinAlmaText;
                            islemButonlari[1].interactable = false;
                            if (_BellekYonetim.VeriOku_i("Puan") < _ItemBilgileri[MaterialIndex + 14].Puan)
                                islemButonlari[0].interactable = false;
                            else
                                islemButonlari[0].interactable = true;
                        }
                        else
                        {
                            TextObjeleri[5].text = SatinAlmaText;
                            islemButonlari[0].interactable = false;
                            islemButonlari[1].interactable = true;
                        }
                    }
                    else
                    {
                        Debug.LogError($"Geri yönde material index hatası: {MaterialIndex}. Materyaller.Length={Materyaller.Length}, ItemBilgileri gerekli index={MaterialIndex + 14}, Count={_ItemBilgileri.Count}");
                        MaterialIndex = -1;
                        Material[] mats = _Renderer.materials;
                        mats[0] = VarsayilanTema;
                        _Renderer.materials = mats;
                        MateryalButonlari[0].interactable = false;
                        MaterialText.text = "0";
                        TextObjeleri[5].text = SatinAlmaText;
                        islemButonlari[0].interactable = false;
                    }
                }
                else
                {
                    Material[] mats = _Renderer.materials;
                    mats[0] = VarsayilanTema;
                    _Renderer.materials = mats;
                    MateryalButonlari[0].interactable = false;
                    MaterialText.text = "0";
                    TextObjeleri[5].text = SatinAlmaText;
                    islemButonlari[0].interactable = false;
                }

            }
            else
            {
                Material[] mats = _Renderer.materials;
                mats[0] = VarsayilanTema;
                _Renderer.materials = mats;
                MateryalButonlari[0].interactable = false;
                MaterialText.text = "0";
                TextObjeleri[5].text = SatinAlmaText;
                islemButonlari[0].interactable = false;
            }
            //-----------------------------------------------------
            if (MaterialIndex != Materyaller.Length - 1)
                MateryalButonlari[1].interactable = true;
                
            // Legacy text'i güncelle
            LegacyItemTextGuncelle();
        }
    }
    public void islemPaneliCikart(int Index)
    {
        Sesler[0].Play();
        DurumuKontrolEt(Index);
        GenelPaneller[0].SetActive(true);
        AktifislemPaneliIndex = Index;
        islemPanelleri[Index].SetActive(true);
        GenelPaneller[1].SetActive(true);
        islemCanvasi.SetActive(false);
        
        // Panel değişince legacy text'i güncelle
        LegacyItemTextGuncelle();
    }
    public void GeriDon()
    {
        Sesler[0].Play();
        
        // Kaydedildi animasyonunu sıfırla
        if (Kaydedildi_Animator.GetBool("ok"))
            Kaydedildi_Animator.SetBool("ok", false);
        
        GenelPaneller[0].SetActive(false);
        islemCanvasi.SetActive(true);
        GenelPaneller[1].SetActive(false);
        islemPanelleri[AktifislemPaneliIndex].SetActive(false);
        
        // Geri dönüldüğünde aktif item varsa "Kaydedildi" mesajı göster
        GeriDondukteSonraDurumKontrol(AktifislemPaneliIndex);
        
        AktifislemPaneliIndex = -1;
    }
    
    void GeriDondukteSonraDurumKontrol(int Bolum)
    {
        if (Bolum == 0) // Şapka
        {
            if (_BellekYonetim.VeriOku_i("AktifSapka") != -1)
            {
                // Aktif şapka varsa "Kaydedildi" mesajını göster
                TextObjeleri[5].text = SatinAlmaText;
            }
            else
            {
                foreach (var item in Sapkalar)
                {
                    item.SetActive(false);
                }
                SapkaText.text = "0";
                TextObjeleri[5].text = SatinAlmaText;
            }
        }
        else if (Bolum == 1) // Sopa
        {
            if (_BellekYonetim.VeriOku_i("AktifSopa") != -1)
            {
                // Aktif sopa varsa "Kaydedildi" mesajını göster
                TextObjeleri[5].text = SatinAlmaText;
            }
            else
            {
                foreach (var item in Sopalar)
                {
                    item.SetActive(false);
                }
                SopaText.text = "0";
                TextObjeleri[5].text = SatinAlmaText;
            }
        }
        else if (Bolum == 2) // Material
        {
            if (_BellekYonetim.VeriOku_i("AktifTema") != -1)
            {
                // Aktif tema varsa "Kaydedildi" mesajını göster
                TextObjeleri[5].text = SatinAlmaText;
            }
            else
            {
                Material[] mats = _Renderer.materials;
                mats[0] = VarsayilanTema;
                _Renderer.materials = mats;
                MaterialText.text = "0";
                TextObjeleri[5].text = SatinAlmaText;
            }
        }
    }
    
    public void AnaMenuyeDon()
    {
        Sesler[0].Play();
        _VeriYonetim.Save(_ItemBilgileri);
        SceneManager.LoadScene(0);
    }
    //----------------*******************-----------------------
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
    
    // TÜM VERİLERİ VARSAYILAN HALINE SIFIRLA
    [ContextMenu("Tüm Verileri Sıfırla")]
    public void TumVerileriSifirla()
    {
        Debug.Log("🔄 TÜM VERİLER SIFIRLANIYOR...");
        
        // 1. Item verilerini sıfırla
        _VeriYonetim.TumVerileriSifirla();
        
        // 2. Aktif seçimleri sıfırla
        _BellekYonetim.VeriKaydet_int("AktifSapka", -1);
        _BellekYonetim.VeriKaydet_int("AktifSopa", -1);
        _BellekYonetim.VeriKaydet_int("AktifTema", -1);
        
        // 3. Diğer game state verilerini varsayılana çevir (opsiyonel)
        _BellekYonetim.VeriKaydet_int("Puan", 10000); // Başlangıç puanı
        
        // 4. Item verilerini yeniden yükle
        _VeriYonetim.Load();
        _ItemBilgileri = _VeriYonetim.ListeyiAktar();
        
        // 5. UI'yi güncelle
        SapkaIndex = -1;
        SopaIndex = -1;
        MaterialIndex = -1;
        
        // 6. Durumları yeniden kontrol et
        DurumuKontrolEt(0, true);
        DurumuKontrolEt(1, true);
        DurumuKontrolEt(2, true);
        
        // 7. Legacy text'i güncelle
        LegacyItemTextGuncelle();
        
        // 8. Puan text'ini güncelle
        PuanText.text = _BellekYonetim.VeriOku_i("Puan").ToString();
        
        Debug.Log("✅ TÜM VERİLER BAŞARILI ŞEKİLDE SIFIRLANDI!");
        Debug.Log("📋 Yeni durum: Tüm index'ler -1, İlk itemler ücretsiz, 10000 puan");
    }
    
    // Sadece aktif seçimleri sıfırla (item verileri korunur)
    [ContextMenu("Aktif Seçimleri Sıfırla")]
    public void AktifSecimleriSifirla()
    {
        Debug.Log("🔄 AKTİF SEÇİMLER SIFIRLANIYOR...");
        
        // Aktif seçimleri sıfırla
        _BellekYonetim.VeriKaydet_int("AktifSapka", -1);
        _BellekYonetim.VeriKaydet_int("AktifSopa", -1);
        _BellekYonetim.VeriKaydet_int("AktifTema", -1);
        
        // UI'yi güncelle
        SapkaIndex = -1;
        SopaIndex = -1;
        MaterialIndex = -1;
        
        // Durumları yeniden kontrol et
        DurumuKontrolEt(0, true);
        DurumuKontrolEt(1, true);
        DurumuKontrolEt(2, true);
        
        // Legacy text'i güncelle
        LegacyItemTextGuncelle();
        
        Debug.Log("✅ AKTİF SEÇİMLER SIFIRLANDI! Artık hiçbir item seçili değil.");
    }
}