using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using Match3;
using TMPro;
using UnityEngine.UI;
using Match3.Enums;
using Match3.Shares;
using DynamicScrollRect;
using System.Collections.Generic;

#if !UNITY_WEBGL
public class UILevelDesignManager : MonoBehaviour
{
    public static UILevelDesignManager Instance { get; private set; }

    [Header("Main References")] [SerializeField]
    private Canvas canvas;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TypewriterEffect typewriterEffect;

    [Header("Level Design Panel")] [SerializeField]
    private Transform levelDesignParent;

    [SerializeField] private UILevelDesign levelDesignPrefab;
    [SerializeField] private Button backBtn;
    [SerializeField] private Button playBtn;
    [SerializeField] private Image panel;

    [Header("Character Info")] [SerializeField]
    private RectTransform heart;

    [SerializeField] private Image heartImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Button teaseBtn;

    [Header("Warning Panel")] [SerializeField]
    private Transform warningPanel;

    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private RawImage renderTexture;

    [Header("Levels Panel")] [SerializeField]
    private Transform levelsPanel;

    [SerializeField] private Button selectLevelBtn;
    [SerializeField] private Button closeLevelsPanel;
    [SerializeField] private UILevelDesign nextLevel;
    [SerializeField] private ScrollContent _content = null;

