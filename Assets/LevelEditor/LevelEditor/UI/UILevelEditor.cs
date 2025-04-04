using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Match3.Shares;
using Match3.Enums;

namespace Match3.LevelEditor
{
    public class UILevelEditor : MonoBehaviour
    {
        public static UILevelEditor Instance { get; private set; }  

        private Canvas _canvas;

        [SerializeField] private Button _selectLevelBtn;

        [Space(10)]
        [SerializeField] private UIHotbarBlockSlot _uiBlockSlotPrefab;
        [SerializeField] private Transform _blockContentsParent;

        [Header("InputFileds")]
        [SerializeField] private TMP_InputField _maxTurnInputField;
        //[SerializeField] private TMP_InputField _characterIDInputField;
        //[SerializeField] private TMP_InputField _unlockCharacterIDInputField;
        [SerializeField] private TMP_InputField _heartUnlockInputField;

        // gird
        [SerializeField] private TMP_InputField _widthInputField;
        [SerializeField] private TMP_InputField _heightInputFiled;
        [SerializeField] private Button _resetLevelBtn;

        [Header("Character Selected")]
        [SerializeField] private UICharacterSelectedSlot _characterSelectedSlot;


        [Header("~Runtime")]
        //public UIHotbarTileSlot[] TileSlots;
        public UIHotbarBlockSlot[] BlockSlots;

    
        private void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
            _canvas = GetComponent<Canvas>();

         
        }

        private void Start()
        {
            GridManager.Instance.OnGridInitialized += OnGridInitialized_UpdateUI;
            LevelEditorInventory.Instance.OnInventoryInitialized += OnInventoryInitialized_LoadUI;

            UIHotbarBlockSlot.OnClicked += OnUiHorbarBlockSlotClicked;
            LevelEditorInventory.Instance.OnBlockChanged += OnBlockChanged_UpdateUI;
            GridManager.Instance.OnLoadNewLevelData += OnLoadNewLevelData_UpdateUI;
            LevelEditorInventory.Instance.OnSelectChanged += OnSelectChanged_UpdateUI;


            _selectLevelBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
             
                LevelDataV2 levelData = GridManager.Instance.GetLevelData();
                int index = LevelEditorManager.Instance.CurrentLevel;
                LevelEditorManager.Instance.SaveLevelData(index, levelData);

                Utilities.WaitAfterEndOfFrame(() =>
                {
                    UILevelEditorManager.Instance.DisplayUILevelSelector(true);
                });
            });

            _maxTurnInputField.onValueChanged.AddListener((value) =>
            {
                GridManager.Instance.MaxTurn = int.Parse(value);
            });
            //_characterIDInputField.onValueChanged.AddListener((value) =>
            //{
            //    GridManager.Instance.UnlockData[0] = int.Parse(value);
            //});
            //_unlockCharacterIDInputField.onValueChanged.AddListener((value) =>
            //{
            //    GridManager.Instance.UnlockData[1] = int.Parse(value);
            //});
            _heartUnlockInputField.onValueChanged.AddListener((value) =>
            {
                GridManager.Instance.Heart = int.Parse(value);
            });

            _widthInputField.onValueChanged.AddListener((value) =>
            {
                GridManager.Instance.Width = int.Parse(value);
            });
            _heightInputFiled.onValueChanged.AddListener((value) =>
            {
                GridManager.Instance.Height = int.Parse(value);
            });
            _resetLevelBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                GridManager.Instance.LoadGridData(GridManager.Instance.Width, GridManager.Instance.Height);
            });
        }


        private void OnDestroy()
        {
            GridManager.Instance.OnGridInitialized -= OnGridInitialized_UpdateUI;
            LevelEditorInventory.Instance.OnInventoryInitialized -= OnInventoryInitialized_LoadUI;

            UIHotbarBlockSlot.OnClicked -= OnUiHorbarBlockSlotClicked;
            LevelEditorInventory.Instance.OnBlockChanged -= OnBlockChanged_UpdateUI;
            GridManager.Instance.OnLoadNewLevelData -= OnLoadNewLevelData_UpdateUI;
            LevelEditorInventory.Instance.OnSelectChanged -= OnSelectChanged_UpdateUI;

            _selectLevelBtn.onClick.RemoveAllListeners();

            _maxTurnInputField.onValueChanged.RemoveAllListeners();
            //_characterIDInputField.onValueChanged.RemoveAllListeners();
            //_unlockCharacterIDInputField.onValueChanged.RemoveAllListeners();
            _heartUnlockInputField.onValueChanged.RemoveAllListeners();
            _widthInputField.onValueChanged.RemoveAllListeners();
            _heightInputFiled.onValueChanged.RemoveAllListeners();


            _resetLevelBtn.onClick.RemoveAllListeners();
        }


        public void DisplayCanvas(bool enable)
        {
            this._canvas.enabled = enable;
        }

        private void OnGridInitialized_UpdateUI()
        {
            DisplayCanvas(true);
        }

        private void OnInventoryInitialized_LoadUI()
        {
            int shortcutIndex = 1;
  
            BlockSlots = new UIHotbarBlockSlot[LevelEditorInventory.Instance.Blocks.Length];
            for (int i = 0; i < LevelEditorInventory.Instance.Blocks.Length; i++, shortcutIndex++)
            {
                Block block = LevelEditorInventory.Instance.Blocks[i];
                UIHotbarBlockSlot uiSlot = Instantiate(_uiBlockSlotPrefab, _blockContentsParent);
                uiSlot.SetData(block, i);
                BlockSlots[i] = uiSlot; 
            }
        }


        private void OnUiHorbarBlockSlotClicked(UIHotbarBlockSlot slot)
        {
            //UnselectAll();
            //slot.Select();
            LevelEditorInventory.Instance.Select(LevelEditorInventory.SelectSource.Block, slot.SlotIndex);
        }

   

        private void UnselectAll()
        {
            for (int i = 0; i < BlockSlots.Length; i++)
                BlockSlots[i].Unselect();
        }

        

        private void OnBlockChanged_UpdateUI(int index)
        {
            Block block = LevelEditorInventory.Instance.Blocks[index];
            BlockSlots[index].SetData(block, BlockSlots[index].SlotIndex);
        }

        private void OnLoadNewLevelData_UpdateUI()
        {
            _maxTurnInputField.text = GridManager.Instance.MaxTurn.ToString();
            //_characterIDInputField.text = GridManager.Instance.UnlockData[0].ToString();
            //_unlockCharacterIDInputField.text = GridManager.Instance.UnlockData[1].ToString();
            _heartUnlockInputField.text = GridManager.Instance.Heart.ToString();

            _widthInputField.text = GridManager.Instance.Width.ToString();
            _heightInputFiled.text = GridManager.Instance.Height.ToString();

            CharacterDataSO characterData = GameDataManager.Instance.GetCharacterDataByID((CharacterID)GridManager.Instance.Heart);
            _characterSelectedSlot.SetData(characterData);
        }


        private void OnSelectChanged_UpdateUI(LevelEditorInventory.SelectSource source, int index)
        {
            UnselectAll();
            if(source == LevelEditorInventory.SelectSource.Block)
            {
                BlockSlots[index].Select();
            }
        }
    }
}
