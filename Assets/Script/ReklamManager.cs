using UnityEngine;
using GoogleMobileAds.Api;

public class ReklamManager : MonoBehaviour
{
    private InterstitialAd interstitial;
    private RewardedAd rewardedAd;

    private static ReklamManager instance;

    // ⏱️ Reklam zamanlayıcı
    private float reklamTimer = 0f;
    public float reklamAraligi = 60f; // saniye cinsinden (60 = 1 dakika)

    private void Awake()
    {
        // Singleton – sahneler arasında kalıcı
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        MobileAds.Initialize(initStatus => { });

        RequestInterstitial();
        RequestRewardedAd();
    }

    private void Update()
    {
        // Süreyi say
        reklamTimer += Time.deltaTime;

        if (reklamTimer >= reklamAraligi)
        {
            GecisReklamiGoster(); // Reklam çağır
            reklamTimer = 0f;     // Sayaç sıfırla
        }
    }

    // ✅ Geçiş Reklamı
    private void RequestInterstitial()
    {
#if UNITY_ANDROID
        // string adUnitId = "ca-app-pub-3370805830819675/1234395232"; // gerçek Geçiş Reklamı
        string adUnitId = "ca-app-pub-3940256099942544/1033173712"; // TEST Geçiş Reklamı
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-3940256099942544/4411468910"; // TEST iOS
#else
        string adUnitId = "unexpected_platform";
#endif
        InterstitialAd.Load(adUnitId, new AdRequest(), (ad, error) =>
        {
            if (error == null) interstitial = ad;
            else Debug.LogWarning("❌ Geçiş reklamı yüklenemedi: " + error);
        });
    }

    // 👇 senin istediğin isim: GecisReklamiGoster
    public void GecisReklamiGoster()
    {
        if (interstitial != null && interstitial.CanShowAd())
        {
            interstitial.Show();
            RequestInterstitial(); // gösterdikten sonra yenisini yükle
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
        // string adUnitId = "ca-app-pub-3370805830819675/1892870789"; // gerçek Ödüllü Reklam
        string adUnitId = "ca-app-pub-3940256099942544/5224354917"; // TEST Ödüllü Reklam
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-3940256099942544/1712485313"; // TEST iOS
#else
        string adUnitId = "unexpected_platform";
#endif
        RewardedAd.Load(adUnitId, new AdRequest(), (ad, error) =>
        {
            if (error == null) rewardedAd = ad;
            else Debug.LogWarning("❌ Ödüllü reklam yüklenemedi: " + error);
        });
    }

    // 👇 senin istediğin isim: OdulluReklamGoster
    public void OdulluReklamGoster(int miktar = 10)
    {
        if (rewardedAd != null && rewardedAd.CanShowAd())
            rewardedAd.Show(r => AddReward(miktar));
        else
            RequestRewardedAd();
    }

    // ✅ Ortak ödül fonksiyonu
    private void AddReward(int miktar)
    {
        int mevcut = PlayerPrefs.GetInt("Puan", 0);
        PlayerPrefs.SetInt("Puan", mevcut + miktar);
        PlayerPrefs.Save();

        Debug.Log($"🎁 Oyuncuya +{miktar} puan verildi! Yeni toplam: {mevcut + miktar}");
    }
}
