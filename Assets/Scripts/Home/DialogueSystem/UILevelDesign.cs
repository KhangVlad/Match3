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
    public event Action OnClicked;

    public CharacterID CharacterID;
    public int index;
    public bool Islocked;

    private void Start()
    {
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        this.button.ButtonInteractableAfter();
        OnClicked?.Invoke();
    }

    private void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }

    public void InitializeData(CharacterID characterID, int ind, bool l)
    {
        CharacterID = characterID;
        Islocked = l;
        this.index = ind;
        levelText.text = (index + 1).ToString();
    }
}