using UnityEngine;

namespace Match3.LevelEditor
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }
        public event System.Action OnGridLoaded;


        [Header("Grid")]
        [SerializeField] private Transform _gridSlotContentParent;
        [SerializeField] private GridSlot _gridPrefab;
        [SerializeField] private GridSlot[] _gridSlots;

        [Header("~Runtime")]
        public int Width;
        public int Height;
        [SerializeField] private Tile[] _tiles;


        #region Properties
        public Tile[] Tiles => _tiles;
        public bool IsGridLoaded { get; private set; } = false;
        #endregion


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;

            Application.targetFrameRate = 60;
        }


        private void Update()
        {
            if (IsGridLoaded == false) return;

            UpdateGridVisualize();

            if (Input.GetMouseButton(0))
            {
                Vector2Int gridPosition = GetGridPositionByMouse();
                if (IsValidGridTile(gridPosition.x, gridPosition.y))
                {
                    if (LevelEditorInventory.Instance.SelectedShortcutIndex < 5)
                    {
                        int index = gridPosition.x + gridPosition.y * Width;

                        Tile selectedTile = LevelEditorInventory.Instance.GetSelectedTile();
                        if (_tiles[index].CurrentBlock is NoneBlock)
                        {
                            if (_tiles[index].ID != selectedTile.ID)
                            {
                                Destroy(_tiles[index].gameObject);
                                _tiles[index] = null;

                                TileID tileID = selectedTile.ID;
                                Tile tile = AddTile(gridPosition.x, gridPosition.y, tileID, BlockID.None);
                                tile.UpdatePosition();
                            }
                        }
                        else
                        {
                            Destroy(_tiles[index].gameObject);
                            _tiles[index] = null;

                            TileID tileID = selectedTile.ID;
                            Tile tile = AddTile(gridPosition.x, gridPosition.y, tileID, BlockID.None);
                            tile.UpdatePosition();
                        }
                    }
                    else if (LevelEditorInventory.Instance.SelectedShortcutIndex < 9)
                    {
                        int index = gridPosition.x + gridPosition.y * Width;
                        Block selectedBlock = LevelEditorInventory.Instance.GetSelectedBlock();
                        if (_tiles[index].CurrentBlock.BlockID != selectedBlock.BlockID)
                        {
                            AddBlock(_tiles[index], selectedBlock);
                        }
                    }
                }
            }
            else if (Input.GetMouseButton(1))
            {

                Vector2Int gridPosition = GetGridPositionByMouse();
                if (IsValidGridTile(gridPosition.x, gridPosition.y))
                {
                    int index = gridPosition.x + gridPosition.y * Width;

                    if (_tiles[index].CurrentBlock is not NoneBlock || _tiles[index].ID != TileID.None)
                    {
                        Destroy(_tiles[index].gameObject);
                        _tiles[index] = null;
                        Tile tile = AddTile(gridPosition.x, gridPosition.y, TileID.None, BlockID.None);
                        tile.UpdatePosition();
                    }

                }
            }
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
                    Tile newTile = AddTile(x, y, TileID.None, BlockID.None);
                    newTile.UpdatePosition();

                    _tiles[x + y * Width] = newTile;
                }
            }


            // grid slots
            _gridSlots = new GridSlot[width * height];
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int index = x + y * Width;
                    GridSlot slot = Instantiate(_gridPrefab, _gridSlotContentParent);
                    slot.transform.position = _tiles[index].transform.position;
                    _gridSlots[index] = slot;
                }
            }

            IsGridLoaded = true;
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

        private void AddBlock(Tile tile, Block blockPrefab)
        {
            Destroy(tile.CurrentBlock.gameObject);
            Block blockInstance = Instantiate(blockPrefab, tile.transform);
            blockInstance.transform.localPosition = Vector3.zero;
            tile.SetBlock(blockInstance);
            blockInstance.GetComponent<SpriteRenderer>().enabled = true;
        }


        private void UpdateGridVisualize()
        {
            for (int i = 0; i < _gridSlots.Length; i++)
            {
                _gridSlots[i].Hover(false);
            }
            Vector2Int gridPosition = GetGridPositionByMouse();
            if (IsValidGridTile(gridPosition.x, gridPosition.y))
            {
                _gridSlots[gridPosition.x + gridPosition.y * Width].Hover(true);
            }
        }

        #region Utilities
        private bool IsValidGridTile(int x, int y)
        {
            return !(x < 0 || x >= Width || y < 0 || y >= Height) &&
               _tiles[x + y * Width] != null;
        }
        public Vector2Int GetGridPositionByMouse()
        {
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return GetGridPosition(worldPosition);
        }


        public Vector2Int GetGridPosition(Vector2 worldPosition)
        {
            int gridX = Mathf.FloorToInt(worldPosition.x / (TileExtension.TILE_WIDTH + TileExtension.SPACING_X));
            int gridY = Mathf.FloorToInt(worldPosition.y / (TileExtension.TILE_HEIGHT + TileExtension.SPACING_Y));
            return new Vector2Int(gridX, gridY);
        }


        public LevelData SaveAsLevelData()
        {
            int[,] blocks = new int[Width, Height];
            TileID[,] tiles = new TileID[Width, Height];
            TileID[] avaiableTiles = new TileID[] {
                TileID.RedFlower,
                TileID.YellowFlower,
                TileID.BlueFlower
            };
            int[,] quests =
            {
                {1, 20},
            };

            for(int i = 0; i < _tiles.Length; i++)
            {
                int x = i % Width;
                int y = i / Width;

                tiles[x, y] = _tiles[i].ID;
                blocks[x, y] = (int)_tiles[i].CurrentBlock.BlockID;
            }

            LevelData levelData = new LevelData(Width, Height)
            {
                MaxTurn = 30,
                Blocks = blocks,
                Tiles = tiles,
                AvaiableTiles = avaiableTiles,
                Quests = quests 
            };

            return levelData;
        }
    }
    #endregion

}

