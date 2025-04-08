using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Match3.Shares;

namespace Match3.LevelEditor
{
    public class UICharacterSelectedSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public static event System.Action<UICharacterSelectedSlot> OnClicked;

        [SerializeField] private Button _addBtn;
        [SerializeField] private Image _iconImage;

        public CharacterDataSO CharacterData { get; private set; }
        private bool _isEnter = false;

        public enum SelectedType
        {
            Character = 1,
            UnlockedByCharcter = 2
        }

        [SerializeField] private SelectedType _selectedType;


        public SelectedType Type => _selectedType;

        private void Start()
        {
            _addBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                OnClicked?.Invoke(this);

                UIInventoryManager.Instance.DisplayCharacterInventory(true);
            });
        }

        private void OnDestroy()
        {
            _addBtn.onClick.RemoveAllListeners();
        }

        public void SetData(CharacterDataSO characterData)
        {
            CharacterData = characterData;
            _iconImage.sprite = characterData.sprite;
            _iconImage.SetNativeSize();
            _iconImage.rectTransform.ScaleIcon(75, 75);
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            _isEnter = true;
            Debug.Log("Enter");
            Utilities.WaitAfter(0.25f, () =>
            {
                if (_isEnter)
                {
                    UIPopupManager.Instance.DisplayUINameInfoPopup(true, new Vector2(-100, -150));
                    UIPopupManager.Instance.SetNameInfoPopupContent(CharacterData.id.ToString());
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