using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DynamicScrollRect;
using Match3;

public class UILevelDesign : ScrollItem<ScrollItemData>
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI requirement;
    [SerializeField] private Transform lockTransform;
    [SerializeField] private Sprite redHeart;
    [SerializeField] private Sprite whiteHeart;
    [SerializeField] private Image[] hearts;
    [SerializeField] private DynamicScrollRect.DynamicScrollRect _dynamicScroll = null;

    [SerializeField]
    private UIQuestsRequiremnet[] questRequirement; //base on quest[,] length to active , max length is 3

    public int totalHeart;
    public int[,] cachedQuest;
    public event Action<UILevelDesign> OnClicked;
    public int index;
    public bool Islocked = false;

    private void Start()
    {
        button.onClick.AddListener(OnClick);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }

    private void OnClick()
    {
        UILevelDesignManager.Instance.HandleLevelDesignClicked(this);
    }


    // public void InitializeData(int ind, bool l, int[,] quest, int total) //quest id and quantity
    // {
    //     Islocked = l;
    //     this.index = ind;
    //     levelText.text = (index + 1).ToString();
    //     cachedQuest = quest;
    //     // requirement.text = FormatRequirements(quest);
    //     requirement.text = "";
    //     lockTransform.gameObject.SetActive(Islocked);
    //     totalHeart = total;
    //         
    //     
    //     if (hearts != null && hearts.Length > 0)
    //     {
    //         for (int i = 0; i < hearts.Length; i++)
    //         {
    //             hearts[i].sprite = Islocked ? whiteHeart : (i < total ? redHeart : whiteHeart);
    //         }
    //     }
    //         for( int i=0;i<quest.Length ;i++)
    //         {
    //             QuestID questId = (QuestID)quest[i, 0];
    //             int quantity = quest[i, 1];
    //             questRequirement[i].gameObject.SetActive(true);
    //             questRequirement[i].Initialize(GameDataManager.Instance.GetQuestDataByID(questId), quantity);
    //         }
    //     levelText.gameObject.SetActive(!Islocked);
    // }
    public void InitializeData(int ind, bool l, int[,] quest, int total) //quest id and quantity
    {
        Islocked = l;
        this.index = ind;
        levelText.text = (index + 1).ToString();
        cachedQuest = quest;
        lockTransform.gameObject.SetActive(Islocked);
        totalHeart = total;

        // Update heart sprites
        if (hearts != null && hearts.Length > 0)
        {
            for (int i = 0; i < hearts.Length; i++)
            {
                hearts[i].sprite = Islocked ? whiteHeart : (i < total ? redHeart : whiteHeart);
            }
        }

        // Determine how many quests we actually have in this level
        int questCount = quest.GetLength(0);

        // Set up quest requirements UI
        for (int i = 0; i < questRequirement.Length; i++)
        {
            // Only activate and initialize if we have a quest for this slot
            if (i < questCount)
            {
                QuestID questId = (QuestID)quest[i, 0];
                int quantity = quest[i, 1];
                questRequirement[i].gameObject.SetActive(true);
                questRequirement[i].Initialize(GameDataManager.Instance.GetQuestDataByID(questId), quantity);
            }
            else
            {
                // Deactivate unused quest requirement slots
                questRequirement[i].gameObject.SetActive(false);
            }
        }

        levelText.gameObject.SetActive(!Islocked);
    }

    public void FocusOnItem()
    {
        _dynamicScroll.StartFocus(this);
        OnClicked?.Invoke(this);
    }

    protected override void InitItemData(ScrollItemData data)
    {
        base.InitItemData(data);
        InitializeData(data.Index, data.IsLocked, data.Quest, data.Total);
    }
}