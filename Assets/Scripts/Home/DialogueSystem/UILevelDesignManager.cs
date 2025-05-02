using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using Match3;
using TMPro;
using UnityEngine.UI;
using Match3.Enums;
using Match3.Shares;

public class UILevelDesignManager : MonoBehaviour
{
    [Header("Main References")] [SerializeField]
    private Canvas canvas;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TypewriterEffect typewriterEffect;

    [Header("Level Design Panel")] [SerializeField]
    private Transform levelDesignParent;

    [SerializeField] private UILevelDesign levelDesignPrefab;
    [SerializeField] private UILevelDesign lockedLevelDesignPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button selectBtn;
    [SerializeField] private Image panel;

    [Header("Character Info")] [SerializeField]
    private RectTransform heart;
    [SerializeField] private Image heartImage;
    [SerializeField] private Image heartHeader;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Slider energySlider;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Button teaseBtn;
    [Header("Warning Panel")] [SerializeField]
    private Transform warningPanel;

    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private TextMeshProUGUI heartText;

    [Header("Levels Panel")] [SerializeField]
    private Transform levelsPanel;

    [SerializeField] private Button selectLevelBtn;
    [SerializeField] private Button closeLevelsPanel;
    [SerializeField] private UILevelDesign nextLevel;
    [Header("Animation Settings")] public LayoutAnimationSettings animationSetting = new();

    private UILevelDesign currentChosenLevel;
    private CharacterID currentCharacterId;
    private CharacterData characterData;
    private CharacterDataSO characterDataSO;
    private CharacterAppearance appearanceData;


    private const float ANIMATION_DURATION = 0.5f;
    private const string HEART_SPRITE_INDEX = "<sprite index=1/>";


    #region Unity Lifecycle

    private void Awake()
    {
        if (canvas == null)
            canvas = GetComponent<Canvas>();
    }

    private void Start()
    {
        RegisterEventListeners();
        levelsPanel.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        UnregisterEventListeners();
    }

    #endregion

    #region Event Handlers

    private void RegisterEventListeners()
    {
        CharacterDisplay.Instance.OnLoadVideosComplete += HandleCharacterInteracted;
        closeButton.onClick.AddListener(HandleCloseButtonClicked);
        selectBtn.onClick.AddListener(HandleSelectLevel);
        selectLevelBtn.onClick.AddListener(HandleOpenSelectLevelContainer);
        closeLevelsPanel.onClick.AddListener(HandleCloseLevelsPanel);
        UserManager.Instance.OnEnergyChanged += UpdateEnergyUI;
        UserManager.Instance.OnUserDataLoaded += InitializeEnergyUI;
        LevelManager.Instance.OnBackScene += ForceUpdateUI;
        teaseBtn.onClick.AddListener(CharacterDisplay.Instance.Tease);
    }


    private void InitializeEnergyUI()
    {
        if (UserManager.Instance != null && UserManager.Instance.UserData != null)
        {
            energySlider.maxValue = 100;
            UpdateEnergyUI(UserManager.Instance.UserData.Energy);
        }
    }

    private void UnregisterEventListeners()
    {
        if (CharacterDisplay.Instance != null) 
            CharacterDisplay.Instance.OnLoadVideosComplete -= HandleCharacterInteracted;
        UserManager.Instance.OnEnergyChanged -= UpdateEnergyUI;
        UserManager.Instance.OnUserDataLoaded -= InitializeEnergyUI;
        LevelManager.Instance.OnBackScene -= ForceUpdateUI;
        closeButton.onClick.RemoveAllListeners();
        selectBtn.onClick.RemoveAllListeners();
        selectLevelBtn.onClick.RemoveAllListeners();
        closeLevelsPanel.onClick.RemoveAllListeners();
        teaseBtn.onClick.RemoveAllListeners();
    }

    private void UpdateEnergyUI(int currentEnergy)
    {
        // Update slider value
        energySlider.value = currentEnergy;

        // Update text display
        energyText.text = $"{currentEnergy}/{100}";
    }

    private void HandleCharacterInteracted(CharacterID id)
    {
        currentCharacterId = id;
        LoadCharacterData(id);
        UpdateUI();

        CharacterDisplay.Instance.TransitionToState(CharacterState.Entry);
        TownCanvasController.Instance.ActiveLevelDesign(true);
    }


    private void ForceUpdateUI()
    {
        if (characterData != null)
        {
            LoadCharacterData(characterData.CharacterID);
            UpdateUI();
        }
    }

