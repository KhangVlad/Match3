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

    public float progress = 0f;
    private float targetProgress = 1f;

    private void Start()
    {
        Debug.Log("Start method called");
        homeButton.onClick.AddListener(OnHomeClick);
        homeButton.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.OnDataLoaded += OnDataLoaded;
        }


        if (CharactersDataManager.Instance != null)
        {
            CharactersDataManager.Instance.OnCharacterDataLoaded += OnCharacterDataLoaded;
        }
    }

    private void OnDestroy()
    {
        homeButton.onClick.RemoveListener(OnHomeClick);
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.OnDataLoaded -= OnDataLoaded;
        }

        if (CharactersDataManager.Instance != null)
        {
            CharactersDataManager.Instance.OnCharacterDataLoaded -= OnCharacterDataLoaded;
        }
    }

    private void OnDataLoaded()
    {
        targetProgress += 0.5f;
        StartCoroutine(UpdateProgress());
    }

    private void OnCharacterDataLoaded()
    {
        targetProgress += 0.5f;
        StartCoroutine(UpdateProgress());
    }

    private IEnumerator UpdateProgress()
    {
        while (progress < targetProgress)
        {
            progress += Time.deltaTime * 0.5f; // Adjust the speed as needed
            slider.value = progress;
            yield return null;
        }

        progress = targetProgress;
        slider.value = progress;

        if (progress >= 1f)
        {
            homeButton.gameObject.SetActive(true);
            goHomeText.DOFade(0f, 1.5f).SetLoops(-1, LoopType.Yoyo);
        }
    }

    private void OnHomeClick()
    {
        Debug.Log("Home button clicked");
        LoadingAnimationController.Instance.SceneSwitch(Loader.Scene.Town);
    }
}