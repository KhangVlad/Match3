using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Match3.Shares;

public class UIPopupManager : MonoBehaviour
{
    public static UIPopupManager Instance { get; private set; }

    private Canvas canvas;
    [SerializeField] private UIPopupTemplate warningPrefab;
    [SerializeField] private UIPopupTemplate successPrefab;
    [SerializeField] private UIPopupTemplate errorPrefab;
    [SerializeField] private UIPopupTemplate infoPrefab;

    // Pools for reusing popups
    private Queue<UIPopupTemplate> warningPool = new Queue<UIPopupTemplate>();
    private Queue<UIPopupTemplate> successPool = new Queue<UIPopupTemplate>();
    private Queue<UIPopupTemplate> errorPool = new Queue<UIPopupTemplate>();
    private Queue<UIPopupTemplate> infoPool = new Queue<UIPopupTemplate>();

    // Track active popups
    private List<UIPopupTemplate> activePopups = new List<UIPopupTemplate>();

    [Header("Animation Settings")] [SerializeField]
    private float popupDuration = 2f;

    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float scaleInDuration = 0.3f;
    [SerializeField] private float scaleOutDuration = 0.2f;
    [SerializeField] private Ease scaleInEase = Ease.OutBack;
    [SerializeField] private Ease scaleOutEase = Ease.InBack;

    [Header("Movement Settings")] [SerializeField]
    private float moveUpDistance = 50f; // How far the popup moves up

    [SerializeField] private float moveUpDuration = 0.8f; // How long it takes to move up
    [SerializeField] private Ease moveUpEase = Ease.OutQuad;

    [Header("Pool Settings")] [SerializeField]
    private int initialPoolSize = 3; // Number of popups to pre-create

    [SerializeField] private bool expandPoolIfNeeded = true;


    [SerializeField] private UIPurchasePopup EncouragePackPrefab;
    [SerializeField] private UIAdsChose _uiAdsChose;


    private void Awake()
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

        canvas = GetComponent<Canvas>();

        // Initialize the pools
        InitializePool(warningPrefab, warningPool);
        InitializePool(successPrefab, successPool);
        InitializePool(errorPrefab, errorPool);
        InitializePool(infoPrefab, infoPool);
    }

    private void Start()
    {
    }

    private void CheckLoseStreak()
    {
        if (!UserManager.Instance.UserData.IsBuyWelcomePack && UserManager.Instance.UserData.LoseStreak >= 2)
        {
            ShowEncouragePackPopup();
            UserManager.Instance.UserData.LoseStreak = 0;
        }
    }

    private void OnEnable()
    {
        CheckLoseStreak();
    }


    private void InitializePool(UIPopupTemplate prefab, Queue<UIPopupTemplate> pool)
    {
        if (prefab == null) return;

        for (int i = 0; i < initialPoolSize; i++)
        {
            UIPopupTemplate popup = CreatePopupInstance(prefab);
            pool.Enqueue(popup);
        }
    }

    private UIPopupTemplate CreatePopupInstance(UIPopupTemplate prefab)
    {
        UIPopupTemplate instance = Instantiate(prefab, canvas.transform);
        instance.gameObject.SetActive(false);

        // Ensure it has a CanvasGroup
        if (instance.GetComponent<CanvasGroup>() == null)
        {
            instance.gameObject.AddComponent<CanvasGroup>();
        }

        return instance;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            ShowEncouragePackPopup();
        }
    }

    public void ShowEncouragePackPopup()
    {
        UIPurchasePopup instance = Instantiate(EncouragePackPrefab, canvas.transform);
        instance.Initialize(ShopManager.Instance.GetShopPackById("welcome"));
    }

    public void ShowAdsChose(Action accept, Action reject, string contentAds = "Double Reward !!!")
    {
        UIAdsChose instance = Instantiate(_uiAdsChose, canvas.transform);
        instance.InitializeContent(contentAds, accept, reject);
    }

    public void ShowWarningPopup(string text)
    {
        ShowFromPool(warningPrefab, warningPool, text);
    }

    public void ShowSuccessPopup(string text)
    {
        ShowFromPool(successPrefab, successPool, text);
    }

    public void ShowErrorPopup(string text)
    {
        ShowFromPool(errorPrefab, errorPool, text);
    }

    public void ShowInfoPopup(string text)
    {
        ShowFromPool(infoPrefab, infoPool, text);
    }

    private void ShowFromPool(UIPopupTemplate prefab, Queue<UIPopupTemplate> pool, string text)
    {
        if (prefab == null)
        {
            Debug.LogError("Popup prefab is null!");
            return;
        }

        // Get a popup from the pool or create a new one
        UIPopupTemplate popupInstance;

        if (pool.Count > 0)
        {
            popupInstance = pool.Dequeue();
        }
        else if (expandPoolIfNeeded)
        {
            popupInstance = CreatePopupInstance(prefab);
        }
        else
        {
            return;
        }

        activePopups.Add(popupInstance);
        RectTransform rectTransform = popupInstance.GetComponent<RectTransform>();
        PositionPopupCenter(rectTransform);
        ShowPopup(popupInstance, text, pool);
    }

    private void ShowPopup(UIPopupTemplate popupTemplate, string text, Queue<UIPopupTemplate> returnPool)
    {
        if (popupTemplate == null) return;

        popupTemplate.transform.DOKill();
        popupTemplate.SetText(text);
        popupTemplate.gameObject.SetActive(true);
        CanvasGroup canvasGroup = popupTemplate.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        popupTemplate.transform.localScale = Vector3.zero;

        Vector2 initialPosition = popupTemplate.GetComponent<RectTransform>().anchoredPosition;
        Vector2 targetPosition = initialPosition + new Vector2(0, moveUpDistance);
        Sequence popupSequence = DOTween.Sequence();
        popupSequence.Append(canvasGroup.DOFade(1f, fadeInDuration));
        popupSequence.Join(popupTemplate.transform.DOScale(1f, scaleInDuration).SetEase(scaleInEase));
        popupSequence.Append(popupTemplate.GetComponent<RectTransform>()
            .DOAnchorPos(targetPosition, moveUpDuration)
            .SetEase(moveUpEase));

        popupSequence.AppendInterval(popupDuration - moveUpDuration);
        popupSequence.Append(canvasGroup.DOFade(0f, fadeOutDuration));
        popupSequence.Join(popupTemplate.transform.DOScale(0f, scaleOutDuration).SetEase(scaleOutEase));

        popupSequence.OnComplete(() =>
        {
            popupTemplate.GetComponent<RectTransform>().anchoredPosition = initialPosition;
            popupTemplate.gameObject.SetActive(false);
            activePopups.Remove(popupTemplate);
            returnPool.Enqueue(popupTemplate);
        });

        popupSequence.Play();
    }


    public void PositionPopupCenter(RectTransform popupRectTransform)
    {
        if (popupRectTransform == null) return;

        popupRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        popupRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        popupRectTransform.pivot = new Vector2(0.5f, 0.5f);
        popupRectTransform.anchoredPosition = Vector2.zero;
    }

    private void OnDestroy()
    {
        foreach (var popup in activePopups)
        {
            if (popup != null)
            {
                popup.transform.DOKill();
            }
        }
    }
}