    private void HandleCloseButtonClicked()
    {
        AudioManager.Instance.PlayCloseBtnSfx();
        TownCanvasController.Instance.ActiveLevelDesign(false);
    }

    private void HandleSelectLevel()
    {
        if (currentChosenLevel == null)
            return;
        AudioManager.Instance.PlayButtonSfx();

        if (UserManager.Instance.HasEnoughEnergy(10)) // move to UILevelInformation  later
        {
            UserManager.Instance.ConsumeEnergy(10);
            LevelManager.Instance.LoadLevelData(characterData.CharacterID, currentChosenLevel.index);
            UILevelInfomation.Instance.LoadLevelData(LevelManager.Instance.LevelData,
                LevelManager.Instance.CurrentLevelIndex);
            Transform vfxTransform = VfxPool.Instance.GetVfxByName("Energy").gameObject.transform;
            vfxTransform.position = selectBtn.transform.position;
            UILevelInfomation.Instance.DisplayCanvas(true);
            UILevelInfomation.Instance.SetQuest(nextLevel.cachedQuest);
        }
        else
        {
            UIPopupManager.Instance.ShowWarningPopup("Not Enough Energy");
        }
    }


    private void HandleOpenSelectLevelContainer()
    {
        levelsPanel.gameObject.SetActive(true);
        // StartCoroutine(levelDesignParent.AnimateLayoutItems(animationSetting));
        StartCoroutine(AnimateLayoutItems(levelDesignParent, animationSetting));
    }


    private void HandleCloseLevelsPanel()
    {
        levelsPanel.gameObject.SetActive(false);
    }

    private void HandleLevelDesignClicked(UILevelDesign levelDesign)
    {
  
        AudioManager.Instance.PlayButtonSfx();
        if (!levelDesign.Islocked)
        {
            levelsPanel.gameObject.SetActive(false);
            LevelManager.Instance.SetCurrentLevelIndex(levelDesign.index);
            typewriterEffect.ResetText();
            currentChosenLevel = levelDesign;
            UpdateDialogueBasedOnLevel(levelDesign);
            nextLevel.InitializeData(levelDesign.index, false, levelDesign.cachedQuest, levelDesign.totalHeart);
        }
        else
        {
            UIPopupManager.Instance.ShowWarningPopup("Try it later!");
        }
    }

    #endregion

    #region UI Management

    public void ActiveCanvas(bool active)
    {
        if (active)
        {
            ShowCanvas();
        }
        else
        {
            HideCanvas();
        }
    }

    private void ShowCanvas()
    {
        int totalHeartPoints = characterData.TotalHeartPoints();
        int requiredHearts = characterDataSO.TotalHeartToUnlock;
        bool hasEnoughHearts = totalHeartPoints >= requiredHearts;
        UpdateHeartUI(hasEnoughHearts, totalHeartPoints, requiredHearts);
        canvas.enabled = true;
        selectBtn.gameObject.SetActive(false);
        canvas.transform.localScale = Vector3.zero;
        canvasGroup.alpha = 0;
        Sequence showSequence = DOTween.Sequence();
        showSequence.Append(canvas.transform.DOScale(Vector3.one, ANIMATION_DURATION).SetEase(Ease.OutBack));
        showSequence.Join(canvasGroup.DOFade(1, ANIMATION_DURATION));
        showSequence.OnComplete(() =>
        {
            string dialogue = hasEnoughHearts
                ? CharacterDisplay.Instance.GetGreetingDialog()
                : CharacterDisplay.Instance.GetLowSympathyDialogue();
            DialogueManager.Instance.ShowDialogue(typewriterEffect, dialogue);
            if (!hasEnoughHearts)
                warningPanel.gameObject.SetActive(true);
        });
    }

    private void HideCanvas()
    {
        Sequence hideSequence = DOTween.Sequence();
        hideSequence.Append(canvas.transform.DOScale(Vector3.zero, ANIMATION_DURATION).SetEase(Ease.InBack));
        hideSequence.Join(canvasGroup.DOFade(0, ANIMATION_DURATION));
        hideSequence.OnComplete(() =>
        {
            ResetState();
            canvas.enabled = false;
        });

        CharacterDisplay.Instance.TransitionToState(CharacterState.Exit);
        typewriterEffect.ResetText();
    }

