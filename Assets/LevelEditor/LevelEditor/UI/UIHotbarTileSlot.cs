using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Match3.Shares;

namespace Match3.LevelEditor
{
    public class UIHotbarTileSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public static event System.Action<UIHotbarTileSlot> OnClicked;

        [SerializeField] private Button _chooseBtn;
        [SerializeField] private Button _removeBtn;
        [SerializeField] private Button _addBtn;
        [SerializeField] private TextMeshProUGUI _shortcutIndexText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Sprite _defaultButtonSprite;
        [SerializeField] private Sprite _selectButtonSprite;



        public Tile Tile { get; private set; }
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

                Tile noneTile = GameDataManager.Instance.GetTileByID(TileID.None);
                //LevelEditorInventory.Instance.SetTile(noneTile, SlotIndex);
            });

            _addBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx(); 
                OnClicked?.Invoke(this);

                UIInventoryManager.Instance.DisplayTileInventory(true);
            });
        }

        private void OnDestroy()
        {
            _chooseBtn.onClick.RemoveAllListeners();
            _removeBtn.onClick.RemoveAllListeners();
            _addBtn.onClick.RemoveAllListeners();
        }

        public void SetData(Tile tile, int slotIndex)
        {
            this.Tile = tile;
            this.SlotIndex = slotIndex;

            _iconImage.sprite = tile.TileSprite;
            _iconImage.SetNativeSize();
            _iconImage.rectTransform.ScaleIcon(75, 75);

            switch (tile)
            {
                case NoneTile:
                    _iconImage.enabled = false;
                    break;
                default:
                    _iconImage.enabled = true;
                    _iconImage.color = Color.white;
                    break;
            }

            if (tile is NoneTile)
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
                if(_isEnter)
                {
                    UIPopupManager.Instance.DisplayUINameInfoPopup(true);
                    UIPopupManager.Instance.SetNameInfoPopupContent(Tile.ID.ToString());
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
