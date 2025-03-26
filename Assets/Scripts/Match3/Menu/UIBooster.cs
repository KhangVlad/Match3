using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace Match3
{
    public class UIBooster : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _quantityText;


        [Header("Sprites")]
        [SerializeField] private Sprite _unselectSprite;
        [SerializeField] private Sprite _selectSprite;

        // Cached
        private BoosterDataSo _data;

        private void Start()
        {
            if (UserManager.Instance.IsBoosterEquipped(_data.ID))
            {
                _button.image.sprite = _selectSprite;
            }
            else
            {
                _button.image.sprite = _unselectSprite;
            }

            _button.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();

                if(UserManager.Instance.IsBoosterEquipped(_data.ID))
                {
                    Unequip();
                }
                else
                {
                    Equip();
                }
            });
        }

     
        private void OnDestroy()
        {
            _button.onClick.RemoveAllListeners();
        }

        public void SetBoosterData(Booster booster)
        {
            BoosterDataSo boosterData = GameDataManager.Instance.GetBoosterDataByID(booster.BoosterID);
            this._data = boosterData;
            _icon.sprite = boosterData.Icon;
            _quantityText.text = booster.Quantity.ToString();
        }

        public void Equip()
        {
            UserManager.Instance.EquipBoosterByID(_data.ID);
            _button.image.sprite = _selectSprite;
        }

        public void Unequip()
        {
            UserManager.Instance.UnequipBoosterByID(_data.ID);
            _button.image.sprite = _unselectSprite;
        }
    }
}
