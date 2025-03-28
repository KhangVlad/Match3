using UnityEngine;
using Match3;
using System.Collections.Generic;

namespace Match3.LevelEditor
{
    public class LevelEditorInventory : MonoBehaviour
    {
        public static LevelEditorInventory Instance { get; private set; }
        public event System.Action OnInventoryInitialized;
        public event System.Action<int> OnTileChanged;
        public event System.Action<int> OnBlockChanged;

        public Tile[] Tiles;
        public Block[] Blocks;


        public int SelectedShortcutIndex;


        private void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            GridManager.Instance.OnGridInitialized += OnGridInitialized_InitalizeInventory;
        }
        private void OnDestroy()
        {
            GridManager.Instance.OnGridInitialized -= OnGridInitialized_InitalizeInventory;
        }

        private void OnGridInitialized_InitalizeInventory()
        {
            Tiles = new Tile[4];
            for(int i = 0; i < 4; i++)
            {
                Tile tile = GameDataManager.Instance.GetTileByID(TileID.None);
                Tiles[i] = tile;    
            }

            Blocks = new Block[4];
            for (int i = 0; i < 4; i++)
            {
                Block block = GameDataManager.Instance.GetBlockByID(BlockID.None);
                Blocks[i] = block;  
            }

            OnInventoryInitialized?.Invoke();
        }

        public void SetTile(Tile tile, int index)
        {
            Tiles[index] = tile;
            OnTileChanged?.Invoke(index);
        }

        public void SetBlock(Block block, int index)
        {
            Blocks[index] = block;
            OnBlockChanged?.Invoke(index);
        }

        public Tile GetSelectedTile()
        {
            if(SelectedShortcutIndex > 0 && SelectedShortcutIndex < 5)
            {
                return Tiles[SelectedShortcutIndex - 1];
            }
            else
            {
                return Tiles[1];
            }
        }

        public Block GetSelectedBlock()
        {
            if (SelectedShortcutIndex > 4 && SelectedShortcutIndex < 9)
            {
                return Blocks[(SelectedShortcutIndex - 1)%4];
            }
            else
            {
                return Blocks[5];
            }
        }
    }
}
