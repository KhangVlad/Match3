using System;
using UnityEngine;

namespace Match3.LevelEditor
{
    public class UILevelEditor : MonoBehaviour
    {
        public static UILevelEditor Instance { get; private set; }  

        private Canvas _canvas;

        [SerializeField] private UIHotbarTileSlot _uiTileSlotPrefab;
        [SerializeField] private UIHotbarBlockSlot _uiBlockSlotPrefab;
        [SerializeField] private Transform _tileContentsParent;
        [SerializeField] private Transform _blockContentsParent;
        [Header("~Runtime")]
        public UIHotbarTileSlot[] TileSlots;
        public UIHotbarBlockSlot[] BlockSlots;

        //[SerializeField] private int _currentSelectedShortcutIndex = 0;

        //public int ShortcutIndex => _currentSelectedShortcutIndex;


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
            GridManager.Instance.OnGridLoaded += OnGridLoaded_UpdateUI;
            LevelEditorInventory.Instance.OnInventoryInitialized += OnInventoryInitialized_LoadUI;

            UIHotbarTileSlot.OnClicked += OnUiHorbarTileSlotClicked;
            UIHotbarBlockSlot.OnClicked += OnUiHorbarBlockSlotClicked;
            LevelEditorInventory.Instance.OnTileChanged += OnTileChanged_UpdateUI;
            LevelEditorInventory.Instance.OnBlockChanged += OnBlockChanged_UpdateUI;
        }

     

        private void OnDestroy()
        {
            GridManager.Instance.OnGridLoaded -= OnGridLoaded_UpdateUI;
            LevelEditorInventory.Instance.OnInventoryInitialized -= OnInventoryInitialized_LoadUI;

            UIHotbarTileSlot.OnClicked -= OnUiHorbarTileSlotClicked;
            UIHotbarBlockSlot.OnClicked -= OnUiHorbarBlockSlotClicked;
            LevelEditorInventory.Instance.OnTileChanged -= OnTileChanged_UpdateUI;
            LevelEditorInventory.Instance.OnBlockChanged -= OnBlockChanged_UpdateUI;
        }

        private void Update()
        {
            if (UIInventoryManager.Instance.InventoryBeingDisplayed) return;

            if(Input.GetKeyDown(KeyCode.Alpha1))
            {
                AudioManager.Instance.PlayButtonSfx();
                UnselectAll();
                Select(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                AudioManager.Instance.PlayButtonSfx();
                UnselectAll();
                Select(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                AudioManager.Instance.PlayButtonSfx();
                UnselectAll();
                Select(3);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                AudioManager.Instance.PlayButtonSfx();
                UnselectAll();
                Select(4);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                AudioManager.Instance.PlayButtonSfx();
                UnselectAll();
                Select(5);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                AudioManager.Instance.PlayButtonSfx();
                UnselectAll();
                Select(6);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                AudioManager.Instance.PlayButtonSfx();
                UnselectAll();
                Select(7);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                AudioManager.Instance.PlayButtonSfx();
                UnselectAll();
                Select(8);
            }
        }

        public void DisplayCanvas(bool enable)
        {
            this._canvas.enabled = enable;
        }

        private void OnGridLoaded_UpdateUI()
        {
            DisplayCanvas(true);
        }

        private void OnInventoryInitialized_LoadUI()
        {
            int shortcutIndex = 1;
            TileSlots = new UIHotbarTileSlot[LevelEditorInventory.Instance.Tiles.Length];
            for (int i = 0; i < LevelEditorInventory.Instance.Tiles.Length; i++, shortcutIndex++)
            {
                Tile tile = LevelEditorInventory.Instance.Tiles[i];
                UIHotbarTileSlot uiSlot = Instantiate(_uiTileSlotPrefab, _tileContentsParent);
                uiSlot.SetShortcutText(shortcutIndex);
                uiSlot.SetData(tile, i);
                TileSlots[i] = uiSlot;
            }

            BlockSlots = new UIHotbarBlockSlot[LevelEditorInventory.Instance.Blocks.Length];
            for (int i = 0; i < LevelEditorInventory.Instance.Blocks.Length; i++, shortcutIndex++)
            {
                Block block = LevelEditorInventory.Instance.Blocks[i];
                UIHotbarBlockSlot uiSlot = Instantiate(_uiBlockSlotPrefab, _blockContentsParent);
                uiSlot.SetShortcutText(shortcutIndex);
                uiSlot.SetData(block, i);
                BlockSlots[i] = uiSlot; 
            }
        }


        private void OnUiHorbarBlockSlotClicked(UIHotbarBlockSlot slot)
        {
            UnselectAll();
            slot.Select();
            LevelEditorInventory.Instance.SelectedShortcutIndex = slot.ShortcutIndex;       
        }

        private void OnUiHorbarTileSlotClicked(UIHotbarTileSlot slot)
        {
            UnselectAll();
            slot.Select();
            LevelEditorInventory.Instance.SelectedShortcutIndex = slot.ShortcutIndex;
        }

        private void UnselectAll()
        {
            for (int i = 0; i < TileSlots.Length; i++)
                TileSlots[i].Unselect();
            for (int i = 0; i < BlockSlots.Length; i++)
                BlockSlots[i].Unselect();
        }

        public void Select(int shortcutIndex)
        {
            LevelEditorInventory.Instance.SelectedShortcutIndex = shortcutIndex;
            for(int i = 0; i < TileSlots.Length;i++)
            {
                if (TileSlots[i].ShortcutIndex == LevelEditorInventory.Instance.SelectedShortcutIndex)
                {
                    TileSlots[i].Select();
                    return;
                }
            }

            for (int i = 0; i < BlockSlots.Length; i++)
            {
                if (BlockSlots[i].ShortcutIndex == LevelEditorInventory.Instance.SelectedShortcutIndex)
                {
                    BlockSlots[i].Select();
                    return;
                }
            }
        }


        private void OnTileChanged_UpdateUI(int index)
        {
            Tile tile = LevelEditorInventory.Instance.Tiles[index];
            TileSlots[index].SetData(tile, TileSlots[index].SlotIndex);
        }

        private void OnBlockChanged_UpdateUI(int index)
        {
            throw new NotImplementedException();
        }
    }
}
