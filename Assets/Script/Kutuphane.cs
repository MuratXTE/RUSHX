using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using GoogleMobileAds.Api;
using Unity.Jobs;


namespace Murat
{
    public class Matematiksel_islemler 
    {
        public void Carpma(int GelenSayi, List<GameObject> Karakterler, Transform Pozisyon, List<GameObject> OlusmaEfektleri)
        {
            int DonguSayisi = (GameManager.AnlikKarakterSayisi * GelenSayi) - GameManager.AnlikKarakterSayisi;
            int sayi = 0;
            foreach (var item in Karakterler)
            {

                if (sayi < DonguSayisi)
                {
                    if (!item.activeInHierarchy)
                    {
                        foreach (var item2 in OlusmaEfektleri)
                        {
                            if (!item2.activeInHierarchy)
                            {

                                item2.SetActive(true);
                                item2.transform.position = Pozisyon.position;
                                item2.GetComponent<ParticleSystem>().Play();
                                item2.GetComponent<AudioSource>().Play();
                                break;
                            }
                        }

                        item.transform.position = Pozisyon.position;
                        item.SetActive(true);
                        sayi++;

                    }

                }
                else
                {
                    sayi = 0;
                    break;
                }

            }
            GameManager.AnlikKarakterSayisi *= GelenSayi;
        }
        public void Toplama(int GelenSayi, List<GameObject> Karakterler, Transform Pozisyon, List<GameObject> OlusmaEfektleri)
        {
            int sayi2 = 0;
            foreach (var item in Karakterler)
            {

                if (sayi2 < GelenSayi)
                {
                    if (!item.activeInHierarchy)
                    {
                        foreach (var item2 in OlusmaEfektleri)
                        {
                            if (!item2.activeInHierarchy)
                            {
                                item2.SetActive(true);
                                item2.transform.position = Pozisyon.position;
                                item2.GetComponent<ParticleSystem>().Play();
                                item2.GetComponent<AudioSource>().Play();
                                break;
                            }
                        }

                        item.transform.position = Pozisyon.position;
                        item.SetActive(true);
                        sayi2++;

                    }

                }
                else
                {
                    sayi2 = 0;
                    break;
                }

            }
            GameManager.AnlikKarakterSayisi += GelenSayi;
        }
        public void Cikartma(int GelenSayi, List<GameObject> Karakterler, List<GameObject> YokOlmaEfektleri)
        {
            if (GameManager.AnlikKarakterSayisi < GelenSayi)
            {
                foreach (var item in Karakterler)
                {

                    foreach (var item2 in YokOlmaEfektleri)
                    {
                        if (!item2.activeInHierarchy)
                        {
                            Vector3 yeniPoz = new Vector3(item.transform.position.x, item.transform.position.y, item.transform.position.z);

                            item2.SetActive(true);
                            item2.transform.position = yeniPoz;
                            item2.GetComponent<ParticleSystem>().Play();
                            item2.GetComponent<AudioSource>().Play();
                            break;
                        }
                    }

                    item.transform.position = Vector3.zero;
                    item.SetActive(false);
                }
                GameManager.AnlikKarakterSayisi = 1;
            }
            else
            {
                int sayi3 = 0;
                foreach (var item in Karakterler)
                {

                    if (sayi3 != GelenSayi)
                    {
                        if (item.activeInHierarchy)
                        {

                            foreach (var item2 in YokOlmaEfektleri)
                            {
                                if (!item2.activeInHierarchy)
                                {

                                    Vector3 yeniPoz = new Vector3(item.transform.position.x, item.transform.position.y, item.transform.position.z);
                                    item2.SetActive(true);
                                    item2.transform.position = yeniPoz;
                                    item2.GetComponent<ParticleSystem>().Play();
                                    item2.GetComponent<AudioSource>().Play();
                                    break;
                                }
                            }



                            item.transform.position = Vector3.zero;
                            item.SetActive(false);
                            sayi3++;

                        }

                    }
                    else
                    {
                        sayi3 = 0;
                        break;
                    }

                }
                GameManager.AnlikKarakterSayisi -= GelenSayi;
            }
        }
        public void Bolme(int GelenSayi, List<GameObject> Karakterler, List<GameObject> YokOlmaEfektleri)
        {

            if (GameManager.AnlikKarakterSayisi <= GelenSayi)
            {
                foreach (var item in Karakterler)
                {
                    foreach (var item2 in YokOlmaEfektleri)
                    {
                        if (!item2.activeInHierarchy)
                        {

                            Vector3 yeniPoz = new Vector3(item.transform.position.x, item.transform.position.y, item.transform.position.z);
                            item2.SetActive(true);
                            item2.transform.position = yeniPoz;
                            item2.GetComponent<ParticleSystem>().Play();
                            item2.GetComponent<AudioSource>().Play();
                            break;
                        }
                    }

                    item.transform.position = Vector3.zero;
                    item.SetActive(false);
                }
                GameManager.AnlikKarakterSayisi = 1;
            }
            else
            {
                int bolen = GameManager.AnlikKarakterSayisi / GelenSayi;

                int sayi3 = 0;
                foreach (var item in Karakterler)
                {

                    if (sayi3 != bolen)
                    {
                        if (item.activeInHierarchy)
                        {

                            foreach (var item2 in YokOlmaEfektleri)
                            {
                                if (!item2.activeInHierarchy)
                                {

                                    Vector3 yeniPoz = new Vector3(item.transform.position.x, item.transform.position.y, item.transform.position.z);
                                    item2.SetActive(true);
                                    item2.transform.position = yeniPoz;
                                    item2.GetComponent<ParticleSystem>().Play();
                                    item2.GetComponent<AudioSource>().Play();
                                    break;
                                }
                            }

                            item.transform.position = Vector3.zero;
                            item.SetActive(false);
                            sayi3++;

                        }

                    }
                    else
                    {
                        sayi3 = 0;
                        break;
                    }

                }
                if (GameManager.AnlikKarakterSayisi % GelenSayi == 0)
                {
                    GameManager.AnlikKarakterSayisi /= GelenSayi;
                }
                else if (GameManager.AnlikKarakterSayisi % GelenSayi == 1)
                {
                    GameManager.AnlikKarakterSayisi /= GelenSayi;
                    GameManager.AnlikKarakterSayisi++;
                }
                else if (GameManager.AnlikKarakterSayisi % GelenSayi == 2)
                {
                    GameManager.AnlikKarakterSayisi /= GelenSayi;
                    GameManager.AnlikKarakterSayisi += 2;
                }
            }
        }
    
    }

