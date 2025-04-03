using UnityEngine;
using DG.Tweening;
using Match3;
using TMPro;
using UnityEngine.UI;
using Match3.Enums;

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
    [SerializeField] private Button selectBtn;
    [SerializeField] private Image panel;
    [SerializeField] private RectTransform heart;
    [SerializeField] private Image heartImage;
    [SerializeField] private Image heartHeader;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image sliderFill; //color of next level
    [SerializeField] private Slider slider; //progress for next level
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Transform warningPanel; // for not enough sympathy
    [SerializeField] private TextMeshProUGUI warningText; // for not enough sympathy
    public CharactersData charData;

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
    }

    private void Start()
    {
        CharacterDisplay.Instance.OnLoadVideosComplete += OnCharacterInteracted;
        closeButton.onClick.AddListener(OnCloseButtonClicked);
        selectBtn.onClick.AddListener(SelectLevel);
    }

    private void OnCloseButtonClicked()
    {
        AudioManager.Instance.PlayCloseBtnSfx();
        TownCanvasController.Instance.ActiveLevelDesign(false);
    }

    private void OnDestroy()
    {
        // ScreenInteraction.Instance.OnCharacterInteracted -= OnCharacterInteracted;
        CharacterDisplay.Instance.OnLoadVideosComplete -= OnCharacterInteracted;
        closeButton.onClick.RemoveAllListeners();
        selectBtn.onClick.RemoveAllListeners();
    }

    private void SetPanelColor(CharacterID id)
    {
        charData = CharactersDataManager.Instance.GetCharacterData(id);
        Color heartColor = CharactersDataManager.Instance.GetHeartColor(charData.GetLevel(), out Color nextLevelColor);
        CharacterAppearance appearance = CharactersDataManager.Instance.GetCharacterAppearanceData(id);
        heartImage.color = heartColor;
        heartHeader.color = nextLevelColor;
        if (appearance != null)
        {
            panel.color = appearance.panelColor;
            heart.anchoredPosition = new Vector2(appearance.heartPosition.x, appearance.heartPosition.y);
        }

        sliderFill.color = nextLevelColor;
        slider.maxValue = charData.GetNextLevelSympathy();
        slider.value = charData.currentSympathy;
        progressText.text = $"{charData.currentSympathy}/{charData.GetNextLevelSympathy()}";
        ;


        CleanLevels();
        if (CharactersDataManager.Instance.TotalHeartPoints() >= charData.totalSympathyRequired)
        {
            InitializeLevels();
        }
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


    private void SelectLevel()
    {
        AudioManager.Instance.PlayButtonSfx();
        LevelManager.Instance.LoadLevelData(currentChosenLevel.index);
        UILevelInfomation.Instance.LoadLevelData(LevelManager.Instance.LevelData, LevelManager.Instance.CurrentLevel);
        VfxPool.Instance.GetVfxByName("Energy").gameObject.transform.position = selectBtn.transform.position;
        UILevelInfomation.Instance.DisplayCanvas(true);
    }

    private void OnCharacterInteracted(CharacterID id)
    {
        SetPanelColor(id);
        CharacterDisplay.Instance.TransitionToState(CharacterState.Entry);
        TownCanvasController.Instance.ActiveLevelDesign(true);
        // InitializeLevels();
        nameText.text = id.ToString();
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
        AudioManager.Instance.PlayButtonSfx();
        typewriterEffect.ResetText();
        currentChosenLevel = levelDesign;
        if (!levelDesign.Islocked)
        {
            DialogueManager.Instance.ShowDialogue(typewriterEffect,
                CharacterDisplay.Instance.GetDialogue(levelDesign.index, 1));
            selectBtn.gameObject.SetActive(true);
        }
        else
        {
            DialogueManager.Instance.ShowDialogue(typewriterEffect, CharacterDisplay.Instance.GetRejectDialogue());
            selectBtn.gameObject.SetActive(false);
        }
    }

    public void ActiveCanvas(bool active)
    {
        int totalHeartPoints = CharactersDataManager.Instance.TotalHeartPoints();
        if (active)
        {
            if (totalHeartPoints < charData.totalSympathyRequired)
            {
                warningPanel.gameObject.SetActive(true);
                warningText.text =
                    $"Just {charData.totalSympathyRequired - totalHeartPoints} more <sprite index=1/> to unlock";
            }
            else
            {
                warningPanel.gameObject.SetActive(false);
            }
            _canvas.enabled = true;
            selectBtn.gameObject.SetActive(false);
            _canvas.transform.localScale = Vector3.zero;
            canvasGroup.alpha = 0;
            _canvas.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            canvasGroup.DOFade(1, 0.5f).OnComplete(() =>
            {
                if (totalHeartPoints >= charData.totalSympathyRequired)
                {
                    DialogueManager.Instance.ShowDialogue(typewriterEffect, CharacterDisplay.Instance.GetDialogue());
                }
                else
                {
                    DialogueManager.Instance.ShowDialogue(typewriterEffect,
                        CharacterDisplay.Instance.GetNotEnoughSympathyDialogue());
                    warningPanel.gameObject.SetActive(true);
                }
            });
        }
        else
        {
            _canvas.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack);
            canvasGroup.DOFade(0, 0.5f).OnComplete(() =>
            {
                currentChosenLevel = null;
                selectBtn.gameObject.SetActive(false);
                _canvas.enabled = false;
            });
            CharacterDisplay.Instance.TransitionToState(CharacterState.Exit);
            typewriterEffect.ResetText();
        }
    }
}