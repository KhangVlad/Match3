using UnityEngine;
using Match3;
using System.Collections.Generic;

namespace Match3.LevelEditor
{
    public class LevelEditorInventory : MonoBehaviour
    {
        public static LevelEditorInventory Instance { get; private set; }
        public event System.Action OnInventoryInitialized;
        public event System.Action<int> OnBlockChanged;
        public event System.Action<SelectSource, int> OnSelectChanged;

        public Block[] Blocks;
     
        [SerializeField] private SelectSource _source;
        public  enum SelectSource
        {
            None = 0,
            Tile = 1,
            Block = 2,
        }
        public int SelectIndex;

        public SelectSource Source => _source;


        private void Awake()
        {
            if (Instance != null && Instance != this)
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
            Blocks = new Block[System.Enum.GetValues(typeof(BlockID)).Length];
            int index = 0;
            foreach (BlockID blockID in System.Enum.GetValues(typeof(BlockID)))
            {
                Block block = GameDataManager.Instance.GetBlockByID(blockID);
                Blocks[index] = block;
                index++;
            }
   
            OnInventoryInitialized?.Invoke();
        }

        public void SetBlock(Block block, int index)
        {
            Blocks[index] = block;
            OnBlockChanged?.Invoke(index);
        }

        public void Select(SelectSource source, int index)
        {
            //this.sele
            this._source = source;
            this.SelectIndex = index;

            OnSelectChanged?.Invoke(source, index);
        }



        public Block GetSelectedBlock()
        {

            return Blocks[SelectIndex];
        }
    }
}
