using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Match3
{
    public class UIGameplayBooster : MonoBehaviour
    {
        public static event System.Action<UIGameplayBooster> OnSelected;
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _quantityText;


        [Header("Sprites")]
        [SerializeField] private Sprite _unselectSprite;
        [SerializeField] private Sprite _selectSprite;

        // Cached
        private Booster _booster;
        private BoosterDataSo _data;

        private void Start()
        {
            _button.onClick.AddListener(() =>
            {
                if(Match3Grid.Instance.Canplay)
                {
                    AudioManager.Instance.PlayButtonSfx();
                    OnSelected?.Invoke(this);
                }
            });

            OnSelected += OnSelected_UpdateUI;
        }


        private void OnDestroy()
        {
            OnSelected -= OnSelected_UpdateUI;
            _button.onClick.RemoveAllListeners();

            if (_booster != null)
            {
                this._booster.OnBoosterUsed -= OnBoosterUsed_UpdateUI;
            }
        }

        public void SetBoosterData(Booster booster)
        {
            BoosterDataSo boosterData = GameDataManager.Instance.GetBoosterDataByID(booster.BoosterID);
            this._booster = booster;
            this._data = boosterData;
            _icon.sprite = boosterData.Icon;
            _quantityText.text = booster.Quantity.ToString();

            this._booster.OnBoosterUsed += OnBoosterUsed_UpdateUI;
        }

   
        public void Select()
        {
            _button.image.sprite = _selectSprite;
          
        }

        public void Unselect()
        {        
            _button.image.sprite = _unselectSprite;
        }

        private void OnSelected_UpdateUI(UIGameplayBooster booster)
        {        
            if (booster == this)
            {
                var selectedBooster = UserManager.Instance.SelectedGameplayBooster;

                if (selectedBooster == null || selectedBooster.BoosterID != _data.ID)
                {
                    UserManager.Instance.SelectGameplayBooster(_data.ID);
                    Select();
                    UIGameplayManager.Instance.DisplayUIGameplayBoosterManager(true);
                }
                else
                {
                    UserManager.Instance.UnselectGameplayBooster();
                    Unselect();
                    UIGameplayManager.Instance.DisplayUIGameplayBoosterManager(false);
                }
            }
            else
            {
                Unselect();
            }
        }

        private void OnBoosterUsed_UpdateUI()
        {
            _quantityText.text = _booster.Quantity.ToString();
            Unselect();
        }


    }
}
