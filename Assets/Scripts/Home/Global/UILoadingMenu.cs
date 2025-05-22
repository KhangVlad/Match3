using Match3;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using TMPro;
using Match3.Shares;

#if !UNITY_WEBGL
public class UILoadingMenu : MonoBehaviour
{
    [Header("UI References")] [SerializeField]
    private Button homeButton;

    [SerializeField] private Slider progressSlider;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private UIComic comic;

    [Header("Loading Settings")] [SerializeField]
    private float progressSpeed = 1f;

    [SerializeField] private float initialProgress = 0f;

    private float _currentProgress;
    private float _targetProgress;
    private bool _isLoading = true;

    // Path constants to avoid typos and make changes easier
    private const string BACKGROUND_PATH = "Sprites/Backgrounds/";
    private const string MORNING_BG_1 = BACKGROUND_PATH + "bg_morning_1";
    private const string MORNING_BG_2 = BACKGROUND_PATH + "bg_morning_2";
    private const string EVENING_BG_1 = BACKGROUND_PATH + "bg_evening_1";
    private const string EVENING_BG_2 = BACKGROUND_PATH + "bg_evening_2";


    [SerializeField] private Transform UIOnBoard; //new user click on quest or ch play
    [SerializeField] private Button _questSignInBtn;
    [SerializeField] private Button _chPlayBtn;
    [SerializeField] private Button _google;
    
    private void Awake()
    {
        _currentProgress = initialProgress;
        _targetProgress = initialProgress;
        progressSlider.value = _currentProgress;
        homeButton.interactable = false;
    }

    private void Start()
    {
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.OnDataLoaded += OnGameDataLoaded;
        }

#if !UNITY_WEBGL
        AuthenticationManager.Instance.OnUserDataLoaded += HandleUserDataLoaded;
        _questSignInBtn.onClick.AddListener(SignInAsGuest);
        _chPlayBtn.onClick.AddListener(SignInAsChPlay);
        _google.onClick.AddListener(SignInAsGoogle);
#endif

        SetBackgroundByTimeOfDay();

        // Start loading process
        _targetProgress += 0.5f; // Initial progress target
    }

    private void SignInAsGuest()
    {
        // Display loading indicator
        progressSlider.gameObject.SetActive(true);
        UIOnBoard.gameObject.SetActive(false);
        
        StartCoroutine(AuthenticationManager.Instance.HandleGuestSignIn(() => {
            if (UserManager.Instance.UserData != null)
            {
                comic.gameObject.SetActive(true);
                comic.OnNewUserCreate();
            }
            else
            {
                Debug.LogError("Guest sign-in failed: User data is null");
                // Show authentication options again if it failed
                UIOnBoard.gameObject.SetActive(true);
                progressSlider.gameObject.SetActive(false);
            }
        }));
    }

    private void SignInAsChPlay()
    {
        // Display loading indicator
        progressSlider.gameObject.SetActive(true);
        UIOnBoard.gameObject.SetActive(false);
        
        StartCoroutine(AuthenticationManager.Instance.HandleChPlaySignIn((() =>
        {
            // Check if user data is properly loaded
            if (UserManager.Instance.UserData != null)
            {
                LoadingAnimationController.Instance.SceneSwitch(Loader.Scene.Town);
            }
            else
            {
                Debug.LogError("CH Play sign-in failed: User data is null");
                // Show authentication options again if it failed
                UIOnBoard.gameObject.SetActive(true);
                progressSlider.gameObject.SetActive(false);
            }
        })));
    }

    private void SignInAsGoogle()
    {
        // Display loading indicator
        progressSlider.gameObject.SetActive(true);
        UIOnBoard.gameObject.SetActive(false);
        
        AuthenticationManager.Instance.HandleGoogleSignIn();
        // Result handling is done in the AuthenticationManager's callback
    }

    private void OnDestroy()
    {
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.OnDataLoaded -= OnGameDataLoaded;
        }

#if !UNITY_WEBGL
        if (AuthenticationManager.Instance != null)
        {
            // AuthenticationManager.Instance.NewUser -= HandleNewUser;
        }
#endif

        StopAllCoroutines();
    }

    private void HandleUserDataLoaded()
    {
        if (UserManager.Instance.UserData == null)
        {
            // Show authentication options when no local data exists
            UIOnBoard.gameObject.SetActive(true);
            progressSlider.gameObject.SetActive(false);
        }
        else
        {
            // Continue loading with existing user data
            StartCoroutine(UpdateProgressRoutine());
        }
    }

    private void HandleNewUser()
    {
        comic.gameObject.SetActive(true);
        comic.OnNewUserCreate();
    }

    private void HandleOldUser()
    {
        progressSlider.gameObject.SetActive(true);
        StartCoroutine(UpdateProgressRoutine());
    }

    private void SetBackgroundByTimeOfDay()
    {
        if (backgroundImage == null || TimeManager.Instance == null)
        {
            return;
        }

        TimeOfDay timeOfDay = TimeManager.Instance.GetCurrentTimeOfDay();

        string backgroundPath;

        switch (timeOfDay)
        {
            case TimeOfDay.Morning:
                backgroundPath = MORNING_BG_1;
                break;
            case TimeOfDay.Afternoon:
            case TimeOfDay.Midday:
                backgroundPath = MORNING_BG_2;
                break;
            case TimeOfDay.Evening:
                backgroundPath = EVENING_BG_1;
                break;
            case TimeOfDay.Night:
                backgroundPath = EVENING_BG_2;
                break;
            default:
                backgroundPath = MORNING_BG_1;
                break;
        }

        Sprite bgSprite = Resources.Load<Sprite>(backgroundPath);
        if (bgSprite != null)
        {
            backgroundImage.sprite = bgSprite;
        }
        else
        {
            Debug.LogWarning($"Could not load background sprite at path: {backgroundPath}");
        }
    }

    private void OnGameDataLoaded()
    {
        _targetProgress += 0.75f;
    }


    private IEnumerator UpdateProgressRoutine()
    {
        while (_isLoading)
        {
            // Only update if we haven't reached target yet
            if (_currentProgress < _targetProgress)
            {
                _currentProgress = Mathf.MoveTowards(_currentProgress, _targetProgress,
                    progressSpeed * Time.deltaTime);

                // Update UI
                progressSlider.value = _currentProgress;

                // Check if loading is complete
                if (_currentProgress >= 1f && _targetProgress >= 1f)
                {
                    CompleteLoading();
                }
            }

            yield return null;
        }
    }

    private void CompleteLoading()
    {
        _isLoading = false;
        homeButton.interactable = true;
        LoadingAnimationController.Instance.SceneSwitch(Loader.Scene.Town);
    }
}
#endif
