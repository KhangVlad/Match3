using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

namespace Match3.LevelEditor
{
    public class UILevelSelectorSlot : MonoBehaviour, IPointerClickHandler
    {
        public static event System.Action<UILevelSelectorSlot, PointerEventData.InputButton> OnClicked;

        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private Button _addNewBtn;
        [SerializeField] private Button _eidtBtn;
        [SerializeField] private Button _removeBtn;
        [SerializeField] private Image _iconImage;
        private Image _triggerImage;


        [Header("~Runtime")]
        [SerializeField] private int _level;


        public int Level => _level;

        private void Awake()
        {
            _triggerImage = GetComponent<Image>();
        }

        private void Start()
        {
            _addNewBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                LevelEditorManager.Instance.AddNewLevelDataAtIndex(_level + 1);
            });
            _eidtBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();

                LevelEditorManager.Instance.SetSelectLevel(_level);
                GridManager.Instance.LoadLevelData(LevelEditorManager.Instance.SelectLevelData);

                UILevelEditorManager.Instance.DisplayUILevelSelector(false);
                UILevelEditorManager.Instance.DisplayUILevelEditor(true);
            });
            _removeBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                LevelEditorManager.Instance.RemoveLevelDataAtIndex(_level);
            });

            OnClicked += OnClickedTriggered;
        }


        private void OnDestroy()
        {
            _addNewBtn.onClick.RemoveAllListeners();
            _eidtBtn.onClick.RemoveAllListeners();
            _removeBtn.onClick.RemoveAllListeners();

            OnClicked -= OnClickedTriggered;
        }

        public void SetData(LevelDataV2 levelData, int level)
        {
            _level = level;
            _levelText.text = $"Level {level + 1}";
        }

        public void SetIcon(Sprite srpite)
        {
            _iconImage.sprite = srpite;
        }


        private void Select()
        {
            Color c = _triggerImage.color;
            c.a = 1f;
            _triggerImage.color = c;    
        }

        private void Unselect()
        {
            Color c = _triggerImage.color;
            c.a = 0f;
            _triggerImage.color = c;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClicked?.Invoke(this, eventData.button);
        }


        private void OnClickedTriggered(UILevelSelectorSlot slot, PointerEventData.InputButton button)
        {
            if (slot == this)
            {
                //Select();
                if(button == PointerEventData.InputButton.Right)
                {
                    UIPopupManager.Instance.UICopyPaste.UpdatePosition();         
                    UICopyPaste.Instance.CachedLevelData = LevelEditorManager.Instance.CharacterLevelData.Levels[_level];
                    UICopyPaste.Instance.CachedIndex = slot.Level;
                    UIPopupManager.Instance.DisplayUICopyPaste(true);
                }
            }
            else
            {
                //Unselect();
            }
        }
    }
}
