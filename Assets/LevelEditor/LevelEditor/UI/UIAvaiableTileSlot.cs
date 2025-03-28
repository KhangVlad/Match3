using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace Match3.LevelEditor
{
    public class UIAvaiableTileSlot : MonoBehaviour
    {
        public static event System.Action<UIAvaiableTileSlot> OnClicked;

        [SerializeField] private Button _removeBtn;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Sprite _defaultButtonSprite;
        [SerializeField] private Sprite _selectButtonSprite;


        public Tile Tile { get; private set; }
        public int SlotIndex { get; private set; }
        public int ShortcutIndex { get; private set; }

        private bool _isEnter = false;

        private void Start()
        {
            _removeBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                GridManager.Instance.RemoveAvaiableTile(SlotIndex);
            });
        }

        private void OnDestroy()
        {
            _removeBtn.onClick.RemoveAllListeners();
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
    }
}
