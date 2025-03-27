using UnityEngine;
using UnityEngine.UI;

namespace Match3.LevelEditor
{
    public class UIBlockInventory : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _closeBtn;
        [SerializeField] private Button _selectBtn;

        [SerializeField] private Transform _contentsParent;
        [SerializeField] private UIBlockSlot _uiBlockSlotPrefab;
        [SerializeField] private UIBlockSlot[] _uiSlots;

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

            GridManager.Instance.OnGridLoaded += LoadAllUIBlocks;
            UIBlockSlot.OnClicked += OnBlockSlotClickTriggered;



            this.gameObject.SetActive(false);
        }


        private void OnDestroy()
        {
            _closeBtn.onClick.RemoveAllListeners();
            _selectBtn.onClick.RemoveAllListeners();


            GridManager.Instance.OnGridLoaded -= LoadAllUIBlocks;
            UIBlockSlot.OnClicked -= OnBlockSlotClickTriggered;
        }

        private void LoadAllUIBlocks()
        {
            _uiSlots = new UIBlockSlot[GameDataManager.Instance.Blocks.Length];
            for (int i = 0; i < GameDataManager.Instance.Blocks.Length; i++)
            {
                Block block = GameDataManager.Instance.Blocks[i];

                UIBlockSlot slot = Instantiate(_uiBlockSlotPrefab, _contentsParent);
                slot.SetData(block, i);
                _uiSlots[i] = slot;
            }
        }

        private void OnBlockSlotClickTriggered(UIBlockSlot slot)
        {
            _currentSelectSlotIndex = slot.SlotIndex;
        }

    }
}
