using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Match3.Enums;
using Match3.Shares;

public class UILevelDesign : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI requirement;
    [SerializeField] private Transform lockTransform;
    [SerializeField] private Sprite redHeart;
    [SerializeField] private Sprite whiteHeart;
    [SerializeField] private Image[] hearts;
    public int totalHeart;
    public int[,] cachedQuest;
    public event Action OnClicked;
    public int index;
    public bool Islocked = false;

    private void Start()
    {
        button.onClick.AddListener(OnClick);
    }
    
    

    private void OnClick()
    {
        OnClicked?.Invoke();
    }

    private void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }

    public void InitializeData(int ind, bool l, int[,] quest, int total) //quest id and quantity
    {
        Islocked = l;
        this.index = ind;
        levelText.text = (index + 1).ToString();
        cachedQuest = quest;
        requirement.text = FormatRequirements(quest);
        lockTransform.gameObject.SetActive(Islocked);
        totalHeart = total;
        if (hearts != null && hearts.Length > 0)
        {
            for (int i = 0; i < hearts.Length; i++)
            {
                hearts[i].sprite = Islocked ? whiteHeart : (i < total ? redHeart : whiteHeart);
            }
        }
    }


    private string FormatRequirements(int[,] quest)
    {
        if (quest == null || quest.GetLength(0) == 0)
            return "No requirements";

        string result = "";
        int questCount = quest.GetLength(0);
        for (int i = 0; i < questCount; i++)
        {
            int questId = quest[i, 0];
            int quantity = quest[i, 1];

            if (questId > 10)
            {
                questId = 1;
            }

            string id = questId.ToString();

            if (quantity < 10)
            {
                result += $"{quantity} <sprite index={id}>"; // notice the space after the number
            }
            else
            {
                result += $"{quantity}<sprite index={id}>";
            }
        }

        return result;
    }
}