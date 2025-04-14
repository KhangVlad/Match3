using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using System.Linq;

namespace Match3
{
    public class Match3Grid : MonoBehaviour
    {
        public static Match3Grid Instance { get; private set; }

        public event System.Action OnAfterPlayerMatchInput;
        public static event System.Action OnEndOfTurn;

        [Header("~Runtime")]
        public int Width;
        public int Height;
        [SerializeField] private LevelDataV2 _levelData;
        [SerializeField] private Tile[] _tiles;
        [SerializeField] private Tile _selectedTile;
        [SerializeField] private Tile _swappedTile;
        private List<Tile> _getTileList = new();
        private Vector2 _mouseDownPosition;
        private Vector2 _mouseUpPosition;
        private float _dragThreshold = 25f;

        private MatchID[] _matchBuffer;        // 0: default
                                               // 1: normal match
                                               // 2: special match

        [SerializeField] private TileID[] _prevTileIDs;
        [SerializeField] private List<int> _fillBlockIndices;

        private HashSet<int> _unlockTileSet;
        private Queue<SpecialTileQueue> _matchColorBurstQueue;
        private Queue<SpecialTileQueue> _matchBlastBombQueue;
        private Queue<SpecialTileQueue> _match4ColumnBombQueue;
        private Queue<SpecialTileQueue> _matchRowBombQueue;
        private List<int> _clearVerticalColumns;
        private List<int> _clearHorizontalRows;

        private HashSet<int> _triggeredMatch5Set;
        private Dictionary<int, Vector2> _colorBurstParentDictionary;
        private Dictionary<Tile, Tile> _match3Dictionary;
        private Dictionary<Tile, Tile> _match4Dictionary;
        private Dictionary<Tile, Tile> _match5Dictionary;
        private Dictionary<int, int> _fillDownDictionary = new();          // x, count

        private bool _hasMatch4 = false;
        private bool _hasColorBurst = false;
        private bool _hasMatch6 = false;

        // match 5
        private Vector2Int _blastBombPatternOffset = new Vector2Int(2, 2);
        private int[,] _blastBombPattern = new int[,]
        {
            { 0, 0, 1, 0, 0 },
            { 0, 1, 1, 1, 0 },
            { 1, 1, 1, 1, 1 },
            { 0, 1, 1, 1, 0 },
            { 0, 0, 1, 0, 0 }
        };


        // spider
        private int[] _4directions = new int[] { 1, 2, 3, 4 };
        private int[] _8directions = new int[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        private List<int> _spiderSpreadingList;
        [SerializeField] private bool _canPlay = true;


        private List<int[,]> _tShapes;
        private List<int[,]> _lShapes;


        // High Levels Logic
        public bool HandleReswapIfNotMatch { get; set; } = true;

        #region Properties
        public Tile[] Tiles => _tiles;
        public Tile SelectedTile => _selectedTile;
        public Tile SwappedTile => _swappedTile;
        public bool Canplay => _canPlay;
        public bool UseBoosterThisTurn { get; private set; } = false;
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
            _unlockTileSet = new();
            _matchColorBurstQueue = new();
            _matchBlastBombQueue = new();
            _match4ColumnBombQueue = new();
            _matchRowBombQueue = new();

            _triggeredMatch5Set = new();

            _clearVerticalColumns = new();
            _clearHorizontalRows = new();
            _spiderSpreadingList = new();
            _colorBurstParentDictionary = new();
            _match3Dictionary = new();
            _match4Dictionary = new();
            _match5Dictionary = new();



            _tShapes = new List<int[,]>
            {
                new int[,] // Original T-shape
                {
                    { 1, 1, 1 },
                    { 0, 1, 0 },
                    { 0, 1, 0 },
                },
                new int[,] // 90� Rotation
                {
                    { 0, 0, 1 },
                    { 1, 1, 1 },
                    { 0, 0, 1 },
                },
                new int[,] // 180� Rotation
                {
                    { 0, 1, 0 },
                    { 0, 1, 0 },
                    { 1, 1, 1 },
                },
                new int[,] // 270� Rotation
                {
                    { 1, 0, 0 },
                    { 1, 1, 1 },
                    { 1, 0, 0 },
                }
            };
            _lShapes = new List<int[,]>
            {
                new int[,] // Original L-shape
                {
                    { 1, 0, 0 },
                    { 1, 0, 0 },
                    { 1, 1, 1 },
                },
                new int[,] // 90� Rotation
                {
                    { 1, 1, 1 },
                    { 1, 0, 0 },
                    { 1, 0, 0 },
                },
                new int[,] // 180� Rotation
                {
                    { 1, 1, 1 },
                    { 0, 0, 1 },
                    { 0, 0, 1 },
                },
                new int[,] // 270� Rotation
                {
                    { 0, 0, 1 },
                    { 0, 0, 1 },
                    { 1, 1, 1 },
                }
            };


            OnAfterPlayerMatchInput += OnAfterPlayerMatchInput_ImplementGameLogic;
            GameplayUserManager.Instance.OnSelectGameplayBooster += OnSelectGameplayBooster_UpdateLogic;
            LoadGridLevel();
            LoadBoosters();

            _canPlay = false;

            StartCoroutine(ImplementGameLogicCoroutine(triggerEvent: false));
        }


        private void OnDestroy()
        {
            GameplayUserManager.Instance.OnSelectGameplayBooster -= OnSelectGameplayBooster_UpdateLogic;
            OnAfterPlayerMatchInput -= OnAfterPlayerMatchInput_ImplementGameLogic;
        }


        private void Update()
        {
            if (GameplayManager.Instance.CurrentState != GameplayManager.GameState.PLAYING) return;

            if (_canPlay)
            {
                if (Utilities.IsPointerOverUIElement()) return;

                if (Input.GetMouseButtonDown(0))
                {
                    Vector2Int gridPosition = InputHandler.Instance.GetGridPositionByMouse();
                    if (GameplayUserManager.Instance.SelectedGameplayBooster is HammerBooster)
                    {
                        Tile tile = _tiles[gridPosition.x + gridPosition.y * Width];
                        switch (tile.CurrentBlock)
                        {
                            case NoneBlock:
                            case Lock:
                            case BushBlock:
                            case Ice:
                            case HardIce:
                            case EternalIce:
                            case Wall01:
                            case Wall02:
                            case Wall03:
                            case Leaf01:
                            case Leaf02:
                            case Leaf03:
                            case Spider:
                            case SpiderNet:
                                UseBoosterThisTurn = true;
                                _canPlay = false;
                                _matchBuffer[tile.X + tile.Y * Width] = MatchID.SpecialMatch;
                                OnAfterPlayerMatchInput?.Invoke();
                                GameplayUserManager.Instance.SelectedGameplayBooster.Use();
                                GameplayUserManager.Instance.UnselectGameplayBooster();
                                break;
                            default:
                                Debug.Log("Case not found!!!!!!");
                                break;
                        }
                    }
                    else if (IsValidMatchTile(gridPosition))
                    {
                        if (_tiles[gridPosition.x + gridPosition.y * Width].CurrentBlock is NoneBlock)
                        {
                            _selectedTile = _tiles[gridPosition.x + gridPosition.y * Width];
                            _mouseDownPosition = Input.mousePosition;
                        }
                    }
                }

                if (Input.GetMouseButtonUp(0))
                {
                    if (_selectedTile != null)
                    {
                        _mouseUpPosition = Input.mousePosition;

                        Vector2 dragDir = DetectDragDirection(_mouseDownPosition, _mouseUpPosition);
                        if (dragDir == Vector2Int.left)
                        {
                            int leftTileX = _selectedTile.X - 1;
                            int leftTileY = _selectedTile.Y;
                            if (IsValidMatchTile(leftTileX, leftTileY))
                            {
                                Tile leftTile = _tiles[leftTileX + leftTileY * Width];
                                if (leftTile.CurrentBlock is not Lock &&
                                    leftTile.CurrentBlock is not BushBlock)
                                {
                                    _swappedTile = leftTile;
                                    SwapPosition(_selectedTile, _swappedTile);
                                    _selectedTile.MoveToGridPosition();
                                    _swappedTile.MoveToGridPosition();
                                }
                            }
                        }
                        else if (dragDir == Vector2Int.right)
                        {
                            int rightTileX = _selectedTile.X + 1;
                            int rightTileY = _selectedTile.Y;
                            if (IsValidMatchTile(rightTileX, rightTileY))
                            {
                                Tile rightTile = _tiles[rightTileX + rightTileY * Width];
                                if (rightTile.CurrentBlock is not Lock &&
                                    rightTile.CurrentBlock is not BushBlock)
                                {
                                    _swappedTile = rightTile;
                                    SwapPosition(_selectedTile, _swappedTile);
                                    _selectedTile.MoveToGridPosition();
                                    _swappedTile.MoveToGridPosition();
                                }
                            }
                        }
                        else if (dragDir == Vector2Int.up)
                        {
                            int upTileX = _selectedTile.X;
                            int upTileY = _selectedTile.Y + 1;
                            if (IsValidMatchTile(upTileX, upTileY))
                            {
                                Tile upTile = _tiles[upTileX + upTileY * Width];
                                if (upTile.CurrentBlock is not Lock &&
                                    upTile.CurrentBlock is not BushBlock)
                                {
                                    _swappedTile = upTile;
                                    SwapPosition(_selectedTile, _swappedTile);
                                    _selectedTile.MoveToGridPosition();
                                    _swappedTile.MoveToGridPosition();
                                }
                            }
                        }
                        else if (dragDir == Vector2Int.down)
                        {
                            int downTileX = _selectedTile.X;
                            int downTileY = _selectedTile.Y - 1;
                            if (IsValidMatchTile(downTileX, downTileY))
                            {
                                Tile downTile = _tiles[downTileX + downTileY * Width];
                                if (downTile.CurrentBlock is not Lock &&
                                    downTile.CurrentBlock is not BushBlock)
                                {
                                    _swappedTile = downTile;
                                    SwapPosition(_selectedTile, _swappedTile);
                                    _selectedTile.MoveToGridPosition();
                                    _swappedTile.MoveToGridPosition();
                                }
                            }
                        }

                        // if found swapped tile
                        if (_swappedTile != null)
                        {
                            _canPlay = false;
                            // wait animation completed
                            Utilities.WaitAfter(AnimationExtensions.TILE_MOVE_TIME, () =>
                            {
                                // Handle in-game booster
                                if (GameplayUserManager.Instance.SelectedGameplayBooster != null)
                                {
                                    Booster booster = GameplayUserManager.Instance.SelectedGameplayBooster;
                                    if (booster is FreeSwitchBooster ||
                                        booster is ExtraMoveBooster)
                                    {
                                        UseBoosterThisTurn = true;
                                        booster.Use();
                                        GameplayUserManager.Instance.UnselectGameplayBooster();
                                        OnAfterPlayerMatchInput?.Invoke();
                                    }
                                    else
                                    {
                                        //Debug.Log("HANMMER NOT HERE");
                                    }
                                }
                                else
                                {
                                    OnAfterPlayerMatchInput?.Invoke();
                                }
                            });
                        }
                    }
                }
            }



            //if (Input.GetKeyDown(KeyCode.S))
            //{
            //    ShuffleGrid();
            //}

            //if (Input.GetKeyDown(KeyCode.M))
            //{
            //    Debug.Log($"Can match: {CanMatch()}");
            //}

            //if (Input.GetKeyDown(KeyCode.W))
            //{
            //    StartCoroutine(ImplementGameLogicCoroutine());
            //}

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Vector2Int gridPosition = InputHandler.Instance.GetGridPositionByMouse();
                if (IsValidMatchTile(gridPosition))
                {
                    Destroy(_tiles[gridPosition.x + gridPosition.y * Width].gameObject);
                    _tiles[gridPosition.x + gridPosition.y * Width] = null;

                    Tile newTile = AddTile(gridPosition.x, gridPosition.y, TileID.RedFlower, BlockID.None);
                    newTile.UpdatePosition();
                    _prevTileIDs[gridPosition.x + gridPosition.y * Width] = TileID.RedFlower;
                }
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Vector2Int gridPosition = InputHandler.Instance.GetGridPositionByMouse();
                if (IsValidMatchTile(gridPosition))
                {
                    Destroy(_tiles[gridPosition.x + gridPosition.y * Width].gameObject);
                    _tiles[gridPosition.x + gridPosition.y * Width] = null;

                    Tile newTile = AddTile(gridPosition.x, gridPosition.y, TileID.BlueFlower, BlockID.None);
                    newTile.UpdatePosition();
                    // newTile.SetSpecialTile(SpecialTileID.RowBomb);
                    //newTile.SetSpecialTile(SpecialTileID.Match5);
                    //newTile.SetSpecialTile(SpecialTileID.Match6);

                    _prevTileIDs[gridPosition.x + gridPosition.y * Width] = TileID.None;
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Vector2Int gridPosition = InputHandler.Instance.GetGridPositionByMouse();
                if (IsValidMatchTile(gridPosition))
                {
                    Destroy(_tiles[gridPosition.x + gridPosition.y * Width].gameObject);
                    _tiles[gridPosition.x + gridPosition.y * Width] = null;

                    Tile newTile = AddTile(gridPosition.x, gridPosition.y, TileID.RedFlower, BlockID.None);
                    newTile.UpdatePosition();
                    newTile.SetSpecialTile(SpecialTileID.ColorBurst);

                    _prevTileIDs[gridPosition.x + gridPosition.y * Width] = TileID.RedFlower;
                }
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                Vector2Int gridPosition = InputHandler.Instance.GetGridPositionByMouse();
                if (IsValidGridTile(gridPosition.x, gridPosition.y))
                {
                    Destroy(_tiles[gridPosition.x + gridPosition.y * Width].gameObject);
                    _tiles[gridPosition.x + gridPosition.y * Width] = null;

                    Tile newTile = AddTile(gridPosition.x, gridPosition.y, TileID.YellowFlower, BlockID.None);
                    newTile.UpdatePosition();
                    newTile.SetSpecialTile(SpecialTileID.ColumnBomb);
                    _prevTileIDs[gridPosition.x + gridPosition.y * Width] = TileID.YellowFlower;
                }
            }
        }


