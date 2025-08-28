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

        public string VeriOku_s(string Key) => PlayerPrefs.GetString(Key);
        public int VeriOku_i(string Key) => PlayerPrefs.GetInt(Key);
        public float VeriOku_f(string Key) => PlayerPrefs.GetFloat(Key);

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
        List<ItemBilgileri> _ItemicListe;

        public void Save(List<ItemBilgileri> _ItemBilgileri)
        {
            if (_ItemBilgileri == null || _ItemBilgileri.Count == 0) return;

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.OpenWrite(Application.persistentDataPath + "/ItemVerileri.gd");
            bf.Serialize(file, _ItemBilgileri);
            file.Close();
        }

        public void Load()
        {
            if (File.Exists(Application.persistentDataPath + "/ItemVerileri.gd"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + "/ItemVerileri.gd", FileMode.Open);
                _ItemicListe = (List<ItemBilgileri>)bf.Deserialize(file);
                file.Close();

                if (_ItemicListe == null || _ItemicListe.Count < 19)
                    VarsayilanItemVerileriOlustur();
            }
            else
            {
                VarsayilanItemVerileriOlustur();
            }
        }

        public void VarsayilanItemVerileriOlustur()
        {
            _ItemicListe = new List<ItemBilgileri>();

            for (int i = 0; i < 5; i++)
                _ItemicListe.Add(new ItemBilgileri { GrupIndex = 0, Item_Index = i, Item_Ad = (i + 1).ToString(), Puan = (i + 1) * 100, SatinAlmaDurumu = i == 0 });

            for (int i = 0; i < 9; i++)
                _ItemicListe.Add(new ItemBilgileri { GrupIndex = 1, Item_Index = i, Item_Ad = (i + 1).ToString(), Puan = (i + 1) * 150, SatinAlmaDurumu = i == 0 });

            for (int i = 0; i < 5; i++)
                _ItemicListe.Add(new ItemBilgileri { GrupIndex = 2, Item_Index = i, Item_Ad = (i + 1).ToString(), Puan = (i + 1) * 200, SatinAlmaDurumu = i == 0 });
        }

        public void TumVerileriSifirla()
        {
            _ItemicListe = new List<ItemBilgileri>();
            VarsayilanItemVerileriOlustur();

            string dosyaYolu = Application.persistentDataPath + "/ItemVerileri.gd";
            if (File.Exists(dosyaYolu))
                File.Delete(dosyaYolu);

            Save(_ItemicListe);
        }

        public bool VeriGeçerliliğiKontrolEt()
        {
            if (_ItemicListe == null || _ItemicListe.Count < 19)
                return false;

            for (int i = 0; i < _ItemicListe.Count; i++)
            {
                if (_ItemicListe[i] == null || string.IsNullOrEmpty(_ItemicListe[i].Item_Ad))
                {
                    if (i <= 4) _ItemicListe[i].Item_Ad = (i + 1).ToString();
                    else if (i <= 13) _ItemicListe[i].Item_Ad = (i - 4).ToString();
                    else _ItemicListe[i].Item_Ad = (i - 13).ToString();
                }
            }
            Save(_ItemicListe);
            return true;
        }

        public List<ItemBilgileri> ListeyiAktar()
        {
            if (_ItemicListe == null)
                VarsayilanItemVerileriOlustur();

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

        List<DilVerileriAnaObje> _DilVerileriicListe;

        public void Dil_Load()
        {
            if (File.Exists(Application.persistentDataPath + "/DilVerileri.gd"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + "/DilVerileri.gd", FileMode.Open);
                _DilVerileriicListe = (List<DilVerileriAnaObje>)bf.Deserialize(file);
                file.Close();
            }
        }

        public List<DilVerileriAnaObje> DilVerileriListeyiAktar() => _DilVerileriicListe;
    }

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
}
