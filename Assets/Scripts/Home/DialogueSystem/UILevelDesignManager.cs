using UnityEngine;
using DG.Tweening;
using Match3;
using TMPro;
using UnityEngine.UI;
using System.Linq;


public class UILevelDesignManager : MonoBehaviour
{
    private Canvas _canvas;
    [SerializeField] private Transform levelDesignParent;
    [SerializeField] private UILevelDesign levelDesignDesignPrefab;
    [SerializeField] private UILevelDesign lockedLevelDesignPrefab;
    [SerializeField] private TypewriterEffect typewriterEffect;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button closeButton;
    [SerializeField] private UILevelDesign currentChosenLevel;
    [SerializeField] private Button playButton;
    [SerializeField] private Image panel;
    [SerializeField] private RectTransform heart;
    [SerializeField] private Image heartImage;
    [SerializeField] private Image heartHeader;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image sliderFill; //color of next level
    [SerializeField] private Slider slider; //progress for next level
    [SerializeField] private TextMeshProUGUI progressText;

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
    }

    private void Start()
    {
        CharacterVideoController.Instance.OnLoadVideosComplete += OnCharacterInteracted;
        closeButton.onClick.AddListener(() => ActiveCanvas(false));
        playButton.onClick.AddListener(() => OnPlayClick());
    }

    private void OnDestroy()
    {
        // ScreenInteraction.Instance.OnCharacterInteracted -= OnCharacterInteracted;
        CharacterVideoController.Instance.OnLoadVideosComplete -= OnCharacterInteracted;
        closeButton.onClick.RemoveAllListeners();
        playButton.onClick.RemoveAllListeners();
    }

    private void SetPanelColor(CharacterID id)
    {
        CharactersData charData = CharactersDataManager.Instance.GetCharacterData(id);
        Color heartColor = CharactersDataManager.Instance.GetHeartColor(charData.GetLevel(), out Color nextLevelColor);
        CharacterAppearance appearance = CharactersDataManager.Instance.GetCharacterAppearanceData(id);
        heartImage.color = heartColor;
        heartHeader.color = nextLevelColor;
        panel.color = appearance.panelColor;
        sliderFill.color = nextLevelColor;
        slider.maxValue = charData.GetNextLevelSympathy();
        slider.value = charData.currentSympathy;
        progressText.text = $"{charData.currentSympathy}/{charData.GetNextLevelSympathy()}";
        heart.anchoredPosition = new Vector2(appearance.heartPosition.x, appearance.heartPosition.y);
    }


    private void OnPlayClick()
    {
        LevelManager.Instance.LoadLevelData(currentChosenLevel.index);
        UILevelInfomation.Instance.LoadLevelData(LevelManager.Instance.LevelData, LevelManager.Instance.CurrentLevel);
        VfxPool.Instance.GetVfxByName("Energy").gameObject.transform.position = playButton.transform.position;
        UILevelInfomation.Instance.DisplayCanvas(true);
    }

    private void OnCharacterInteracted(CharacterID id)
    {
        SetPanelColor(id);
        CharacterDisplay.Instance.TransitionToState(CharacterState.Entry);
        ActiveCanvas(true);
        InitializeLevels();
        nameText.text = id.ToString();
    }

    private void InitializeLevels()
    {
        CleanLevels();
        for (int i = 0; i < 4; i++)
        {
            if (i < 2)
            {
                UILevelDesign levelDesign = Instantiate(levelDesignDesignPrefab, levelDesignParent);
                levelDesign.InitializeData(i, false);
                levelDesign.OnClicked += () => OnLevelDesignClicked(levelDesign);
            }
            else
            {
                UILevelDesign levelDesign = Instantiate(lockedLevelDesignPrefab, levelDesignParent);
                levelDesign.InitializeData(i, true);
                levelDesign.OnClicked += () => OnLevelDesignClicked(levelDesign);
            }
        }
    }


    private void CleanLevels()
    {
        foreach (Transform child in levelDesignParent)
        {
            Destroy(child.gameObject);
        }
    }

    private void OnLevelDesignClicked(UILevelDesign levelDesign)
    {
        typewriterEffect.ResetText();
        currentChosenLevel = levelDesign;
        if (!levelDesign.Islocked)
        {
            DialogueManager.Instance.ShowDialogue(typewriterEffect,
                CharacterDisplay.Instance.GetDialogue(levelDesign.index));
            playButton.gameObject.SetActive(true);
        }
        else
        {
            DialogueManager.Instance.ShowDialogue(typewriterEffect, CharacterDisplay.Instance.GetRejectDialogue());
            playButton.gameObject.SetActive(false);
        }
    }

    private void ActiveCanvas(bool active)
    {
        if (active)
        {
            _canvas.enabled = true;
            playButton.gameObject.SetActive(false);
            _canvas.transform.localScale = Vector3.zero;
            canvasGroup.alpha = 0;
            _canvas.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            canvasGroup.DOFade(1, 0.5f).OnComplete(() =>
            {
                DialogueManager.Instance.ShowDialogue(typewriterEffect, CharacterDisplay.Instance.GetDialogue());
            });
        }
        else
        {
            _canvas.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack);
            canvasGroup.DOFade(0, 0.5f).OnComplete(() =>
            {
                currentChosenLevel = null;
                playButton.gameObject.SetActive(false);
                _canvas.enabled = false;
            });
            CharacterDisplay.Instance.TransitionToState(CharacterState.Exit);
            CharacterDisplay.Instance.CloseDialogue();
            typewriterEffect.ResetText();
        }
    }
}