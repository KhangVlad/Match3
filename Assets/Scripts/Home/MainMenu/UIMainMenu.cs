using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Match3.Shares;

public class UIMainMenu : MonoBehaviour
{
     private Canvas _canvas;

    [Header("Buttons")]
    [SerializeField] private UIMenuTabBtn _shopBtn;
    [SerializeField] private UIMenuTabBtn _townBtn;
    [SerializeField] private UIMenuTabBtn _mapBtn;

    [Header("Select Tabs")]
    [SerializeField] private Sprite _defaultBtnsSprite;
    [SerializeField] private Sprite _selectBtnsSprite;
    [SerializeField] private RectTransform _selectionTransform;

    private float _defaultWidth;
    private const float TargetWidth = 500;
    private float _untargetWidth;

    private void Awake()
    {
        _defaultWidth = Screen.width / 3f;
        _untargetWidth = (1080 - TargetWidth) / 2f;
        _canvas = GetComponent<Canvas>();
    }

    private void Start()
    {
        InitButtons();
        SelectTab(_townBtn);
    }

    private void InitButtons()
    {
        AddTabListener(_shopBtn);
        AddTabListener(_townBtn);
        AddTabListener(_mapBtn);
    }

    private void AddTabListener(UIMenuTabBtn tab)
    {
        tab.Button.onClick.AddListener(() => SelectTab(tab));
    }

    private void SelectTab(UIMenuTabBtn selectedTab)
    {
        SetEnableInteractableAllBtns(true);
        selectedTab.Button.interactable = false;

        SetDefaultUITabs();
        selectedTab.Button.image.sprite = _selectBtnsSprite;

        SetAllUntargetWidths();
        SetTabWidth(selectedTab, TargetWidth);
        selectedTab.Select();
        Utilities.WaitAfterEndOfFrame(() =>
        {
            _selectionTransform.position = selectedTab.Button.image.rectTransform.position;
        });
    }

    private void SetDefaultUITabs()
    {
        foreach (var tab in new[] { _shopBtn, _townBtn, _mapBtn })
        {
            tab.Button.image.sprite = _defaultBtnsSprite;
            tab.Unselect();
        }
    }

    private void SetAllUntargetWidths()
    {
        foreach (var tab in new[] { _shopBtn, _townBtn, _mapBtn })
        {
            SetTabWidth(tab, _untargetWidth);
        }
    }

    private void SetTabWidth(UIMenuTabBtn tab, float width)
    {
        var rect = tab.Button.image.rectTransform;
        rect.sizeDelta = new Vector2(width, rect.sizeDelta.y);
    }

    private void SetEnableInteractableAllBtns(bool enable)
    {
        _shopBtn.Button.interactable = enable;
        _townBtn.Button.interactable = enable;
        _mapBtn.Button.interactable = enable;
    }

    public void DisplayCanvas(bool enable)
    {
        _canvas.enabled = enable;
    }

    private void OnDestroy()
    {
        _shopBtn.Button.onClick.RemoveAllListeners();
        _townBtn.Button.onClick.RemoveAllListeners();
        _mapBtn.Button.onClick.RemoveAllListeners();
    }
}
