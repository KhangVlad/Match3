using UnityEngine;
using DG.Tweening;
using Match3;
using TMPro;
using UnityEngine.UI;
using Match3.Enums;
using Match3.Shares;

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
    [SerializeField] private TextMeshProUGUI heartText; // for not enough sympathy
    // public CharactersData charData;
    public CharacterData charData;
    public CharacterDataSO charDataSO;
    public CharacterAppearance appearanceData;
    

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
        charData = UserManager.Instance.GetCharacterData(id);
        charDataSO = GameDataManager.Instance.GetCharacterDataByID(id);
        appearanceData = GameDataManager.Instance.GetCharacterAppearanceData(id);
        heartText.text = charData.TotalHeartPoints().ToString();
        // // Color heartColor = GameDataManager.Instance.GetHeartColor(charData.GetLevel(), out Color nextLevelColor);
        // // heartImage.color = heartColor;
        // // heartHeader.color = nextLevelColor;
        // if (appearanceData != null)
        // {
        //     panel.color = appearanceData.panelColor;
        //     heart.anchoredPosition = new Vector2(appearanceData.heartPosition.x, appearanceData.heartPosition.y);
        // }
        //
        // // sliderFill.color = nextLevelColor;
        // // slider.maxValue = charData.GetNextLevelSympathy();
        // // slider.value = charData.currentSympathy;
        // // progressText.text = $"{charData.currentSympathy}/{charData.GetNextLevelSympathy()}";
        // ;
        //
        //
        CleanLevels();
        if (UserManager.Instance.GetTotalHeart() >= charDataSO.TotalHeartToUnlock)
        {
            InitializeLevels(id);
        }
    }

    private void InitializeLevels(CharacterID id)
    {
        CleanLevels();

        Debug.Log($"InitializeLevels: {id}");
        if(GameDataManager.Instance.TryGetCharacterLevelDataByID(id, out CharacterLevelDataV2 characterLevelData))
        {
            for (int i = 0; i < characterLevelData.Levels.Count; i++)
            {
                LevelDataV2 levelData = characterLevelData.Levels[i];

                UILevelDesign levelDesign = Instantiate(levelDesignDesignPrefab, levelDesignParent);
                levelDesign.InitializeData(id, i, false);
                levelDesign.OnClicked += () => OnLevelDesignClicked(levelDesign);
            }
        }
    }


    private void SelectLevel()
    {
        AudioManager.Instance.PlayButtonSfx();
        LevelManager.Instance.LoadLevelData(currentChosenLevel.CharacterID, currentChosenLevel.index);
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
        int totalHeartPoints = charData.TotalHeartPoints();
        if (active)
        {
            if (totalHeartPoints < charDataSO.TotalHeartToUnlock)
            {
                warningPanel.gameObject.SetActive(true);
                warningText.text =
                    $"Just {charDataSO.TotalHeartToUnlock - totalHeartPoints} more <sprite index=1/> to unlock";
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
                if (totalHeartPoints >= charDataSO.TotalHeartToUnlock)
                {
                    DialogueManager.Instance.ShowDialogue(typewriterEffect,
                        CharacterDisplay.Instance.GetGreetingDialog());
                }
                else
                {
                    DialogueManager.Instance.ShowDialogue(typewriterEffect,
                        CharacterDisplay.Instance.GetLowSympathyDialogue());
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