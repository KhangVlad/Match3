using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Match3.LevelEditor
{
    public class UITileSlot : MonoBehaviour
    {
        public static System.Action<UITileSlot> OnClicked;
        [SerializeField] private Button _button;

        [Header("Sprites")]
        [SerializeField] private Sprite _defaultButtonSprite;
        [SerializeField] private Sprite _selectButtonSprite;
        private void Start()
        {
            _button.onClick.AddListener(() =>
            {
                OnClicked?.Invoke(this);
            });

            OnClicked += OnUITilSlotClicked;
        }

     
        private void OnDestroy()
        {
            _button.onClick.RemoveAllListeners();
            OnClicked -= OnUITilSlotClicked;
        }

        private void OnUITilSlotClicked(UITileSlot slot)
        {
            if(slot == this)
            {
                Select();
            }
            else
            {
                Unselect();
            }
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
    }
}