    private void UpdateHeartUI(bool hasEnoughHearts, int totalHeartPoints, int requiredHearts)
    {
        heartText.text = totalHeartPoints.ToString();
        selectLevelBtn.gameObject.SetActive(hasEnoughHearts);
        warningPanel.gameObject.SetActive(!hasEnoughHearts);
        nextLevel.gameObject.SetActive(hasEnoughHearts);

        if (!hasEnoughHearts)
        {
            warningText.text = $"Just {requiredHearts - totalHeartPoints} more {HEART_SPRITE_INDEX} to unlock";
        }
    }

    private void UpdateDialogueBasedOnLevel(UILevelDesign levelDesign)
    {
        if (!levelDesign.Islocked)
        {
            DialogueManager.Instance.ShowDialogue(
                typewriterEffect,
                CharacterDisplay.Instance.GetDialogue(levelDesign.index, 1)
            );
            selectBtn.gameObject.SetActive(true);
        }
        else
        {
            DialogueManager.Instance.ShowDialogue(
                typewriterEffect,
                CharacterDisplay.Instance.GetRejectDialogue()
            );
            selectBtn.gameObject.SetActive(false);
        }
    }

    private void ResetState()
    {
        currentChosenLevel = null;
        selectBtn.gameObject.SetActive(false);
    }

    #endregion

    #region Level Management

    private void LoadCharacterData(CharacterID id)
    {
        characterData = UserManager.Instance.GetCharacterData(id);
        characterDataSO = GameDataManager.Instance.GetCharacterDataSOByID(id);
        appearanceData = GameDataManager.Instance.GetCharacterAppearanceData(id);
        nameText.text = id.ToString();
        heartText.text = characterData.TotalHeartPoints().ToString();
    }

    private void UpdateUI()
    {
        CleanLevels();
        if (appearanceData != null)
        {
            panel.color = appearanceData.panelColor;
            heart.anchoredPosition = new Vector2(appearanceData.heartPosition.x, appearanceData.heartPosition.y);
        }

        if (UserManager.Instance.GetTotalHeart() >= characterDataSO.TotalHeartToUnlock)
        {
            InitializeLevels(currentCharacterId);
        }
    }

    private void InitializeLevels(CharacterID id)
    {
        if (!GameDataManager.Instance.TryGetCharacterLevelDataByID(id, out CharacterLevelDataV2 characterLevelData))
        {
            Debug.LogWarning($"No level data found for character ID: {id}");
            return;
        }

        int h = characterData.higestLevel;
        nextLevel.OnClicked += () => HandleLevelDesignClicked(nextLevel);
        for (int i = 0; i < characterLevelData.Levels.Count; i++)
        {
            int levelIndex = i;
            bool islock = i > h;
            UILevelDesign levelDesign = Instantiate(levelDesignPrefab, levelDesignParent);
            RectTransform rectTransform = levelDesign.GetComponent<RectTransform>();
            if (rectTransform == null)
                continue;
            levelDesign.InitializeData(levelIndex, islock, characterLevelData.Levels[i].Quests,
                characterData.Hearts[i]);
            levelDesign.OnClicked += () => HandleLevelDesignClicked(levelDesign);
        }

        nextLevel.InitializeData(h, false, characterLevelData.Levels[h].Quests,
            characterData.Hearts[h]); // fix this later , get the next level play of user on character
    }

    private IEnumerator AnimateLayoutItems(Transform layoutTransform, LayoutAnimationSettings settings)
    {
        layoutTransform.gameObject.SetActive(true);

        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutTransform.GetComponent<RectTransform>());
        yield return null;
        yield return null;
        for (int i = 0; i < layoutTransform.childCount; i++)
        {
            Transform item = layoutTransform.GetChild(i);
            if (!item.gameObject.activeSelf)
                continue;

            RectTransform rectTransform = item.GetComponent<RectTransform>();
            if (rectTransform == null)
                continue;

            Vector2 targetPosition = rectTransform.anchoredPosition;

            // Set initial position (off-screen to the left)
            rectTransform.anchoredPosition = new Vector2(targetPosition.x + settings.offsetX, targetPosition.y);

            // Animate to final position with delay based on index
            float delay = i * settings.delay;
            rectTransform.DOAnchorPos(targetPosition, settings.duration)
                .SetDelay(delay)
                .SetEase(settings.easeType);
        }
    }

    private void CleanLevels()
    {
        foreach (Transform child in levelDesignParent)
        {
            Destroy(child.gameObject);
        }
    }

    #endregion
}