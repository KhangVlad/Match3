using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Match3.Shares;
namespace Match3.LevelEditor
{
    public class UIAvaiableTileSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public static event System.Action<UIAvaiableTileSlot> OnClicked;

        [SerializeField] private Button _selectBtn;
        [SerializeField] private Button _removeBtn;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Sprite _defaultButtonSprite;
        [SerializeField] private Sprite _selectButtonSprite;


        public Tile Tile { get; private set; }
        public int SlotIndex { get; private set; }

        private bool _isEnter = false;

        private void Start()
        {
            _selectBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                OnClicked?.Invoke(this);
            });

            _removeBtn.onClick.AddListener(() =>
            {         
                AudioManager.Instance.PlayButtonSfx();
                GridManager.Instance.RemoveAvaiableTile(SlotIndex);
            });

            OnClicked += OnUITilSlotClicked;
            LevelEditorInventory.Instance.OnSelectChanged += OnSelectChanged_UpdateUI;
        }


        private void OnDestroy()
        {
            _selectBtn.onClick.RemoveAllListeners();
            _removeBtn.onClick.RemoveAllListeners();

            OnClicked -= OnUITilSlotClicked;
            LevelEditorInventory.Instance.OnSelectChanged -= OnSelectChanged_UpdateUI;
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
        }


        private void Select()
        {
            _selectBtn.interactable = false;
            _selectBtn.image.sprite = _selectButtonSprite;
        }

        private void Unselect()
        {
            _selectBtn.interactable = true;
            _selectBtn.image.sprite = _defaultButtonSprite;
        }


        private void OnUITilSlotClicked(UIAvaiableTileSlot slot)
        {
            if (slot == this)
            {
                LevelEditorInventory.Instance.Select(LevelEditorInventory.SelectSource.Tile, slot.SlotIndex);
                //Select();
            }
            //else
            //{
            //    Unselect();
            //}
        }



        public void OnPointerEnter(PointerEventData eventData)
        {
            _isEnter = true;
            Utilities.WaitAfter(0.25f, () =>
            {
                if (_isEnter)
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

        private void OnSelectChanged_UpdateUI(LevelEditorInventory.SelectSource source, int index)
        {
            if(source == LevelEditorInventory.SelectSource.Tile && index == SlotIndex)
            {
                Select();
            }
            else
            {
                Unselect();
            }
        }
    }
}
