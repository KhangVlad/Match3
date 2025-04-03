using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Match3.Shares;

namespace Match3.LevelEditor
{
    public class UICharacterSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public static System.Action<UICharacterSlot> OnClicked;
        [SerializeField] private Button _button;
        [SerializeField] private Image _iconImage;

        [Header("Sprites")]
        [SerializeField] private Sprite _defaultButtonSprite;
        [SerializeField] private Sprite _selectButtonSprite;


        [Header("Runtime")]
        [SerializeField] private CharacterDataSO _characterData;

        private bool _isEnter = false;

        public int SlotIndex { get; private set; }

        private void Start()
        {
            _button.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                OnClicked?.Invoke(this);
            });

            OnClicked += OnUITilSlotClicked;
        }


        private void OnDestroy()
        {
            _button.onClick.RemoveAllListeners();
            OnClicked -= OnUITilSlotClicked;
        }

        private void OnUITilSlotClicked(UICharacterSlot slot)
        {
            if (slot == this)
            {
                Select();
            }
            else
            {
                Unselect();
            }
        }


        public void SetData(CharacterDataSO data, int slotIndex)
        {
            this._characterData = data;
            this.SlotIndex = slotIndex;

            _iconImage.sprite = data.sprite;
            _iconImage.SetNativeSize();
            _iconImage.rectTransform.ScaleIcon(85, 85);
        }

        private void Select()
        {
            _button.interactable = false;
            _button.image.sprite = _selectButtonSprite;
        }

        private void Unselect()
        {
            _button.interactable = true;
            _button.image.sprite = _defaultButtonSprite;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isEnter = true;

            Utilities.WaitAfter(0.25f, () =>
            {
                if (_isEnter)
                {
                    UIPopupManager.Instance.DisplayUINameInfoPopup(true);
                    UIPopupManager.Instance.SetNameInfoPopupContent(_characterData.id.ToString());
                }
            });
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isEnter = false;
            UIPopupManager.Instance.DisplayUINameInfoPopup(false);
        }
    }
}
