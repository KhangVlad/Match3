using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Match3.LevelEditor
{
    public class UITileInventory : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _closeBtn;
        [SerializeField] private Button _selectBtn;

        [SerializeField] private Transform _contentsParent;
        [SerializeField] private UITileSlot _uiTileSlotPrefab;
        [SerializeField] private UITileSlot[] _uiSlots;

        private int _currentSelectSlotIndex;

        private void Start()
        {
            _closeBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();

                this.gameObject.SetActive(false);
            });

            _selectBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                this.gameObject.SetActive(false);


                Tile tile = GameDataManager.Instance.Tiles[_currentSelectSlotIndex];
                int index = (LevelEditorInventory.Instance.SelectedShortcutIndex-1) % 4;
                LevelEditorInventory.Instance.SetTile(tile, index);
            });

            GridManager.Instance.OnGridLoaded += LoadAllUITiles;
            UITileSlot.OnClicked += OnTileSlotClickTriggered;



            this.gameObject.SetActive(false);
        }


        private void OnDestroy()
        {
            _closeBtn.onClick.RemoveAllListeners();
            _selectBtn.onClick.RemoveAllListeners();


            GridManager.Instance.OnGridLoaded -= LoadAllUITiles;
            UITileSlot.OnClicked -= OnTileSlotClickTriggered;
        }

        private void LoadAllUITiles()
        {
            _uiSlots = new UITileSlot[GameDataManager.Instance.Tiles.Length];
            for (int i = 0; i < GameDataManager.Instance.Tiles.Length; i++)
            {
                Tile tile = GameDataManager.Instance.Tiles[i];

                UITileSlot slot = Instantiate(_uiTileSlotPrefab, _contentsParent);
                slot.SetData(tile, i);
                _uiSlots[i] = slot;
            }
        }

        private void OnTileSlotClickTriggered(UITileSlot slot)
        {
            _currentSelectSlotIndex = slot.SlotIndex;
        }
    }
}
