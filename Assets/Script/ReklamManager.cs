using UnityEngine;
using GoogleMobileAds.Api;

public class ReklamManager : MonoBehaviour
{
    private InterstitialAd interstitial;
    private RewardedAd rewardedAd;
    private static ReklamManager instance;

    [Header("Otomatik Geçiş Reklamı")]
    public bool otomatikGecisReklami = true;   // Inspector'den aç/kapat
    public float reklamAraligi = 60f;          // Kaç saniyede bir otomatik gösterilecek

    // ✅ Singleton erişimi için static property
    public static ReklamManager Instance
    {
        get
        {
            if (instance == null)
            {
                // Sahnede mevcut bir ReklamManager ara
                instance = FindFirstObjectByType<ReklamManager>();

                // Eğer bulunamadıysa yeni bir GameObject oluştur ve ReklamManager ekle
                if (instance == null)
                {
                    GameObject reklamObj = new GameObject("ReklamManager");
                    instance = reklamObj.AddComponent<ReklamManager>();
                    DontDestroyOnLoad(reklamObj);
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        // Singleton – sahneler arasında kalıcı
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAds();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void InitializeAds()
    {
        MobileAds.Initialize(initStatus => { });
        RequestInterstitial();
        RequestRewardedAd();

        // 🔹 Otomatik reklam sistemi başlat
        if (otomatikGecisReklami)
            InvokeRepeating(nameof(GecisReklamiGoster), reklamAraligi, reklamAraligi);
    }

    // ✅ Geçiş Reklamı
    private void RequestInterstitial()
    {
#if UNITY_ANDROID
        //string adUnitId = "ca-app-pub-3370805830819675/1234395232"; // GERÇEK ID
        string adUnitId = "ca-app-pub-3940256099942544/1033173712";   // TEST ID
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-3940256099942544/4411468910";   // TEST iOS
#else
        string adUnitId = "unexpected_platform";
#endif
        InterstitialAd.Load(adUnitId, new AdRequest(), (ad, error) =>
        {
            if (error == null) interstitial = ad;
        });
    }

    public void GecisReklamiGoster()
    {
        if (interstitial != null && interstitial.CanShowAd())
        {
            interstitial.Show();
            RequestInterstitial(); // Gösterdikten sonra yeniden yükle
        }
        else
        {
            RequestInterstitial();
        }
    }

    // ✅ Ödüllü Reklam
    private void RequestRewardedAd()
    {
#if UNITY_ANDROID
        //string adUnitId = "ca-app-pub-3370805830819675/1892870789"; // GERÇEK ID
        string adUnitId = "ca-app-pub-3940256099942544/5224354917";   // TEST ID
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-3940256099942544/1712485313";   // TEST iOS
#else
        string adUnitId = "unexpected_platform";
#endif
        RewardedAd.Load(adUnitId, new AdRequest(), (ad, error) =>
        {
            if (error == null) rewardedAd = ad;
        });
    }

    public void OdulluReklamGoster(int miktar = 10)
    {
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show(r => AddReward(miktar));
            RequestRewardedAd(); // Gösterildikten sonra tekrar yükle
        }
        else
        {
            RequestRewardedAd();
        }
    }

    // ✅ Ortak ödül fonksiyonu
    private void AddReward(int miktar)
    {
        int mevcut = PlayerPrefs.GetInt("Puan", 0);
        PlayerPrefs.SetInt("Puan", mevcut + miktar);
        PlayerPrefs.Save();
    }
}