        private void LoadGridLevel()
        {
            // 0: void block
            // 1: fill block
            // 2: none block <-> normal tile

            _levelData = LevelManager.Instance.LevelData;
            this.Width = _levelData.Blocks.GetLength(0);
            this.Height = _levelData.Blocks.GetLength(1);

            _tiles = new Tile[Width * Height];
            _prevTileIDs = new TileID[Width * Height];
            _fillBlockIndices = new();
            //Debug.Log($"{Width}  {Height}");

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    BlockID blockID = (BlockID)System.Enum.ToObject(typeof(BlockID), _levelData.Blocks[x, y]);
                    TileID tileID = _levelData.Tiles[x, y];
                    switch (blockID)
                    {
                        case BlockID.None:
                            Tile newTile = AddTile(x, y, tileID, BlockID.None);
                            newTile.UpdatePosition();
                            break;
                        case BlockID.Void:
                            newTile = AddTile(x, y, tileID, BlockID.Void);
                            newTile.UpdatePosition();
                            break;
                        case BlockID.Fill:
                            newTile = AddTile(x, y, tileID, BlockID.Fill);
                            newTile.UpdatePosition();

                            _fillBlockIndices.Add(x + y * Width);
                            break;
                        case BlockID.Lock:
                            newTile = AddTile(x, y, tileID, BlockID.Lock);
                            newTile.UpdatePosition();
                            break;
                        case BlockID.Ice:
                            newTile = AddTile(x, y, tileID, BlockID.Ice);
                            newTile.UpdatePosition();
                            break;
                        case BlockID.HardIce:
                            newTile = AddTile(x, y, tileID, BlockID.HardIce);
                            newTile.UpdatePosition();
                            break;
                        case BlockID.EternalIce:
                            newTile = AddTile(x, y, tileID, BlockID.EternalIce);
                            newTile.UpdatePosition();
                            break;
                        case BlockID.Bush:
                            newTile = AddTile(x, y, tileID, BlockID.Bush);
                            newTile.UpdatePosition();
                            break;
                        case BlockID.Leaf_01:
                            newTile = AddTile(x, y, tileID, BlockID.Leaf_01);
                            newTile.UpdatePosition();
                            break;
                        case BlockID.Leaf_02:
                            newTile = AddTile(x, y, tileID, BlockID.Leaf_02);
                            newTile.UpdatePosition();
                            break;
                        case BlockID.Leaf_03:
                            newTile = AddTile(x, y, tileID, BlockID.Leaf_03);
                            newTile.UpdatePosition();
                            break;
                        case BlockID.Wall_01:
                            newTile = AddTile(x, y, tileID, BlockID.Wall_01);
                            newTile.UpdatePosition();
                            break;
                        case BlockID.Wall_02:
                            newTile = AddTile(x, y, tileID, BlockID.Wall_02);
                            newTile.UpdatePosition();
                            break;
                        case BlockID.Wall_03:
                            newTile = AddTile(x, y, tileID, BlockID.Wall_03);
                            newTile.UpdatePosition();
                            break;
                        case BlockID.Spider:
                            newTile = AddTile(x, y, tileID, BlockID.Spider);
                            newTile.UpdatePosition();
                            break;
                        case BlockID.SpiderNet:
                            newTile = AddTile(x, y, tileID, BlockID.SpiderNet);
                            newTile.UpdatePosition();
                            break;
                        case BlockID.SpiderOnNet:
                            newTile = AddTile(x, y, tileID, BlockID.SpiderOnNet);
                            newTile.UpdatePosition();
                            break;
                        default:
                            Debug.LogError("Not set this case");
                            break;
                    }

                    _prevTileIDs[x + y * Width] = _tiles[x + y * Width].ID;
                }
            }

