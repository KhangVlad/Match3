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

        [SerializeField] private Sprite _quantitySelectBG;
        [SerializeField] private Sprite _quantityUnSelectBG;
        [SerializeField] private Image _quantity_bg;

        [SerializeField] private Image _checkImage;
        // Cached
        private BoosterDataSo _data;


        public Booster CachedBooster {get; private set;}

        private void Start()
        {
            if (GameplayUserManager.Instance.IsBoosterEquipped(_data.ID))
            {
                _button.image.sprite = _selectSprite;
                _quantity_bg.sprite = _quantitySelectBG;
                _checkImage.enabled = true;
                _quantityText.gameObject.SetActive(false);
            }
            else
            {
                _button.image.sprite = _unselectSprite;
                _quantity_bg.sprite = _quantityUnSelectBG;
                _checkImage.enabled = false;
                _quantityText.gameObject.SetActive(true);
            }

            _button.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();

                if(GameplayUserManager.Instance.IsBoosterEquipped(_data.ID))
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
            CachedBooster = booster;
            BoosterDataSo boosterData = GameDataManager.Instance.GetBoosterDataByID(booster.BoosterID);
            this._data = boosterData;
            _icon.sprite = boosterData.Icon;
            _quantityText.text = booster.Quantity.ToString();
        }

        public void Equip()
        {
            GameplayUserManager.Instance.EquipBoosterByID(_data.ID);
            _button.image.sprite = _selectSprite;
            _quantity_bg.sprite = _quantitySelectBG;
            _checkImage.enabled = true;
            _quantityText.gameObject.SetActive(false);
        }

        public void Unequip()
        {
            GameplayUserManager.Instance.UnequipBoosterByID(_data.ID);
            _button.image.sprite = _unselectSprite;
            _quantity_bg.sprite = _quantityUnSelectBG;
            _quantityText.gameObject.SetActive(true);
            _checkImage.enabled = false;
        }
    }
}
