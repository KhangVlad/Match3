using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class AdManager : MonoBehaviour
{
    // Static instance that can be accessed from other scripts
    public static AdManager Instance { get; private set; }

    private BannerView bannerView;
    private InterstitialAd interstitialAd;
    private RewardedAd rewardedAd;

    private string appId = "ca-app-pub-9170427956315773~9160858460";
    private string bannerUnitId = "ca-app-pub-9170427956315773/1617394459";

    private string interstitialUnitId = "ca-app-pub-3940256099942544/1033173712";

    // private string rewardedUnitId = "ca-app-pub-3940256099942544/5224354917"; test id
    private string rewardedUnitId = "ca-app-pub-9170427956315773/1617394459";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); 
            return;
        }
    }

    void Start()
    {
        // Initialize the Google Mobile Ads SDK
        MobileAds.Initialize(initStatus =>
        {
            LoadRewardedAd();
        });
    }

    
    #region Banner Methods

    public void RequestBanner()
    {
        bannerView = new BannerView(bannerUnitId, AdSize.Banner, AdPosition.Bottom);
        AdRequest request = new AdRequest();
        bannerView.LoadAd(request);
    }

    #endregion

    #region Interstitial Methods

    public void LoadInterstitialAd()
    {
        // Clean up the old ad before loading a new one
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }

        Debug.Log("Loading interstitial ad...");

        // Create an ad request
        var adRequest = new AdRequest();

        // Load the interstitial with the request
        InterstitialAd.Load(interstitialUnitId, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                // If error is not null, the load request failed
                if (error != null || ad == null)
                {
                    Debug.LogError($"Interstitial ad failed to load: {error?.GetMessage()}");
                    return;
                }

                Debug.Log("Interstitial ad loaded successfully");
                interstitialAd = ad;

                // Register for ad events
                RegisterInterstitialEvents(interstitialAd);
            });
    }

    private void RegisterInterstitialEvents(InterstitialAd ad)
    {
        // Raised when an impression is recorded for an ad
        ad.OnAdImpressionRecorded += () => { Debug.Log("Interstitial ad impression recorded"); };

        // Raised when a click is recorded for an ad
        ad.OnAdClicked += () => { Debug.Log("Interstitial ad was clicked"); };

        // Raised when an ad opened full screen content
        ad.OnAdFullScreenContentOpened += () => { Debug.Log("Interstitial ad full screen content opened"); };

        // Raised when the ad closed full screen content
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Interstitial ad full screen content closed");
            LoadInterstitialAd(); // Load the next ad
        };

        // Raised when the ad failed to open full screen content
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError($"Interstitial ad failed to open full screen content: {error.GetMessage()}");
            LoadInterstitialAd(); // Load the next ad
        };
    }

    public void ShowInterstitial()
    {
        if (interstitialAd != null)
        {
            interstitialAd.Show();
        }
        else
        {
            Debug.Log("Interstitial ad not ready yet");
            LoadInterstitialAd();
        }
    }

    #endregion

    #region Rewarded Ad Methods

    public void LoadRewardedAd()
    {
        // Clean up the old ad before loading a new one
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        Debug.Log("Loading rewarded ad...");

        // Create an ad request
        var adRequest = new AdRequest();

        // Load the rewarded ad with the request
        RewardedAd.Load(rewardedUnitId, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                // If error is not null, the load request failed
                if (error != null || ad == null)
                {
                    Debug.LogError($"Rewarded ad failed to load: {error?.GetMessage()}");
                    return;
                }

                Debug.Log("Rewarded ad loaded successfully");
                rewardedAd = ad;

                // Register for ad events
                RegisterRewardedAdEvents(rewardedAd);
            });
    }

    private void RegisterRewardedAdEvents(RewardedAd ad)
    {
        // Raised when an impression is recorded for an ad
        ad.OnAdImpressionRecorded += () => { Debug.Log("Rewarded ad impression recorded"); };

        // Raised when a click is recorded for an ad
        ad.OnAdClicked += () => { Debug.Log("Rewarded ad was clicked"); };

        // Raised when an ad opened full screen content
        ad.OnAdFullScreenContentOpened += () => { Debug.Log("Rewarded ad full screen content opened"); };

        // Raised when the ad closed full screen content
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded ad full screen content closed");
            LoadRewardedAd(); // Load the next ad
        };

        // Raised when the ad failed to open full screen content
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError($"Rewarded ad failed to open full screen content: {error.GetMessage()}");
            LoadRewardedAd(); // Load the next ad
        };
    }

    public void ShowRewardedAd(Action complete)
    {
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show((Reward reward) =>
            {
                Debug.Log($"User rewarded with {reward.Amount} {reward.Type}");
                complete?.Invoke();
            });
        }
        else
        {
            Debug.Log("Rewarded ad not ready yet");
            LoadRewardedAd();
        }
    }

    #endregion

    void OnDestroy()
    {
        // Clean up when the GameObject is destroyed
        if (bannerView != null)
        {
            bannerView.Destroy();
        }

        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
        }

        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
        }
    }
}

