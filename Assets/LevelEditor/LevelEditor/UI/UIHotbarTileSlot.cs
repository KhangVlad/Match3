using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Match3.LevelEditor
{
    public class UIHotbarTileSlot : MonoBehaviour
    {
        public static event System.Action<UIHotbarTileSlot> OnClicked;

        [SerializeField] private Button _chooseBtn;
        [SerializeField] private Button _removeBtn;
        [SerializeField] private TextMeshProUGUI _shortcutIndexText;
        [SerializeField] private Image _icon;
        [SerializeField] private Sprite _defaultButtonSprite;
        [SerializeField] private Sprite _selectButtonSprite;



        public Tile Tile { get; private set; }
        public int SlotIndex { get; private set; }
        public int ShortcutIndex { get; private set; }


        private void Start()
        {
            _chooseBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                OnClicked?.Invoke(this);
            });

            _removeBtn.onClick.AddListener(() =>
            {

            });
        }

        private void OnDestroy()
        {
            _chooseBtn.onClick.RemoveAllListeners();
        }

        public void SetData(Tile tile, int slotIndex)
        {
            this.Tile = tile;
            this.SlotIndex = slotIndex;

            _icon.sprite = tile.TileSprite;
            _icon.SetNativeSize();
            _icon.rectTransform.ScaleIcon(75, 75);
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

    }
}