            // Initialize match buffer
            _matchBuffer = new MatchID[_tiles.Length];
            for (int i = 0; i < _matchBuffer.Length; i++)
            {
                SetMatchBuffer(i, MatchID.None);
            }
        }

        private void LoadBoosters()
        {
            for (int i = 0; i < GameplayUserManager.Instance.EquipBoosters.Count; i++)
            {
                int attempts = 0;
                Booster booster = GameplayUserManager.Instance.EquipBoosters[i];
                if (booster.Quantity == 0) continue;
                while (true)
                {
                    int randomTileIndex = Random.Range(0, _tiles.Length);
                    int x = randomTileIndex % Width;
                    int y = randomTileIndex / Width;
                    if (IsvalidPlaceBoosterTiles(x, y))
                    {
                        switch (booster.BoosterID)
                        {
                            case BoosterID.AxisBomb:
                                if (Random.Range(0, 2) == 0)
                                {
                                    _tiles[randomTileIndex].SetSpecialTile(SpecialTileID.RowBomb);
                                }
                                else
                                {
                                    _tiles[randomTileIndex].SetSpecialTile(SpecialTileID.ColumnBomb);
                                }
                                break;
                            case BoosterID.BlastBomb:
                                _tiles[randomTileIndex].SetSpecialTile(SpecialTileID.BlastBomb);
                                break;
                            case BoosterID.ColorBurst:
                                _tiles[randomTileIndex].SetSpecialTile(SpecialTileID.ColorBurst);
                                break;
                            default:
                                Debug.LogError("Case not found");
                                break;
                        }

                        booster.Use();
                        break;
                    }

                    attempts++;
                    if (attempts > 20)
                    {
                        Debug.LogError("Something went wrong");
                        break;
                    }
                }
            }

            bool IsvalidPlaceBoosterTiles(int x, int y)
            {
                return !(x < 0 || x >= Width || y < 0 || y >= Height) &&
                _tiles[x + y * Width] != null &&
               _tiles[x + y * Width].CurrentBlock is NoneBlock &&
               _tiles[x + y * Width].SpecialProperties == SpecialTileID.None;
            }
        }


        private Tile AddTile(int x, int y, TileID tileID, BlockID blockID, bool display = true)
        {
            // Tile
            Tile tilePrefab = GameDataManager.Instance.GetTileByID(tileID);
            Tile tileInstance = Instantiate(tilePrefab, this.transform);
            tileInstance.Display(display);
            tileInstance.SetSpecialTile(SpecialTileID.None);
            tileInstance.TileTransform.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            tileInstance.SetGridPosition(x, y);
            _tiles[x + y * Width] = tileInstance;

            // Block
            Block blockPrefab = GameDataManager.Instance.GetBlockByID(blockID);
            Block blockInstance = Instantiate(blockPrefab, tileInstance.transform);
            blockInstance.transform.localPosition = Vector3.zero;

            tileInstance.SetBlock(blockInstance);

            return tileInstance;
        }


        private void ShuffleGrid()
        {
            Debug.Log("ShuffleGrid");

            for (int i = _tiles.Length - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);

                Tile currTile = _tiles[i];
                Tile randomTile = _tiles[randomIndex];
                if (currTile == null || randomTile == null ||
                    currTile.CurrentBlock is not NoneBlock ||
                    randomTile.CurrentBlock is not NoneBlock) continue;

                SwapPosition(currTile, randomTile);
                currTile.UpdatePosition();
                randomTile.UpdatePosition();
            }
        }


        private void OnSelectGameplayBooster_UpdateLogic(Booster booster)
        {
            if (booster is HammerBooster)
            {
                _selectedTile = null;
                _swappedTile = null;
            }
        }



        private void OnAfterPlayerMatchInput_ImplementGameLogic()
        {
            StartCoroutine(ImplementGameLogicCoroutine(triggerEvent: true));
        }


        private IEnumerator ImplementGameLogicCoroutine(bool triggerEvent)
        {
            //Debug.Log($"ImplementGameLogicCoroutine");
            int attempts = 0;
            bool isMatch = false;


            while (true)
            {
                int colorBurstCount = 0;

                //if (attempts != 0)
                //    yield return new WaitForSeconds(0.1f);

                yield return StartCoroutine(HandleMatchCoroutine());
                HandleTriggerAllSpecialTiles();

                bool hasMatched = false;
                // play match animation
                _unlockTileSet.Clear();
                if (_match3Dictionary.Count > 0)
                {
                    //foreach (var tile in _match3Dictionary)
                    //{
                    //    Tile t = tile.Value;
                    //    Tile nb = tile.Key;
                    //    if (t != null && nb != null)
                    //    {
                    //        float offsetX = -(t.transform.position.x - nb.transform.position.x) * 0.0f;
                    //        float offsetY = (t.transform.position.y - nb.transform.position.y) * 0.0f;
                    //        Vector2 offsetPosition = new Vector2(offsetX, offsetY);
                    //        //if (GameplayManager.Instance.HasTileQuest(nb, out QuestID questID) == false)
                    //        //{
                    //        //    nb.transform.DOMove((Vector2)t.transform.position, 0.2f).SetEase(Ease.InSine);
                    //        //}
                    //        //nb.transform.DOMove((Vector2)t.transform.position, 0.2f).SetEase(Ease.InSine);
                    //    }
                    //}

                    foreach (var tile in _match3Dictionary)
                    {
                        Tile t = tile.Value;
                        Tile nb = tile.Key;
                        if (t != null && nb != null)
                        {
                            TilePositionInfo tileInfo = new TilePositionInfo(t.ID, t.transform.position);
                            TilePositionInfo nbTileInfo = new TilePositionInfo(nb.ID, (Vector2)nb.transform.position);
                            MatchAnimManager.Instance.Add(tileInfo, tileInfo);
                            MatchAnimManager.Instance.Add(tileInfo, nbTileInfo);
                        }
                    }
                }


                if (_match4Dictionary.Count > 0)
                {

                    foreach (var tile in _match4Dictionary)
                    {
                        Tile t = tile.Value;
                        Tile nb = tile.Key;
                        if (t != null && nb != null)
                        {
                            float offsetX = -(t.transform.position.x - nb.transform.position.x) * 0.0f;
                            float offsetY = (t.transform.position.y - nb.transform.position.y) * 0.0f;
                            Vector2 offsetPosition = new Vector2(offsetX, offsetY);

                            //if (GameplayManager.Instance.HasTileQuest(nb, out QuestID questID) == false)
                            //{

                            //}
                            nb.transform.DOMove((Vector2)t.transform.position, 0.2f).SetEase(Ease.InSine);
                        }
                    }
                    yield return new WaitForSeconds(0.2f);
                    int multiplier = 1;
                    foreach (var e in _match4Dictionary)
                    {
                        Tile t = e.Value;
                        Tile nb = e.Key;
                        if (t != null && nb != null)
                        {
                            TilePositionInfo tileInfo = new TilePositionInfo(t.ID, t.transform.position);
                            TilePositionInfo nbTileInfo = new TilePositionInfo(nb.ID, (Vector2)nb.transform.position);// + new Vector2(0,0.02f) * multiplier);
                            MatchAnimManager.Instance.Add(tileInfo, nbTileInfo);
                            MatchAnimManager.Instance.Add(tileInfo, tileInfo);
                        }
                        multiplier++;
                        // if (spectialTileEffectPostiionSet.Contains(new Vector2Int(t.X, t.Y)) == false)
                        // {
                        //     spectialTileEffectPostiionSet.Add(new Vector2Int(t.X, t.Y));
                        // }
                    }

                    // HashSet<Vector2Int> spectialTileEffectPostiionSet = new();

                }

                if (_match5Dictionary.Count > 0)
                {
                    foreach (var tile in _match5Dictionary)
                    {
                        Tile t = tile.Value;
                        Tile nb = tile.Key;
                        if (t != null && nb != null)
                        {
                            float offsetX = -(t.transform.position.x - nb.transform.position.x) * 0.0f;
                            float offsetY = (t.transform.position.y - nb.transform.position.y) * 0.0f;
                            Vector2 offsetPosition = new Vector2(offsetX, offsetY);


                            nb.transform.DOMove((Vector2)t.transform.position, 0.2f).SetEase(Ease.InSine);
                        }
                    }
                    yield return new WaitForSeconds(0.2f);
                    foreach (var e in _match5Dictionary)
                    {
                        Tile t = e.Value;
                        Tile nb = e.Key;
                        if (t != null && nb != null)
                        {
                            TilePositionInfo tileInfo = new TilePositionInfo(t.ID, t.transform.position);
                            TilePositionInfo nbTileInfo = new TilePositionInfo(nb.ID, (Vector2)nb.transform.position);
                            MatchAnimManager.Instance.Add(tileInfo, nbTileInfo);
                            MatchAnimManager.Instance.Add(tileInfo, tileInfo);
                        }
                    }
                }


                for (int i = 0; i < _matchBuffer.Length; i++)
                {
                    Tile tile = _tiles[i];
                    if (tile == null) continue;

                    MatchID matchID = _matchBuffer[i];
                    if (matchID == MatchID.Match)
                    {
                        AudioManager.Instance.PlayMatch3Sfx();
                    }

                    // Match & Unlock
                    if (matchID == MatchID.Match)
                    {
                        tile.Match(_tiles, Width);
                        HandleUnlockTileNeighbors(tile);

                        hasMatched = true;
                    }
                    else if (matchID == MatchID.SpecialMatch)
                    {
                        tile.Match(_tiles, Width);
                        if (_unlockTileSet.Contains(i) == false)
                        {
                            _unlockTileSet.Add(i);
                            tile.Unlock();
                        }
                        hasMatched = true;
                    }
                    else if (matchID == MatchID.ColorBurst)
                    {
                        // vfx
                        if (_colorBurstParentDictionary.ContainsKey(tile.X + tile.Y * Width))
                        {
                            Vector2 startColoBurstPosition = _colorBurstParentDictionary[tile.X + tile.Y * Width];
                            PlaySingleColorBurstLineVfx(startColoBurstPosition, tile.transform.position, 0.4f);
                            if (colorBurstCount % 3 == 0)
                            {
                                Debug.Log("Color burst wait");
                                //yield return new WaitForSeconds(0.2f);
                            }
                            colorBurstCount++;
                        }


                        tile.Match(_tiles, Width);
                        if (_unlockTileSet.Contains(i) == false)
                        {
                            _unlockTileSet.Add(i);
                            tile.Unlock();
                        }
                        hasMatched = true;
                    }
                    SetMatchBuffer(i, MatchID.None);
                }


                // match 6
                while (_matchColorBurstQueue.Count > 0)
                {
                    var e = _matchColorBurstQueue.Dequeue();
                    int x = e.Index % Width;
                    int y = e.Index / Width;

                    Tile tile = AddTile(x, y, TileID.None, BlockID.None, display: false);
                    tile.UpdatePosition();
                    tile.SetSpecialTile(SpecialTileID.ColorBurst);
                }

                // match 5
                while (_matchBlastBombQueue.Count > 0)
                {
                    var e = _matchBlastBombQueue.Dequeue();
                    int x = e.Index % Width;
                    int y = e.Index / Width;

                    Tile tile = AddTile(x, y, e.TileID, BlockID.None, display: false);
                    tile.UpdatePosition();
                    tile.SetSpecialTile(SpecialTileID.BlastBomb);
                }


                // match 4 horizontal
                while (_matchRowBombQueue.Count > 0)
                {
                    var e = _matchRowBombQueue.Dequeue();

                    int x = e.Index % Width;
                    int y = e.Index / Width;

                    Tile tile = AddTile(x, y, e.TileID, BlockID.None, display: false);
                    tile.UpdatePosition();
                    tile.Display(true);
                    tile.SetSpecialTile(SpecialTileID.RowBomb);
                }


                // match 4 vertical
                while (_match4ColumnBombQueue.Count > 0)
                {
                    var e = _match4ColumnBombQueue.Dequeue();

                    int x = e.Index % Width;
                    int y = e.Index / Width;

                    Tile tile = AddTile(x, y, e.TileID, BlockID.None, display: false);
                    tile.UpdatePosition();
                    tile.Display(true);
                    tile.SetSpecialTile(SpecialTileID.ColumnBomb);
                }



                MatchAnimManager.Instance.PlayCollectAnimation();


                if (hasMatched)
                {
                    // yield return new WaitForSeconds(0.3f);
                    for (int i = 0; i < _tiles.Length; i++)
                    {
                        if (_tiles[i] != null)
                            _tiles[i].Display(true);
                    }

                    yield return new WaitForSeconds(0.05f);
                    yield return StartCoroutine(AutoFillCoroutine());
                }


                _hasMatch6 = false;
                _hasColorBurst = false;
                _hasMatch4 = false;


                attempts++;
                if (attempts > 50)
                {
                    Debug.LogError("Something went wrong");
                    break;
                }

                Debug.Log($"HasMatch: {HasMatch()}");

                if (HasMatch() == false)
                {
                    break;
                }

                if (hasMatched)
                {
                    yield return StartCoroutine(ShuffleGridUntilCanMatchCoroutine());
                }
            }

            if (HandleReswapIfNotMatch)
            {
                if (isMatch == false && _selectedTile != null && _swappedTile != null)
                {
                    SwapPosition(_selectedTile, _swappedTile);
                    _selectedTile.MoveToGridPosition();
                    _swappedTile.MoveToGridPosition();
                }
            }

            _selectedTile = null;
            _swappedTile = null;

            yield return new WaitForSeconds(0.2f);
            _canPlay = true;

            _triggeredMatch5Set.Clear();
            _colorBurstParentDictionary.Clear();

            _match3Dictionary.Clear();
            _match4Dictionary.Clear();
            _match5Dictionary.Clear();


            if (triggerEvent)
            {
                if (UseBoosterThisTurn == false)
                {
                    HandleSpiderNetSpreading();
                    HandleLeafGrowth();
                }
                else
                {
                    UseBoosterThisTurn = false;
                }

                OnEndOfTurn?.Invoke();
            }
        }

        private IEnumerator ShuffleGridUntilCanMatchCoroutine()
        {
            // shuffle grid if no match found
            while (true)
            {
                if (CanMatch())
                {
                    break;
                }

                yield return new WaitForSeconds(0.5f);
                ShuffleGrid();
            }
        }

        private bool HasMatch()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Tile currTile = _tiles[x + y * Width];
                    if (currTile == null) continue;
                    if (currTile.ID == TileID.None) continue;
                    if (currTile.SpecialProperties == SpecialTileID.BlastBomb) continue;
                    if (currTile.SpecialProperties == SpecialTileID.ColorBurst) continue;
                    if (currTile.CurrentBlock is not NoneBlock &&
                        currTile.CurrentBlock is not Lock &&
                        currTile.CurrentBlock is not BushBlock) continue;

                    int sameIDCount = 0;

                    // Horizontal check
                    for (int h = x + 1; h < Width; h++)
                    {
                        Tile nbTile = _tiles[h + y * Width]; // Use y instead of currTile.Y
                        if (nbTile == null) break;
                        if (nbTile.SpecialProperties == SpecialTileID.BlastBomb) break;
                        if (nbTile.SpecialProperties == SpecialTileID.ColorBurst) break;
                        if (nbTile != null && currTile.ID == nbTile.ID &&
                            (nbTile.CurrentBlock is NoneBlock ||
                            nbTile.CurrentBlock is Lock ||
                            nbTile.CurrentBlock is BushBlock))
                        {
                            sameIDCount++;
                        }
                        else
                        {
                            break; // Stop if IDs are not the same
                        }
                    }

                    if (sameIDCount >= 2)
                    {
                        return true;
                    }

                    sameIDCount = 0;
                    for (int v = y + 1; v < Height; v++)
                    {
                        Tile nbTile = _tiles[x + v * Width]; // Correct index calculation
                        if (nbTile is null) break;
                        if (nbTile.SpecialProperties == SpecialTileID.BlastBomb) break;
                        if (nbTile.SpecialProperties == SpecialTileID.ColorBurst) break;
                        if (nbTile != null && currTile.ID == nbTile.ID &&
                            (nbTile.CurrentBlock is NoneBlock || nbTile.CurrentBlock is Lock || nbTile.CurrentBlock is BushBlock))
                        {
                            sameIDCount++;
                        }
                        else
                        {
                            break; // Stop if IDs are not the same
                        }
                    }

                    if (sameIDCount >= 2)
                    {
                        return true;
                    }

                }
            }

            return false;
        }
        private bool CanMatch()
        {
            bool HasEnoughSameNeighborsDown(TileID tileID, int newX, int newY)
            {
                // down, down of down
                if (IsValidMatchTile(newX, newY - 1))
                {
                    if (IsValidMatchTile(newX, newY - 2))
                    {
                        Tile downTile = _tiles[newX + (newY - 1) * Width];
                        Tile downOfDownTile = _tiles[newX + (newY - 2) * Width];
                        if (tileID == downTile.ID && tileID == downOfDownTile.ID &&
                            (downTile.CurrentBlock is NoneBlock || downTile.CurrentBlock is Lock || downTile.CurrentBlock is BushBlock) &&
                            (downOfDownTile.CurrentBlock is NoneBlock || downOfDownTile.CurrentBlock is Lock || downOfDownTile.CurrentBlock is BushBlock))
                        {
                            // Debug.Log($"D: {newX}  {newY}");
                            return true;
                        }
                    }
                }
                return false;
            }
            bool HasEnoughSameNeighborsTop(TileID tileID, int newX, int newY)
            {
                // top, top of top
                if (IsValidMatchTile(newX, newY + 1))
                {
                    if (IsValidMatchTile(newX, newY + 2))
                    {
                        Tile topTile = _tiles[newX + (newY + 1) * Width];
                        Tile topOfTopTile = _tiles[newX + (newY + 2) * Width];
                        if (tileID == topTile.ID && tileID == topOfTopTile.ID &&
                            (topTile.CurrentBlock is NoneBlock || topTile.CurrentBlock is Lock || topTile.CurrentBlock is BushBlock) &&
                            (topOfTopTile.CurrentBlock is NoneBlock || topOfTopTile.CurrentBlock is Lock || topOfTopTile.CurrentBlock is BushBlock))
                        {
                            // Debug.Log($"C: {newX}  {newY}");
                            return true;
                        }
                    }
                }
                return false;
            }
            bool HasEnoughSameNeighborsRight(TileID tileID, int newX, int newY)
            {
                // right, right of right
                if (IsValidMatchTile(newX + 1, newY))
                {
                    if (IsValidMatchTile(newX + 2, newY))
                    {
                        Tile rightTile = _tiles[newX + 1 + newY * Width];
                        Tile rightOfRightTile = _tiles[newX + 2 + newY * Width];

                        if (tileID == rightTile.ID && tileID == rightOfRightTile.ID &&
                            (rightTile.CurrentBlock is NoneBlock || rightTile.CurrentBlock is Lock || rightTile.CurrentBlock is BushBlock) &&
                            (rightOfRightTile.CurrentBlock is NoneBlock || rightOfRightTile.CurrentBlock is Lock || rightOfRightTile.CurrentBlock is BushBlock))
                        {
                            // Debug.Log($"B: {newX}  {newY} ");
                            return true;
                        }
                    }
                }
                return false;
            }
            bool HasEnoughSameNeighborsLeft(TileID tileID, int newX, int newY)
            {
                // left, left of left
                if (IsValidMatchTile(newX - 1, newY))
                {
                    if (IsValidMatchTile(newX - 2, newY))
                    {
                        Tile leftTile = _tiles[newX - 1 + newY * Width];
                        Tile leftOfLeftTile = _tiles[newX - 2 + newY * Width];

                        if (tileID == leftTile.ID && tileID == leftOfLeftTile.ID &&
                            (leftTile.CurrentBlock is NoneBlock || leftTile.CurrentBlock is Lock || leftTile.CurrentBlock is BushBlock) &&
                            (leftOfLeftTile.CurrentBlock is NoneBlock || leftOfLeftTile.CurrentBlock is Lock || leftOfLeftTile.CurrentBlock is BushBlock))
                        {
                            // Debug.Log($"A: {tileID} {newX} {newY}");
                            return true;
                        }
                    }
                }
                return false;
            }

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Tile tile = _tiles[x + y * Width];
                    if (tile == null) continue;

                    // left 
                    if (IsValidMatchTile(x - 1, y))
                    {
                        if (HasEnoughSameNeighborsLeft(tile.ID, x - 1, y))
                        {
                            return true;
                        }
                        if (HasEnoughSameNeighborsTop(tile.ID, x - 1, y))
                        {
                            return true;
                        }
                        if (HasEnoughSameNeighborsDown(tile.ID, x - 1, y))
                        {
                            return true;
                        }
                    }

                    // right 
                    if (IsValidMatchTile(x + 1, y))
                    {
                        if (HasEnoughSameNeighborsRight(tile.ID, x + 1, y))
                        {
                            // Debug.Log($"{tile.X} {tile.Y}  {tile.Data.ID}");
                            return true;
                        }
                        if (HasEnoughSameNeighborsTop(tile.ID, x + 1, y))
                        {
                            return true;
                        }
                        if (HasEnoughSameNeighborsDown(tile.ID, x + 1, y))
                        {
                            return true;
                        }
                    }

                    // up 
                    if (IsValidMatchTile(x, y + 1))
                    {
                        if (HasEnoughSameNeighborsTop(tile.ID, x, y + 1))
                        {
                            return true;
                        }
                        if (HasEnoughSameNeighborsLeft(tile.ID, x, y + 1))
                        {
                            return true;
                        }
                        if (HasEnoughSameNeighborsRight(tile.ID, x, y + 1))
                        {
                            return true;
                        }
                    }

                    // down 
                    if (IsValidMatchTile(x, y - 1))
                    {
                        if (HasEnoughSameNeighborsDown(tile.ID, x, y - 1))
                        {
                            return true;
                        }
                        if (HasEnoughSameNeighborsLeft(tile.ID, x, y - 1))
                        {
                            return true;
                        }
                        if (HasEnoughSameNeighborsRight(tile.ID, x, y - 1))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private IEnumerator HandleMatchCoroutine()
        {
            FindFlashBomb();
            HandleSpecialMatch();
            // Debug.Log("CheckMatch3");
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Tile currTile = _tiles[x + y * Width];
                    if (currTile == null) continue;
                    if (currTile.ID == TileID.None) continue;
                    if (currTile.SpecialProperties == SpecialTileID.BlastBomb) continue;
                    if (currTile.SpecialProperties == SpecialTileID.ColorBurst) continue;
                    if (currTile.CurrentBlock is not NoneBlock && currTile.CurrentBlock is not Lock && currTile.CurrentBlock is not BushBlock) continue;
                    if (_matchBuffer[x + y * Width] != MatchID.None) continue;


                    int sameIDCountInRow = 0;
                    int sameIDCountInColumn = 0;

                    // Horizontal check
                    for (int h = x + 1; h < Width; h++)
                    {
                        Tile nbTile = _tiles[h + y * Width];
                        if (nbTile == null)
                        {
                            break;
                        }
                        if (nbTile.SpecialProperties == SpecialTileID.BlastBomb) break;
                        if (nbTile.SpecialProperties == SpecialTileID.ColorBurst) break;

                        if (currTile.ID == nbTile.ID &&
                            (nbTile.CurrentBlock is NoneBlock ||
                            nbTile.CurrentBlock is Lock ||
                            nbTile.CurrentBlock is BushBlock))
                        {
                            sameIDCountInRow++;
                        }
                        else
                        {
                            break; // Stop if IDs are not the same
                        }
                    }

                    // vertical check
                    for (int v = y + 1; v < Height - 1; v++)
                    {
                        Tile nbTile = _tiles[x + v * Width]; // Correct index calculation

                        if (nbTile == null) break;
                        if (nbTile.SpecialProperties == SpecialTileID.BlastBomb) break;
                        if (nbTile.SpecialProperties == SpecialTileID.ColorBurst) break;

                        if (currTile.ID == nbTile.ID &&
                            (nbTile.CurrentBlock is NoneBlock ||
                            nbTile.CurrentBlock is Lock ||
                            nbTile.CurrentBlock is BushBlock))
                        {
                            sameIDCountInColumn++;
                        }
                        else
                        {
                            break; // Stop if IDs are not the same
                        }
                    }

                    HandleMatchSameIDInRow(currTile, sameIDCountInRow);
                    HandleMatchSameIDInColumn(currTile, sameIDCountInColumn);
                }
            }
            yield return null;

            MatchPreviousTilesDifferent();
        }
        private void HandleUnlockTileNeighbors(Tile tile)
        {
            if (IsValidGridTile(tile.X - 1, tile.Y))
            {
                int index = tile.X - 1 + tile.Y * Width;
                if (_unlockTileSet.Contains(index) == false)
                {
                    _unlockTileSet.Add(index);
                    _tiles[index].Unlock();
                }
            }
            if (IsValidGridTile(tile.X + 1, tile.Y))
            {
                int index = tile.X + 1 + tile.Y * Width;
                if (_unlockTileSet.Contains(index) == false)
                {
                    _unlockTileSet.Add(index);
                    _tiles[index].Unlock();
                }
            }
            if (IsValidGridTile(tile.X, tile.Y - 1))
            {
                int index = tile.X + (tile.Y - 1) * Width;
                if (_unlockTileSet.Contains(index) == false)
                {
                    _unlockTileSet.Add(index);
                    _tiles[index].Unlock();
                }
            }
            if (IsValidGridTile(tile.X, tile.Y + 1))
            {
                int index = tile.X + (tile.Y + 1) * Width;
                if (_unlockTileSet.Contains(index) == false)
                {
                    _unlockTileSet.Add(index);
                    _tiles[index].Unlock();
                }
            }
        }

        private void HandleTriggerAllSpecialTiles()
        {
            for (int i = 0; i < _matchBuffer.Length; i++)
            {
                Tile tile = _tiles[i];
                MatchID matchID = _matchBuffer[i];

                if (matchID.IsSpecialMatch())
                {
                    if (_triggeredMatch5Set.Contains(i) == false)
                    {
                        if (tile.SpecialProperties == SpecialTileID.RowBomb)
                        {
                            //Debug.Log("Handle Special Match 4 Horizontal");
                            EnableHorizontalMatchBuffer(tile.Y);
                            PlayClearHorizontalVFX(tile);
                        }
                        else if (tile.SpecialProperties == SpecialTileID.ColumnBomb)
                        {
                            //Debug.Log("Handle Special Match 4 Vertical");
                            EnableVerticalMatchBuffer(tile.X);
                            PlayClearVerticalVFX(tile);
                        }
                        else if (tile.SpecialProperties == SpecialTileID.BlastBomb)
                        {
                            //Debug.Log("Handle Special Match 5");
                            PlayFlashBombVfx(tile);
                            HandleBlastBomb(tile);
                        }
                        else if (tile.SpecialProperties == SpecialTileID.ColorBurst)
                        {
                            Debug.Log("Handle Special Color Burst");
                            Utilities.Shuffle(_4directions);
                            for (int j = 0; j < _4directions.Length; j++)
                            {
                                bool found = false;
                                switch (_4directions[j])
                                {
                                    case 1: // left
                                        if (IsValidMatchTile(tile.X - 1, tile.Y))
                                        {
                                            found = true;
                                            ClearAllBoardTileID(tile, _tiles[tile.X - 1 + tile.Y * Width].ID);
                                        }
                                        break;
                                    case 2: // right
                                        if (IsValidMatchTile(tile.X + 1, tile.Y))
                                        {
                                            found = true;
                                            ClearAllBoardTileID(tile, _tiles[tile.X + 1 + tile.Y * Width].ID);
                                        }
                                        break;
                                    case 3: // up
                                        if (IsValidMatchTile(tile.X, tile.Y + 1))
                                        {
                                            found = true;
                                            ClearAllBoardTileID(tile, _tiles[tile.X + (tile.Y + 1) * Width].ID);
                                        }
                                        break;
                                    case 4: // down
                                        if (IsValidMatchTile(tile.X, tile.Y - 1))
                                        {
                                            found = true;
                                            ClearAllBoardTileID(tile, _tiles[tile.X + (tile.Y - 1) * Width].ID);
                                        }
                                        break;
                                }

                                if (found) break;
                            }
                        }
                    }
                }
            }
        }

        private IEnumerator AutoFillCoroutine(System.Action onCompleted = null)
        {
            yield return StartCoroutine(FillGridCoroutine());
            onCompleted?.Invoke();
        }

        private void HandleLeafGrowth()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (IsValidGridTile(x, y))
                    {
                        int index = x + y * Width;
                        Tile tile = _tiles[index];
                        if (tile.CurrentBlock is Leaf02)
                        {
                            Leaf02 leaf = ((Leaf02)tile.CurrentBlock);
                            leaf.ExistTurnCount++;
                            if (leaf.ExistTurnCount > 2)
                            {
                                tile.ChangeBlock(BlockID.Leaf_01);
                            }
                        }
                        else if (tile.CurrentBlock is Leaf03)
                        {
                            // tile.ChangeBlock(BlockID.Leaf_02);
                            Leaf03 leaf = ((Leaf03)tile.CurrentBlock);
                            leaf.ExistTurnCount++;
                            if (leaf.ExistTurnCount > 2)
                            {
                                tile.ChangeBlock(BlockID.Leaf_02);
                            }
                        }
                    }
                }
            }
        }

        private void HandleSpiderNetSpreading()
        {
            _spiderSpreadingList.Clear();
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int index = x + y * Width;
                    if (_tiles[index] == null) continue;
                    if (_tiles[index].CurrentBlock is Spider ||
                        _tiles[index].CurrentBlock is SpiderOnNet)
                    {
                        _tiles[index].ChangeBlock(BlockID.SpiderNet);
                        _spiderSpreadingList.Add(index);
                    }
                }
            }

            bool spreadToNeighbor = false;
            for (int i = 0; i < _spiderSpreadingList.Count; i++)
            {
                int tileIndex = _spiderSpreadingList[i];
                int x = tileIndex % Width;
                int y = tileIndex / Width;

                Utilities.Shuffle(_8directions);

                for (int d = 0; d < _8directions.Length; d++)
                {
                    int dx = 0, dy = 0;
                    switch (_8directions[d])
                    {
                        case 1: dx = -1; dy = 0; break; // Left
                        case 2: dx = 1; dy = 0; break;  // Right
                        case 3: dx = 0; dy = 1; break;  // Up
                        case 4: dx = 0; dy = -1; break; // Down
                        case 5: dx = -1; dy = 1; break; // Top-left
                        case 6: dx = 1; dy = 1; break;  // Top-right
                        case 7: dx = -1; dy = -1; break; // Bottom-left
                        case 8: dx = 1; dy = -1; break;  // Bottom-right
                    }

                    int nx = x + dx;
                    int ny = y + dy;

                    if (IsValidNonBlockTile(nx, ny))
                    {
                        Tile neighborTile = _tiles[nx + ny * Width];
                        neighborTile.ChangeBlock(BlockID.Spider);
                        spreadToNeighbor = true;
                        Debug.Log("A");
                        break;
                    }
                }
                if (spreadToNeighbor)
                    break;
            }

            if (spreadToNeighbor == false)
            {
                for (int i = 0; i < _spiderSpreadingList.Count; i++)
                {
                    int tileIndex = _spiderSpreadingList[i];
                    int x = tileIndex % Width;
                    int y = tileIndex / Width;

                    Utilities.Shuffle(_8directions);
                    for (int d = 0; d < _8directions.Length; d++)
                    {
                        int dx = 0, dy = 0;
                        switch (_8directions[d])
                        {
                            case 1: dx = -1; dy = 0; break; // Left
                            case 2: dx = 1; dy = 0; break;  // Right
                            case 3: dx = 0; dy = 1; break;  // Up
                            case 4: dx = 0; dy = -1; break; // Down
                            case 5: dx = -1; dy = 1; break; // Top-left
                            case 6: dx = 1; dy = 1; break;  // Top-right
                            case 7: dx = -1; dy = -1; break; // Bottom-left
                            case 8: dx = 1; dy = -1; break;  // Bottom-right
                        }

                        int nx = x + dx;
                        int ny = y + dy;

                        if (IsValidGridTile(nx, ny))
                        {
                            Tile neighborTile = _tiles[nx + ny * Width];
                            if (neighborTile.CurrentBlock is SpiderNet)
                            {
                                neighborTile.ChangeBlock(BlockID.SpiderOnNet);
                                spreadToNeighbor = true;
                                break;
                            }
                        }
                    }
                    if (spreadToNeighbor)
                        break;
                }
            }

            if (spreadToNeighbor == false)
            {
                //   Debug.Log("Last case");
                for (int i = 0; i < _spiderSpreadingList.Count; i++)
                {
                    int tileIndex = _spiderSpreadingList[i];
                    int x = tileIndex % Width;
                    int y = tileIndex / Width;
                    _tiles[x + y * Width].ChangeBlock(BlockID.SpiderOnNet);
                }
            }
        }


        private void SetMatchBuffer(int index, MatchID matchID)
        {
            if (_matchBuffer[index] == MatchID.None)
            {
                _matchBuffer[index] = matchID;
                return;
            }
            if (matchID == MatchID.None)
            {
                _matchBuffer[index] = matchID;
                return;
            }

            if (matchID == MatchID.ColorBurst ||
                (matchID == MatchID.SpecialMatch && _matchBuffer[index] != MatchID.ColorBurst))
            {
                _matchBuffer[index] = matchID;
            }
        }


        private void HandleMatchSameIDInRow(Tile tile, int sameIDCountInRow)
        {
            if (sameIDCountInRow < 2) return;

            int x = tile.X;
            int y = tile.Y;
            bool hasMatch4Horizontal = false;
            bool hasMatch4Vertical = false;
            _clearVerticalColumns.Clear();
            for (int h = 0; h <= sameIDCountInRow; h++) // Loop correctly over sameIDCount
            {
                int index = (x + h) + y * Width;

                SpecialTileID specialTileID = _tiles[index].SpecialProperties;

                switch (specialTileID)
                {
                    default:
                    case SpecialTileID.None:
                        break;
                    case SpecialTileID.RowBomb:
                        hasMatch4Horizontal = true;
                        break;
                    case SpecialTileID.ColumnBomb:
                        _clearVerticalColumns.Add(index);
                        hasMatch4Vertical = true;
                        break;
                }
            }


            if (sameIDCountInRow >= 4)
            {
                _hasColorBurst = true;
                bool foundColorBurstTile = false;
                if (_selectedTile != null && _swappedTile != null)
                {
                    for (int h = 0; h <= sameIDCountInRow; h++)
                    {
                        int index = (x + h) + y * Width;
                        if (_selectedTile.Equal(_tiles[index]) || _swappedTile.Equals(index))
                        {
                            _matchColorBurstQueue.Enqueue(new SpecialTileQueue(_tiles[index].ID, index));
                            foundColorBurstTile = true;
                            break;
                        }
                    }
                }

                for (int h = 0; h <= sameIDCountInRow; h++)
                {
                    int index = (x + h) + y * Width;
                    if (foundColorBurstTile == false)
                    {
                        if (_tiles[index].ID != _prevTileIDs[index])
                        {
                            int xx = index % Width;
                            int yy = index / Width;
                            _matchColorBurstQueue.Enqueue(new SpecialTileQueue(_tiles[index].ID, index));
                            foundColorBurstTile = true;
                        }
                    }
                }

                if (foundColorBurstTile == false)
                {
                    Debug.Log($"Not found match5 tile oriign");
                    int index = (x + sameIDCountInRow / 2) + y * Width;
                    _matchColorBurstQueue.Enqueue(new SpecialTileQueue(_tiles[index].ID, index));
                }

                for (int h = 0; h <= sameIDCountInRow; h++) // Loop correctly over sameIDCount
                {
                    int index = (x + h) + y * Width;
                    //_matchBuffer[index] = MatchID.Match;
                    SetMatchBuffer(index, MatchID.Match);
                }

            }
            else if (sameIDCountInRow == 3)
            {
                _hasMatch4 = true;
                if (hasMatch4Vertical)
                {
                    //Debug.Log($"Clear vertical");
                    for (int xx = 0; xx < _clearVerticalColumns.Count; xx++)
                    {
                        int index = _clearVerticalColumns[xx];
                        int frameX = index % Width;

                        EnableVerticalMatchBuffer(frameX);
                        PlayClearVerticalVFX(_tiles[frameX + tile.Y * Width]);
                    }
                }

                if (hasMatch4Horizontal)
                {
                    EnableHorizontalMatchBuffer(y);
                    PlayClearHorizontalVFX(tile);

                }
                else
                {
                    bool foundMatch4Tile = false;
                    if (_selectedTile != null && _swappedTile != null)
                    {
                        for (int h = 0; h <= 3; h++)
                        {
                            int index = (x + h) + y * Width;
                            if (_selectedTile.Equal(_tiles[index]) || _swappedTile.Equal(_tiles[index]))
                            {
                                _matchRowBombQueue.Enqueue(new SpecialTileQueue(_tiles[index].ID, index));
                                foundMatch4Tile = true;
                                break;
                            }
                        }
                    }


                    for (int h = 0; h <= 3; h++) // Loop correctly over sameIDCount
                    {
                        int index = (x + h) + y * Width;
                        if (foundMatch4Tile == false)
                        {
                            if (_tiles[index].ID != _prevTileIDs[index])
                            {
                                int xx = index % Width;
                                int yy = index / Width;
                                _matchRowBombQueue.Enqueue(new SpecialTileQueue(_tiles[index].ID, index));

                                foundMatch4Tile = true;
                            }
                        }

                        //_matchBuffer[index] = MatchID.Match;
                        SetMatchBuffer(index, MatchID.Match);
                    }

                    if (foundMatch4Tile == false)
                    {
                        Debug.Log($"Not found match4 tile oriign");
                        int index = x + 1 + y * Width;
                        _matchRowBombQueue.Enqueue(new SpecialTileQueue(_tiles[index].ID, index));
                    }
                }
            }
            else if (sameIDCountInRow == 2)
            {
                if (hasMatch4Horizontal)
                {
                    _hasMatch4 = true;

                    EnableHorizontalMatchBuffer(y);
                    PlayClearHorizontalVFX(tile);
                }

                if (hasMatch4Vertical)
                {
                    _hasMatch4 = true;

                    //Debug.Log($"Clear vertical");
                    for (int xx = 0; xx < _clearVerticalColumns.Count; xx++)
                    {
                        int index = _clearVerticalColumns[xx];
                        int frameX = index % Width;
                        EnableVerticalMatchBuffer(frameX);
                        PlayClearVerticalVFX(_tiles[frameX + tile.Y * Width]);
                    }
                }


                // default

                for (int h = 0; h <= sameIDCountInRow; h++) // Loop correctly over sameIDCount
                {
                    int index = (x + h) + y * Width;
                    SetMatchBuffer(index, MatchID.Match);
                }
            }
            HandleCollectInrow(tile, sameIDCountInRow);
        }
        private void HandleMatchSameIDInColumn(Tile tile, int sameIDCountInColumn)
        {
            if (sameIDCountInColumn < 2) return;

            int x = tile.X;
            int y = tile.Y;
            bool hasMatch4Horizontal = false;
            bool hasMatch4Vertical = false;
            _clearHorizontalRows.Clear();

            for (int v = 0; v <= sameIDCountInColumn; v++)  // Loop correctly over sameIDCount
            {
                int index = x + (y + v) * Width;

                SpecialTileID specialTileID = _tiles[index].SpecialProperties;
                switch (specialTileID)
                {
                    default:
                    case SpecialTileID.None:
                        break;
                    case SpecialTileID.RowBomb:
                        hasMatch4Horizontal = true;
                        _clearHorizontalRows.Add(index);
                        break;
                    case SpecialTileID.ColumnBomb:
                        hasMatch4Vertical = true;
                        break;
                }
            }


            if (sameIDCountInColumn >= 4)
            {
                _hasColorBurst = true;
                bool foundColorBurstTile = false;
                if (_selectedTile != null && _swappedTile != null)
                {
                    for (int v = 0; v <= sameIDCountInColumn; v++)
                    {
                        int index = x + (y + v) * Width;
                        if (_selectedTile.Equal(_tiles[index]) || _swappedTile.Equal(_tiles[index]))
                        {
                            _matchColorBurstQueue.Enqueue(new SpecialTileQueue(_tiles[index].ID, index));
                            foundColorBurstTile = true;
                            break;
                        }
                    }

                }

                for (int v = 0; v <= sameIDCountInColumn; v++)
                {
                    int index = x + (y + v) * Width;
                    //_matchBuffer[index] = MatchID.Match;
                    SetMatchBuffer(index, MatchID.Match);
                    if (foundColorBurstTile == false)
                    {
                        if (_tiles[index].ID != _prevTileIDs[index])
                        {
                            _matchColorBurstQueue.Enqueue(new SpecialTileQueue(_tiles[index].ID, index));
                            foundColorBurstTile = true;
                        }
                    }
                }

                if (foundColorBurstTile == false)
                {
                    int index = x + y * Width;
                    _matchColorBurstQueue.Enqueue(new SpecialTileQueue(_tiles[index].ID, index));
                }
            }
            else if (sameIDCountInColumn == 3)
            {
                _hasMatch4 = true;
                if (hasMatch4Horizontal)
                {
                    for (int yy = 0; yy < _clearHorizontalRows.Count; yy++)
                    {
                        int index = _clearHorizontalRows[yy];
                        int frameY = index / Width;
                        PlayClearHorizontalVFX(_tiles[tile.X + frameY * Width]);

                        AudioManager.Instance.PlayMatch4Sfx();
                    }
                }

                if (hasMatch4Vertical)
                {
                    EnableVerticalMatchBuffer(x);
                    PlayClearVerticalVFX(tile);

                    AudioManager.Instance.PlayMatch4Sfx();
                }
                else
                {
                    bool foundMatch4Tile = false;
                    if (_selectedTile != null && _swappedTile != null)
                    {
                        for (int v = 0; v <= 3; v++)
                        {
                            int index = x + (y + v) * Width;
                            if (_selectedTile.Equal(_tiles[index]) || _swappedTile.Equal(_tiles[index]))
                            {
                                _match4ColumnBombQueue.Enqueue(new SpecialTileQueue(_tiles[index].ID, index));
                                foundMatch4Tile = true;
                                break;
                            }
                        }

                    }


                    for (int v = 0; v <= 3; v++)
                    {
                        int index = x + (y + v) * Width;

                        if (foundMatch4Tile == false)
                        {
                            if (_tiles[index].ID != _prevTileIDs[index])
                            {
                                _match4ColumnBombQueue.Enqueue(new SpecialTileQueue(_tiles[index].ID, index));
                                foundMatch4Tile = true;
                            }
                        }

                        //_matchBuffer[index] = MatchID.Match;
                        SetMatchBuffer(index, MatchID.Match);
                    }

                    if (foundMatch4Tile == false)
                    {
                        Debug.Log($"Not found match4 tile oriign");
                        int index = x + y * Width;
                        _match4ColumnBombQueue.Enqueue(new SpecialTileQueue(_tiles[index].ID, index));
                    }
                }
            }
            else if (sameIDCountInColumn == 2)
            {
                if (hasMatch4Vertical)
                {
                    _hasMatch4 = true;
                    EnableVerticalMatchBuffer(x);
                    PlayClearVerticalVFX(tile);

                    AudioManager.Instance.PlayMatch4Sfx();
                }

                if (hasMatch4Horizontal)
                {
                    _hasMatch4 = true;
                    for (int yy = 0; yy < _clearHorizontalRows.Count; yy++)
                    {
                        int index = _clearHorizontalRows[yy];
                        int frameY = index / Width;
                        EnableHorizontalMatchBuffer(frameY);
                        PlayClearHorizontalVFX(_tiles[tile.X + frameY * Width]);

                        AudioManager.Instance.PlayMatch4Sfx();
                    }
                }

                // default
                for (int v = 0; v <= sameIDCountInColumn; v++) // Loop correctly over sameIDCount
                {
                    _matchBuffer[x + (y + v) * Width] = MatchID.Match;
                }
            }

            HandleCollectInColumn(tile, sameIDCountInColumn);
        }
        private void HandleSpecialMatch()
        {
            if (_selectedTile == null || _swappedTile == null) return;

            switch (_selectedTile.SpecialProperties)
            {
                case SpecialTileID.ColumnBomb:
                case SpecialTileID.RowBomb:
                    if (_swappedTile.SpecialProperties == SpecialTileID.RowBomb ||
                        _swappedTile.SpecialProperties == SpecialTileID.ColumnBomb)
                    {
                        // if (_selectedTile.ID == _swappedTile.ID)
                        // {
                        Debug.Log("============= Clear + ==============");
                        EnableVerticalMatchBuffer(_selectedTile.X);
                        EnableHorizontalMatchBuffer(_selectedTile.Y);

                        PlayClearVerticalVFX(_selectedTile);
                        PlayClearHorizontalVFX(_selectedTile);

                        _hasMatch4 = true;
                        AudioManager.Instance.PlayMatch4Sfx();
                        // }
                    }
                    else if (_swappedTile.SpecialProperties == SpecialTileID.BlastBomb)
                    {
                        CombineMatch4AndMatch5();
                        _hasColorBurst = true;
                        AudioManager.Instance.PlayMatch5Sfx();
                    }
                    break;
                case SpecialTileID.BlastBomb:
                    if (_swappedTile.SpecialProperties == SpecialTileID.None)
                    {
                        AudioManager.Instance.PlayMatch5Sfx();
                        PlayFlashBombVfx(_selectedTile);
                        HandleBlastBomb(_selectedTile);
                        _triggeredMatch5Set.Add(_selectedTile.X + _selectedTile.Y * Width);
                        _hasColorBurst = true;
                    }
                    else if (_swappedTile.SpecialProperties == SpecialTileID.RowBomb ||
                            _swappedTile.SpecialProperties == SpecialTileID.ColumnBomb)
                    {
                        CombineMatch4AndMatch5();

                        _hasColorBurst = true;
                        AudioManager.Instance.PlayMatch5Sfx();
                    }
                    else if (_swappedTile.SpecialProperties == SpecialTileID.BlastBomb)
                    {
                        ClearAllTiles();
                        PlayClearAllTilesVfx();

                        _hasColorBurst = true;
                        AudioManager.Instance.PlayMatch5Sfx();
                    }
                    break;
                case SpecialTileID.ColorBurst:
                    if (_swappedTile.SpecialProperties == SpecialTileID.None)
                    {
                        ClearAllBoardTileID(_selectedTile, _swappedTile.ID);

                        _matchBuffer[_selectedTile.X + _selectedTile.Y * Width] = MatchID.Match;

                        _hasMatch6 = true;
                        AudioManager.Instance.PlayColorBurstSFX();
                    }
                    break;
                case SpecialTileID.None:
                    if (_swappedTile.SpecialProperties == SpecialTileID.ColorBurst)
                    {
                        ClearAllBoardTileID(_swappedTile, _selectedTile.ID);
                        _matchBuffer[_swappedTile.X + _swappedTile.Y * Width] = MatchID.Match;

                        _hasMatch6 = true;
                        AudioManager.Instance.PlayColorBurstSFX();
                    }
                    else if (_swappedTile.SpecialProperties == SpecialTileID.BlastBomb)
                    {
                        PlayFlashBombVfx(_swappedTile);
                        for (int y = 0; y < _blastBombPattern.GetLength(1); y++)
                        {
                            for (int x = 0; x < _blastBombPattern.GetLength(0); x++)
                            {
                                if (_blastBombPattern[x, y] == 1)
                                {
                                    int xx = _swappedTile.X + x - _blastBombPatternOffset.x;
                                    int yy = _swappedTile.Y + y - _blastBombPatternOffset.y;

                                    if (IsValidGridTile(xx, yy))
                                    {
                                        int index = xx + yy * Width;
                                        //_matchBuffer[index] = MatchID.Match;
                                        SetMatchBuffer(index, MatchID.Match);
                                    }
                                }
                            }
                        }

                        _hasColorBurst = true;
                        AudioManager.Instance.PlayMatch5Sfx();
                    }
                    break;
            }


            void CombineMatch4AndMatch5()
            {
                EnableVerticalMatchBuffer(_selectedTile.X);
                PlayClearVerticalVFX(_selectedTile);

                if (IsValidGridTile(_selectedTile.X - 1, _selectedTile.Y))
                {
                    EnableVerticalMatchBuffer(_selectedTile.X - 1);
                    PlayClearVerticalVFX(_tiles[_selectedTile.X - 1 + _selectedTile.Y * Width]);
                }
                if (IsValidGridTile(_selectedTile.X + 1, _selectedTile.Y))
                {
                    EnableVerticalMatchBuffer(_selectedTile.X + 1);
                    PlayClearVerticalVFX(_tiles[_selectedTile.X + 1 + _selectedTile.Y * Width]);
                }


                EnableHorizontalMatchBuffer(_selectedTile.Y);
                PlayClearHorizontalVFX(_selectedTile);

                if (IsValidGridTile(_selectedTile.X, _selectedTile.Y - 1))
                {
                    EnableHorizontalMatchBuffer(_selectedTile.Y - 1);
                    PlayClearHorizontalVFX(_tiles[_selectedTile.X + (_selectedTile.Y - 1) * Width]);
                }

                if (IsValidGridTile(_selectedTile.X, _selectedTile.Y + 1))
                {
                    EnableHorizontalMatchBuffer(_selectedTile.Y + 1);
                    PlayClearHorizontalVFX(_tiles[_selectedTile.X + (_selectedTile.Y + 1) * Width]);
                }
            }

            void ClearAllTiles()
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int index = x + y * Width;
                        if (_tiles[index].CurrentBlock is NoneBlock)
                        {
                            //_matchBuffer[index] = MatchID.Match;
                            SetMatchBuffer(index, MatchID.Match);
                        }
                        else
                        {
                            //_matchBuffer[index] = MatchID.SpecialMatch;
                            SetMatchBuffer(index, MatchID.SpecialMatch);
                        }
                    }
                }
            }
        }
        private bool FindFlashBomb()
        {
            bool FoundFlashBomb(List<int[,]> shape, int xIndex, int yIndex)
            {
                for (int i = 0; i < shape.Count; i++)
                {
                    bool found = true;
                    int w = shape[i].GetLength(0);
                    int h = shape[i].GetLength(1);
                    TileID targetTileID = default;

                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < h; x++)
                        {
                            if (shape[i][x, y] == 1)
                            {
                                int offsetX = xIndex + x;
                                int offsetY = yIndex + y;
                                if (IsValidMatchTile(offsetX, offsetY) && _tiles[offsetX + offsetY * Width].SpecialProperties == SpecialTileID.None)
                                {
                                    targetTileID = _tiles[offsetX + offsetY * Width].ID;
                                    break;
                                }
                            }
                        }
                    }


                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            if (shape[i][x, y] == 1)
                            {
                                int offsetX = xIndex + x;
                                int offsetY = yIndex + y;

                                if (IsValidMatchTile(offsetX, offsetY))
                                {
                                    Tile nbTile = _tiles[offsetX + offsetY * Width];
                                    if (targetTileID != nbTile.ID)
                                    {
                                        found = false;
                                    }
                                    else
                                    {
                                        if (_matchBuffer[offsetX + offsetY * Width] != MatchID.None) found = false;
                                        if (nbTile.SpecialProperties != SpecialTileID.None) found = false;
                                    }
                                }
                                else
                                {
                                    found = false;
                                }
                            }
                            if (found == false) break;
                        }
                        if (found == false) break;
                    }

                    if (found)
                    {
                        bool foundBlashBombTile = false;
                        for (int y = 0; y < h; y++)
                        {
                            for (int x = 0; x < w; x++)
                            {
                                if (shape[i][x, y] == 1)
                                {
                                    int offsetX = xIndex + x;
                                    int offsetY = yIndex + y;
                                    int index = offsetX + offsetY * Width;
                                    if (IsValidMatchTile(offsetX, offsetY))
                                    {
                                        //_matchBuffer[index] = MatchID.Match;
                                        SetMatchBuffer(index, MatchID.Match);
                                    }

                                    if (_selectedTile != null && _swappedTile != null)
                                    {
                                        if (_selectedTile.Equal(_tiles[index]))
                                        {
                                            _matchBlastBombQueue.Enqueue(new SpecialTileQueue(_selectedTile.ID, index));
                                            foundBlashBombTile = true;
                                        }
                                        else if (_swappedTile.Equal(_tiles[index]))
                                        {
                                            _matchBlastBombQueue.Enqueue(new SpecialTileQueue(_swappedTile.ID, index));
                                            foundBlashBombTile = true;
                                        }
                                    }
                                }
                            }
                        }

                        if (foundBlashBombTile)
                        {
                            return true;
                        }
                        int lastMatchIndex = 0;

                        if (foundBlashBombTile == false)
                        {
                            for (int y = 0; y < h; y++)
                            {
                                for (int x = 0; x < w; x++)
                                {
                                    if (shape[i][x, y] == 1)
                                    {
                                        int offsetX = xIndex + x;
                                        int offsetY = yIndex + y;
                                        int index = offsetX + offsetY * Width;
                                        lastMatchIndex = index;


                                        if (_prevTileIDs[index] != _tiles[index].ID)
                                        {
                                            _matchBlastBombQueue.Enqueue(new SpecialTileQueue(TileID.None, index));
                                            foundBlashBombTile = true;
                                            return true;
                                        }
                                    }

                                }
                            }
                        }

                        Debug.Log("??????");
                        // not found -> Get last match tile index
                        _matchBlastBombQueue.Enqueue(new SpecialTileQueue(TileID.None, lastMatchIndex));
                        return true;
                    }
                }

                return false;
            }


            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (IsValidMatchTile(x, y))
                    {
                        int index = x + y * Width;
                        if (_matchBuffer[index] != MatchID.None) continue;

                        Tile tile = _tiles[index];
                        if (FoundFlashBomb(_tShapes, tile.X, tile.Y))
                        {
                            //Debug.Log($"Found flash bomb at: {tile.X}  {tile.Y}");
                            return true;
                        }
                        else
                        {

                            if (FoundFlashBomb(_lShapes, tile.X, tile.Y))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private void HandleBlastBomb(Tile tile)
        {
            for (int y = 0; y < _blastBombPattern.GetLength(1); y++)
            {
                for (int x = 0; x < _blastBombPattern.GetLength(0); x++)
                {
                    if (_blastBombPattern[x, y] == 1)
                    {
                        int xx = tile.X + x - _blastBombPatternOffset.x;
                        int yy = tile.Y + y - _blastBombPatternOffset.y;

                        if (IsValidGridTile(xx, yy))
                        {
                            int index = xx + yy * Width;
                            //_matchBuffer[index] = MatchID.SpecialMatch;
                            SetMatchBuffer(index, MatchID.SpecialMatch);
                        }
                    }
                }
            }
        }

        private void EnableHorizontalMatchBuffer(int y)
        {
            for (int x = 0; x < Width; x++)
            {
                _matchBuffer[x + y * Width] = MatchID.SpecialMatch;
            }
        }

        private void EnableVerticalMatchBuffer(int x)
        {
            for (int y = 0; y < Height; y++)
            {
                _matchBuffer[x + y * Width] = MatchID.SpecialMatch;
            }
        }

        private void ClearAllBoardTileID(Tile tile, TileID tileID)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int index = x + y * Width;
                    if (IsValidSpecialMatchTile(x, y) && _matchBuffer[index] != MatchID.ColorBurst)
                    {
                        if (_tiles[index].ID == tileID)
                        {
                            //_matchBuffer[index] = MatchID.ColorBurst;
                            SetMatchBuffer(index, MatchID.ColorBurst);

                            if (index != tile.X + tile.Y * Width)
                            {
                                if (_colorBurstParentDictionary.ContainsKey(index) == false)
                                    _colorBurstParentDictionary.Add(index, tile.transform.position);
                            }
                        }
                    }
                }
            }
        }


        private void CheckDifferent()
        {
            for (int i = 0; i < _tiles.Length; i++)
            {
                if (_tiles[i].ID != _prevTileIDs[i])
                {
                    int x = i % Width;
                    int y = i / Width;
                    Debug.Log($"Diff: {x} {y}");
                }
            }
        }

        private void MatchPreviousTilesDifferent()
        {
            for (int i = 0; i < _tiles.Length; i++)
            {
                if (_tiles[i] == null) continue;
                _prevTileIDs[i] = _tiles[i].ID;
            }
        }


        private void HandleCollectInrow(Tile tile, int sameIDCountInRow)
        {
            int originIndex = (tile.X + 1) + tile.Y * Width;
            int x = tile.X;
            int y = tile.Y;
            for (int h = 0; h <= sameIDCountInRow; h++) // Loop correctly over sameIDCount
            {
                int index = (x + h) + y * Width;
                if (_tiles[index].ID != _prevTileIDs[index])
                {
                    originIndex = _tiles[index].X + _tiles[index].Y * Width;
                }

                if (_selectedTile != null)
                    if (_tiles[index].Equal(_selectedTile))
                    {
                        originIndex = _selectedTile.X + _selectedTile.Y * Width;
                        break;
                    }

                if (_swappedTile != null)
                    if (_tiles[index].Equal(_swappedTile))
                    {
                        originIndex = _swappedTile.X + _swappedTile.Y * Width;
                        break;
                    }
            }

            for (int h = 0; h <= sameIDCountInRow; h++) // Loop correctly over sameIDCount
            {
                int index = (x + h) + y * Width;
                if (index == originIndex) continue;
                if (sameIDCountInRow == 2)
                {
                    if (_match3Dictionary.ContainsKey(_tiles[index]) == false)
                        _match3Dictionary.Add(_tiles[index], _tiles[originIndex]);
                }
                else if (sameIDCountInRow == 3)
                {
                    if (_match4Dictionary.ContainsKey(_tiles[index]) == false)
                        _match4Dictionary.Add(_tiles[index], _tiles[originIndex]);
                }
                else if (sameIDCountInRow >= 4)
                {
                    if (_match5Dictionary.ContainsKey(_tiles[index]) == false)
                        _match5Dictionary.Add(_tiles[index], _tiles[originIndex]);
                }
            }
        }

        private void HandleCollectInColumn(Tile tile, int sameIDCountInColumn)
        {
            int originIndex = (tile.X + 1) + tile.Y * Width;
            int x = tile.X;
            int y = tile.Y;
            for (int v = 0; v <= sameIDCountInColumn; v++)
            {
                int index = x + (y + v) * Width;
                if (_tiles[index].ID != _prevTileIDs[index])
                {
                    originIndex = _tiles[index].X + _tiles[index].Y * Width;
                }

                if (_selectedTile != null)
                    if (_tiles[index].Equal(_selectedTile))
                    {
                        originIndex = _selectedTile.X + _selectedTile.Y * Width;
                        break;
                    }

                if (_swappedTile != null)
                    if (_tiles[index].Equal(_swappedTile))
                    {
                        originIndex = _swappedTile.X + _swappedTile.Y * Width;
                        break;
                    }
            }

            for (int v = 0; v <= sameIDCountInColumn; v++)
            {
                int index = x + (y + v) * Width;
                if (index == originIndex) continue;

                Debug.Log($"sameIDCountInColumn: {sameIDCountInColumn}");
                if (sameIDCountInColumn == 2)
                {
                    if (_match3Dictionary.ContainsKey(_tiles[index]) == false)
                        _match3Dictionary.Add(_tiles[index], _tiles[originIndex]);
                }
                else if (sameIDCountInColumn == 3)
                {
                    if (_match4Dictionary.ContainsKey(_tiles[index]) == false)
                        _match4Dictionary.Add(_tiles[index], _tiles[originIndex]);
                }
                else if (sameIDCountInColumn >= 4)
                {
                    if (_match5Dictionary.ContainsKey(_tiles[index]) == false)
                        _match5Dictionary.Add(_tiles[index], _tiles[originIndex]);
                }

            }
        }



        private bool CanFill()
        {
            //for (int y = 0; y < Height - 1; y++)
            //{
            //    for (int x = 0; x < Width; x++)
            //    {
            //        Tile currTile = _tiles[x + y * Width];
            //        Tile aboveTile = _tiles[x + (y + 1) * Width];

            //        if (currTile.Data.ID == TileID.None && aboveTile.Data.ID != TileID.None)
            //        {
            //            return true;
            //        }
            //    }
            //}
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Tile tile = _tiles[x + y * Width];
                    if (tile == null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool _tileHasMove;
        private IEnumerator FillGridCoroutine()
        {
            _tileHasMove = false;
            int attempts = 0;
            while (true)
            {
                if (attempts++ > Height)
                {
                    Debug.LogError("=========== something went wrong ===========");
                    break;
                }

                _tileHasMove = false;
                for (int y = 0; y < Height - 1; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        Tile currTile = _tiles[x + y * Width];
                        if (currTile == null)
                        {
                            for (int yy = y + 1; yy < Height - 1; yy++)
                            {
                                Tile aboveTile = _tiles[x + yy * Width];

                                if (aboveTile != null && aboveTile.CurrentBlock is NoneBlock)
                                {
                                    _tiles[x + y * Width] = _tiles[x + yy * Width];
                                    _tiles[x + y * Width].SetGridPosition(x, y);
                                    _tiles[x + yy * Width] = null;

                                    _tileHasMove = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (_tileHasMove == false)
                {
                    break;
                }
            }

            float maxMoveY = 0;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Tile currTile = _tiles[x + y * Width];
                    if (currTile == null || currTile.CurrentBlock is not NoneBlock)
                    {
                        continue;
                    }
                    float moveY = (currTile.transform.position.y - currTile.GetWorldPosition().y) / 1.5f;
                    //currTile.MoveToGridPosition(AnimationExtensions.TILE_MOVE_TIME * moveY);
                    if (maxMoveY < moveY)
                    {
                        maxMoveY = moveY;
                    }
                }
            }

            _fillDownDictionary.Clear();
            for (int i = 0; i < _fillBlockIndices.Count; i++)
            {
                int index = _fillBlockIndices[i];
                int x = index % Width;
                int y = index / Width;

                for (int yy = 0; yy < y; yy++)
                {
                    if (_tiles[x + yy * Width] == null)
                    {
                        TileID randomTileID = _levelData.AvaiableTiles[Random.Range(0, _levelData.AvaiableTiles.Length)];
                        Tile newTile = AddTile(x, yy, randomTileID, BlockID.None, display: false);

                        if (_fillDownDictionary.ContainsKey(x) == false)
                        {
                            _fillDownDictionary.Add(x, 1);
                        }
                        else
                        {
                            _fillDownDictionary[x]++;
                        }
                        int offsetFillY = Height - yy + _fillDownDictionary[x] - 2;    // 2: 1 fill block + sizeY = height - 1
                        newTile.UpdatePosition(0, offsetFillY);
                        newTile.Display(true);
                    }
                }
            }


            for (int y = 0; y < Height; y++)
            {
                bool hasFilledTile = false;
                for (int x = 0; x < Width; x++)
                {
                    int i = x + y * Width;
                    if (_tiles[i] != null)
                    {
                        _tiles[i].Display(true);
                        if (_tiles[i].IsCorrectPosition(out float distance) == false)
                        {
                            _tiles[i].FallDownToGridPosition(AnimationExtensions.TILE_FALLDOWN_TIME * distance);
                            hasFilledTile = true;
                        }
                    }
                }
                //if (hasFilledTile)
                //    yield return new WaitForSeconds(0.1f);
            }


            if (_fillBlockIndices.Count > 0)
            {
                int maxColumnFillCount = 1;
                foreach (var e in _fillDownDictionary)
                {
                    if (e.Value > maxColumnFillCount)
                        maxColumnFillCount = e.Value;
                }

                //yield return new WaitForSeconds(AnimationExtensions.TILE_FALLDOWN_TIME * maxColumnFillCount);
                yield return new WaitForSeconds(AnimationExtensions.TILE_FALLDOWN_TIME * maxColumnFillCount);
            }

            yield return null;
        }


        private void SwapPosition(Tile tileA, Tile tileB)
        {
            // sort render order
            tileA.SetRenderOrder(2);
            tileB.SetRenderOrder(0);

            // Temporarily store tileA's grid position
            int tempX = tileA.X;
            int tempY = tileA.Y;

            // Swap the tiles in the grid array
            _tiles[tileA.X + tileA.Y * Width] = tileB;
            _tiles[tileB.X + tileB.Y * Width] = tileA;

            // Update the tiles' positions
            tileA.SetGridPosition(tileB.X, tileB.Y);
            tileB.SetGridPosition(tempX, tempY);
        }

        private bool IsValidGridTile(int x, int y)
        {
            return !(x < 0 || x >= Width || y < 0 || y >= Height) &&
               _tiles[x + y * Width] != null;
        }

        private bool IsValidMatchTile(int x, int y)
        {
            return !(x < 0 || x >= Width || y < 0 || y >= Height) &&
                _tiles[x + y * Width] != null &&
                (_tiles[x + y * Width].CurrentBlock is NoneBlock ||
                _tiles[x + y * Width].CurrentBlock is Lock ||
                _tiles[x + y * Width].CurrentBlock is BushBlock);
        }
        private bool IsValidMatchTile(Vector2Int gridPosition)
        {
            return IsValidMatchTile(gridPosition.x, gridPosition.y);
        }

        private bool IsValidSpecialMatchTile(int x, int y)
        {
            return !(x < 0 || x >= Width || y < 0 || y >= Height) &&
                _tiles[x + y * Width] != null;
        }

        private bool IsValidNonBlockTile(int x, int y)
        {
            return !(x < 0 || x >= Width || y < 0 || y >= Height) &&
              _tiles[x + y * Width] != null &&
              _tiles[x + y * Width].CurrentBlock is NoneBlock;
        }

        private Tile GetTopMostTile(int x)
        {
            for (int y = Height - 1; y >= 0; y--)
            {
                if (_tiles[x + y * Width] != null)
                {
                    return _tiles[x + y * Width];
                }
            }
            return null;
        }
        private Tile GetLeftTile(Tile tile)
        {
            if (IsValidMatchTile(tile.X - 1, tile.Y))
            {
                return _tiles[tile.X - 1 + tile.Y * Width];
            }
            return null;
        }
        private Tile GetRightTile(Tile tile)
        {
            if (IsValidMatchTile(tile.X + 1, tile.Y))
            {
                return _tiles[tile.X + 1 + tile.Y * Width];
            }
            return null;
        }
        private Tile GetTopRightTile(Tile tile)
        {
            if (IsValidMatchTile(tile.X + 1, tile.Y + 1))
            {
                return _tiles[tile.X + 1 + (tile.Y + 1) * Width];
            }
            return null;
        }


        private List<Tile> GetVerticalTiles(int x)
        {
            _getTileList.Clear();

            for (int y = 0; y < Height; y++)
            {
                Tile t = _tiles[x + y * Width];
                if (t == null) continue;

                _getTileList.Add(t);
            }

            return _getTileList;
        }
        private List<Tile> GetHorizontalTiles(int y)
        {
            _getTileList.Clear();

            for (int x = 0; x < Width; x++)
            {
                Tile t = _tiles[x + y * Width];
                if (t == null) continue;

                _getTileList.Add(t);
            }

            return _getTileList;
        }

        private Vector2Int DetectDragDirection(Vector2 start, Vector2 end)
        {
            Vector2 dragVector = end - start;

            if (dragVector.magnitude < _dragThreshold)
            {
                return Vector2Int.zero;
            }

            if (Mathf.Abs(dragVector.x) > Mathf.Abs(dragVector.y))
            {
                // Horizontal drag
                if (dragVector.x > 0)
                {
                    return Vector2Int.right;
                }
                else
                {
                    return Vector2Int.left;
                }
            }
            else
            {
                // Vertical drag
                if (dragVector.y > 0)
                {
                    return Vector2Int.up;
                }
                else
                {
                    return Vector2Int.down;
                }
            }
        }




        #region VFX
        private void PlayClearHorizontalVFX(Tile tile)
        {
            // Line
            ClearAxisLine clearAxisLinePrefab = Resources.Load<ClearAxisLine>("Effects/ClearAxisLine");
            if (clearAxisLinePrefab == null) Debug.LogError("Missing clearAxisLinePrefab!!!");
            ClearAxisLine clearAxisLine = Instantiate(clearAxisLinePrefab, TileExtension.TileCenter(), Quaternion.identity);
            Vector2 targetLeft = new Vector2(_tiles[0 + tile.Y * Width].transform.position.x - TileExtension.TILE_WIDTH / 2f, _tiles[0 + tile.Y * Width].transform.position.y);
            Vector2 targetRight = new Vector2(_tiles[Width - 1 + tile.Y * Width].transform.position.x + TileExtension.TILE_WIDTH / 2f, _tiles[Width - 1 + tile.Y * Width].transform.position.y);
            clearAxisLine.ActiveAxisLine(tile.transform.position, targetLeft, targetRight, AnimationExtensions.CLEAR_AXIS_DURATION);
        }

        private void PlayClearVerticalVFX(Tile tile)
        {
            // Line
            ClearAxisLine clearAxisLinePrefab = Resources.Load<ClearAxisLine>("Effects/ClearAxisLine");
            if (clearAxisLinePrefab == null) Debug.LogError("Missing clearAxisLinePrefab!!!");
            ClearAxisLine clearAxisLine = Instantiate(clearAxisLinePrefab, TileExtension.TileCenter(), Quaternion.identity);
            Vector2 targetDown = new Vector2(_tiles[tile.X + 0 * Width].transform.position.x, _tiles[tile.X + 0 * Width].transform.position.y - TileExtension.TILE_HEIGHT / 2f);
            Vector2 targetUp = new Vector2(_tiles[tile.X + (Height - 1) * Width].transform.position.x, _tiles[tile.X + (Height - 1) * Width].transform.position.y + TileExtension.TILE_HEIGHT / 2f);
            clearAxisLine.ActiveAxisLine(tile.transform.position, targetDown, targetUp, AnimationExtensions.CLEAR_AXIS_DURATION);
        }

        private void PlayFlashBombVfx(Tile tile)
        {
            GameObject vfxPrefab = Resources.Load<GameObject>("Effects/Match5VFX");
            if (vfxPrefab == null) Debug.LogError("Missing vfx prefab !!!");
            GameObject vfxInstance = Instantiate(vfxPrefab, (Vector2)tile.transform.position + TileExtension.TileCenter(), Quaternion.identity);
        }

        private void PlayClearAllTilesVfx()
        {
            GameObject vfxPrefab = Resources.Load<GameObject>("Effects/ClearTilesVfx");
            if (vfxPrefab == null) Debug.LogError("Missing vfx prefab !!!");

            Tile tile = _tiles[(Width / 2) + (Height / 2) * Width];
            GameObject vfxInstance = Instantiate(vfxPrefab, (Vector2)tile.transform.position + new Vector2(0.5f, 0.5f), Quaternion.identity);
        }


        private void PlaySingleColorBurstLineVfx(Vector2 pointA, Vector2 pointB, float duration)
        {
            ColorBurstLine colorBurstLinePrefab = Resources.Load<ColorBurstLine>("Effects/ColorBurstVfx");
            ColorBurstLine vfx = Instantiate(colorBurstLinePrefab, this.transform);
            vfx.transform.position = new Vector3(0.5f, 0.5f, 0f);
            vfx.SetLine(pointB, pointA, duration);
        }
        #endregion
    }

    //public enum MatchID
    //{
    //    None = 0,
    //    Matched = 1,
    //    Match4Vertical = 2,
    //    Match4Horizontal = 3,
    //    Match5 = 4,
    //    Match6 = 5, 
    //}

    [System.Serializable]
    public struct SpecialTileQueue
    {
        public TileID TileID;
        public int Index;

        public SpecialTileQueue(TileID tileID, int index)
        {
            this.TileID = tileID;
            this.Index = index;
        }
    }
}

public enum MatchID : byte
{
    None = 0,
    Match = 1,
    SpecialMatch = 2,
    ColorBurst = 3,
}

public static class TileMatchExtensions
{
    public static bool IsSpecialMatch(this MatchID matchID)
    {
        return matchID == MatchID.SpecialMatch ||
                matchID == MatchID.ColorBurst;
    }

}
