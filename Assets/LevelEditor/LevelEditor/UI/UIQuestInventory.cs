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


                Block block = GameDataManager.Instance.Blocks[_currentSelectSlotIndex];
                int index = (LevelEditorInventory.Instance.SelectedShortcutIndex - 1) % 4;
                LevelEditorInventory.Instance.SetBlock(block, index);
            });

            GridManager.Instance.OnGridInitialized += LoadAllUIBlocks;
            UIBlockSlot.OnClicked += OnBlockSlotClickTriggered;



            this.gameObject.SetActive(false);
        }


        private void OnDestroy()
        {
            _closeBtn.onClick.RemoveAllListeners();
            _selectBtn.onClick.RemoveAllListeners();


            GridManager.Instance.OnGridInitialized -= LoadAllUIBlocks;
            UIBlockSlot.OnClicked -= OnBlockSlotClickTriggered;
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

        private void OnBlockSlotClickTriggered(UIBlockSlot slot)
        {
            _currentSelectSlotIndex = slot.SlotIndex;
        }
    }
}
