using UnityEngine;
using System.Collections.Generic;
using Match3.Shares;

namespace Match3.LevelEditor
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }
        public event System.Action OnGridInitialized;
        public event System.Action OnLoadNewLevelData;
        public event System.Action OnDataHasChanged;

        public event System.Action<Tile, int> OnAvaiableTilesAdded;
        public event System.Action<int> OnAvaiableTilesRemoval;

        public event System.Action<Quest, int> OnQuestAdded;
        public event System.Action<int> OnQuestRemoval;



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

        // Level data cached
        public int MaxTurn = 25;
        public int Heart = 0;
        public int Energy = 0;
        public List<Tile> AvaiableTiles;
        public List<Quest> Quests;
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

        private void Update()
        {
            if (IsGridLoaded == false) return;

            UpdateGridVisualize();

            if (Input.GetMouseButton(0))
            {
                if (Utilities.IsPointerOverUIElement()) return;
                Vector2Int gridPosition = GetGridPositionByMouse();
                if (IsValidGridTile(gridPosition.x, gridPosition.y))
                {
                    if (LevelEditorInventory.Instance.Source == LevelEditorInventory.SelectSource.Tile)
                    {
                        int index = gridPosition.x + gridPosition.y * Width;

                        Tile selectedTile = AvaiableTiles[LevelEditorInventory.Instance.SelectIndex];
                        if (_tiles[index].CurrentBlock is NoneBlock)
                        {
                            if (_tiles[index].ID != selectedTile.ID)
                            {
                                Destroy(_tiles[index].gameObject);
                                _tiles[index] = null;

                                TileID tileID = selectedTile.ID;
                                Tile tile = AddTile(gridPosition.x, gridPosition.y, tileID, BlockID.None);
                                tile.UpdatePosition();

                                OnDataHasChanged?.Invoke();
                            }
                        }
                        else
                        {
                            Destroy(_tiles[index].gameObject);
                            _tiles[index] = null;

                            TileID tileID = selectedTile.ID;
                            Tile tile = AddTile(gridPosition.x, gridPosition.y, tileID, BlockID.None);
                            tile.UpdatePosition();

                            OnDataHasChanged?.Invoke();
                        }
                    }
                    else if (LevelEditorInventory.Instance.Source == LevelEditorInventory.SelectSource.Block)
                    {
                        int index = gridPosition.x + gridPosition.y * Width;
                        Block selectedBlock = LevelEditorInventory.Instance.GetSelectedBlock();
                        if (_tiles[index].CurrentBlock.BlockID != selectedBlock.BlockID)
                        {
                            AddBlock(_tiles[index], selectedBlock);

                            OnDataHasChanged?.Invoke();
                        }
                    }


                    //if (LevelEditorInventory.Instance.SelectedBlockIndex < 5)
                    //{
                    //    int index = gridPosition.x + gridPosition.y * Width;

                    //    Tile selectedTile = LevelEditorInventory.Instance.GetSelectedTile();
                    //    if (_tiles[index].CurrentBlock is NoneBlock)
                    //    {
                    //        if (_tiles[index].ID != selectedTile.ID)
                    //        {
                    //            Destroy(_tiles[index].gameObject);
                    //            _tiles[index] = null;

                    //            TileID tileID = selectedTile.ID;
                    //            Tile tile = AddTile(gridPosition.x, gridPosition.y, tileID, BlockID.None);
                    //            tile.UpdatePosition();

                    //            OnDataHasChanged?.Invoke();
                    //        }
                    //    }
                    //    else
                    //    {
                    //        Destroy(_tiles[index].gameObject);
                    //        _tiles[index] = null;

                    //        TileID tileID = selectedTile.ID;
                    //        Tile tile = AddTile(gridPosition.x, gridPosition.y, tileID, BlockID.None);
                    //        tile.UpdatePosition();

                    //        OnDataHasChanged?.Invoke();
                    //    }
                    //}
                    //else if (LevelEditorInventory.Instance.SelectedBlockIndex < 9)
                    //{
                    //    int index = gridPosition.x + gridPosition.y * Width;
                    //    Block selectedBlock = LevelEditorInventory.Instance.GetSelectedBlock();
                    //    if (_tiles[index].CurrentBlock.BlockID != selectedBlock.BlockID)
                    //    {
                    //        AddBlock(_tiles[index], selectedBlock);

                    //        OnDataHasChanged?.Invoke();
                    //    }
                    //}
                }
            }
            else if (Input.GetMouseButton(1))
            {
                if (Utilities.IsPointerOverUIElement()) return;
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

                        OnDataHasChanged?.Invoke();
                    }

                }
            }
        }

        public void LoadLevelData(LevelDataV2 levelData)
        {
            RemoveOldGridManagerData();

            MaxTurn = levelData.MaxTurn;
            Heart = levelData.Heart;
            Energy = levelData.Energy;


            Quests = new();
            if (levelData.Quests != null)
            {
                //Debug.Log(levelData.Quests.GetLength(0));
                for (int i = 0; i < levelData.Quests.GetLength(0); i++)
                {
                    Quest quest = new Quest()
                    {
                        QuestID = (QuestID)levelData.Quests[i, 0],
                        Quantity = levelData.Quests[i, 1]
                    };
                    Quests.Add(quest);
                }
            }


            AvaiableTiles = new();
            if (levelData.AvaiableTiles != null)
            {
                for (int i = 0; i < levelData.AvaiableTiles.Length; i++)
                {
                    Tile tile = GameDataManager.Instance.GetTileByID(levelData.AvaiableTiles[i]);
                    AvaiableTiles.Add(tile);
                }
            }


            this.Width = levelData.Tiles.GetLength(0);
            this.Height = levelData.Tiles.GetLength(1);
            _tiles = new Tile[Width * Height];

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    TileID tileID = levelData.Tiles[x, y];
                    BlockID blockID = (BlockID)levelData.Blocks[x, y];

                    Tile newTile = AddTile(x, y, tileID, blockID);
                    newTile.UpdatePosition();

                    _tiles[x + y * Width] = newTile;
                }
            }

            // grid slots
            _gridSlots = new GridSlot[Width * Height];
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

            if (IsGridLoaded == false)
            {
                IsGridLoaded = true;
                OnGridInitialized?.Invoke();
            }
            OnLoadNewLevelData?.Invoke();
        }

        public void LoadGridData(int width, int height)
        {
            RemoveOldGridManagerData();
            AvaiableTiles = new();
            Quests = new();
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

            if (IsGridLoaded == false)
            {
                IsGridLoaded = true;
                OnGridInitialized?.Invoke();
            }

            OnLoadNewLevelData?.Invoke();
        }


        private void RemoveOldGridManagerData()
        {
            if (_tiles != null)
            {
                for (int i = 0; i < _tiles.Length; i++)
                    Destroy(_tiles[i].gameObject);
                _tiles = null;
            }


            if (_gridSlots != null)
            {
                for (int i = 0; i < _gridSlots.Length; i++)
                    Destroy(_gridSlots[i].gameObject);
                _gridSlots = null;
            }
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

            blockInstance.GetComponent<SpriteRenderer>().enabled = true;

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
            if (Utilities.IsPointerOverUIElement()) return;
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


        public void AddAvaiableTile(Tile tile)
        {
            AvaiableTiles.Add(tile);
            OnAvaiableTilesAdded?.Invoke(tile, AvaiableTiles.Count - 1);
        }

        public void RemoveAvaiableTile(int index)
        {
            AvaiableTiles.RemoveAt(index);
            OnAvaiableTilesRemoval?.Invoke(index);
        }



        public void AddQuest(Quest quest)
        {
            Quests.Add(quest);
            OnQuestAdded?.Invoke(quest, Quests.Count - 1);
        }

        public void RemoveQuest(int index)
        {
            Quests.RemoveAt(index);
            OnQuestRemoval?.Invoke(index);
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


        //public LevelDataV1 GetLevelData()
        //{
        //    int[,] blocks = new int[Width, Height];
        //    TileID[,] tiles = new TileID[Width, Height];
        //    TileID[] avaiableTiles = new TileID[AvaiableTiles.Count];
        //    for (int i = 0; i < AvaiableTiles.Count; i++)
        //    {
        //        avaiableTiles[i] = AvaiableTiles[i].ID;
        //    }
        //    int[,] quests = new int[Quests.Count, 2];
        //    for (int i = 0; i < Quests.Count; i++)
        //    {
        //        quests[i, 0] = (int)Quests[i].QuestID;
        //        quests[i, 1] = (int)Quests[i].Quantity;
        //    }

        //    for (int i = 0; i < _tiles.Length; i++)
        //    {
        //        int x = i % Width;
        //        int y = i / Width;

        //        tiles[x, y] = _tiles[i].ID;
        //        blocks[x, y] = (int)_tiles[i].CurrentBlock.BlockID;
        //    }

        //    LevelDataV1 levelData = new LevelDataV1(Width, Height)
        //    {
        //        MaxTurn = this.MaxTurn,
        //        Blocks = blocks,
        //        Tiles = tiles,
        //        AvaiableTiles = avaiableTiles,
        //        Quests = quests,
        //        Unlock = this.UnlockData
        //    };

        //    return levelData;
        //}

        public LevelDataV2 GetLevelData()
        {
            int[,] blocks = new int[Width, Height];
            TileID[,] tiles = new TileID[Width, Height];
            TileID[] avaiableTiles = new TileID[AvaiableTiles.Count];
            for (int i = 0; i < AvaiableTiles.Count; i++)
            {
                avaiableTiles[i] = AvaiableTiles[i].ID;
            }
            int[,] quests = new int[Quests.Count, 2];
            for (int i = 0; i < Quests.Count; i++)
            {
                quests[i, 0] = (int)Quests[i].QuestID;
                quests[i, 1] = (int)Quests[i].Quantity;
            }

            for (int i = 0; i < _tiles.Length; i++)
            {
                int x = i % Width;
                int y = i / Width;

                tiles[x, y] = _tiles[i].ID;
                blocks[x, y] = (int)_tiles[i].CurrentBlock.BlockID;
            }

            LevelDataV2 levelData = new LevelDataV2(Width, Height)
            {
                MaxTurn = this.MaxTurn,
                Blocks = blocks,
                Tiles = tiles,
                AvaiableTiles = avaiableTiles,
                Quests = quests,
                Heart = this.Heart,
                Energy = this.Energy
            };

            return levelData;
        }
    }
    #endregion

}

