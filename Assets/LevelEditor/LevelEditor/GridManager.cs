using UnityEngine;

namespace Match3.LevelEditor
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }
        public event System.Action OnGridLoaded;


        [Header("~Runtime")]
        public int Width;
        public int Height;
        [SerializeField] private Tile[] _tiles;


        #region Properties
        public Tile[] Tiles => _tiles;
        #endregion


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
            //Utilities.WaitAfterEndOfFrame(() =>
            //{
            //    LoadGridData(5, 6);
            //});
          
        }

        public void LoadGridData(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            _tiles = new Tile[width * height];

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Tile newTile = AddTile(x, y, TileID.RedCandle, BlockID.None);
                    newTile.UpdatePosition();

                    _tiles[x + y * Width] = newTile;
                }
            }

            OnGridLoaded?.Invoke();
        }

        private Tile AddTile(int x, int y, TileID tileID, BlockID blockID, bool display = true)
        {
            // Tile
            Tile tilePrefab = GameDataManager.Instance.GetTileByID(tileID);
            Tile tileInstance = Instantiate(tilePrefab, this.transform);
            tileInstance.Display(display);
            tileInstance.SetSpecialTile(SpecialTileID.None);
            tileInstance.SetGridPosition(x, y);
            _tiles[x + y * Width] = tileInstance;

            // Block
            Block blockPrefab = GameDataManager.Instance.GetBlockByID(blockID);
            Block blockInstance = Instantiate(blockPrefab, tileInstance.transform);
            blockInstance.transform.localPosition = Vector3.zero;

            tileInstance.SetBlock(blockInstance);

            return tileInstance;
        }
    }
}
