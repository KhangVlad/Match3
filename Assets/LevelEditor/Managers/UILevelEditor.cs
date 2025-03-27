using System;
using UnityEngine;

namespace Match3.LevelEditor
{
    public class UILevelEditor : MonoBehaviour
    {
        private Canvas _canvas;

        [SerializeField] private UITileSlot _uiTileSlotPrefab;
        [SerializeField] private UIBlockSlot _uiBlockSlotPrefab;
        [SerializeField] private Transform _tileContentsParent;
        [SerializeField] private Transform _blockContentsParent;
        [Header("~Runtime")]
        public UITileSlot[] TileSlots;
        public UIBlockSlot[] BlockSlots;

        [SerializeField] private int _currentSelectedShortcutIndex = 0;


        public int ShortcutIndex => _currentSelectedShortcutIndex;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        private void Start()
        {
            GridManager.Instance.OnGridLoaded += OnGridLoaded_UpdateUI;
            LevelEditorInventory.Instance.OnInventoryInitialized += OnInventoryInitialized_LoadUI;

            UITileSlot.OnClicked += OnUiTileSlotClicked;
            UIBlockSlot.OnClicked += OnUiBlockSlotClicked;
        }

        private void OnDestroy()
        {
            GridManager.Instance.OnGridLoaded -= OnGridLoaded_UpdateUI;
            LevelEditorInventory.Instance.OnInventoryInitialized -= OnInventoryInitialized_LoadUI;

            UITileSlot.OnClicked -= OnUiTileSlotClicked;
            UIBlockSlot.OnClicked -= OnUiBlockSlotClicked;
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Alpha1))
            {
                UnselectAll();
                Select(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                UnselectAll();
                Select(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                UnselectAll();
                Select(3);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                UnselectAll();
                Select(4);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                UnselectAll();
                Select(5);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                UnselectAll();
                Select(6);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                UnselectAll();
                Select(7);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha8))
            {
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
            TileSlots = new UITileSlot[LevelEditorInventory.Instance.Tiles.Length];
            for (int i = 0; i < LevelEditorInventory.Instance.Tiles.Length; i++, shortcutIndex++)
            {
                UITileSlot uiSlot = Instantiate(_uiTileSlotPrefab, _tileContentsParent);
                uiSlot.SetShortcutText(shortcutIndex);
                TileSlots[i] = uiSlot;
            }

            BlockSlots = new UIBlockSlot[LevelEditorInventory.Instance.Blocks.Length];
            for (int i = 0; i < LevelEditorInventory.Instance.Blocks.Length; i++, shortcutIndex++)
            {
                UIBlockSlot uiSlot = Instantiate(_uiBlockSlotPrefab, _blockContentsParent);
                uiSlot.SetShortcutText(shortcutIndex);
                BlockSlots[i] = uiSlot; 
            }
        }


        private void OnUiBlockSlotClicked(UIBlockSlot slot)
        {
            UnselectAll();
            slot.Select();
            _currentSelectedShortcutIndex = slot.ShortcutIndex;       
        }

        private void OnUiTileSlotClicked(UITileSlot slot)
        {
            UnselectAll();
            slot.Select();
            _currentSelectedShortcutIndex = slot.ShortcutIndex;
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
            _currentSelectedShortcutIndex = shortcutIndex;
            for(int i = 0; i < TileSlots.Length;i++)
            {
                if (TileSlots[i].ShortcutIndex == _currentSelectedShortcutIndex)
                {
                    TileSlots[i].Select();
                    return;
                }
            }

            for (int i = 0; i < BlockSlots.Length; i++)
            {
                if (BlockSlots[i].ShortcutIndex == _currentSelectedShortcutIndex)
                {
                    BlockSlots[i].Select();
                    return;
                }
            }
        }
    }
}
