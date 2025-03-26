using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILevelDesign : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI levelText;
    public event Action OnClicked;

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

    public void InitializeData(int ind, bool l)
    {
        Islocked = l;
        this.index = ind;
        levelText.text = (index + 1).ToString();
    }
}