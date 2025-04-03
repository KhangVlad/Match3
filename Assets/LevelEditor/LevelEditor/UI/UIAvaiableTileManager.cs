using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace Match3.LevelEditor
{
    public class UIAvaiableTileManager : MonoBehaviour
    {
        [SerializeField] private Button _addNewBtn;

        [SerializeField] private UIAvaiableTileSlot _uiSlotPrefab;
        public List<UIAvaiableTileSlot> UISlots = new();


        private void Start()
        {
            _addNewBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                UIInventoryManager.Instance.DisplayTileInventory(true);
            });


            GridManager.Instance.OnLoadNewLevelData += OnLoadNewLevelData_LoadUI;
            GridManager.Instance.OnAvaiableTilesAdded += OnAvaiableTilesAdded_UpdateUI;
            GridManager.Instance.OnAvaiableTilesRemoval += OnAvaiableTilesRemoval_UpdateUI;
        }

   

        private void OnDestroy()
        {
            _addNewBtn.onClick.RemoveAllListeners();
            GridManager.Instance.OnLoadNewLevelData -= OnLoadNewLevelData_LoadUI;
            GridManager.Instance.OnAvaiableTilesAdded -= OnAvaiableTilesAdded_UpdateUI;
            GridManager.Instance.OnAvaiableTilesRemoval -= OnAvaiableTilesRemoval_UpdateUI;
        }


        private void OnLoadNewLevelData_LoadUI()
        {
            for(int i = 0; i < UISlots.Count; i++)
            {
                Destroy(UISlots[i].gameObject);
            }
            UISlots.Clear();

            for (int i = 0; i < GridManager.Instance.AvaiableTiles.Count; i++)
            {
                UIAvaiableTileSlot slot = Instantiate(_uiSlotPrefab, this.transform);
                slot.SetData(GridManager.Instance.AvaiableTiles[i], i);
                UISlots.Add(slot);
            }
        }


        private void OnAvaiableTilesRemoval_UpdateUI(int index)
        {
            Destroy(UISlots[index].gameObject);
            UISlots.RemoveAt(index);


            // update data when remove any tile
            for(int i = 0; i < GridManager.Instance.AvaiableTiles.Count; i++)
            {
                UISlots[i].SetData(GridManager.Instance.AvaiableTiles[i], i);
            }
        }

        private void OnAvaiableTilesAdded_UpdateUI(Tile tile, int slotIndex)
        {
            UIAvaiableTileSlot slot = Instantiate(_uiSlotPrefab, this.transform);
            slot.SetData(tile, slotIndex);
            UISlots.Add(slot);
        }
    }
}
