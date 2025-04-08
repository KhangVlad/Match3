using Match3;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using TMPro;

public class UILoadingMenu : MonoBehaviour
{
    [SerializeField] private Button homeButton;
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI goHomeText;
    [SerializeField] private Image bgLoading;
    public float progress = 0f;
    private float targetProgress = 1f;

    private void Start()
    {
        homeButton.onClick.AddListener(OnHomeClick);
        SetBgByTime();
        GameDataManager.Instance.OnDataLoaded += OnDataLoaded;
        Invoke("OnDataLoaded", 0.5f);
   
    }

    private void SetBgByTime()
    {
        TimeOfDay timeOfDay = TimeManager.Instance.GetCurrentTimeOfDay();
        Debug.Log("Current Time of Day: " + timeOfDay);
        switch (timeOfDay)
        {
            case TimeOfDay.Morning:
                bgLoading.sprite = Resources.Load<Sprite>("Sprites/backgrounds/bg_morning_1");
                break;
            case TimeOfDay.Afternoon:
                bgLoading.sprite = Resources.Load<Sprite>("Sprites/Backgrounds/bg_morning_2");
                break;
            case TimeOfDay.Midday:
                bgLoading.sprite = Resources.Load<Sprite>("Sprites/Backgrounds/bg_morning_2");
                break;
            case TimeOfDay.Evening:
                bgLoading.sprite = Resources.Load<Sprite>("Sprites/Backgrounds/bg_evening_1");
                break;
            case TimeOfDay.Night:
                bgLoading.sprite = Resources.Load<Sprite>("Sprites/Backgrounds/bg_evening_2");
                break;
        }
    }

    // private void OnEnable()
    // {
    //     if (GameDataManager.Instance != null)
    //     {
    //         GameDataManager.Instance.OnDataLoaded += OnDataLoaded;
    //     }
    //
    //
    //     if (GameDataManager.Instance != null)
    //     {
    //         GameDataManager.Instance.OnCharacterDataLoaded += OnCharacterDataLoaded;
    //     }
    // }

    private void OnDestroy()
    {
        homeButton.onClick.RemoveListener(OnHomeClick);
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.OnDataLoaded -= OnDataLoaded;
        }
    }

    private void OnDataLoaded()
    {
        Debug.Log( "Call");
        targetProgress += 1f;
        StartCoroutine(UpdateProgress());
    }


    private IEnumerator UpdateProgress()
    {
        while (progress < targetProgress)
        {
            progress += Time.deltaTime * 1f; // Adjust the speed as needed
            slider.value = progress;
            yield return null;
        }

        progress = targetProgress;
        slider.value = progress;

        if (progress >= 1f)
        {
            homeButton.interactable = true;
            goHomeText.DOFade(0.5f, 1.5f).SetLoops(-1, LoopType.Yoyo);
        }
    }

    private void OnHomeClick()
    {
        AudioManager.Instance.PlayButtonSfx();
        homeButton.interactable = false;
        LoadingAnimationController.Instance.SceneSwitch(Loader.Scene.Town);
    }
}