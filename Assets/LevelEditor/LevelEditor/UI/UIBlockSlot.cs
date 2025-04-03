using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Match3.LevelEditor
{
    public class UIBlockSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public static System.Action<UIBlockSlot> OnClicked;
        [SerializeField] private Button _button;
        [SerializeField] private Image _iconImage;

        [Header("Sprites")]
        [SerializeField] private Sprite _defaultButtonSprite;
        [SerializeField] private Sprite _selectButtonSprite;


        [Header("Runtime")]
        [SerializeField] private Block _block;

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

        private void OnUITilSlotClicked(UIBlockSlot slot)
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


        public void SetData(Block block, int slotIndex)
        {
            this._block = block;
            this.SlotIndex = slotIndex;

            _iconImage.sprite = _block.GetComponent<SpriteRenderer>().sprite;
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
                    UIPopupManager.Instance.SetNameInfoPopupContent(_block.BlockID.ToString());
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
