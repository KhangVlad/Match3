using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;


namespace Match3.LevelEditor
{
    public class UIHotbarBlockSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public static event System.Action<UIHotbarBlockSlot> OnClicked;

        [SerializeField] private Button _chooseBtn;
        [SerializeField] private Button _removeBtn;
        [SerializeField] private Button _addBtn;
        [SerializeField] private TextMeshProUGUI _shortcutIndexText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Sprite _defaultButtonSprite;
        [SerializeField] private Sprite _selectButtonSprite;

        public Block Block { get; private set; }
        public int SlotIndex { get; private set; }
        public int ShortcutIndex { get; private set; }

        private bool _isEnter = false;

        private void Start()
        {
            _chooseBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                OnClicked?.Invoke(this);
            });

            _removeBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                OnClicked?.Invoke(this);

                Block noneBlock = GameDataManager.Instance.GetBlockByID(BlockID.None);
                LevelEditorInventory.Instance.SetBlock(noneBlock, SlotIndex);
            });

            _addBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                OnClicked?.Invoke(this);

                UIInventoryManager.Instance.DisplayBlockInventory(true);
            });
        }

        private void OnDestroy()
        {
            _chooseBtn.onClick.RemoveAllListeners();
            _removeBtn.onClick.RemoveAllListeners();
            _addBtn.onClick.RemoveAllListeners();
        }

        public void SetData(Block block, int slotIndex)
        {
            this.Block = block;
            this.SlotIndex = slotIndex;

            _iconImage.sprite = block.GetComponent<SpriteRenderer>().sprite;
            _iconImage.SetNativeSize();
            _iconImage.rectTransform.ScaleIcon(75, 75);

            switch (block)
            {
                case NoneBlock:
                    _iconImage.enabled = false;
                    break;
                case VoidBlock:
                    _iconImage.enabled = true;
                    break;
                case FillBlock:
                    _iconImage.enabled = true;
                    break;
                default:
                    _iconImage.enabled = true;
                    break;
            }

            if(block is NoneBlock)
            {
                _removeBtn.gameObject.SetActive(false);
                _addBtn.gameObject.SetActive(true);
            }
            else
            {
                _removeBtn.gameObject.SetActive(true);
                _addBtn.gameObject.SetActive(false);
            }
        }

        public void SetShortcutText(int index)
        {
            ShortcutIndex = index;
            _shortcutIndexText.text = index.ToString();
        }

        public void Select()
        {
            _chooseBtn.interactable = false;
            _chooseBtn.image.sprite = _selectButtonSprite;
        }

        public void Unselect()
        {
            _chooseBtn.interactable = true;
            _chooseBtn.image.sprite = _defaultButtonSprite;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isEnter = true;

            Utilities.WaitAfter(0.25f, () =>
            {
                if (_isEnter)
                {
                    UIPopupManager.Instance.DisplayUINameInfoPopup(true);
                    UIPopupManager.Instance.SetNameInfoPopupContent(Block.BlockID.ToString());
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