    public class BellekYonetim
    {
        public void VeriKaydet_string(string Key, string value)
        {
            PlayerPrefs.SetString(Key, value);
            PlayerPrefs.Save();
        }
        public void VeriKaydet_int(string Key, int value)
        {
            PlayerPrefs.SetInt(Key, value);
            PlayerPrefs.Save();
        }
        public void VeriKaydet_float(string Key, float value)
        {
            PlayerPrefs.SetFloat(Key, value);
            PlayerPrefs.Save();
        }

        public string VeriOku_s(string Key)
        {
            return PlayerPrefs.GetString(Key);
        }
        public int VeriOku_i(string Key)
        {
            return PlayerPrefs.GetInt(Key);
        }
        public float VeriOku_f(string Key)
        {
            return PlayerPrefs.GetFloat(Key);
        }

        public void KontrolEtVeTanimla()
        {
            if (!PlayerPrefs.HasKey("SonLevel"))
            {
                PlayerPrefs.SetInt("SonLevel", 5);
                PlayerPrefs.SetInt("Puan", 100);
                PlayerPrefs.SetInt("AktifSapka", -1);
                PlayerPrefs.SetInt("AktifSopa", -1);
                PlayerPrefs.SetInt("AktifTema", -1);
                PlayerPrefs.SetFloat("MenuSes", 1);
                PlayerPrefs.SetFloat("MenuFx", 1);
                PlayerPrefs.SetFloat("OyunSes", 1);
                PlayerPrefs.SetString("Dil", "EN");
                PlayerPrefs.SetInt("Gecisreklamisayisi", 1);
            }
        }
    }

