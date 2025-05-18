using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAdsChose : MonoBehaviour
{
    [SerializeField] private Button WatchAds;
    [SerializeField] private Button RejectWatch;
    [SerializeField] private TextMeshProUGUI contentAds;

    private Action onAcceptCallback;
    private Action onRejectCallback;

    private void Start()
    {
        // Assign button listeners
        WatchAds.onClick.AddListener(AcceptAds);
        RejectWatch.onClick.AddListener(RejectAds);
    }

    public void InitializeContent(string t, Action acceptCallback = null, Action rejectCallback = null)
    {
        contentAds.text = t;
        onAcceptCallback = acceptCallback;
        onRejectCallback = rejectCallback;
    }

    private void AcceptAds()
    {
        onAcceptCallback?.Invoke(); // Call the callback if it exists
        gameObject.SetActive(false); // Optionally hide the popup
    }

    private void RejectAds()
    {
        onRejectCallback?.Invoke();
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        WatchAds.onClick.RemoveAllListeners();
        RejectWatch.onClick.RemoveAllListeners();
    }
}