    private UILevelDesign currentChosenLevel;
    private CharacterID currentCharacterId;
    private CharacterData characterData;
    private CharacterDataSO characterDataSO;
    private CharacterAppearance appearanceData;
    private const float ANIMATION_DURATION = 0.5f;
    private const string HEART_SPRITE_INDEX = "<sprite index=1/>";
    private int currentEnergy;


    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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
        CharacterDisplay.Instance.OnNewAngryState += OnNewAngryState;
        UserManager.Instance.OnUserDataLoaded += InitializeEnergyUI;
        LevelManager.Instance.OnBackScene += ForceUpdateUI;
        UserManager.Instance.OnEnergyChanged += OnEnergyChanger;
        backBtn.onClick.AddListener(HandleCloseButtonClicked);
        playBtn.onClick.AddListener(HandleSelectLevel);
        selectLevelBtn.onClick.AddListener(HandleOpenSelectLevelContainer);
        closeLevelsPanel.onClick.AddListener(HandleCloseLevelsPanel);
        // UserManager.Instance.OnEnergyChanged += UpdateEnergyUI;
        teaseBtn.onClick.AddListener(HandleTeaseInput);
        currentEnergy = UserManager.Instance.UserData.Energy;
    }

    private void OnNewAngryState()
    {
        DialogueManager.Instance.ShowDialogue(typewriterEffect, CharacterDisplay.Instance.GetLowSympathyDialogue());
    }

    private void OnEnergyChanger(int c)
    {
        currentEnergy = c;
        if (currentEnergy < 5)
        {
        }
    }

    private void HandleTeaseInput()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        VfxGameObject a = VfxPool.Instance.GetVfxByName("angry");
        a.gameObject.transform.position = mouseWorldPos;
        CharacterDisplay.Instance.Tease();
    }


    private void InitializeEnergyUI()
    {
        // if (UserManager.Instance != null && UserManager.Instance.UserData != null)
        // {
        //     energySlider.maxValue = 100;
        //     UpdateEnergyUI(UserManager.Instance.UserData.Energy);
        // }
    }

    private void UnregisterEventListeners()
    {
        if (CharacterDisplay.Instance != null)
            CharacterDisplay.Instance.OnLoadVideosComplete -= HandleCharacterInteracted;
        // UserManager.Instance.OnEnergyChanged -= UpdateEnergyUI;
        UserManager.Instance.OnUserDataLoaded -= InitializeEnergyUI;
        LevelManager.Instance.OnBackScene -= ForceUpdateUI;
        backBtn.onClick.RemoveAllListeners();
        playBtn.onClick.RemoveAllListeners();
        selectLevelBtn.onClick.RemoveAllListeners();
        closeLevelsPanel.onClick.RemoveAllListeners();
        teaseBtn.onClick.RemoveAllListeners();
    }

    // private void UpdateEnergyUI(int currentEnergy)
    // {
    //     // Update slider value
    //     energySlider.value = currentEnergy;
    //
    //     // Update text display
    //     energyText.text = $"{currentEnergy}/{100}";
    // }

    private void HandleCharacterInteracted(CharacterID id)
    {
        currentCharacterId = id;
        LoadCharacterData(id);
        UpdateUI();
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
            LevelManager.Instance.LoadLevelData(characterData.CharacterID, currentChosenLevel.index);
            UILevelInfomation.Instance.LoadLevelData(LevelManager.Instance.LevelData,
                LevelManager.Instance.CurrentLevelIndex);
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
        // StartCoroutine(AnimateLayoutItems(levelDesignParent, animationSetting));
    }


    private void HandleCloseLevelsPanel()
    {
        levelsPanel.gameObject.SetActive(false);
    }

    public void HandleLevelDesignClicked(UILevelDesign levelDesign)
    {
        Debug.Log($"Clicked on level design: {levelDesign.index}");
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
        renderTexture.enabled = true;
        int totalHeartPoints = UserManager.Instance.GetTotalHeart();
        int requiredHearts = characterDataSO.TotalHeartToUnlock;
        bool hasEnoughHearts = totalHeartPoints >= requiredHearts;
        UpdateHeartUI(hasEnoughHearts, totalHeartPoints, requiredHearts);
        canvas.enabled = true;
        // playBtn.gameObject.SetActive(false);
        canvas.transform.localScale = Vector3.zero;
        canvasGroup.alpha = 1;
        string dialogue = hasEnoughHearts
            ? CharacterDisplay.Instance.GetGreetingDialog()
            : CharacterDisplay.Instance.GetLowSympathyDialogue();
        DialogueManager.Instance.ShowDialogue(typewriterEffect, dialogue);
        if (!hasEnoughHearts)
            warningPanel.gameObject.SetActive(true);
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
        renderTexture.enabled = false;
    }

    private void UpdateHeartUI(bool hasEnoughHearts, int totalHeartPoints, int requiredHearts)
    {
        // heartText.text = totalHeartPoints.ToString();
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
            // playBtn.gameObject.SetActive(true);
        }
        else
        {
            DialogueManager.Instance.ShowDialogue(
                typewriterEffect,
                CharacterDisplay.Instance.GetRejectDialogue()
            );
            // playBtn.gameObject.SetActive(false);
        }
    }

    private void ResetState()
    {
        currentChosenLevel = null;
        // playBtn.gameObject.SetActive(false);
    }

    #endregion

    #region Level Management

    private void LoadCharacterData(CharacterID id)
    {
        characterData = UserManager.Instance.GetCharacterData(id);
        characterDataSO = GameDataManager.Instance.GetCharacterDataSOByID(id);
        appearanceData = GameDataManager.Instance.GetCharacterAppearanceData(id);
        heartImage.color =
            GameDataManager.Instance.characterColor.heartColors[
                characterDataSO.CurrentSympathyLevel(characterData.TotalHeartPoints())];
        nameText.text = id.ToString();
    }

    private void UpdateUI()
    {
        // CleanLevels();
        if (appearanceData != null)
        {
            panel.color = appearanceData.panelColor;
            // heart.anchoredPosition = new Vector2(appearanceData.heartPosition.x, appearanceData.heartPosition.y);
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
        List<ScrollItemData> contentDatas = new List<ScrollItemData>();
        int h = characterData.higestLevel;
        currentChosenLevel = nextLevel;
        for (int i = 0; i < characterLevelData.Levels.Count; i++)
        {
            bool islock = i > h;
            contentDatas.Add(new ScrollItemData(i, islock, characterLevelData.Levels[i].Quests,
                characterData.Hearts[i]));
        }

        nextLevel.InitializeData(h, false, characterLevelData.Levels[h].Quests,
            characterData.Hearts[h]);
        _content.InitScrollContent(contentDatas);
    }

    private IEnumerator AnimateLayoutItems(Transform layoutTransform, LayoutAnimationSettings settings)
    {
        layoutTransform.gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutTransform.GetComponent<RectTransform>());
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

    // private void CleanLevels()
    // {
    //     foreach (Transform child in levelDesignParent)
    //     {
    //         Destroy(child.gameObject);
    //     }
    // }

    #endregion
}
#endif