    [Serializable]
    public class ItemBilgileri
    {
        public int GrupIndex;
        public int Item_Index;
        public string Item_Ad;
        public int Puan;
        public bool SatinAlmaDurumu;
    }
    public class VeriYonetimi
    {

        public void Save(List<ItemBilgileri> _ItemBilgileri)
        {
            try
            {
                if (_ItemBilgileri == null || _ItemBilgileri.Count == 0)
                {
                    Debug.LogError("Kaydedilecek item verisi boş!");
                    return;
                }
                
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.OpenWrite(Application.persistentDataPath + "/ItemVerileri.gd");
                bf.Serialize(file, _ItemBilgileri);
                file.Close();
                
                Debug.Log($"✅ {_ItemBilgileri.Count} item başarıyla kaydedildi.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Veri kaydedilirken hata: {e.Message}");
            }
        }

        List<ItemBilgileri> _ItemicListe;
        public void Load()
        {
            if (File.Exists(Application.persistentDataPath + "/ItemVerileri.gd"))
            {
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    FileStream file = File.Open(Application.persistentDataPath + "/ItemVerileri.gd", FileMode.Open);
                    _ItemicListe = (List<ItemBilgileri>)bf.Deserialize(file); 
                    file.Close();
                    
                    // Yüklenen verinin geçerliliğini kontrol et
                    if (_ItemicListe == null || _ItemicListe.Count < 19) // Minimum 19 item gerekli (5 şapka + 9 sopa + 5 material)
                    {
                        Debug.LogWarning($"Yüklenen item verisi yetersiz. Count: {(_ItemicListe?.Count ?? 0)}. Varsayılan veriler oluşturuluyor.");
                        VarsayilanItemVerileriOlustur();
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Item verileri yüklenirken hata: {e.Message}. Varsayılan veriler oluşturuluyor.");
                    VarsayilanItemVerileriOlustur();
                }
            }
            else
            {
                Debug.Log("Item verileri dosyası bulunamadı. Varsayılan veriler oluşturuluyor.");
                VarsayilanItemVerileriOlustur();
            }
        }
        
        public void VarsayilanItemVerileriOlustur()
        {
            _ItemicListe = new List<ItemBilgileri>();
            
            // 5 Şapka (0-4)
            for (int i = 0; i < 5; i++)
            {
                _ItemicListe.Add(new ItemBilgileri
                {
                    GrupIndex = 0,
                    Item_Index = i,
                    Item_Ad = (i + 1).ToString(), // Sadece sayı: 1, 2, 3, 4, 5
                    Puan = (i + 1) * 100,
                    SatinAlmaDurumu = i == 0 // İlk şapka ücretsiz
                });
            }
            
            // 9 Sopa (5-13)  
            for (int i = 0; i < 9; i++)
            {
                _ItemicListe.Add(new ItemBilgileri
                {
                    GrupIndex = 1,
                    Item_Index = i,
                    Item_Ad = (i + 1).ToString(), // Sadece sayı: 1, 2, 3, ..., 9
                    Puan = (i + 1) * 150,
                    SatinAlmaDurumu = i == 0 // İlk sopa ücretsiz
                });
            }
            
            // 5 Material (14-18)
            for (int i = 0; i < 5; i++)
            {
                _ItemicListe.Add(new ItemBilgileri
                {
                    GrupIndex = 2,
                    Item_Index = i,
                    Item_Ad = (i + 1).ToString(), // Sadece sayı: 1, 2, 3, 4, 5
                    Puan = (i + 1) * 200,
                    SatinAlmaDurumu = i == 0 // İlk tema ücretsiz
                });
            }
            
            Debug.Log($"Varsayılan item verileri oluşturuldu. Toplam: {_ItemicListe.Count}");
        }
        
