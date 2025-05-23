using UnityEngine;
using UnityEngine.UI;

namespace Match3.LevelEditor
{
    public class UIQuestInventory : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _closeBtn;
        [SerializeField] private Button _selectBtn;

        [SerializeField] private Transform _contentsParent;
        [SerializeField] private UIQuestSlot _uiQuestSlotPrefab;
        [SerializeField] private UIQuestSlot[] _uiSlots;

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

                QuestDataSO questData = GameDataManager.Instance.QuestDataSos[_currentSelectSlotIndex];
                Quest quest = new Quest()
                {
                    QuestID = questData.QuestID,
                    Quantity = 1
                };
                GridManager.Instance.AddQuest(quest);       
            });

            GridManager.Instance.OnGridInitialized += LoadAllUIBlocks;
            UIQuestSlot.OnClicked += OnBlockSlotClickTriggered;



            this.gameObject.SetActive(false);
        }


        private void OnDestroy()
        {
            _closeBtn.onClick.RemoveAllListeners();
            _selectBtn.onClick.RemoveAllListeners();


            GridManager.Instance.OnGridInitialized -= LoadAllUIBlocks;
            UIQuestSlot.OnClicked -= OnBlockSlotClickTriggered;
        }

        private void LoadAllUIBlocks()
        {
            _uiSlots = new UIQuestSlot[GameDataManager.Instance.QuestDataSos.Length];
            for (int i = 0; i < GameDataManager.Instance.QuestDataSos.Length; i++)
            {
                QuestDataSO questData = GameDataManager.Instance.QuestDataSos[i];

                UIQuestSlot slot = Instantiate(_uiQuestSlotPrefab, _contentsParent);
                slot.SetData(questData, i);
                _uiSlots[i] = slot;
            }
        }

        private void OnBlockSlotClickTriggered(UIQuestSlot slot)
        {
            _currentSelectSlotIndex = slot.SlotIndex;
        }
    }
}
