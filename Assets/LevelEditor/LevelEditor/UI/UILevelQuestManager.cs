using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Match3.LevelEditor
{
    public class UILevelQuestManager : MonoBehaviour
    {
        [SerializeField] private Button _addNewBtn;

        [SerializeField] private UILevelQuestSlot _uiSlotPrefab;
        public List<UILevelQuestSlot> UISlots = new();


        private void Start()
        {
            _addNewBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                UIInventoryManager.Instance.DisplayQuestInventory(true);
            });


            GridManager.Instance.OnLoadNewLevelData += OnLoadNewLevelData_LoadUI;
            GridManager.Instance.OnQuestAdded += OnQuestAddedAdded_UpdateUI;
            GridManager.Instance.OnQuestRemoval += OnQuestRemoval_UpdateUI;
        }



        private void OnDestroy()
        {
            _addNewBtn.onClick.RemoveAllListeners();
            GridManager.Instance.OnLoadNewLevelData -= OnLoadNewLevelData_LoadUI;
            GridManager.Instance.OnQuestAdded -= OnQuestAddedAdded_UpdateUI;
            GridManager.Instance.OnQuestRemoval -= OnQuestRemoval_UpdateUI;
        }


        private void OnLoadNewLevelData_LoadUI()
        {
            for (int i = 0; i < UISlots.Count; i++)
            {
                Destroy(UISlots[i].gameObject);
            }
            UISlots.Clear();

            for (int i = 0; i < GridManager.Instance.Quests.Count; i++)
            {
                UILevelQuestSlot slot = Instantiate(_uiSlotPrefab, this.transform);
                slot.SetData(GridManager.Instance.Quests[i], i);
                UISlots.Add(slot);
            }
        }


        private void OnQuestRemoval_UpdateUI(int index)
        {
            Destroy(UISlots[index].gameObject);
            UISlots.RemoveAt(index);


            // update data when remove any tile
            for (int i = 0; i < GridManager.Instance.Quests.Count; i++)
            {
                UISlots[i].SetData(GridManager.Instance.Quests[i], i);
            }
        }

        private void OnQuestAddedAdded_UpdateUI(Quest quest, int slotIndex)
        {
            UILevelQuestSlot slot = Instantiate(_uiSlotPrefab, this.transform);
            slot.SetData(quest, slotIndex);
            UISlots.Add(slot);
        }
    }
}