        // TÜM VERİLERİ SIFIRLA - Emergency reset
        public void TumVerileriSifirla()
        {
            try
            {
                // Item verilerini tamamen sıfırla
                _ItemicListe = new List<ItemBilgileri>();
                VarsayilanItemVerileriOlustur();
                
                // Dosyayı sil ve yeniden oluştur
                string dosyaYolu = Application.persistentDataPath + "/ItemVerileri.gd";
                if (File.Exists(dosyaYolu))
                {
                    File.Delete(dosyaYolu);
                    Debug.Log("🗑️ Eski item verisi dosyası silindi.");
                }
                
                // Yeni varsayılan veriyi kaydet
                Save(_ItemicListe);
                
                Debug.Log("🔄 TÜM VERİLER SIFIRLANDI! Varsayılan durum: İlk itemler ücretsiz, diğerleri ücretli.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Veri sıfırlama hatası: {e.Message}");
            }
        }
        
        public bool VeriGeçerliliğiKontrolEt()
        {
            if (_ItemicListe == null)
            {
                Debug.LogError("ItemicListe null!");
                return false;
            }
            
            if (_ItemicListe.Count < 19)
            {
                Debug.LogError($"Yetersiz item sayısı: {_ItemicListe.Count}. Minimum 19 gerekli.");
                return false;
            }
            
            bool veriHataliMi = false;
            
            // Her item'in temel verilerini kontrol et
            for (int i = 0; i < _ItemicListe.Count; i++)
            {
                if (_ItemicListe[i] == null)
                {
                    Debug.LogError($"Item {i} null!");
                    veriHataliMi = true;
                }
                else if (string.IsNullOrEmpty(_ItemicListe[i].Item_Ad))
                {
                    Debug.LogError($"Item {i} adı boş!");
                    veriHataliMi = true;
                    
                    // Boş adı otomatik düzelt
                    if (i <= 4) // Şapka
                        _ItemicListe[i].Item_Ad = (i + 1).ToString();
                    else if (i <= 13) // Sopa  
                        _ItemicListe[i].Item_Ad = (i - 4).ToString();
                    else // Material
                        _ItemicListe[i].Item_Ad = (i - 13).ToString();
                        
                    Debug.Log($"✅ Item {i} adı otomatik düzeltildi: {_ItemicListe[i].Item_Ad}");
                }
            }
            
            if (veriHataliMi)
            {
                Debug.LogWarning("⚠️ Bazı veriler otomatik düzeltildi. Veriyi yeniden kaydediyoruz...");
                // Düzeltilmiş veriyi kaydet
                Save(_ItemicListe);
                return true; // Düzeltildi, devam edebilir
            }
            
            Debug.Log("✅ Tüm item verileri geçerli.");
            return true;
        }
        public List<ItemBilgileri> ListeyiAktar()
        {
            // Null kontrolü
            if (_ItemicListe == null)
            {
                Debug.LogWarning("ItemicListe null. Varsayılan veriler oluşturuluyor.");
                VarsayilanItemVerileriOlustur();
            }
            
            return _ItemicListe;
        }
        public void ilkKurulumDosyaOlusturma(List<ItemBilgileri> _ItemBilgileri, List<DilVerileriAnaObje> _DilVerileri)
        {
            if (!File.Exists(Application.persistentDataPath + "/ItemVerileri.gd"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Create(Application.persistentDataPath + "/ItemVerileri.gd");
                bf.Serialize(file, _ItemBilgileri);
                file.Close();
            }

            if (!File.Exists(Application.persistentDataPath + "/DilVerileri.gd"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Create(Application.persistentDataPath + "/DilVerileri.gd");
                bf.Serialize(file, _DilVerileri);
                file.Close();
            }
        }


        //--------------------------------
        List<DilVerileriAnaObje> _DilVerileriicListe;
        public void Dil_Load()
        {
            if (File.Exists(Application.persistentDataPath + "/DilVerileri.gd"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + "/DilVerileri.gd", FileMode.Open);
                _DilVerileriicListe = (List<DilVerileriAnaObje >)bf.Deserialize(file);
                file.Close();
            }
        }
        public List<DilVerileriAnaObje> DilVerileriListeyiAktar()
        {
            return _DilVerileriicListe;

        }
    }


    //-------------D�L Y�NET�M�------------------

    [Serializable]
    public class DilVerileriAnaObje
    {
        public List<DilVerileri_TR> _DilVerileri_EN = new List<DilVerileri_TR>();
        public List<DilVerileri_TR> _DilVerileri_TR = new List<DilVerileri_TR>();
        public List<DilVerileri_TR> _DilVerileri_DE = new List<DilVerileri_TR>();
    }
    [Serializable]
    public class DilVerileri_TR
    {
        public string Metin;
    }

    //-------------REKLAM Y�NET�M�------------------

    public class ReklamYonetim
    {
        private InterstitialAd interstitial;
        private RewardedAd _RewardedAd;
        //GE��� REKLAMI
        public void RequestInterstitial()
        {
            string AdUnitId;
#if UNITY_ANDROID
            AdUnitId = "ca-app-pub-3940256099942544/1033173712";    //TEST REKLAM ID
            //AdUnitId = "ca-app-pub-3370805830819675/1234395232"; //GER�EK REKLAM ID
#elif UNITY_IPHONE
        AdUnitId = "ca-app-pub-3940256099942544/4411468910";
#else
        AdUnitId = "unexpected_platform";
#endif
            // Reklam iste�i olu�tur
            AdRequest request = new AdRequest();
            // Yeni interstitial reklam y�kle
            InterstitialAd.Load(AdUnitId, request, (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    return;
                }
                interstitial = ad;
                interstitial.OnAdFullScreenContentClosed += GecisReklamiKapatildi;
            });
        }
        void GecisReklamiKapatildi()
        {
            if (interstitial != null)
            {
                interstitial.Destroy();
                interstitial = null;
            }
            RequestInterstitial();
        }
        public void GecisReklamiGoster()
        {
            if (PlayerPrefs.GetInt("Gecisreklamisayisi") == 2)
            {
                if (interstitial != null && interstitial.CanShowAd())
                {
                    PlayerPrefs.SetInt("Gecisreklamisayisi", 1);
                    interstitial.Show();
                }
                else
                {
                    if (interstitial != null)
                    {
                        interstitial.Destroy();
                        interstitial = null;
                    }
                    RequestInterstitial();
                }
            }
            else
            {
                PlayerPrefs.SetInt("Gecisreklamisayisi", PlayerPrefs.GetInt("Gecisreklamisayisi") + 1);
            }
        }
        //�D�LL� REKLAM
        public void RequestRewardedAd()
        {
            string AdUnitId;
#if UNITY_ANDROID
            AdUnitId = "ca-app-pub-3940256099942544/5224354917";     //TEST REKLAM ID
            //AdUnitId = "ca-app-pub-3370805830819675/1892870789";   //GER�EK REKLAM ID
#elif UNITY_IPHONE
        AdUnitId = "ca-app-pub-3940256099942544/1712485313";
#else
        AdUnitId = "unexpected_platform";
#endif
            AdRequest request = new AdRequest();
            RewardedAd.Load(AdUnitId, request, (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    return;
                }
                _RewardedAd = ad;
                _RewardedAd.OnAdFullScreenContentClosed += () => OdulluReklamKapatildi();
            });
        }
        private void OdulluReklamTamamlandi(Reward e)
        {
            string type = e.Type;
            double amount = e.Amount;
            Debug.Log("�d�l Al�ns�n : " + type + "----" + amount);
        }
        private void OdulluReklamKapatildi()
        {
            Debug.Log("Reklam Kapat�ld�");
            RequestRewardedAd();
        }

        public void OdulluReklamGoster()
        {
            if (_RewardedAd != null && _RewardedAd.CanShowAd())
                _RewardedAd.Show(OdulluReklamTamamlandi);
        }
    }
}



