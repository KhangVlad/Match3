using UnityEngine;
using Match3;

namespace Match3.LevelEditor
{
    public class LevelEditorInventory : MonoBehaviour
    {
        public static LevelEditorInventory Instance { get; private set; }
        public event System.Action OnInventoryInitialized;

        public Tile[] Tiles;
        public Block[] Blocks;
        

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
            GridManager.Instance.OnGridLoaded += OnGridLoaded_InitalizeInventory;
        }
        private void OnDestroy()
        {
            GridManager.Instance.OnGridLoaded -= OnGridLoaded_InitalizeInventory;
        }

        private void OnGridLoaded_InitalizeInventory()
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
    }
}
