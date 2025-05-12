using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using System.Linq;
using Match3.Enums;
using Match3.Shares;

namespace Match3
{
    public class Match3Grid : MonoBehaviour
    {
        public static Match3Grid Instance { get; private set; }

        public event System.Action OnAfterPlayerMatchInput;
        public static event System.Action OnEndOfTurn;

        [Header("Fill Type")]
        [SerializeField] private FillType _fillType;

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
        private float _dragThreshold = 15f;

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

        private HashSet<int> _triggeredMatch5Set;
        private Dictionary<Tile, List<Tile>> _colorBurstParentDictionary;
        private Dictionary<Tile, Tile> _match3Dictionary;
        private Dictionary<Tile, Tile> _match4Dictionary;
        private Dictionary<Tile, Tile> _match5Dictionary;
        private Dictionary<Tile, Tile> _matchAnimationTileDictionary;
        private Dictionary<Tile, Tile> _blastBombDictionary;
        private Dictionary<int, int> _fillDownDictionary = new();          // x, count
        private Queue<Tile> _emissiveTileQueue;
        private HashSet<Vector2Int> _matchThisPlayTurnSet;
        private HashSet<Vector2Int> _unlockThisPlayTurnSet;
        private List<ColorBurstLineFX> _cachedColorBurstLine;
        private HashSet<Tile> _activeRowBombSet;
        private HashSet<Tile> _activeColumnBombSet;
        private List<Tile> _bfsTiles;
        private List<int> _bfsSteps;
        //private Dictionary<Coroutine, bool> _singleRowBombCoroutineDict;
        //private Dictionary<Coroutine, bool> _singleColumnBombCoroutineDict;
        //private List<Coroutine> _singleRowBombCoroutineKey;
        //private List<Coroutine> _singleColumnBombCoroutineKey;
        private Coroutine _handleColumnBombCoroutine = null;
        private Coroutine _handleRowBombCoroutine = null;


        // match 5
        private Vector2Int _blastBombPatternOffset = new Vector2Int(2, 2);
        private int[,] _blastBombPattern = new int[,]
        {
            { 0, 0, 0, 0, 0 },
            { 0, 1, 1, 1, 0 },
            { 0, 1, 1, 1, 0 },
            { 0, 1, 1, 1, 0 },
            { 0, 0, 0, 0, 0 }
        };


        // spider
        private int[] _4directions = new int[] { 1, 2, 3, 4 };
        private int[] _8directions = new int[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        private List<int> _spiderSpreadingList;
        [SerializeField] private bool _canPlay = true;


        private List<int[,]> _tShapes;
        private List<int[,]> _lShapes;


        [Header("VFX")]
        [SerializeField] private float _rocketPlayTime = 0.5f;
        private bool _isBlastBombTriggered = false;


        // High Levels Logic
        public bool HandleReswapIfNotMatch { get; set; } = true;

        #region Properties
        public Tile[] Tiles => _tiles;
        public Tile SelectedTile => _selectedTile;
        public Tile SwappedTile => _swappedTile;
        public bool Canplay => _canPlay;
        public bool UseBoosterThisTurn { get; private set; } = false;
        public bool SwapTileHasMatched { get; private set; } = false;
        #endregion


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
            // _fillType = FillType.Dropdown;
            _fillType = FillType.SandFalling;
        }

        private void Start()
        {
            _unlockTileSet = new();
            _matchColorBurstQueue = new();
            _matchBlastBombQueue = new();
            _match4ColumnBombQueue = new();
            _matchRowBombQueue = new();
            _triggeredMatch5Set = new();
            _spiderSpreadingList = new();
            _colorBurstParentDictionary = new();
            _match3Dictionary = new();
            _match4Dictionary = new();
            _match5Dictionary = new();
            _matchAnimationTileDictionary = new();
            _blastBombDictionary = new();
            _emissiveTileQueue = new();
            _matchThisPlayTurnSet = new();
            _unlockThisPlayTurnSet = new();
            _cachedColorBurstLine = new();
            _activeRowBombSet = new();
            _activeColumnBombSet = new();
            _bfsTiles = new();
            _bfsSteps = new();


            _tShapes = new List<int[,]>
            {
                new int[,] // Original T-shape
                {
                    { 1, 2, 1 },
                    { 0, 1, 0 },
                    { 0, 1, 0 },
                },
                new int[,] // 90� Rotation
                {
                    { 0, 0, 1 },
                    { 1, 1, 2 },
                    { 0, 0, 1 },
                },
                new int[,] // 180� Rotation
                {
                    { 0, 1, 0 },
                    { 0, 1, 0 },
                    { 1, 2, 1 },
                },
                new int[,] // 270� Rotation
                {
                    { 1, 0, 0 },
                    { 2, 1, 1 },
                    { 1, 0, 0 },
                }
            };
            _lShapes = new List<int[,]>
            {
                new int[,] // Original L-shape
                {
                    { 1, 0, 0 },
                    { 1, 0, 0 },
                    { 2, 1, 1 },
                },
                new int[,] // 90� Rotation
                {
                    { 2, 1, 1 },
                    { 1, 0, 0 },
                    { 1, 0, 0 },
                },
                new int[,] // 180� Rotation
                {
                    { 1, 1, 2 },
                    { 0, 0, 1 },
                    { 0, 0, 1 },
                },
                new int[,] // 270� Rotation
                {
                    { 0, 0, 1 },
                    { 0, 0, 1 },
                    { 1, 1, 2 },
                }
            };

            OnAfterPlayerMatchInput += OnAfterPlayerMatchInput_ImplementGameLogic;
            GameplayUserManager.Instance.OnSelectGameplayBooster += OnSelectGameplayBooster_UpdateLogic;
            LoadGridLevel();
            LoadBoosters();

            _canPlay = false;

            StartCoroutine(ImplementGameLogicCoroutine(triggerEvent: false));
            //Time.timeScale = 0.2f;
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
                        if (IsValidGridTile(gridPosition.x, gridPosition.y))
                        {
                            Tile tile = _tiles[gridPosition.x + gridPosition.y * Width];
                            if (tile != null)
                            {
                                switch (tile.CurrentBlock)
                                {
                                    case NoneBlock:
                                    case Lock:
                                    case Ice:
                                    case HardIce:
                                    case EternalIce:
                                    case Wall01:
                                    case Wall02:
                                    case Wall03:
                                    case Bush01:
                                    case Bush02:
                                    case Bush03:
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
                        HandleSwap(dragDir);

                        // if found swapped tile
                        if (_swappedTile != null)
                        {
                            _canPlay = false;
                            // wait animation completed
                            Utilities.WaitAfter(TileAnimationExtensions.TILE_MOVE_TIME, () =>
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


#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Vector2Int gridPosition = InputHandler.Instance.GetGridPositionByMouse();
                if (IsValidMatchTile(gridPosition))
                {
                    Destroy(_tiles[gridPosition.x + gridPosition.y * Width].gameObject);
                    _tiles[gridPosition.x + gridPosition.y * Width] = null;

                    Tile newTile = AddTile(gridPosition.x, gridPosition.y, TileID.RedFlower, BlockID.None);
                    newTile.UpdatePosition();
                    //_prevTileIDs[gridPosition.x + gridPosition.y * Width] = TileID.RedFlower;
                }
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Vector2Int gridPosition = InputHandler.Instance.GetGridPositionByMouse();
                if (IsValidMatchTile(gridPosition))
                {
                    Destroy(_tiles[gridPosition.x + gridPosition.y * Width].gameObject);
                    _tiles[gridPosition.x + gridPosition.y * Width] = null;

                    Tile newTile = AddTile(gridPosition.x, gridPosition.y, TileID.None, BlockID.None);
                    newTile.UpdatePosition();
                    newTile.SetSpecialTile(SpecialTileID.BlastBomb);
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

                    Tile newTile = AddTile(gridPosition.x, gridPosition.y, TileID.None, BlockID.None);
                    newTile.UpdatePosition();
                    newTile.SetSpecialTile(SpecialTileID.RowBomb);

                    _prevTileIDs[gridPosition.x + gridPosition.y * Width] = TileID.None;
                }
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                Vector2Int gridPosition = InputHandler.Instance.GetGridPositionByMouse();
                if (IsValidGridTile(gridPosition.x, gridPosition.y))
                {
                    if (_tiles[gridPosition.x + gridPosition.y * Width] != null)
                    {
                        Destroy(_tiles[gridPosition.x + gridPosition.y * Width].gameObject);
                        _tiles[gridPosition.x + gridPosition.y * Width] = null;

                        Tile newTile = AddTile(gridPosition.x, gridPosition.y, TileID.None, BlockID.None);
                        newTile.UpdatePosition();
                        newTile.SetSpecialTile(SpecialTileID.ColumnBomb);
                        _prevTileIDs[gridPosition.x + gridPosition.y * Width] = TileID.None;
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                Vector2Int gridPosition = InputHandler.Instance.GetGridPositionByMouse();
                if (IsValidGridTile(gridPosition.x, gridPosition.y))
                {
                    // _tiles[gridPosition.x + gridPosition.y * Width].PlayAppearAnimation(0.2f);

                    _tiles[gridPosition.x + gridPosition.y * Width].transform.localScale = Vector3.zero; // Start invisible
                    _tiles[gridPosition.x + gridPosition.y * Width].transform.DOScale(Vector3.one, 0.5f) // Scale up to full size
                        .SetEase(Ease.OutBack);     // Nice bouncy ease
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                Vector2Int gridPosition = InputHandler.Instance.GetGridPositionByMouse();
                if (IsValidGridTile(gridPosition.x, gridPosition.y))
                {
                    Destroy(_tiles[gridPosition.x + gridPosition.y * Width].gameObject);
                    _tiles[gridPosition.x + gridPosition.y * Width] = null;

                    Tile newTile = AddTile(gridPosition.x, gridPosition.y, TileID.None, BlockID.None);
                    newTile.UpdatePosition();
                    newTile.SetSpecialTile(SpecialTileID.ColorBurst);
                    _prevTileIDs[gridPosition.x + gridPosition.y * Width] = TileID.None;
                }
            }
#endif
        }

        #region LOADER
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
                            newTile = AddTile(x, y, TileID.None, BlockID.Void);
                            newTile.UpdatePosition();
                            break;
                        case BlockID.Fill:
                            newTile = AddTile(x, y, TileID.None, BlockID.Fill);
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
                        case BlockID.Bush_01:
                            newTile = AddTile(x, y, tileID, BlockID.Bush_01);
                            newTile.UpdatePosition();
                            break;
                        case BlockID.Bush_02:
                            newTile = AddTile(x, y, tileID, BlockID.Bush_02);
                            newTile.UpdatePosition();
                            break;
                        case BlockID.Bush_03:
                            newTile = AddTile(x, y, tileID, BlockID.Bush_03);
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
                                    _tiles[randomTileIndex].ReturnToPool();
                                    AddTile(x, y, TileID.None, BlockID.None, display: true);
                                    _tiles[randomTileIndex].UpdatePosition();
                                    _tiles[randomTileIndex].SetSpecialTile(SpecialTileID.RowBomb);
                                }
                                else
                                {
                                    _tiles[randomTileIndex].ReturnToPool();
                                    AddTile(x, y, TileID.None, BlockID.None, display: true);
                                    _tiles[randomTileIndex].UpdatePosition();
                                    _tiles[randomTileIndex].SetSpecialTile(SpecialTileID.ColumnBomb);
                                }
                                break;
                            case BoosterID.BlastBomb:
                                _tiles[randomTileIndex].ReturnToPool();
                                AddTile(x, y, TileID.None, BlockID.None, display: true);
                                _tiles[randomTileIndex].UpdatePosition();
                                _tiles[randomTileIndex].SetSpecialTile(SpecialTileID.BlastBomb);
                                break;
                            case BoosterID.ColorBurst:
                                _tiles[randomTileIndex].ReturnToPool();
                                AddTile(x, y, TileID.None, BlockID.None, display: true);
                                _tiles[randomTileIndex].UpdatePosition();
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
        #endregion




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



        #region HANDLE GAME LOGIC
        private void HandleSwap(Vector2 dragDir)
        {
            if (dragDir == Vector2Int.left)
            {
                int leftTileX = _selectedTile.X - 1;
                int leftTileY = _selectedTile.Y;
                if (IsValidMatchTile(leftTileX, leftTileY))
                {
                    Debug.Log("Drag left");
                    Tile leftTile = _tiles[leftTileX + leftTileY * Width];
                    _swappedTile = leftTile;


                    if (_selectedTile.SpecialProperties == SpecialTileID.None &&
                        _swappedTile.SpecialProperties == SpecialTileID.None)
                    {
                        Debug.Log("Caser A");
                        SwapPosition(_selectedTile, _swappedTile);
                        _selectedTile.MoveToGridPosition();
                        _swappedTile.MoveToGridPosition();
                    }
                    else if (_selectedTile.SpecialProperties != SpecialTileID.None &&
                        _swappedTile.SpecialProperties != SpecialTileID.None)
                    {
                        Debug.Log("Case B");
                        if (_selectedTile.SpecialProperties == SpecialTileID.BlastBomb &&
                            _swappedTile.SpecialProperties == SpecialTileID.BlastBomb)
                        {
                            Debug.Log("Baslt bomb X Blast Bomb");
                            _selectedTile.MoveToPosition(_swappedTile.GetWorldPosition(), TileAnimationExtensions.TILE_MOVE_TIME, Ease.Linear);
                        }
                        else if(_selectedTile.SpecialProperties == SpecialTileID.BlastBomb && 
                            _swappedTile.SpecialProperties == SpecialTileID.ColorBurst)
                        {
                            Debug.Log($"BlastBomb X Color BUrst");
                            _selectedTile.MoveToPosition(_swappedTile.GetWorldPosition(), TileAnimationExtensions.TILE_MOVE_TIME, Ease.Linear);
                        }
                        else if (_selectedTile.SpecialProperties == SpecialTileID.ColorBurst &&
                           _swappedTile.SpecialProperties == SpecialTileID.BlastBomb)
                        {
                            Debug.Log($"Color Burst X  BlastBomb");
                            _selectedTile.MoveToPosition(_swappedTile.GetWorldPosition(), TileAnimationExtensions.TILE_MOVE_TIME, Ease.Linear);
                        }
                        else if (_selectedTile.SpecialProperties == SpecialTileID.BlastBomb)
                        {
                            if (_swappedTile.SpecialProperties != SpecialTileID.ColumnBomb ||
                                _swappedTile.SpecialProperties != SpecialTileID.RowBomb)
                            {
                                SwapPosition(_selectedTile, _swappedTile);
                                _selectedTile.MoveToGridPosition();
                                _swappedTile.MoveToGridPosition();
                            }
                        }
                        else if (_swappedTile.SpecialProperties == SpecialTileID.BlastBomb)
                        {
                            if (_selectedTile.SpecialProperties != SpecialTileID.ColumnBomb ||
                                _selectedTile.SpecialProperties != SpecialTileID.RowBomb)
                            {
                                SwapPosition(_selectedTile, _swappedTile);
                                _selectedTile.MoveToGridPosition();
                                _swappedTile.MoveToGridPosition();
                            }
                        }
                        else
                        {
                            _selectedTile.MoveToPosition(_swappedTile.GetWorldPosition(), TileAnimationExtensions.TILE_MOVE_TIME, Ease.Linear);
                        }
                    }
                    else
                    {
                        Debug.Log("Case C");
                        SwapPosition(_selectedTile, _swappedTile);
                        _selectedTile.MoveToGridPosition();
                        _swappedTile.MoveToGridPosition();
                    }
                    //Debug.Log("VFX HERE");
                    // vfx
                    // if (GameDataManager.Instance.TryGetVfxByID(Enums.VisualEffectID.Slash, out var vfxPrefab))
                    // {
                    //     SlashVfx slashVfx01 = Instantiate((SlashVfx)vfxPrefab, _selectedTile.TileTransform.position, Quaternion.identity, _selectedTile.transform);
                    //     Destroy(slashVfx01.gameObject, 0.5f);
                    // }
                }
            }
            else if (dragDir == Vector2Int.right)
            {
                int rightTileX = _selectedTile.X + 1;
                int rightTileY = _selectedTile.Y;
                if (IsValidMatchTile(rightTileX, rightTileY))
                {
                    Tile rightTile = _tiles[rightTileX + rightTileY * Width];
                    Debug.Log("Drag right");
                    _swappedTile = rightTile;

                    if (_selectedTile.SpecialProperties == SpecialTileID.None &&
                        _swappedTile.SpecialProperties == SpecialTileID.None)
                    {
                        SwapPosition(_selectedTile, _swappedTile);
                        _selectedTile.MoveToGridPosition();
                        _swappedTile.MoveToGridPosition();
                    }
                    else if (_selectedTile.SpecialProperties != SpecialTileID.None &&
                        _swappedTile.SpecialProperties != SpecialTileID.None)
                    {
                        if (_selectedTile.SpecialProperties == SpecialTileID.BlastBomb &&
                          _swappedTile.SpecialProperties == SpecialTileID.BlastBomb)
                        {
                            _selectedTile.MoveToPosition(_swappedTile.GetWorldPosition(), TileAnimationExtensions.TILE_MOVE_TIME, Ease.Linear);
                        }
                        else if (_selectedTile.SpecialProperties == SpecialTileID.BlastBomb)
                        {
                            if (_swappedTile.SpecialProperties != SpecialTileID.ColumnBomb ||
                                _swappedTile.SpecialProperties != SpecialTileID.RowBomb)
                            {
                                SwapPosition(_selectedTile, _swappedTile);
                                _selectedTile.MoveToGridPosition();
                                _swappedTile.MoveToGridPosition();
                            }
                        }
                        else if (_swappedTile.SpecialProperties == SpecialTileID.BlastBomb)
                        {
                            if (_selectedTile.SpecialProperties != SpecialTileID.ColumnBomb ||
                                _selectedTile.SpecialProperties != SpecialTileID.RowBomb)
                            {
                                SwapPosition(_selectedTile, _swappedTile);
                                _selectedTile.MoveToGridPosition();
                                _swappedTile.MoveToGridPosition();
                            }
                        }
                        else
                        {
                            _selectedTile.MoveToPosition(_swappedTile.GetWorldPosition(), TileAnimationExtensions.TILE_MOVE_TIME, Ease.Linear);
                        }
                    }
                    else
                    {
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
                    _swappedTile = upTile;

                    if (_selectedTile.SpecialProperties == SpecialTileID.None &&
                      _swappedTile.SpecialProperties == SpecialTileID.None)
                    {
                        SwapPosition(_selectedTile, _swappedTile);
                        _selectedTile.MoveToGridPosition();
                        _swappedTile.MoveToGridPosition();
                    }
                    else if (_selectedTile.SpecialProperties != SpecialTileID.None &&
                        _swappedTile.SpecialProperties != SpecialTileID.None)
                    {
                        if (_selectedTile.SpecialProperties == SpecialTileID.BlastBomb &&
                           _swappedTile.SpecialProperties == SpecialTileID.BlastBomb)
                        {
                            _selectedTile.MoveToPosition(_swappedTile.GetWorldPosition(), TileAnimationExtensions.TILE_MOVE_TIME, Ease.Linear);
                        }
                        else if (_selectedTile.SpecialProperties == SpecialTileID.BlastBomb)
                        {
                            if (_swappedTile.SpecialProperties != SpecialTileID.ColumnBomb ||
                                _swappedTile.SpecialProperties != SpecialTileID.RowBomb)
                            {
                                SwapPosition(_selectedTile, _swappedTile);
                                _selectedTile.MoveToGridPosition();
                                _swappedTile.MoveToGridPosition();
                            }
                        }
                        else if (_swappedTile.SpecialProperties == SpecialTileID.BlastBomb)
                        {
                            if (_selectedTile.SpecialProperties != SpecialTileID.ColumnBomb ||
                                _selectedTile.SpecialProperties != SpecialTileID.RowBomb)
                            {
                                SwapPosition(_selectedTile, _swappedTile);
                                _selectedTile.MoveToGridPosition();
                                _swappedTile.MoveToGridPosition();
                            }
                        }
                        else
                        {
                            _selectedTile.MoveToPosition(_swappedTile.GetWorldPosition(), TileAnimationExtensions.TILE_MOVE_TIME, Ease.Linear);
                        }
                    }
                    else
                    {
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
                    _swappedTile = downTile;

                    if (_selectedTile.SpecialProperties == SpecialTileID.None &&
                     _swappedTile.SpecialProperties == SpecialTileID.None)
                    {
                        SwapPosition(_selectedTile, _swappedTile);
                        _selectedTile.MoveToGridPosition();
                        _swappedTile.MoveToGridPosition();
                    }
                    else if (_selectedTile.SpecialProperties != SpecialTileID.None &&
                        _swappedTile.SpecialProperties != SpecialTileID.None)
                    {
                        if (_selectedTile.SpecialProperties == SpecialTileID.BlastBomb &&
                           _swappedTile.SpecialProperties == SpecialTileID.BlastBomb)
                        {
                            _selectedTile.MoveToPosition(_swappedTile.GetWorldPosition(), TileAnimationExtensions.TILE_MOVE_TIME, Ease.Linear);
                        }
                        else if (_selectedTile.SpecialProperties == SpecialTileID.BlastBomb)
                        {
                            if (_swappedTile.SpecialProperties != SpecialTileID.ColumnBomb ||
                                _swappedTile.SpecialProperties != SpecialTileID.RowBomb)
                            {
                                SwapPosition(_selectedTile, _swappedTile);
                                _selectedTile.MoveToGridPosition();
                                _swappedTile.MoveToGridPosition();
                            }
                        }
                        else if (_swappedTile.SpecialProperties == SpecialTileID.BlastBomb)
                        {
                            if (_selectedTile.SpecialProperties != SpecialTileID.ColumnBomb ||
                                _selectedTile.SpecialProperties != SpecialTileID.RowBomb)
                            {
                                SwapPosition(_selectedTile, _swappedTile);
                                _selectedTile.MoveToGridPosition();
                                _swappedTile.MoveToGridPosition();
                            }
                        }
                        else
                        {
                            _selectedTile.MoveToPosition(_swappedTile.GetWorldPosition(), TileAnimationExtensions.TILE_MOVE_TIME, Ease.Linear);
                        }
                    }
                    else
                    {
                        SwapPosition(_selectedTile, _swappedTile);
                        _selectedTile.MoveToGridPosition();
                        _swappedTile.MoveToGridPosition();
                    }
                }
            }
        }

        private IEnumerator ImplementGameLogicCoroutine(bool triggerEvent)
        {
            //Debug.Log($"ImplementGameLogicCoroutine");
            int attempts = 0;
            bool isMatch = false;
            SwapTileHasMatched = false;

            while (true)
            {
                // int colorBurstCount = 0;
                //if (attempts != 0)
                //    yield return new WaitForSeconds(0.1f);
                bool hasMatched = false;
                _unlockTileSet.Clear();



                yield return StartCoroutine(HandleMatchCoroutine());
                HandleTriggerAllSpecialTiles();

                // Collect animation
                yield return StartCoroutine(HandleCollectAnimationCoroutine());


                #region Handle Row And Column Bomb

                if (_handleColumnBombCoroutine != null) StopCoroutine(_handleColumnBombCoroutine);
                if (_handleRowBombCoroutine != null) StopCoroutine(_handleRowBombCoroutine);


                bool columnDone = false;
                bool rowDone = false;
                if (_activeColumnBombSet.Count > 0)
                {
                    _handleColumnBombCoroutine = StartCoroutine(HandleAllColumnBombCoroutine(() => columnDone = true));
                }
                else
                {
                    columnDone = true;
                }


                if (_activeRowBombSet.Count > 0)
                {
                    _handleRowBombCoroutine = StartCoroutine(HandleAllRowBombCoroutine(() => rowDone = true));
                }
                else
                {
                    rowDone = true;
                }

                // Wait until both are done 
                while (!columnDone || !rowDone)
                {
                    yield return null;
                }
                #endregion


                #region ColorBurst
                // Color burst
                _cachedColorBurstLine.Clear();
                if (_colorBurstParentDictionary.Count > 0)
                {
                    float colorBurstDuration = 1f;
                    foreach (var e in _colorBurstParentDictionary)
                    {
                        Tile t = e.Key;
                        ColorBurstFX colorBurstFX = (ColorBurstFX)VFXPoolManager.Instance.GetEffect(VisualEffectID.ColorBurstFX);
                        colorBurstFX.transform.position = t.TileTransform.position;
                        colorBurstFX.Play(colorBurstDuration);
                        colorBurstFX.PlayAnimtion();

                        //t.PlayScaleTile(0.8f, 0.2f, Ease.OutBack);
                        for (int i = 0; i < e.Value.Count; i++)
                        {
                            Tile nb = e.Value[i];
                            nb.PlayShaking(colorBurstDuration);


                            nb.Bloom(true);
                            LightningLine lightningLineFX = (LightningLine)VFXPoolManager.Instance.GetEffect(VisualEffectID.LightingLine);
                            lightningLineFX.transform.position = Vector2.zero;
                            float reachTaretTime = 0.1f;
                            lightningLineFX.ActiveLightningLine((Vector2)t.TileTransform.position, (Vector2)nb.TileTransform.position, reachTaretTime, colorBurstDuration);


                            EndLineColorBurstFX endLineColorBurstFX = (EndLineColorBurstFX)VFXPoolManager.Instance.GetEffect(VisualEffectID.EndLineColorBurstFX);
                            endLineColorBurstFX.transform.position = nb.TileTransform.position;
                            endLineColorBurstFX.Play(colorBurstDuration);


                        }
                    }

                    yield return new WaitForSeconds(colorBurstDuration);

                    foreach (var e in _colorBurstParentDictionary)
                    {
                        for (int i = 0; i < e.Value.Count; i++)
                        {
                            Tile nb = e.Value[i];
                            if (GameplayManager.Instance.HasTileQuest(nb.ID))
                            {
                                MatchAnimManager.Instance.Collect(nb.transform.position, nb.ID);
                            }
                        }
                    }
                }
                #endregion


                #region  Handle Match
                HandleMatchAndUnlock(ref hasMatched);
                if (hasMatched)
                {
                    isMatch = true;
                    SwapTileHasMatched = true;

                    _selectedTile = null;
                    _swappedTile = null;
                }
                #endregion


                HandleSpawnSpecialTile();
                yield return StartCoroutine(MatchAnimManager.Instance.PlayCollectAnimationCoroutine());

                if (_emissiveTileQueue.Count > 0)
                {
                    yield return new WaitForSeconds(0.1f);
                    while (_emissiveTileQueue.Count > 0)
                    {
                        Tile tile = _emissiveTileQueue.Dequeue();
                        if (GameplayManager.Instance.HasTileQuest(tile, out var questID))
                        {
                            float appearAnimationDuration = 0.2f;
                            tile.PlayAppearAnimation(appearAnimationDuration);
                            tile.Display(true);
                        }
                        tile.StopEmissive();
                    }
                }


                if (hasMatched)
                {
                    // yield return new WaitForSeconds(0.3f);
                    for (int i = 0; i < _tiles.Length; i++)
                    {
                        if (_tiles[i] != null)
                            _tiles[i].Display(true);
                    }

                    // float waitFillTime = _activeRowBombSet.Count > 0 || _activeColumnBombSet.Count > 0 ? 1 : 0.05f;
                    //yield return new WaitForSeconds(0.05f);
                    // yield return new WaitForSeconds(0.05f);

                    if (_isBlastBombTriggered)
                    {
                        yield return new WaitForSeconds(0.1f);
                    }
                    yield return StartCoroutine(AutoFillCoroutine());
                }


                _colorBurstParentDictionary.Clear();
                _match3Dictionary.Clear();
                _match4Dictionary.Clear();
                _match5Dictionary.Clear();
                _blastBombDictionary.Clear();



                _activeColumnBombSet.Clear();
                _activeRowBombSet.Clear();
                _isBlastBombTriggered = false;
                //_singleColumnBombCoroutineKey.Clear();
                //_singleRowBombCoroutineKey.Clear();
                //_singleColumnBombCoroutineDict.Clear();
                //_singleRowBombCoroutineDict.Clear();


                attempts++;
                if (attempts > 50)
                {
                    Debug.LogError("Something went wrong");
                    break;
                }

                // Debug.Log($"HasMatch: {HasMatch()}");

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

            yield return new WaitForSeconds(0.1f);
            _canPlay = true;

            if (triggerEvent)
            {
                if (UseBoosterThisTurn == false && isMatch)
                {
                    HandleSpiderNetSpreading();
                    HandleBushGrowth();
                }
                OnEndOfTurn?.Invoke();
            }

            // Debug.Log($"Attempt:  {attempts}");
            _triggeredMatch5Set.Clear();
            _unlockThisPlayTurnSet.Clear();
            _matchThisPlayTurnSet.Clear();
            UseBoosterThisTurn = false;
        }


        // ================== Handle match logic ====================
        private IEnumerator HandleMatchCoroutine()
        {
            HandleFindColorBurst();
            HandleFindBlastBomb();
            yield return StartCoroutine(HandleSpecialMatchCoroutine());

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Tile currTile = _tiles[x + y * Width];
                    if (currTile == null) continue;
                    if (IsValidCollectTile(currTile) == false) continue;
                    int sameIDCountInRow = 0;
                    int sameIDCountInColumn = 0;

                    // Horizontal check
                    for (int h = x + 1; h < Width; h++)
                    {
                        Tile nbTile = _tiles[h + y * Width];
                        if (nbTile == null) break;
                        if (currTile.ID == nbTile.ID && IsValidCollectTile(nbTile))
                        {
                            sameIDCountInRow++;
                        }
                        else break;
                    }

                    // vertical check
                    for (int v = y + 1; v < Height - 1; v++)
                    {
                        Tile nbTile = _tiles[x + v * Width]; // Correct index calculation
                        if (nbTile == null) break;
                        if (currTile.ID == nbTile.ID & IsValidCollectTile(nbTile))
                        {
                            sameIDCountInColumn++;
                        }
                        else break;
                    }

                    HandleMatchSameIDInRow(currTile, sameIDCountInRow);
                    HandleMatchSameIDInColumn(currTile, sameIDCountInColumn);
                }
            }
            yield return null;
            MatchPreviousTilesDifferent();
        }

        private void HandleBlastBomb(Tile tile)
        {
            Debug.Log("HandleBlastBomb");
            _isBlastBombTriggered = true;
            PlayFlashBombVfx(tile);
            Dictionary<int, int> highestBombTileDict = new();
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
                            if (_tiles[index] == null) continue;
                            _tiles[index].Display(false);
                            SetMatchBuffer(index, MatchID.SpecialMatch);

                            if (GameplayManager.Instance.HasTileQuest(_tiles[index].ID))
                            {
                                MatchAnimManager.Instance.Collect(_tiles[index].transform.position, _tiles[index].ID);
                            }

                            if (highestBombTileDict.ContainsKey(xx) == false)
                            {
                                highestBombTileDict.Add(xx, yy);
                            }
                            else
                            {
                                highestBombTileDict[xx] = yy;
                            }
                        }
                    }
                }
            }

            // Slightly bounce up
            foreach (var e in highestBombTileDict)
            {
                int tileX = e.Key;
                int tileY = e.Value;

                for (int y = tileY + 1; y < Height; y++)
                {
                    if (IsValidGridTile(tileX, y))
                    {
                        int index = tileX + y * Width;
                        if (_tiles[index] == null) continue;
                        Vector2 cachedTilePosition = (Vector2)_tiles[index].transform.position;
                        Vector2 bouncePosition = (Vector2)_tiles[index].transform.position + new Vector2(0, 0.5f);
                        float bounceTime = 0.1f;
                        _tiles[index].MoveToPosition(bouncePosition, bounceTime, Ease.OutBack, () =>
                        {
                            _tiles[index].MoveToPosition(cachedTilePosition, bounceTime, Ease.OutBack);
                        });
                    }
                }
            }
        }

        private Dictionary<Coroutine, bool> _singleAllColumnBombCoroutineDict = new Dictionary<Coroutine, bool>();
        private List<Coroutine> _singleAllColumnBombCoroutineKey = new List<Coroutine>();
        private IEnumerator HandleAllRowBombCoroutine(System.Action onComplete)
        {
            _singleAllColumnBombCoroutineDict.Clear();
            _singleAllColumnBombCoroutineKey.Clear();
            if (_activeRowBombSet.Count > 0)
            {
                foreach (var tile in _activeRowBombSet)
                {
                    if (tile.HasTriggeredRowBomb) continue;
                    tile.HasTriggeredRowBomb = true;

                    BaseVisualEffect vfxPrefab = VFXPoolManager.Instance.GetEffect(VisualEffectID.HorizontalRocket);
                    vfxPrefab.transform.position = tile.TileTransform.position;
                    vfxPrefab.Play(_rocketPlayTime);


                    int centerX = tile.X;
                    int y = tile.Y;
                    // Spread out from the center
                    for (int offset = 0; offset < Width; offset++)
                    {
                        int leftX = centerX - offset;
                        int rightX = centerX + offset;

                        bool leftValid = IsValidGridTile(leftX, y);
                        bool rightValid = IsValidGridTile(rightX, y);

                        if (leftValid)
                        {
                            int index = leftX + y * Width;
                            if (_tiles[index] == null) continue;
                            if (_tiles[index].IsDisplay == false) continue;
                            bool isDisplay = offset == Width - 1;
                            _tiles[index].Display(isDisplay);
                            _tiles[index].PlayMatchVFX();

                            // Collect
                            if (GameplayManager.Instance.HasTileQuest(_tiles[index].ID))
                            {
                                MatchAnimManager.Instance.Collect(_tiles[index].transform.position, _tiles[index].ID);
                            }

                            if (_tiles[index].SpecialProperties == SpecialTileID.ColumnBomb)
                            {
                                if (_tiles[index].HasTriggererColumnBomb == false)
                                {
                                    Coroutine coroutine = null;
                                    coroutine = StartCoroutine(HandleColumnBombCoroutine(_tiles[index], () =>
                                    {
                                        if (coroutine != null)
                                            _singleAllColumnBombCoroutineDict[coroutine] = true;
                                    }));
                                    if (coroutine != null)
                                        _singleAllColumnBombCoroutineDict.Add(coroutine, false);
                                }
                            }
                            else if (_tiles[index].SpecialProperties == SpecialTileID.BlastBomb)
                            {
                                HandleBlastBomb(_tiles[index]);
                            }
                            else if (_tiles[index].SpecialProperties == SpecialTileID.ColorBurst)
                            {
                                if (_tiles[index].HasTriggerColorBurst == false)
                                {
                                    HandleTriggerColorBurst(_tiles[index]);
                                    if (_colorBurstParentDictionary.ContainsKey(_tiles[index]))
                                    {
                                        PlayColorBurstVFX(_tiles[index], removeCached: true);
                                    }
                                }
                            }
                            SetMatchBuffer(index, MatchID.SpecialMatch);
                        }

                        if (rightValid && offset != 0) // Avoid double-playing the center tile
                        {
                            int index = rightX + y * Width;
                            if (_tiles[index] == null) continue;
                            if (_tiles[index].IsDisplay == false) continue;
                            bool isDisplay = offset == Width - 1;
                            _tiles[index].Display(isDisplay);
                            _tiles[index].PlayMatchVFX();


                            // Collect
                            if (GameplayManager.Instance.HasTileQuest(_tiles[index].ID))
                            {
                                MatchAnimManager.Instance.Collect(_tiles[index].transform.position, _tiles[index].ID);
                            }


                            if (_tiles[index].SpecialProperties == SpecialTileID.ColumnBomb)
                            {
                                if (_tiles[index].HasTriggererColumnBomb == false)
                                {
                                    Coroutine coroutine = null;
                                    coroutine = StartCoroutine(HandleColumnBombCoroutine(_tiles[index], () =>
                                    {
                                        if (coroutine != null)
                                            _singleAllColumnBombCoroutineDict[coroutine] = true;
                                    }));
                                    if (coroutine != null)
                                        _singleAllColumnBombCoroutineDict.Add(coroutine, false);
                                }
                            }
                            else if (_tiles[index].SpecialProperties == SpecialTileID.BlastBomb)
                            {
                                HandleBlastBomb(_tiles[index]);
                            }
                            else if (_tiles[index].SpecialProperties == SpecialTileID.ColorBurst)
                            {
                                if (_tiles[index].HasTriggerColorBurst == false)
                                {
                                    HandleTriggerColorBurst(_tiles[index]);
                                    if (_colorBurstParentDictionary.ContainsKey(_tiles[index]))
                                    {
                                        PlayColorBurstVFX(_tiles[index], removeCached: true);
                                    }
                                }
                            }
                            SetMatchBuffer(index, MatchID.SpecialMatch);
                        }

                        if (offset < Width - 1)
                        {
                            float linearTime = _rocketPlayTime / HorizontalRocketVfx.MAX_ROCKET_DISTANCE;
                            yield return new WaitForSeconds(linearTime);
                        }
                    }
                }
                if (_singleAllColumnBombCoroutineDict.Count > 0)
                {
                    // Wait for each coroutine in the queue to finish
                    foreach (var e in _singleAllColumnBombCoroutineDict.Keys)
                        _singleAllColumnBombCoroutineKey.Add(e);

                    for (int i = 0; i < _singleAllColumnBombCoroutineKey.Count; i++)
                    {
                        var key = _singleAllColumnBombCoroutineKey[i];
                        yield return new WaitUntil(() => _singleAllColumnBombCoroutineDict[key]);
                        _singleAllColumnBombCoroutineDict[key] = true;
                    }
                }
            }
            onComplete?.Invoke();
        }

        private IEnumerator HandleRowBombCoroutine(Tile tile, System.Action onCompleted)
        {
            if (tile.HasTriggeredRowBomb) yield break;
            tile.HasTriggeredRowBomb = true;

            BaseVisualEffect vfxPrefab = VFXPoolManager.Instance.GetEffect(VisualEffectID.HorizontalRocket);
            vfxPrefab.transform.position = tile.TileTransform.position;
            vfxPrefab.Play(_rocketPlayTime);

            int centerX = tile.X;
            int y = tile.Y;

            var singleColumnBombCoroutineDict = new Dictionary<Coroutine, bool>();
            var _singleColumnBombCoroutineKey = new List<Coroutine>();

            // Spread out from the center
            for (int offset = 0; offset < Width; offset++)
            {
                int leftX = centerX - offset;
                int rightX = centerX + offset;

                bool leftValid = IsValidGridTile(leftX, y);
                bool rightValid = IsValidGridTile(rightX, y);
                //Debug.Log($"leftX:  {leftX}   {y}    {_tiles[leftX + y * Width] == null} {leftValid}");
                if (leftValid)
                {
                    int index = leftX + y * Width;
                    if (_tiles[index] == null) continue;
                    if (_tiles[index].IsDisplay == false) continue;
                    bool isDisplay = offset == Width - 1;
                    _tiles[index].Display(false);
                    _tiles[index].PlayMatchVFX();

                    // Collect FX
                    if (GameplayManager.Instance.HasTileQuest(_tiles[index].ID))
                    {
                        MatchAnimManager.Instance.Collect(_tiles[index].transform.position, _tiles[index].ID);
                    }


                    //Debug.Log($"AA: {index%Width}  {index /Width} {_tiles[index].SpecialProperties}");
                    if (_tiles[index].SpecialProperties == SpecialTileID.ColumnBomb)
                    {
                        if (_tiles[index].HasTriggererColumnBomb == false)
                        {
                            Coroutine coroutine = null;
                            coroutine = StartCoroutine(HandleColumnBombCoroutine(_tiles[index], () =>
                            {
                                if (coroutine != null)
                                    singleColumnBombCoroutineDict[coroutine] = true;
                            }));
                            if (coroutine != null)
                                singleColumnBombCoroutineDict.Add(coroutine, false);
                        }
                    }
                    else if (_tiles[index].SpecialProperties == SpecialTileID.BlastBomb)
                    {
                        HandleBlastBomb(_tiles[index]);
                    }
                    else if (_tiles[index].SpecialProperties == SpecialTileID.ColorBurst)
                    {
                        if (_tiles[index].HasTriggerColorBurst == false)
                        {
                            HandleTriggerColorBurst(_tiles[index]);
                            if (_colorBurstParentDictionary.ContainsKey(_tiles[index]))
                            {
                                PlayColorBurstVFX(_tiles[index], removeCached: true);
                            }
                        }
                    }
                    SetMatchBuffer(index, MatchID.SpecialMatch);
                }

                if (rightValid && offset != 0) // Avoid double-playing the center tile
                {
                    int index = rightX + y * Width;
                    if (_tiles[index] == null) continue;
                    if (_tiles[index].IsDisplay)
                    {
                        bool isDisplay = offset == Width - 1;
                        _tiles[index].Display(false);
                        _tiles[index].PlayMatchVFX();
                    }

                    // Collect FX
                    if (GameplayManager.Instance.HasTileQuest(_tiles[index].ID))
                    {
                        MatchAnimManager.Instance.Collect(_tiles[index].transform.position, _tiles[index].ID);
                    }


                    if (_tiles[index].SpecialProperties == SpecialTileID.ColumnBomb)
                    {
                        if (_tiles[index].HasTriggererColumnBomb == false)
                        {
                            Coroutine coroutine = null;
                            coroutine = StartCoroutine(HandleColumnBombCoroutine(_tiles[index], () =>
                            {
                                if (coroutine != null)
                                {
                                    if (singleColumnBombCoroutineDict.ContainsKey(coroutine))
                                        singleColumnBombCoroutineDict[coroutine] = true;
                                }
                            }));
                            if (coroutine != null)
                                singleColumnBombCoroutineDict.Add(coroutine, false);
                        }
                    }
                    else if (_tiles[index].SpecialProperties == SpecialTileID.BlastBomb)
                    {
                        HandleBlastBomb(_tiles[index]);
                    }
                    else if (_tiles[index].SpecialProperties == SpecialTileID.ColorBurst)
                    {
                        if (_tiles[index].HasTriggerColorBurst == false)
                        {
                            HandleTriggerColorBurst(_tiles[index]);
                            if (_colorBurstParentDictionary.ContainsKey(_tiles[index]))
                            {
                                PlayColorBurstVFX(_tiles[index], removeCached: true);
                            }
                        }
                    }
                    SetMatchBuffer(index, MatchID.SpecialMatch);
                }

                if (offset < Width - 1)
                {
                    yield return new WaitForSeconds(_rocketPlayTime / HorizontalRocketVfx.MAX_ROCKET_DISTANCE);
                }
            }

            //yield return new WaitForSeconds(_rocketPlayTime / HorizontalRocketVfx.MAX_ROCKET_DISTANCE);


            //Debug.Log($"This: {_singleColumnBombCoroutineDict.Count}");
            if (singleColumnBombCoroutineDict.Count > 0)
            {
                // Wait for each coroutine in the queue to finish
                foreach (var e in singleColumnBombCoroutineDict.Keys)
                    _singleColumnBombCoroutineKey.Add(e);

                for (int i = 0; i < _singleColumnBombCoroutineKey.Count; i++)
                {
                    var key = _singleColumnBombCoroutineKey[i];
                    if (key == null) continue;
                    yield return new WaitUntil(() =>
                    {
                        return singleColumnBombCoroutineDict[key];
                    });
                }
            }
            singleColumnBombCoroutineDict.Clear();
            _singleColumnBombCoroutineKey.Clear();
            onCompleted?.Invoke();
        }

        private Dictionary<Coroutine, bool> _singleAllRowBombCoroutineDict = new Dictionary<Coroutine, bool>();
        private List<Coroutine> _singleAllRowBombCoroutineKey = new List<Coroutine>();
        private IEnumerator HandleAllColumnBombCoroutine(System.Action onComplete)
        {
            _singleAllRowBombCoroutineDict.Clear();
            _singleAllRowBombCoroutineKey.Clear();
            foreach (var tile in _activeColumnBombSet)
            {
                if (tile.HasTriggererColumnBomb) continue;
                tile.HasTriggererColumnBomb = true;

                BaseVisualEffect vfxPrefab = VFXPoolManager.Instance.GetEffect(VisualEffectID.VerticalRocket);
                vfxPrefab.transform.position = tile.TileTransform.position;
                vfxPrefab.Play(_rocketPlayTime);

                int x = tile.X;
                int centerY = tile.Y;

                for (int offset = 0; offset < Height; offset++)
                {
                    int topY = centerY + offset;
                    int bottomY = centerY - offset;

                    bool topValid = IsValidGridTile(x, topY);
                    bool bottomValid = IsValidGridTile(x, bottomY);

                    if (topValid)
                    {
                        int index = x + topY * Width;
                        if (_tiles[index] == null) continue;
                        if (_tiles[index].IsDisplay == false) continue;
                        _tiles[index].Display(false);
                        _tiles[index].PlayMatchVFX();

                        // Collect FX
                        if (GameplayManager.Instance.HasTileQuest(_tiles[index].ID))
                        {
                            MatchAnimManager.Instance.Collect(_tiles[index].transform.position, _tiles[index].ID);
                        }

                        if (_tiles[index].SpecialProperties == SpecialTileID.RowBomb)
                        {
                            //Debug.Log("Handle Row Bomb AA");
                            if (_tiles[index].HasTriggeredRowBomb == false)
                            {
                                Coroutine coroutine = null;
                                coroutine = StartCoroutine(HandleRowBombCoroutine(_tiles[index], () =>
                                {
                                    if (coroutine != null)
                                    {
                                        _singleAllRowBombCoroutineDict[coroutine] = true;
                                    }
                                }));
                                if (coroutine != null)
                                    _singleAllRowBombCoroutineDict.Add(coroutine, false);
                            }
                        }
                        else if (_tiles[index].SpecialProperties == SpecialTileID.BlastBomb)
                        {
                            HandleBlastBomb(_tiles[index]);
                        }
                        else if (_tiles[index].SpecialProperties == SpecialTileID.ColorBurst)
                        {
                            if (_tiles[index].HasTriggerColorBurst == false)
                            {
                                HandleTriggerColorBurst(_tiles[index]);
                                if (_colorBurstParentDictionary.ContainsKey(_tiles[index]))
                                {
                                    PlayColorBurstVFX(_tiles[index], removeCached: true);
                                }
                            }
                        }
                        SetMatchBuffer(index, MatchID.SpecialMatch);
                    }

                    if (bottomValid && offset != 0) // Avoid double-playing the center tile
                    {
                        int index = x + bottomY * Width;
                        if (_tiles[index] == null) continue;
                        if (_tiles[index].IsDisplay == false) continue;
                        _tiles[index].Display(false);

                        // Collect FX
                        if (GameplayManager.Instance.HasTileQuest(_tiles[index].ID))
                        {
                            MatchAnimManager.Instance.Collect(_tiles[index].transform.position, _tiles[index].ID);
                        }

                        if (_tiles[index].SpecialProperties == SpecialTileID.RowBomb)
                        {
                            //Debug.Log("Handle Row Bomb BB");
                            if (_tiles[index].HasTriggeredRowBomb == false)
                            {
                                Coroutine coroutine = null;
                                coroutine = StartCoroutine(HandleRowBombCoroutine(_tiles[index], () =>
                                {
                                    if (coroutine != null)
                                    {
                                        _singleAllRowBombCoroutineDict[coroutine] = true;
                                    }
                                }));
                                if (coroutine != null)
                                    _singleAllRowBombCoroutineDict.Add(coroutine, false);
                            }
                        }
                        else if (_tiles[index].SpecialProperties == SpecialTileID.BlastBomb)
                        {
                            HandleBlastBomb(_tiles[index]);
                        }
                        else if (_tiles[index].SpecialProperties == SpecialTileID.ColorBurst)
                        {
                            if (_tiles[index].HasTriggerColorBurst == false)
                            {
                                HandleTriggerColorBurst(_tiles[index]);
                                if (_colorBurstParentDictionary.ContainsKey(_tiles[index]))
                                {
                                    PlayColorBurstVFX(_tiles[index], removeCached: true);
                                }
                            }
                        }
                        SetMatchBuffer(index, MatchID.SpecialMatch);
                    }

                    if (offset < Height - 1)
                        yield return new WaitForSeconds(_rocketPlayTime / VerticalRocketVfx.MAX_ROCKET_DISTANCE);
                }
            }
            //yield return new WaitForSeconds(_rocketPlayTime / VerticalRocketVfx.MAX_ROCKET_DISTANCE);

            if (_singleAllRowBombCoroutineDict.Count > 0)
            {
                // Wait for each coroutine in the queue to finish
                foreach (var e in _singleAllRowBombCoroutineDict.Keys)
                    _singleAllRowBombCoroutineKey.Add(e);

                for (int i = 0; i < _singleAllRowBombCoroutineKey.Count; i++)
                {
                    var key = _singleAllRowBombCoroutineKey[i];
                    yield return new WaitUntil(() => _singleAllRowBombCoroutineDict[key]);
                }
            }

            onComplete?.Invoke();
        }
        private IEnumerator HandleColumnBombCoroutine(Tile tile, System.Action onCompleted)
        {
            if (tile.HasTriggererColumnBomb) yield break;
            tile.HasTriggererColumnBomb = true;

            BaseVisualEffect vfxPrefab = VFXPoolManager.Instance.GetEffect(VisualEffectID.VerticalRocket);
            vfxPrefab.transform.position = tile.TileTransform.position;
            vfxPrefab.Play(_rocketPlayTime);

            int x = tile.X;
            int centerY = tile.Y;

            var _singleRowBombCoroutineDict = new Dictionary<Coroutine, bool>();
            var _singleRowBombCoroutineKey = new List<Coroutine>();
            for (int offset = 0; offset < Height; offset++)
            {
                int topY = centerY + offset;
                int bottomY = centerY - offset;

                bool topValid = IsValidGridTile(x, topY);
                bool bottomValid = IsValidGridTile(x, bottomY);

                if (topValid)
                {
                    int index = x + topY * Width;
                    if (_tiles[index] == null) continue;
                    if (_tiles[index].IsDisplay == false) continue;
                    _tiles[index].Display(false);
                    _tiles[index].PlayMatchVFX();

                    // Collect FX
                    if (GameplayManager.Instance.HasTileQuest(_tiles[index].ID))
                    {
                        MatchAnimManager.Instance.Collect(_tiles[index].transform.position, _tiles[index].ID);
                    }


                    if (_tiles[index].SpecialProperties == SpecialTileID.RowBomb)
                    {
                        if (_tiles[index].HasTriggeredRowBomb == false)
                        {
                            Coroutine coroutine = null;
                            coroutine = StartCoroutine(HandleRowBombCoroutine(_tiles[index], () =>
                            {
                                if (coroutine != null)
                                    _singleRowBombCoroutineDict[coroutine] = true;
                            }));
                            if (coroutine != null)
                                _singleRowBombCoroutineDict.Add(coroutine, false);
                        }
                    }
                    else if (_tiles[index].SpecialProperties == SpecialTileID.BlastBomb)
                    {
                        HandleBlastBomb(_tiles[index]);
                    }
                    else if (_tiles[index].SpecialProperties == SpecialTileID.ColorBurst)
                    {
                        if (_tiles[index].HasTriggerColorBurst == false)
                        {
                            HandleTriggerColorBurst(_tiles[index]);
                            if (_colorBurstParentDictionary.ContainsKey(_tiles[index]))
                            {
                                PlayColorBurstVFX(_tiles[index], removeCached: true);
                            }
                        }
                    }

                    SetMatchBuffer(index, MatchID.SpecialMatch);
                }

                if (bottomValid && offset != 0) // Avoid double-playing the center tile
                {
                    int index = x + bottomY * Width;
                    if (_tiles[index] == null) continue;
                    if (_tiles[index].IsDisplay == false) continue;
                    _tiles[index].Display(false);
                    _tiles[index].PlayMatchVFX();

                    // Collect FX
                    if (GameplayManager.Instance.HasTileQuest(_tiles[index].ID))
                    {
                        MatchAnimManager.Instance.Collect(_tiles[index].transform.position, _tiles[index].ID);
                    }

                    if (_tiles[index].SpecialProperties == SpecialTileID.RowBomb)
                    {
                        if (_tiles[index].HasTriggeredRowBomb == false)
                        {
                            Coroutine coroutine = null;
                            coroutine = StartCoroutine(HandleRowBombCoroutine(_tiles[index], () =>
                            {
                                if (coroutine != null)
                                    _singleRowBombCoroutineDict[coroutine] = true;
                            }));
                            if (coroutine != null)
                                _singleRowBombCoroutineDict.Add(coroutine, false);
                        }
                    }
                    else if (_tiles[index].SpecialProperties == SpecialTileID.BlastBomb)
                    {
                        HandleBlastBomb(_tiles[index]);
                    }
                    else if (_tiles[index].SpecialProperties == SpecialTileID.ColorBurst)
                    {
                        if (_tiles[index].HasTriggerColorBurst == false)
                        {
                            HandleTriggerColorBurst(_tiles[index]);
                            if (_colorBurstParentDictionary.ContainsKey(_tiles[index]))
                            {
                                PlayColorBurstVFX(_tiles[index], removeCached: true);
                            }
                        }
                    }
                    SetMatchBuffer(index, MatchID.SpecialMatch);
                }
                if (offset < Height - 1)
                    yield return new WaitForSeconds(_rocketPlayTime / VerticalRocketVfx.MAX_ROCKET_DISTANCE);
            }

            onCompleted?.Invoke();

            if (_singleRowBombCoroutineDict.Count > 0)
            {
                // Wait for each coroutine in the queue to finish
                foreach (var e in _singleRowBombCoroutineDict.Keys)
                    _singleRowBombCoroutineKey.Add(e);

                for (int i = 0; i < _singleRowBombCoroutineKey.Count; i++)
                {
                    var key = _singleRowBombCoroutineKey[i];
                    yield return new WaitUntil(() => _singleRowBombCoroutineDict[key]);
                }
            }
        }

        private void HandleFindColorBurst()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Tile currTile = _tiles[x + y * Width];
                    if (IsValidCollectTile(currTile) == false) continue;
                    int sameIDCountInRow = 0;
                    int sameIDCountInColumn = 0;

                    // Horizontal check
                    for (int h = x + 1; h < Width; h++)
                    {
                        Tile nbTile = _tiles[h + y * Width];
                        if (nbTile == null) continue;
                        if (currTile.ID == nbTile.ID && IsValidCollectTile(nbTile))
                        {
                            sameIDCountInRow++;
                        }
                        else break;
                    }

                    // vertical check
                    for (int v = y + 1; v < Height - 1; v++)
                    {
                        Tile nbTile = _tiles[x + v * Width]; // Correct index calculation
                        if (nbTile == null) continue;
                        if (currTile.ID == nbTile.ID && IsValidCollectTile(nbTile))
                        {
                            sameIDCountInColumn++;
                        }
                        else break;
                    }

                    if (sameIDCountInRow >= 4)
                    {
                        _bfsTiles.Clear();
                        bool foundColorBurstTile = false;
                        if (_selectedTile != null && _swappedTile != null)
                        {
                            for (int h = 0; h <= sameIDCountInRow; h++)
                            {
                                int index = (x + h) + y * Width;
                                if (_selectedTile.Equal(_tiles[index]) || _swappedTile.Equals(index))
                                {
                                    foundColorBurstTile = true;
                                    Match3Algorithm.FloodFillBFS(_tiles, _tiles[index].X, _tiles[index].Y, Width, Height, _tiles[index].ID, ref _bfsTiles, ref _bfsSteps);
                                    HandleCollectAnimation(_tiles[index], _bfsTiles);
                                    _matchColorBurstQueue.Enqueue(new SpecialTileQueue(_tiles[index].ID, index));

                                    // Effect
                                    PlayTilesEffect(_bfsTiles, _bfsSteps, VisualEffectID.SpecialTileMergeGhostFX);
                                    break;
                                }
                            }
                        }

                        if (foundColorBurstTile == false)
                        {
                            // Debug.Log($"Not found match5 tile oriign");
                            int index = x + y * Width;
                            Match3Algorithm.FloodFillBFS(_tiles, _tiles[index].X, _tiles[index].Y, Width, Height, _tiles[index].ID, ref _bfsTiles, ref _bfsSteps);
                            Tile medianTile = Match3Algorithm.GetMedianTile(_bfsTiles);
                            HandleCollectAnimation(medianTile, _bfsTiles);
                            _matchColorBurstQueue.Enqueue(new SpecialTileQueue(medianTile.ID, medianTile.X + medianTile.Y * Width));

                            // Effect
                            PlayTilesEffect(_bfsTiles, _bfsSteps, VisualEffectID.SpecialTileMergeGhostFX);
                        }

                        for (int i = 0; i < _bfsTiles.Count; i++)
                        {
                            int index = _bfsTiles[i].X + _bfsTiles[i].Y * Width;
                            SetMatchBuffer(index, MatchID.Match);
                        }
                    }

                    if (sameIDCountInColumn >= 4)
                    {
                        _bfsTiles.Clear();
                        bool foundColorBurstTile = false;
                        if (_selectedTile != null && _swappedTile != null)
                        {
                            for (int v = 0; v <= sameIDCountInColumn; v++)
                            {
                                int index = x + (y + v) * Width;
                                if (_selectedTile.Equal(_tiles[index]) || _swappedTile.Equal(_tiles[index]))
                                {
                                    foundColorBurstTile = true;
                                    Match3Algorithm.FloodFillBFS(_tiles, _tiles[index].X, _tiles[index].Y, Width, Height, _tiles[index].ID, ref _bfsTiles, ref _bfsSteps);
                                    HandleCollectAnimation(_tiles[index], _bfsTiles);
                                    _matchColorBurstQueue.Enqueue(new SpecialTileQueue(_tiles[index].ID, index));

                                    // Effect
                                    PlayTilesEffect(_bfsTiles, _bfsSteps, VisualEffectID.SpecialTileMergeGhostFX);
                                    break;
                                }
                            }

                        }
                        if (foundColorBurstTile == false)
                        {
                            int index = x + y * Width;
                            Match3Algorithm.FloodFillBFS(_tiles, _tiles[index].X, _tiles[index].Y, Width, Height, _tiles[index].ID, ref _bfsTiles, ref _bfsSteps);
                            Tile medianTile = Match3Algorithm.GetMedianTile(_bfsTiles);
                            HandleCollectAnimation(medianTile, _bfsTiles);
                            _matchColorBurstQueue.Enqueue(new SpecialTileQueue(medianTile.ID, medianTile.X + medianTile.Y * Width));

                            // Effect
                            PlayTilesEffect(_bfsTiles, _bfsSteps, VisualEffectID.SpecialTileMergeGhostFX);
                        }

                        for (int i = 0; i < _bfsTiles.Count; i++)
                        {
                            int index = _bfsTiles[i].X + _bfsTiles[i].Y * Width;
                            SetMatchBuffer(index, MatchID.Match);
                        }
                    }
                }
            }
        }

        private bool HandleFindBlastBomb()
        {
            bool FoundBlastBomb(List<int[,]> shapes, int xIndex, int yIndex, ref List<Tile> bfsTiles)
            {
                bfsTiles.Clear();
                for (int i = 0; i < shapes.Count; i++)
                {
                    bool found = true;
                    int w = shapes[i].GetLength(0);
                    int h = shapes[i].GetLength(1);
                    TileID targetTileID = default;

                    bool foundFirstValidTile = false;
                    for (int y = 0; y < h && !foundFirstValidTile; y++)
                    {
                        for (int x = 0; x < h; x++)
                        {
                            if (shapes[i][x, y] != 0)
                            {
                                int offsetX = xIndex + x;
                                int offsetY = yIndex + y;
                                if (IsValidMatchTile(offsetX, offsetY) && _tiles[offsetX + offsetY * Width] != null && _tiles[offsetX + offsetY * Width].SpecialProperties == SpecialTileID.None)
                                {
                                    targetTileID = _tiles[offsetX + offsetY * Width].ID;
                                    foundFirstValidTile = true;
                                    break;
                                }
                            }
                        }
                    }


                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            if (shapes[i][x, y] != 0)
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
                        for (int y = 0; y < h && !foundBlashBombTile; y++)
                        {
                            for (int x = 0; x < w; x++)
                            {
                                if (shapes[i][x, y] == 0) continue;
                                int offsetX = xIndex + x;
                                int offsetY = yIndex + y;
                                int index = offsetX + offsetY * Width;

                                if (_selectedTile != null && _swappedTile != null)
                                {
                                    if (_selectedTile.Equal(_tiles[index]))
                                    {
                                        foundBlashBombTile = true;
                                        Match3Algorithm.FloodFillBFS(_tiles, _selectedTile.X, _selectedTile.Y, Width, Height, _selectedTile.ID, ref bfsTiles, ref _bfsSteps);
                                        Tile medianTile = Match3Algorithm.GetMedianTile(bfsTiles);
                                        if (bfsTiles.Count > 0)
                                        {
                                            _matchBlastBombQueue.Enqueue(new SpecialTileQueue(medianTile.ID, medianTile.X + medianTile.Y * Width));
                                            for (int j = 0; j < bfsTiles.Count; j++)
                                            {
                                                Tile t = bfsTiles[j];
                                                SetMatchBuffer(t.X + t.Y * Width, MatchID.Match);
                                                if (_blastBombDictionary.ContainsKey(t) == false)
                                                {
                                                    _blastBombDictionary.Add(t, medianTile);
                                                }
                                            }
                                            return true;
                                        }
                                        else
                                        {
                                            Debug.Log($"Why bfs tiles count == {_bfsTiles.Count}???");
                                        }

                                    }
                                    else if (_swappedTile.Equal(_tiles[index]))
                                    {
                                        foundBlashBombTile = true;
                                        Match3Algorithm.FloodFillBFS(_tiles, _swappedTile.X, _swappedTile.Y, Width, Height, _swappedTile.ID, ref bfsTiles, ref _bfsSteps);
                                        Tile medianTile = Match3Algorithm.GetMedianTile(bfsTiles);
                                        _matchBlastBombQueue.Enqueue(new SpecialTileQueue(medianTile.ID, medianTile.X + medianTile.Y * Width));

                                        for (int j = 0; j < bfsTiles.Count; j++)
                                        {
                                            Tile t = bfsTiles[j];
                                            SetMatchBuffer(t.X + t.Y * Width, MatchID.Match);
                                            if (_blastBombDictionary.ContainsKey(t) == false)
                                            {
                                                _blastBombDictionary.Add(t, medianTile);
                                            }
                                        }
                                        return true;
                                    }
                                }
                            }
                        }

                        // not found blast bomb origin
                        if (FindOriginalBlastBombIndex(shapes[i], out int xxx, out int yyy))
                        {
                            int offsetX = xIndex + xxx;
                            int offsetY = yIndex + yyy;
                            Match3Algorithm.FloodFillBFS(_tiles, offsetX, offsetY, Width, Height, _tiles[offsetX + offsetY * Width].ID, ref bfsTiles, ref _bfsSteps);
                            Tile medianTile = Match3Algorithm.GetMedianTile(bfsTiles);
                            _matchBlastBombQueue.Enqueue(new SpecialTileQueue(medianTile.ID, medianTile.X + medianTile.Y * Width));

                            for (int j = 0; j < bfsTiles.Count; j++)
                            {
                                Tile t = bfsTiles[j];
                                SetMatchBuffer(t.X + t.Y * Width, MatchID.Match);
                                if (_blastBombDictionary.ContainsKey(t) == false)
                                {
                                    _blastBombDictionary.Add(t, medianTile);
                                }
                            }
                            return true;
                        }
                        else
                        {
                            Debug.Log("?????");
                        }
                    }
                }
                // shape = null;
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
                        if (tile == null) continue;
                        if (FoundBlastBomb(_tShapes, tile.X, tile.Y, ref _bfsTiles))
                        {
                            // Effect
                            PlayTilesEffect(_bfsTiles, _bfsSteps, VisualEffectID.SpecialTileMergeGhostFX);
                            return true;
                        }
                        else
                        {
                            if (FoundBlastBomb(_lShapes, tile.X, tile.Y, ref _bfsTiles))
                            {
                                // Effect
                                PlayTilesEffect(_bfsTiles, _bfsSteps, VisualEffectID.SpecialTileMergeGhostFX);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        private IEnumerator HandleSpecialMatchCoroutine()
        {
            if (_selectedTile == null || _swappedTile == null) yield break;
            // if (_matchBuffer[_selectedTile.X + _selectedTile.Y * Width] != MatchID.None ||
            //     _matchBuffer[_selectedTile.X + _selectedTile.Y * Width] != MatchID.Match) return;
            switch (_selectedTile.SpecialProperties)
            {
                case SpecialTileID.ColumnBomb:
                    if (_swappedTile.SpecialProperties == SpecialTileID.RowBomb)
                    {
                        _swappedTile.HasTriggeredRowBomb = true;
                        if (_activeColumnBombSet.Contains(_selectedTile) == false)
                            _activeColumnBombSet.Add(_selectedTile);
                        if (_activeRowBombSet.Contains(_selectedTile) == false)
                            _activeRowBombSet.Add(_selectedTile);

                        AudioManager.Instance.PlayMatch4Sfx();
                    }
                    else if (_swappedTile.SpecialProperties == SpecialTileID.ColumnBomb)
                    {
                        if (_activeColumnBombSet.Contains(_selectedTile) == false)
                            _activeColumnBombSet.Add(_selectedTile);
                        if (_activeRowBombSet.Contains(_selectedTile) == false)
                            _activeRowBombSet.Add(_selectedTile);
                        _swappedTile.HasTriggererColumnBomb = true;

                        AudioManager.Instance.PlayMatch4Sfx();
                    }
                    else if (_swappedTile.SpecialProperties == SpecialTileID.ColorBurst)
                    {
                        Debug.Log("Handle Colum Bomb + Color Burst here");

                        _tiles[_selectedTile.X + _selectedTile.Y * Width].ReturnToPool();
                        _tiles[_selectedTile.X + _selectedTile.Y * Width] = null;

                        SetMatchBuffer(_swappedTile.X + _swappedTile.Y * Width, MatchID.SpecialMatch);
                        _swappedTile.HasTriggerColorBurst = true;
                        TileID mostTileID = FindMostTile(out int count);

                        float colorBurstDuration = 1f;
                        ColorBurstFX colorBurstFX = (ColorBurstFX)VFXPoolManager.Instance.GetEffect(VisualEffectID.ColorBurstFX);
                        colorBurstFX.transform.position = _swappedTile.TileTransform.position;
                        colorBurstFX.Play(colorBurstDuration + count * 0.1f);
                        colorBurstFX.PlayAnimtion();

                        for (int i = 0; i < _tiles.Length; i++)
                        {
                            int x = i % Width;
                            int y = i / Width;
                            if (_tiles[i] == null) continue;

                            if (_tiles[i].ID == mostTileID)
                            {
                                PlayColorBurstLineFX(_swappedTile, _tiles[i], colorBurstDuration);
                                yield return new WaitForSeconds(0.1f);

                                _tiles[i].ReturnToPool();
                                Tile newTile = AddTile(x, y, TileID.None, BlockID.None, display: true);
                                newTile.UpdatePosition();
                                newTile.PlayAppearAnimation(0.1f);
                                if (Random.value < 0.5f)
                                {
                                    newTile.SetSpecialTile(SpecialTileID.RowBomb);
                                    SetMatchBuffer(i, MatchID.SpecialMatch);
                                }
                                else
                                {
                                    newTile.SetSpecialTile(SpecialTileID.ColumnBomb);
                                    SetMatchBuffer(i, MatchID.SpecialMatch);
                                }
                            }
                        }
                        yield return new WaitForSeconds(colorBurstDuration);
                    }
                    else if (_swappedTile.SpecialProperties == SpecialTileID.BlastBomb)
                    {
                        //Debug.Log("Here");
                        _selectedTile.HasTriggererColumnBomb = true;
                        _swappedTile.HasTriggerBlastBomb = true;
                        yield return StartCoroutine(HandleRowBombAndColumnBombCoroutine(_selectedTile, _swappedTile));
                        AudioManager.Instance.PlayMatch5Sfx();
                    }
                    else if (_swappedTile.SpecialProperties == SpecialTileID.None)
                    {
                        if (_selectedTile.SpecialProperties == SpecialTileID.RowBomb)
                        {
                            if (_activeRowBombSet.Contains(_selectedTile) == false)
                                _activeRowBombSet.Add(_selectedTile);
                        }
                        if (_selectedTile.SpecialProperties == SpecialTileID.ColumnBomb)
                        {
                            //EnableVerticalMatchBuffer(_selectedTile.X);
                            if (_activeColumnBombSet.Contains(_selectedTile) == false)
                                _activeColumnBombSet.Add(_selectedTile);
                        }
                    }
                    break;
                case SpecialTileID.RowBomb:
                    if (_swappedTile.SpecialProperties == SpecialTileID.RowBomb)
                    {
                        _swappedTile.HasTriggeredRowBomb = true;
                        if (_activeRowBombSet.Contains(_selectedTile) == false)
                        {
                            _activeRowBombSet.Add(_selectedTile);
                        }
                        if (_activeColumnBombSet.Contains(_selectedTile) == false)
                        {
                            _activeColumnBombSet.Add(_selectedTile);
                        }
                        AudioManager.Instance.PlayMatch4Sfx();
                    }
                    else if (_swappedTile.SpecialProperties == SpecialTileID.ColumnBomb)
                    {
                        _swappedTile.HasTriggererColumnBomb = true;
                        if (_activeRowBombSet.Contains(_selectedTile) == false)
                        {
                            _activeRowBombSet.Add(_selectedTile);
                        }
                        if (_activeColumnBombSet.Contains(_selectedTile) == false)
                        {
                            _activeColumnBombSet.Add(_selectedTile);
                        }
                        AudioManager.Instance.PlayMatch4Sfx();
                    }
                    else if (_swappedTile.SpecialProperties == SpecialTileID.BlastBomb)
                    {
                        _selectedTile.HasTriggeredRowBomb = true;
                        _swappedTile.HasTriggerBlastBomb = true;
                        yield return StartCoroutine(HandleRowBombAndColumnBombCoroutine(_selectedTile, _swappedTile));
                        AudioManager.Instance.PlayMatch5Sfx();
                    }
                    else if (_swappedTile.SpecialProperties == SpecialTileID.None)
                    {
                        Debug.Log("???");
                        if (_selectedTile.SpecialProperties == SpecialTileID.RowBomb)
                        {
                            if (_activeRowBombSet.Contains(_selectedTile) == false)
                                _activeRowBombSet.Add(_selectedTile);
                        }
                        if (_selectedTile.SpecialProperties == SpecialTileID.ColumnBomb)
                        {
                            //EnableVerticalMatchBuffer(_selectedTile.X);
                            if (_activeColumnBombSet.Contains(_selectedTile) == false)
                                _activeColumnBombSet.Add(_selectedTile);
                        }
                    }
                    else if (_swappedTile.SpecialProperties == SpecialTileID.ColorBurst)
                    {
                        Debug.Log("RowBomb + Color Burst");

                        _tiles[_selectedTile.X + _selectedTile.Y * Width].ReturnToPool();
                        _tiles[_selectedTile.X + _selectedTile.Y * Width] = null;

                        SetMatchBuffer(_swappedTile.X + _swappedTile.Y * Width, MatchID.SpecialMatch);
                        _swappedTile.HasTriggerColorBurst = true;
                        TileID mostTileID = FindMostTile(out int count);

                        float colorBurstDuration = 1f;
                        ColorBurstFX colorBurstFX = (ColorBurstFX)VFXPoolManager.Instance.GetEffect(VisualEffectID.ColorBurstFX);
                        colorBurstFX.transform.position = _swappedTile.TileTransform.position;
                        colorBurstFX.Play(colorBurstDuration + count * 0.1f);
                        colorBurstFX.PlayAnimtion();

                        for (int i = 0; i < _tiles.Length; i++)
                        {
                            int x = i % Width;
                            int y = i / Width;
                            if (_tiles[i] == null) continue;

                            if (_tiles[i].ID == mostTileID)
                            {
                                PlayColorBurstLineFX(_swappedTile, _tiles[i], colorBurstDuration);
                                yield return new WaitForSeconds(0.1f);

                                _tiles[i].ReturnToPool();
                                Tile newTile = AddTile(x, y, TileID.None, BlockID.None, display: true);
                                newTile.UpdatePosition();
                                newTile.PlayAppearAnimation(0.1f);
                                if (Random.value < 0.5f)
                                {
                                    newTile.SetSpecialTile(SpecialTileID.RowBomb);
                                    SetMatchBuffer(i, MatchID.SpecialMatch);
                                }
                                else
                                {
                                    newTile.SetSpecialTile(SpecialTileID.ColumnBomb);
                                    SetMatchBuffer(i, MatchID.SpecialMatch);
                                }
                            }
                        }
                        yield return new WaitForSeconds(colorBurstDuration);
                    }
                    break;
                case SpecialTileID.BlastBomb:
                    if (_swappedTile.SpecialProperties == SpecialTileID.None)
                    {
                        AudioManager.Instance.PlayMatch5Sfx();
                        PlayFlashBombVfx(_selectedTile);
                        HandleBlastBomb(_selectedTile);
                        // UpdateBlastBombTriggerBuffer(_selectedTile);
                        _triggeredMatch5Set.Add(_selectedTile.X + _selectedTile.Y * Width);
                    }
                    else if (_swappedTile.SpecialProperties == SpecialTileID.RowBomb)
                    {
                        _selectedTile.HasTriggerBlastBomb = true;
                        _swappedTile.HasTriggeredRowBomb = true;
                        yield return StartCoroutine(HandleRowBombAndColumnBombCoroutine(_selectedTile, _swappedTile));
                        AudioManager.Instance.PlayMatch5Sfx();

                        //CombineMatch4AndMatch5();
                        //AudioManager.Instance.PlayMatch5Sfx();
                    }
                    else if (_swappedTile.SpecialProperties == SpecialTileID.ColumnBomb)
                    {
                        _selectedTile.HasTriggerBlastBomb = true;
                        _swappedTile.HasTriggererColumnBomb = true;
                        yield return StartCoroutine(HandleRowBombAndColumnBombCoroutine(_selectedTile, _swappedTile));
                        AudioManager.Instance.PlayMatch5Sfx();
                    }
                    else if (_swappedTile.SpecialProperties == SpecialTileID.BlastBomb)
                    {
                        _selectedTile.Display(false);
                        _swappedTile.Display(false);
                        _selectedTile.HasTriggerBlastBomb = true;
                        _swappedTile.HasTriggerBlastBomb = true;

                        Debug.Log("BlastBomb X BlastBomb");
                        BigBlastBombExplosionFX vfx = (BigBlastBombExplosionFX)VFXPoolManager.Instance.GetEffect(VisualEffectID.BigBlastBombExplosionFX);
                        vfx.transform.position = _selectedTile.TileTransform.position;
                        vfx.Play(duration: 1f);
                        yield return new WaitForSeconds(1f);
                        CameraShakeManager.Instance.Shake(intensity: 0.2f, duration: 0.1f);

                        BigBlastBombClear(_selectedTile);
                        AudioManager.Instance.PlayMatch5Sfx();
                    }
                    break;
                case SpecialTileID.ColorBurst:
                    if (_swappedTile.SpecialProperties == SpecialTileID.None)
                    {
                        ClearAllBoardTileID(_selectedTile, _swappedTile.ID);
                        _matchBuffer[_selectedTile.X + _selectedTile.Y * Width] = MatchID.Match;

                        AudioManager.Instance.PlayColorBurstSFX();
                    }
                    else if (_swappedTile.SpecialProperties == SpecialTileID.RowBomb ||
                            _swappedTile.SpecialProperties == SpecialTileID.ColumnBomb)
                    {
                        Debug.Log($"ColorBust +  {_swappedTile.SpecialProperties}");

                        _tiles[_swappedTile.X + _swappedTile.Y * Width].ReturnToPool();
                        _tiles[_swappedTile.X + _swappedTile.Y * Width] = null;

                        SetMatchBuffer(_selectedTile.X + _selectedTile.Y * Width, MatchID.SpecialMatch);
                        _selectedTile.HasTriggerColorBurst = true;
                        TileID mostTileID = FindMostTile(out int count);

                        float colorBurstDuration = 1f;
                        ColorBurstFX colorBurstFX = (ColorBurstFX)VFXPoolManager.Instance.GetEffect(VisualEffectID.ColorBurstFX);
                        colorBurstFX.transform.position = _selectedTile.TileTransform.position;
                        colorBurstFX.Play(colorBurstDuration + count * 0.1f);
                        colorBurstFX.PlayAnimtion();

                        for (int i = 0; i < _tiles.Length; i++)
                        {
                            int x = i % Width;
                            int y = i / Width;
                            if (_tiles[i] == null) continue;

                            if (_tiles[i].ID == mostTileID)
                            {
                                PlayColorBurstLineFX(_selectedTile, _tiles[i], colorBurstDuration);
                                yield return new WaitForSeconds(0.1f);

                                _tiles[i].ReturnToPool();
                                Tile newTile = AddTile(x, y, TileID.None, BlockID.None, display: true);
                                newTile.UpdatePosition();
                                newTile.PlayAppearAnimation(0.1f);
                                if (Random.value < 0.5f)
                                {
                                    newTile.SetSpecialTile(SpecialTileID.RowBomb);
                                    SetMatchBuffer(i, MatchID.SpecialMatch);
                                }
                                else
                                {
                                    newTile.SetSpecialTile(SpecialTileID.ColumnBomb);
                                    SetMatchBuffer(i, MatchID.SpecialMatch);
                                }
                            }
                        }
                        yield return new WaitForSeconds(colorBurstDuration);
                    }
                    break;
                case SpecialTileID.None:
                    if (_swappedTile.SpecialProperties == SpecialTileID.ColorBurst)
                    {
                        ClearAllBoardTileID(_swappedTile, _selectedTile.ID);
                        _matchBuffer[_swappedTile.X + _swappedTile.Y * Width] = MatchID.Match;
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
                                        SetMatchBuffer(index, MatchID.Match);
                                    }
                                }
                            }
                        }

                        AudioManager.Instance.PlayMatch5Sfx();
                    }
                    else if (_swappedTile.SpecialProperties == SpecialTileID.RowBomb)
                    {
                        if (_activeRowBombSet.Contains(_selectedTile) == false)
                        {
                            _activeRowBombSet.Add(_swappedTile);
                        }
                    }
                    else if (_swappedTile.SpecialProperties == SpecialTileID.ColumnBomb)
                    {
                        if (_activeColumnBombSet.Contains(_selectedTile) == false)
                        {
                            _activeColumnBombSet.Add(_swappedTile);
                        }
                    }
                    break;
            }


            void CombineMatch4AndMatch5()
            {
                PlayClearVerticalVFX(_selectedTile);
                if (_activeColumnBombSet.Contains(_selectedTile) == false)
                    _activeColumnBombSet.Add(_selectedTile);

                if (IsValidGridTile(_selectedTile.X - 1, _selectedTile.Y))
                {
                    PlayClearVerticalVFX(_tiles[_selectedTile.X - 1 + _selectedTile.Y * Width]);
                    if (_activeColumnBombSet.Contains(_tiles[_selectedTile.X - 1 + _selectedTile.Y * Width]) == false)
                        _activeColumnBombSet.Add(_tiles[_selectedTile.X - 1 + _selectedTile.Y * Width]);
                }
                if (IsValidGridTile(_selectedTile.X + 1, _selectedTile.Y))
                {
                    PlayClearVerticalVFX(_tiles[_selectedTile.X + 1 + _selectedTile.Y * Width]);
                    if (_activeColumnBombSet.Contains(_tiles[_selectedTile.X + 1 + _selectedTile.Y * Width]) == false)
                        _activeColumnBombSet.Add(_tiles[_selectedTile.X + 1 + _selectedTile.Y * Width]);
                }


                if (_activeRowBombSet.Contains(_selectedTile) == false)
                    _activeRowBombSet.Add(_selectedTile);

                if (IsValidGridTile(_selectedTile.X, _selectedTile.Y - 1))
                {
                    if (_activeRowBombSet.Contains(_tiles[_selectedTile.X + (_selectedTile.Y - 1) * Width]) == false)
                        _activeRowBombSet.Add(_tiles[_selectedTile.X + (_selectedTile.Y - 1) * Width]);
                }

                if (IsValidGridTile(_selectedTile.X, _selectedTile.Y + 1))
                {
                    if (_activeRowBombSet.Contains(_tiles[_selectedTile.X + (_selectedTile.Y + 1) * Width]) == false)
                        _activeRowBombSet.Add(_tiles[_selectedTile.X + (_selectedTile.Y + 1) * Width]);
                }
            }

            void ClearAllTiles()
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int index = x + y * Width;
                        if (_tiles[index] == null) continue;
                        if (_tiles[index].CurrentBlock is NoneBlock)
                        {
                            SetMatchBuffer(index, MatchID.Match);
                        }
                        else
                        {
                            SetMatchBuffer(index, MatchID.SpecialMatch);
                        }
                    }
                }
            }

            void BigBlastBombClear(Tile bigBlastBombTile)
            {
                int size = 4;
                for (int y = bigBlastBombTile.Y - size; y <= bigBlastBombTile.Y + size; y++)
                {
                    for (int x = bigBlastBombTile.X - size; x <= bigBlastBombTile.X + size; x++)
                    {
                        if (IsValidGridTile(x, y))
                        {
                            if (_tiles[x + y * Width] == null) continue;
                            _tiles[x + y * Width].HasTriggerBlastBomb = true;
                            _tiles[x + y * Width].HasTriggerColorBurst = true;
                            _tiles[x + y * Width].HasTriggeredRowBomb = true;
                            _tiles[x + y * Width].HasTriggererColumnBomb = true;
                            SetMatchBuffer(x + y * Width, MatchID.SpecialMatch);
                        }
                    }
                }
            }

            TileID FindMostTile(out int count)
            {
                var mostCommonGroup = _tiles
                    .Where(tile => tile != null && tile.ID != TileID.None) // filter out TileID.None
                    .GroupBy(tile => tile.ID)
                    .OrderByDescending(group => group.Count())
                    .FirstOrDefault();

                if (mostCommonGroup != null)
                {
                    count = mostCommonGroup.Count();
                    return mostCommonGroup.Key;
                }

                count = 0;
                return TileID.None;
            }
        }


        private IEnumerator HandleRowBombAndColumnBombCoroutine(Tile selectedTile, Tile swappedTile)
        {
            Vector3 centerPosition = (selectedTile.transform.position + swappedTile.transform.position) / 2f;


            selectedTile.PlayScaleTile(1.5f, 0.1f, Ease.Linear, () =>
            {
                selectedTile.PlayScaleTile(1f, 0.1f, Ease.Linear, () =>
                {
                    selectedTile.Emissive(0.2f);
                    selectedTile.PlayScaleTile(1.5f, 0.1f, Ease.Linear, () =>
                    {
                        selectedTile.PlayScaleTile(1f, 0.1f, Ease.Linear);
                    });
                });
            });



            swappedTile.PlayScaleTile(1.5f, 0.1f, Ease.Linear, () =>
            {
                swappedTile.PlayScaleTile(1f, 0.1f, Ease.Linear, () =>
                {
                    swappedTile.Emissive(0.2f);
                    swappedTile.PlayScaleTile(1.5f, 0.1f, Ease.Linear, () =>
                    {
                        swappedTile.PlayScaleTile(1f, 0.1f, Ease.Linear);
                    });
                });
            });


            selectedTile.SetRenderOrder(5);
            Vector3 offsetA = (swappedTile.transform.position - selectedTile.transform.position).normalized * 0.5f;
            selectedTile.MoveToPosition(swappedTile.transform.position + offsetA, 0.2f, Ease.Linear, () =>
            {
                selectedTile.SetRenderOrder(10);
                selectedTile.MoveToPosition(swappedTile.transform.position, 0.2f, Ease.Linear, () =>
                {
                    selectedTile.MoveToPosition(centerPosition, 0.1f, Ease.Linear);
                });
            });

            swappedTile.SetRenderOrder(10);
            Vector3 offsetB = (selectedTile.transform.position - swappedTile.transform.position).normalized * 0.5f;
            swappedTile.MoveToPosition(selectedTile.transform.position + offsetB, 0.2f, Ease.Linear, () =>
            {
                swappedTile.SetRenderOrder(5);
                swappedTile.MoveToPosition(selectedTile.transform.position, 0.2f, Ease.Linear, () =>
                {
                    swappedTile.MoveToPosition(centerPosition, 0.1f, Ease.Linear);
                });
            });


            yield return new WaitForSeconds(0.5f);
            Debug.Log("HandleRowBombAndColumnBomb");
            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    int offsetX = selectedTile.X + x;
                    int offsetY = selectedTile.Y + y;

                    HorizontalRocketVfx horizontalVfx = (HorizontalRocketVfx)VFXPoolManager.Instance.GetEffect(VisualEffectID.HorizontalRocket);
                    horizontalVfx.transform.position = new Vector3(offsetX + 0.5f, offsetY + 0.5f, 0f);
                    horizontalVfx.Play(_rocketPlayTime);

                    VerticalRocketVfx verticalVfx = (VerticalRocketVfx)VFXPoolManager.Instance.GetEffect(VisualEffectID.VerticalRocket);
                    verticalVfx.transform.position = new Vector3(offsetX + 0.5f, offsetY + 0.5f, 0f);
                    verticalVfx.Play(_rocketPlayTime);

                    ExplosionVertical(offsetX);
                    ExplosionHorizontal(offsetY);
                }
            }
            PlayFlashBombVfx(_selectedTile);
            //yield return new WaitForSeconds(_rocketPlayTime);



            void ExplosionHorizontal(int y)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (IsValidGridTile(x, y) == false) continue;

                    _matchBuffer[x + y * Width] = MatchID.SpecialMatch;
                    if (_tiles[x + y * Width] != null)
                    {
                        if (_tiles[x + y * Width].CurrentBlock is NoneBlock)
                        {
                            _tiles[x + y * Width].Display(false);
                        }
                    }
                }
            }

            void ExplosionVertical(int x)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (IsValidGridTile(x, y) == false) continue;

                    _matchBuffer[x + y * Width] = MatchID.SpecialMatch;
                    if (_tiles[x + y * Width] != null)
                    {
                        if (_tiles[x + y * Width].CurrentBlock is NoneBlock)
                        {
                            _tiles[x + y * Width].Display(false);
                        }
                    }
                }
            }
        }

        private void HandleMatchSameIDInRow(Tile tile, int sameIDCountInRow)
        {
            if (sameIDCountInRow < 2) return;

            int x = tile.X;
            int y = tile.Y;

            if (sameIDCountInRow == 3)
            {
                bool foundMatch4Tile = false;
                for (int h = 0; h <= 3; h++)
                {
                    int index = (x + h) + y * Width;
                    SetMatchBuffer(index, MatchID.Match);

                    // Effect
                    var vfx = VFXPoolManager.Instance.GetEffect(VisualEffectID.SpecialTileMergeGhostFX);
                    vfx.transform.position = _tiles[index].TileTransform.position;
                    vfx.Play();
                }

                if (_selectedTile != null && _swappedTile != null)
                {
                    for (int h = 0; h <= 3; h++)
                    {
                        int index = (x + h) + y * Width;
                        if (foundMatch4Tile == false)
                        {
                            if (_selectedTile.Equal(_tiles[index]) || _swappedTile.Equal(_tiles[index]))
                            {
                                _matchRowBombQueue.Enqueue(new SpecialTileQueue(TileID.None, index));
                                foundMatch4Tile = true;
                            }
                        }
                    }
                }
                int originIndex = x + 1 + y * Width;
                int cachedIndex = originIndex;
                bool foundOriginIndex = false;
                if (foundMatch4Tile == false)
                {
                    for (int h = 0; h <= 3; h++) // Loop correctly over sameIDCount
                    {
                        int index = (x + h) + y * Width;
                        if (_tiles[index].ID != _prevTileIDs[index])
                        {
                            if (originIndex != index)
                            {
                                cachedIndex = index;
                            }
                            else
                            {
                                foundOriginIndex = true;
                                break;
                            }
                        }
                    }
                    originIndex = foundOriginIndex ? originIndex : cachedIndex;
                    _matchRowBombQueue.Enqueue(new SpecialTileQueue(TileID.None, originIndex));
                }
            }
            else if (sameIDCountInRow == 2)
            {
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


            if (sameIDCountInColumn == 3)
            {
                bool foundMatch4Tile = false;
                if (_selectedTile != null && _swappedTile != null)
                {
                    for (int v = 0; v <= 3; v++)
                    {
                        int index = x + (y + v) * Width;
                        if (_selectedTile.Equal(_tiles[index]) || _swappedTile.Equal(_tiles[index]))
                        {
                            _match4ColumnBombQueue.Enqueue(new SpecialTileQueue(TileID.None, index));
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
                            // _match4ColumnBombQueue.Enqueue(new SpecialTileQueue(_tiles[index].ID, index));
                            _match4ColumnBombQueue.Enqueue(new SpecialTileQueue(TileID.None, index));
                            foundMatch4Tile = true;
                        }
                    }
                    SetMatchBuffer(index, MatchID.Match);

                    // Effect
                    var vfx = VFXPoolManager.Instance.GetEffect(VisualEffectID.SpecialTileMergeGhostFX);
                    vfx.transform.position = _tiles[index].TileTransform.position;
                    vfx.Play();
                }

                if (foundMatch4Tile == false)
                {
                    Debug.Log($"Not found match4 tile oriign");
                    int index = x + y * Width;
                    _match4ColumnBombQueue.Enqueue(new SpecialTileQueue(TileID.None, index));
                }
            }
            else if (sameIDCountInColumn == 2)
            {
                for (int v = 0; v <= sameIDCountInColumn; v++) // Loop correctly over sameIDCount
                {
                    _matchBuffer[x + (y + v) * Width] = MatchID.Match;
                }
            }

            HandleCollectInColumn(tile, sameIDCountInColumn);
        }
        private void MatchPreviousTilesDifferent()
        {
            for (int i = 0; i < _tiles.Length; i++)
            {
                if (_tiles[i] == null) continue;
                _prevTileIDs[i] = _tiles[i].ID;
            }
        }

        // ======================================





        private IEnumerator HandleCollectAnimationCoroutine()
        {
            if (_blastBombDictionary.Count > 0)
            {
                foreach (var tile in _blastBombDictionary)
                {
                    Tile t = tile.Value;
                    Tile nb = tile.Key;
                    if (t != null && nb != null)
                    {
                        float offsetX = -(t.transform.position.x - nb.transform.position.x) * 0.0f;
                        float offsetY = (t.transform.position.y - nb.transform.position.y) * 0.0f;
                        Vector2 offsetPosition = new Vector2(offsetX, offsetY);
                        //nb.transform.DOMove((Vector2)t.transform.position, 0.2f).SetEase(Ease.InSine);
                        nb.MoveToPosition((Vector2)t.transform.position, 0.2f, Ease.InSine);
                    }
                }
                yield return new WaitForSeconds(0.2f);
                int multiplier = 1;
                foreach (var e in _blastBombDictionary)
                {
                    Tile t = e.Value;
                    Tile nb = e.Key;
                    if (t != null && nb != null)
                    {
                        TilePositionInfo tileInfo = new TilePositionInfo(t.ID, t.transform.position, t.X + t.Y * Width);
                        TilePositionInfo nbTileInfo = new TilePositionInfo(nb.ID, (Vector2)nb.transform.position, nb.X + nb.Y * Width);// + new Vector2(0,0.02f) * multiplier);
                        MatchAnimManager.Instance.AddAnotherMatch(tileInfo, nbTileInfo);
                        MatchAnimManager.Instance.AddAnotherMatch(tileInfo, tileInfo);
                    }
                    multiplier++;
                }
            }

            if (_match3Dictionary.Count > 0)
            {
                foreach (var tile in _match3Dictionary)
                {
                    Tile t = tile.Value;
                    Tile nb = tile.Key;
                    if (t != null && nb != null)
                    {
                        TilePositionInfo tileInfo = new TilePositionInfo(t.ID, t.transform.position, t.X + t.Y * Width);
                        TilePositionInfo nbTileInfo = new TilePositionInfo(nb.ID, (Vector2)nb.transform.position, nb.X + nb.Y * Width);
                        MatchAnimManager.Instance.AddMatch3(tileInfo, tileInfo);
                        MatchAnimManager.Instance.AddMatch3(tileInfo, nbTileInfo);
                    }
                }
            }

            if (_match4Dictionary.Count > 0)
            {
                Tile t = null;
                Tile nb = null;
                foreach (var tile in _match4Dictionary)
                {
                    t = tile.Value;
                    nb = tile.Key;
                    if (t != null && nb != null)
                    {
                        float offsetX = -(t.transform.position.x - nb.transform.position.x) * 0.0f;
                        float offsetY = (t.transform.position.y - nb.transform.position.y) * 0.0f;
                        Vector2 offsetPosition = new Vector2(offsetX, offsetY);
                        nb.transform.DOMove((Vector2)t.transform.position, TileAnimationExtensions.TILE_COLLECT_MOVE_TIME).SetEase(Ease.InSine);

                        Utilities.WaitAfter(TileAnimationExtensions.TILE_COLLECT_MOVE_TIME * 0.7f, () =>
                        {
                            nb.Emissive(TileAnimationExtensions.TILE_COLLECT_MOVE_TIME * 0.3f);
                        });
                    }
                }
                Utilities.WaitAfter(TileAnimationExtensions.TILE_COLLECT_MOVE_TIME * 0.7f, () =>
                {
                    t.Emissive(TileAnimationExtensions.TILE_COLLECT_MOVE_TIME * 0.3f);
                });

                yield return new WaitForSeconds(TileAnimationExtensions.TILE_COLLECT_MOVE_TIME);
                int multiplier = 1;
                foreach (var e in _match4Dictionary)
                {
                    t = e.Value;
                    nb = e.Key;
                    if (t != null && nb != null)
                    {
                        TilePositionInfo tileInfo = new TilePositionInfo(t.ID, t.transform.position, t.X + t.Y * Width);
                        TilePositionInfo nbTileInfo = new TilePositionInfo(nb.ID, (Vector2)nb.transform.position, nb.X + nb.Y * Width);// + new Vector2(0,0.02f) * multiplier);

                        MatchAnimManager.Instance.AddAnotherMatch(tileInfo, nbTileInfo);
                        MatchAnimManager.Instance.AddAnotherMatch(tileInfo, tileInfo);
                    }
                    multiplier++;
                }
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
                        nb.transform.DOMove((Vector2)t.transform.position, TileAnimationExtensions.TILE_COLLECT_MOVE_TIME).SetEase(Ease.InSine);
                    }
                }
                yield return new WaitForSeconds(TileAnimationExtensions.TILE_COLLECT_MOVE_TIME);
                foreach (var e in _match5Dictionary)
                {
                    Tile t = e.Value;
                    Tile nb = e.Key;
                    if (t != null && nb != null)
                    {
                        TilePositionInfo tileInfo = new TilePositionInfo(t.ID, t.transform.position, t.X + t.Y * Width);
                        TilePositionInfo nbTileInfo = new TilePositionInfo(nb.ID, (Vector2)nb.transform.position, nb.X + nb.Y * Width);
                        MatchAnimManager.Instance.AddAnotherMatch(tileInfo, nbTileInfo);
                        MatchAnimManager.Instance.AddAnotherMatch(tileInfo, tileInfo);
                    }
                }
            }

            if (_matchAnimationTileDictionary.Count > 0)
            {
                foreach (var tile in _matchAnimationTileDictionary)
                {
                    Tile t = tile.Value;
                    Tile nb = tile.Key;
                    if (t != null && nb != null)
                    {
                        float offsetX = -(t.transform.position.x - nb.transform.position.x) * 0.0f;
                        float offsetY = (t.transform.position.y - nb.transform.position.y) * 0.0f;
                        Vector2 offsetPosition = new Vector2(offsetX, offsetY);
                        nb.transform.DOMove((Vector2)t.transform.position, TileAnimationExtensions.TILE_COLLECT_MOVE_TIME).SetEase(Ease.InSine);
                    }
                }
                yield return new WaitForSeconds(TileAnimationExtensions.TILE_COLLECT_MOVE_TIME);
                foreach (var e in _matchAnimationTileDictionary)
                {
                    Tile t = e.Value;
                    Tile nb = e.Key;
                    if (t != null && nb != null)
                    {
                        TilePositionInfo tileInfo = new TilePositionInfo(t.ID, t.transform.position, t.X + t.Y * Width);
                        TilePositionInfo nbTileInfo = new TilePositionInfo(nb.ID, (Vector2)nb.transform.position, nb.X + nb.Y * Width);
                        MatchAnimManager.Instance.AddAnotherMatch(tileInfo, nbTileInfo);
                        MatchAnimManager.Instance.AddAnotherMatch(tileInfo, tileInfo);
                    }
                }
            }
            _matchAnimationTileDictionary.Clear();
        }
        private void HandleMatchAndUnlock(ref bool hasMatched)
        {
            for (int i = 0; i < _matchBuffer.Length; i++)
            {
                Tile tile = _tiles[i];
                if (tile == null) continue;
                int x = i % Width;
                int y = i / Width;

                MatchID matchID = _matchBuffer[i];
                if (matchID == MatchID.Match)
                {
                    AudioManager.Instance.PlayMatch3Sfx();
                }
                // Match & Unlock
                if (matchID == MatchID.Match)
                {
                    _matchThisPlayTurnSet.Add(new Vector2Int(x, y));

                    tile.Match(_tiles, Width);
                    HandleUnlockTileNeighbors(tile);
                    hasMatched = true;
                }
                else if (matchID == MatchID.SpecialMatch)
                {
                    _matchThisPlayTurnSet.Add(new Vector2Int(x, y));
                    tile.Match(_tiles, Width);
                    if (_unlockTileSet.Contains(i) == false)
                    {
                        _unlockThisPlayTurnSet.Add(new Vector2Int(x, y));

                        _unlockTileSet.Add(i);
                        tile.Unlock();
                    }
                    hasMatched = true;
                }
                else if (matchID == MatchID.ColorBurst)
                {
                    // vfx
                    // if (_colorBurstParentDictionary.ContainsKey(tile.X + tile.Y * Width))
                    // {
                    //     Vector2 startColoBurstPosition = _colorBurstParentDictionary[tile.X + tile.Y * Width];
                    //     PlaySingleColorBurstLineVfx(startColoBurstPosition, tile.transform.position, colorBurstDuration);
                    // }

                    tile.Match(_tiles, Width);
                    if (_unlockTileSet.Contains(i) == false)
                    {
                        _unlockThisPlayTurnSet.Add(new Vector2Int(x, y));

                        _unlockTileSet.Add(i);
                        tile.Unlock();
                    }
                    hasMatched = true;
                }
                SetMatchBuffer(i, MatchID.None);
            }
        }
        private void HandleSpawnSpecialTile()
        {
            // match 6
            while (_matchColorBurstQueue.Count > 0)
            {
                var e = _matchColorBurstQueue.Dequeue();
                int x = e.Index % Width;
                int y = e.Index / Width;

                Tile tile = AddTile(x, y, TileID.None, BlockID.None, display: true);
                tile.UpdatePosition();
                tile.SetSpecialTile(SpecialTileID.ColorBurst);
                tile.Emissive(0.1f);
                _emissiveTileQueue.Enqueue(tile);

                Utilities.WaitAfter(0.1f, () =>
                {
                    tile.Display(false);
                    ColorBurstFX colorBurstFX = (ColorBurstFX)VFXPoolManager.Instance.GetEffect(VisualEffectID.ColorBurstFX);
                    colorBurstFX.transform.position = tile.TileTransform.position;
                    colorBurstFX.SetTarget(tile.TileTransform);
                    colorBurstFX.Play(1f);
                    colorBurstFX.AppearPopupAnimation();
                });


            }

            // match 5
            while (_matchBlastBombQueue.Count > 0)
            {
                var e = _matchBlastBombQueue.Dequeue();
                int x = e.Index % Width;
                int y = e.Index / Width;

                Tile tile = AddTile(x, y, TileID.None, BlockID.None, display: false);
                tile.UpdatePosition();
                tile.SetSpecialTile(SpecialTileID.BlastBomb);

                tile.Emissive(0.1f);
                _emissiveTileQueue.Enqueue(tile);

                // ===BlastBomb
                // BlastBombAppearFX vfx = (BlastBombAppearFX)VFXPoolManager.Instance.GetEffect(VisualEffectID.BlastBombAppear);
                // vfx.transform.position = tile.TileTransform.position;
                // vfx.Play();
            }


            // match 4 horizontal
            while (_matchRowBombQueue.Count > 0)
            {
                var e = _matchRowBombQueue.Dequeue();

                int x = e.Index % Width;
                int y = e.Index / Width;


                Tile tile = AddTile(x, y, TileID.None, BlockID.None, display: false);
                tile.UpdatePosition();
                tile.SetSpecialTile(SpecialTileID.RowBomb);
                tile.Display(false);

                // tile.StopEmissive();
                tile.Emissive(0.1f);
                _emissiveTileQueue.Enqueue(tile);

                Utilities.WaitAfter(0.1f, () =>
                {
                    HorizontalRocketVfx vfx = (HorizontalRocketVfx)VFXPoolManager.Instance.GetEffect(VisualEffectID.HorizontalRocket);
                    vfx.transform.position = tile.TileTransform.position;
                    vfx.PlayAnimtion();
                    Utilities.WaitAfter(0.25f, () =>
                    {
                        vfx.ReturnToPool();
                        tile.Display(true);
                    });
                });
            }


            // match 4 vertical
            while (_match4ColumnBombQueue.Count > 0)
            {
                var e = _match4ColumnBombQueue.Dequeue();

                int x = e.Index % Width;
                int y = e.Index / Width;

                Tile tile = AddTile(x, y, e.TileID, BlockID.None, display: false);
                tile.UpdatePosition();
                tile.SetSpecialTile(SpecialTileID.ColumnBomb);
                tile.Display(false);

                tile.Emissive(0.1f);
                _emissiveTileQueue.Enqueue(tile);

                Transform cachedTileTransform = tile.TileTransform;
                Utilities.WaitAfter(0.1f, () =>
                 {
                     VerticalRocketVfx vfx = (VerticalRocketVfx)VFXPoolManager.Instance.GetEffect(VisualEffectID.VerticalRocket);
                     vfx.transform.position = tile.TileTransform.position;

                     vfx.PlayAnimtion();
                     vfx.SetTargetTransform(cachedTileTransform);
                     Utilities.WaitAfter(0.25f, () =>
                     {
                         vfx.ReturnToPool();
                         tile.Display(true);
                     });
                 });
            }

        }
        #endregion





        #region SHUFFLE
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
        #endregion



        #region HANDLE TILES AFTER TURN FINISHED
        private void HandleBushGrowth()
        {
            int growthTurn = 3;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (IsValidGridTile(x, y))
                    {
                        int index = x + y * Width;
                        Tile tile = _tiles[index];
                        if (tile == null) continue;
                        if (_unlockThisPlayTurnSet.Contains(new Vector2Int(tile.X, tile.Y))) continue;
                        if (tile.CurrentBlock is Bush01)
                        {
                            Bush01 bush = ((Bush01)tile.CurrentBlock);
                            bush.ExistTurnCount++;
                            if (bush.ExistTurnCount >= growthTurn)
                            {
                                tile.ChangeBlock(BlockID.Bush_02);
                            }
                        }
                        else if (tile.CurrentBlock is Bush02)
                        {
                            Bush02 bush = ((Bush02)tile.CurrentBlock);
                            bush.ExistTurnCount++;
                            if (bush.ExistTurnCount >= growthTurn)
                            {
                                tile.ChangeBlock(BlockID.Bush_03);
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
        #endregion




        private void HandleUnlockTileNeighbors(Tile tile)
        {

            if (IsValidGridTile(tile.X - 1, tile.Y))
            {
                int index = tile.X - 1 + tile.Y * Width;
                if (_tiles[index] != null)
                {
                    if (_unlockTileSet.Contains(index) == false)
                    {
                        _unlockTileSet.Add(index);
                        _tiles[index].Unlock();
                    }

                    if (_unlockThisPlayTurnSet.Contains(new Vector2Int(tile.X - 1, tile.Y)) == false)
                    {
                        _unlockThisPlayTurnSet.Add(new Vector2Int(tile.X - 1, tile.Y));
                    }
                }
            }
            if (IsValidGridTile(tile.X + 1, tile.Y))
            {
                int index = tile.X + 1 + tile.Y * Width;
                if (_tiles[index] != null)
                {
                    if (_unlockTileSet.Contains(index) == false)
                    {
                        _unlockTileSet.Add(index);
                        _tiles[index].Unlock();
                    }

                    if (_unlockThisPlayTurnSet.Contains(new Vector2Int(tile.X + 1, tile.Y)) == false)
                    {
                        _unlockThisPlayTurnSet.Add(new Vector2Int(tile.X + 1, tile.Y));
                    }
                }
            }
            if (IsValidGridTile(tile.X, tile.Y - 1))
            {
                int index = tile.X + (tile.Y - 1) * Width;
                if (_tiles[index] != null)
                {
                    if (_unlockTileSet.Contains(index) == false)
                    {
                        _unlockTileSet.Add(index);
                        _tiles[index].Unlock();
                    }

                    if (_unlockThisPlayTurnSet.Contains(new Vector2Int(tile.X, tile.Y - 1)) == false)
                    {
                        _unlockThisPlayTurnSet.Add(new Vector2Int(tile.X, tile.Y - 1));
                    }
                }
            }
            if (IsValidGridTile(tile.X, tile.Y + 1))
            {
                int index = tile.X + (tile.Y + 1) * Width;
                if (_tiles[index] != null)
                {
                    if (_unlockTileSet.Contains(index) == false)
                    {
                        _unlockTileSet.Add(index);
                        _tiles[index].Unlock();
                    }

                    if (_unlockThisPlayTurnSet.Contains(new Vector2Int(tile.X, tile.Y + 1)) == false)
                    {
                        _unlockThisPlayTurnSet.Add(new Vector2Int(tile.X, tile.Y + 1));
                    }
                }
            }
        }

        private void HandleTriggerAllSpecialTiles()
        {
            for (int i = 0; i < _matchBuffer.Length; i++)
            {
                Tile tile = _tiles[i];
                if (tile == null) continue;
                MatchID matchID = _matchBuffer[i];

                if (matchID.IsSpecialMatch())
                {
                    if (_triggeredMatch5Set.Contains(i) == false)
                    {
                        if (tile.SpecialProperties == SpecialTileID.RowBomb)
                        {
                            //Debug.Log("Handle Special Match 4 Horizontal");
                            //EnableHorizontalMatchBuffer(tile.Y);
                            if (tile.HasTriggeredRowBomb == false)
                            {
                                if (_activeRowBombSet.Contains(tile) == false)
                                {
                                    _activeRowBombSet.Add(tile);
                                }
                            }
                        }
                        else if (tile.SpecialProperties == SpecialTileID.ColumnBomb)
                        {
                            if (tile.HasTriggererColumnBomb == false)
                            {
                                if (_activeColumnBombSet.Contains(tile) == false)
                                {
                                    _activeColumnBombSet.Add(tile);
                                }
                            }
                        }
                        else if (tile.SpecialProperties == SpecialTileID.BlastBomb)
                        {
                            //Debug.Log("Handle Special Match 5");
                            if (tile.HasTriggerBlastBomb == false)
                            {
                                PlayFlashBombVfx(tile);
                                UpdateBlastBombTriggerBuffer(tile);
                            }
                        }
                        else if (tile.SpecialProperties == SpecialTileID.ColorBurst)
                        {
                            if (tile.HasTriggerColorBurst == false)
                            {
                                Debug.Log("Handle Special Color Burst");
                                HandleTriggerColorBurst(tile);
                            }
                        }
                    }
                }
            }
        }

        private void HandleTriggerColorBurst(Tile tile)
        {
            tile.HasTriggerColorBurst = true;
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
        private IEnumerator AutoFillCoroutine(System.Action onCompleted = null)
        {
            // Debug.Log("AutoFillCoroutine");
            if (_fillType == FillType.Dropdown)
            {
                yield return StartCoroutine(DropdDownFillGridCoroutine());
            }
            else if (_fillType == FillType.SandFalling)
            {
                yield return StartCoroutine(SandFallingFillGridCoroutine());
            }
            onCompleted?.Invoke();
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




        private void UpdateBlastBombTriggerBuffer(Tile tile)
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
                                if (_colorBurstParentDictionary.ContainsKey(tile) == false)
                                {
                                    _colorBurstParentDictionary[tile] = new();
                                    _colorBurstParentDictionary[tile].Add(_tiles[index]);
                                    //_colorBurstParentDictionary.Add(index, tile.transform.position);
                                }
                                else
                                {
                                    _colorBurstParentDictionary[tile].Add(_tiles[index]);
                                }
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


        private void HandleCollectAnimation(Tile tile, List<Tile> bfsTiles)
        {
            Debug.Log("HandleCollectFloodFill");
            for (int i = 0; i < bfsTiles.Count; i++)
            {
                if (_matchAnimationTileDictionary.ContainsKey(bfsTiles[i]) == false)
                {
                    _matchAnimationTileDictionary.Add(bfsTiles[i], tile);
                }
            }
        }

        private void HandleCollectInrow(Tile tile, int sameIDCountInRow)
        {
            int offsetX = 0;
            if (sameIDCountInRow == 2) offsetX = 0;
            else if (sameIDCountInRow == 3) offsetX = 1;
            else if (sameIDCountInRow == 4) offsetX = 2;

            int originIndex = (tile.X + offsetX) + tile.Y * Width;
            int x = tile.X;
            int y = tile.Y;
            bool foundOriginIndex = false;
            for (int h = 0; h <= sameIDCountInRow; h++) // Loop correctly over sameIDCount
            {
                int index = (x + h) + y * Width;
                if (_selectedTile != null)
                    if (_tiles[index].Equal(_selectedTile))
                    {
                        originIndex = _selectedTile.X + _selectedTile.Y * Width;
                        foundOriginIndex = true;
                        break;
                    }

                if (_swappedTile != null)
                    if (_tiles[index].Equal(_swappedTile))
                    {
                        originIndex = _swappedTile.X + _swappedTile.Y * Width;
                        foundOriginIndex = true;
                        break;
                    }
            }
            if (foundOriginIndex == false)
            {
                // Debug.Log("Not found origin index");
                int cachedIndex = originIndex;
                for (int h = 0; h <= sameIDCountInRow; h++)
                {
                    int index = (x + h) + y * Width;
                    if (_prevTileIDs[index] != _tiles[index].ID)
                    {
                        if (originIndex != index)
                        {
                            cachedIndex = index;
                        }
                        else
                        {
                            foundOriginIndex = true;
                            break;
                        }
                    }
                }

                originIndex = foundOriginIndex ? originIndex : cachedIndex;
                // originIndex = (tile.X + 1) + tile.Y * Width;
            }


            for (int h = 0; h <= sameIDCountInRow; h++) // Loop correctly over sameIDCount
            {
                int index = (x + h) + y * Width;
                if (index == originIndex) continue;
                if (sameIDCountInRow == 2)
                {
                    if (_match3Dictionary.ContainsKey(_tiles[index]) == false)
                    {
                        _match3Dictionary.Add(_tiles[index], _tiles[originIndex]);
                    }

                }
                else if (sameIDCountInRow == 3)
                {
                    if (_match4Dictionary.ContainsKey(_tiles[index]) == false)
                    {
                        _match4Dictionary.Add(_tiles[index], _tiles[originIndex]);
                    }

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
            int originIndex = tile.X + tile.Y * Width;
            int x = tile.X;
            int y = tile.Y;
            bool foundOriginIndex = false;
            for (int v = 0; v <= sameIDCountInColumn; v++)
            {
                int index = x + (y + v) * Width;
                if (_selectedTile != null)
                    if (_tiles[index].Equal(_selectedTile))
                    {
                        originIndex = _selectedTile.X + _selectedTile.Y * Width;
                        foundOriginIndex = true;
                        break;
                    }

                if (_swappedTile != null)
                    if (_tiles[index].Equal(_swappedTile))
                    {
                        originIndex = _swappedTile.X + _swappedTile.Y * Width;
                        foundOriginIndex = true;
                        break;
                    }
            }


            if (foundOriginIndex == false)
            {
                int cachedIndex = originIndex;
                for (int v = 0; v <= sameIDCountInColumn; v++)
                {
                    int index = x + (y + v) * Width;
                    if (_prevTileIDs[index] != _tiles[index].ID)
                    {
                        if (originIndex != index)
                        {
                            cachedIndex = index;
                        }
                        else
                        {
                            foundOriginIndex = true;
                            break;
                        }
                    }
                }
                originIndex = foundOriginIndex ? originIndex : cachedIndex;
            }

            for (int v = 0; v <= sameIDCountInColumn; v++)
            {
                int index = x + (y + v) * Width;
                if (index == originIndex) continue;

                if (sameIDCountInColumn == 2)
                {
                    if (_match3Dictionary.ContainsKey(_tiles[index]) == false)
                    {
                        _match3Dictionary.Add(_tiles[index], _tiles[originIndex]);
                    }

                }
                else if (sameIDCountInColumn == 3)
                {
                    if (_match4Dictionary.ContainsKey(_tiles[index]) == false)
                    {
                        _match4Dictionary.Add(_tiles[index], _tiles[originIndex]);
                    }
                }
                else if (sameIDCountInColumn >= 4)
                {
                    if (_match5Dictionary.ContainsKey(_tiles[index]) == false)
                        _match5Dictionary.Add(_tiles[index], _tiles[originIndex]);
                }
            }
        }

        private bool FindOriginalBlastBombIndex(int[,] shape, out int x, out int y)
        {
            int width = shape.GetLength(0);
            int height = shape.GetLength(1);

            for (int yy = 0; yy < height; yy++)
            {
                for (int xx = 0; xx < width; xx++)
                {
                    if (shape[xx, yy] == 2)
                    {
                        x = xx;
                        y = yy;
                        return true;
                    }
                }
            }

            x = 0;
            y = 0;
            Debug.LogError("Not found origin source !!!!!!!!!");
            return false;
        }

        private void HandleCollectBlastBomb(int[,] shape, int xIndex, int yIndex)
        {
            int startX = xIndex;
            int startY = yIndex;
            int endX = startX + shape.GetLength(0);
            int endY = startY + shape.GetLength(1);

            int innerX = 0, innerY = 0;
            int sourceIndex = 0;
            bool foundSourceIndex = false;
            for (int y = startY; y < endY; y++, innerY++)
            {
                innerX = 0;
                for (int x = startX; x < endX; x++, innerX++)
                {
                    int index = x + y * Width;
                    if (IsValidGridTile(x, y) == false) continue;
                    if (shape[innerX, innerY] == 0) continue;
                    if (_tiles[index].Equal(_selectedTile))
                    {
                        sourceIndex = index;
                        foundSourceIndex = true;
                        break;
                    }
                    if (_tiles[index].Equal(_swappedTile))
                    {
                        sourceIndex = index;
                        foundSourceIndex = true;
                        break;
                    }
                }
            }

            innerX = 0;
            innerY = 0;

            if (foundSourceIndex)
            {
                for (int y = startY; y < endY; y++, innerY++)
                {
                    innerX = 0;
                    for (int x = startX; x < endX; x++, innerX++)
                    {
                        if (shape[innerX, innerY] == 0) continue;
                        int index = x + y * Width;
                        if (index == sourceIndex) continue;
                        if (_blastBombDictionary.ContainsKey(_tiles[index]) == false)
                            _blastBombDictionary.Add(_tiles[index], _tiles[sourceIndex]);
                    }
                }
            }
            else
            {
                if (FindOriginalBlastBombIndex(shape, out int xxx, out int yyy))
                {
                    sourceIndex = (xIndex + xxx) + (yIndex + yyy) * Width;
                    bool isSourceIndexValid = false;
                    for (int y = startY; y < endY && !isSourceIndexValid; y++, innerY++)
                    {
                        innerX = 0;
                        for (int x = startX; x < endX; x++, innerX++)
                        {
                            if (shape[innerX, innerY] == 0) continue;
                            int index = x + y * Width;
                            if (index == sourceIndex)
                            {
                                isSourceIndexValid = true;
                                break;
                            }
                        }
                    }

                    if (isSourceIndexValid)
                    {
                        innerX = 0;
                        innerY = 0;
                        for (int y = startY; y < endY; y++, innerY++)
                        {
                            innerX = 0;
                            for (int x = startX; x < endX; x++, innerX++)
                            {
                                if (shape[innerX, innerY] == 0) continue;
                                int index = x + y * Width;
                                if (index == sourceIndex) continue;
                                if (_blastBombDictionary.ContainsKey(_tiles[index]) == false)
                                    _blastBombDictionary.Add(_tiles[index], _tiles[sourceIndex]);
                            }
                        }
                    }
                    else
                    {
                        innerX = 0;
                        innerY = 0;
                        for (int y = startY; y < endY; y++, innerY++)
                        {
                            innerX = 0;
                            for (int x = startX; x < endX; x++, innerX++)
                            {
                                if (shape[innerX, innerY] == 0) continue;
                                int index = x + y * Width;
                                if (index == sourceIndex) continue;

                                if (_blastBombDictionary.ContainsKey(_tiles[index]) == false)
                                    _blastBombDictionary.Add(_tiles[index], _tiles[sourceIndex]);
                            }
                        }
                    }
                }
            }
        }

        //private bool CanFill()
        //{
        //    for (int y = 0; y < Height; y++)
        //    {
        //        for (int x = 0; x < Width; x++)
        //        {
        //            Tile tile = _tiles[x + y * Width];
        //            if (tile == null)
        //            {
        //                return true;
        //            }
        //        }
        //    }
        //    return false;
        //}

        private bool _tileHasMove;

        private IEnumerator DropdDownFillGridCoroutine()
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
                                if (aboveTile == null) continue;
                                if (aboveTile.CurrentBlock.CanFillDownThrough() == false) break;
                                if (aboveTile.CurrentBlock is NoneBlock)
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

            _fillDownDictionary.Clear();
            for (int i = 0; i < _fillBlockIndices.Count; i++)
            {
                int index = _fillBlockIndices[i];
                int x = index % Width;
                int y = index / Width;

                int fillLength = 1;
                for (int yy = y; yy >= 0; yy--)
                {
                    Tile currTile = _tiles[x + yy * Width];
                    if (currTile == null)
                    {
                        if (IsValidGridTile(x, yy + 1))
                        {
                            Tile aboveTile = _tiles[x + (yy + 1) * Width];
                            if (aboveTile != null && aboveTile.CurrentBlock is not NoneBlock && aboveTile.CurrentBlock is not FillBlock && aboveTile.CurrentBlock is not VoidBlock) break;
                            fillLength++;
                        }
                    }
                }
                // Debug.Log($"Fill Length:    {x}     {fillLength}");

                for (int yy = y; yy >= 0; yy--)
                {
                    Tile currTile = _tiles[x + yy * Width];
                    if (currTile == null)
                    {
                        if (IsValidGridTile(x, yy + 1))
                        {
                            Tile aboveTile = _tiles[x + (yy + 1) * Width];
                            // Debug.Log($"AAA:   {x}  {yy}");
                            if (aboveTile != null && aboveTile.CurrentBlock is not NoneBlock && aboveTile.CurrentBlock is not FillBlock && aboveTile.CurrentBlock is not VoidBlock) break;
                            // Debug.Log($"BBB:   {x}  {yy}");
                            TileID randomTileID = _levelData.AvaiableTiles[UnityEngine.Random.Range(0, _levelData.AvaiableTiles.Length)];
                            Tile newTile = AddTile(x, yy, randomTileID, BlockID.None, display: false);
                            if (_fillDownDictionary.ContainsKey(x) == false)
                            {
                                _fillDownDictionary.Add(x, 1);
                            }
                            else
                            {
                                _fillDownDictionary[x]++;
                            }
                            // int offsetFillY = Height - yy + _fillDownDictionary[x] - 2;    // 2: 1 fill block + sizeY = height - 1
                            newTile.SetTileVisualizePosition(newTile.X, (y - 1) + fillLength - _fillDownDictionary[x]);
                            newTile.Display(true);
                        }

                    }
                }
            }


            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int i = x + y * Width;
                    if (_tiles[i] != null)
                    {
                        _tiles[i].Display(true);
                        if (_tiles[i].IsCorrectPosition(out float distance) == false)
                        {
                            _tiles[i].FallDownToGridPosition(TileAnimationExtensions.TILE_FALLDOWN_TIME * distance);
                        }
                    }
                }
            }

            if (_fillBlockIndices.Count > 0)
            {
                int maxColumnFillCount = 1;
                foreach (var e in _fillDownDictionary)
                {
                    if (e.Value > maxColumnFillCount)
                        maxColumnFillCount = e.Value;
                }
                yield return new WaitForSeconds(TileAnimationExtensions.TILE_FALLDOWN_TIME * maxColumnFillCount);
            }

            yield return null;
        }



        private IEnumerator SandFallingFillGridCoroutine()
        {
            void SwapTile(int x, int y, int xx, int yy)
            {
                var temp = _tiles[x + y * Width];
                _tiles[x + y * Width] = _tiles[xx + yy * Width];
                _tiles[xx + yy * Width] = temp;
            }

            _tileHasMove = false;
            int attempts = 0;

            // Drop down avaiable tiles
            while (true)
            {
                if (attempts++ > Width * Height)
                {
                    Debug.LogError("=========== something went wrong ===========");
                    break;
                }

                _tileHasMove = false;
                for (int y = 1; y < Height - 1; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        Tile currTile = _tiles[x + y * Width];
                        if (currTile != null)
                        {
                            if (IsValidMoveTile(x, y) == false)
                            {
                                //Debug.Log($"Block {currTile.CurrentBlock.BlockID} cannot move:   {x}  {y}");
                                continue;
                            }
                            if (currTile.CurrentBlock.CanFillDownThrough() == false)
                            {
                                //Debug.Log($"Block {currTile.CurrentBlock.BlockID} cannot drop down:   {x}  {y}");
                                continue;
                            }

                            // Check below tiles
                            for (int yy = y - 1; yy >= 0; yy--)
                            {
                                //Debug.Log($"Loop:  ({x}   {y})       ({x}   {yy})");
                                if (IsValidGridTile(x, yy))
                                {
                                    if (_tiles[x + yy * Width] != null)
                                    {
                                        if (_tiles[x + yy * Width].CurrentBlock is not VoidBlock &&
                                           _tiles[x + yy * Width].CurrentBlock is not NoneBlock)
                                        {
                                            //Debug.Log($"early break  {_tiles[x + yy * Width].CurrentBlock}");
                                            break;
                                        }

                                        //if (_tiles[x + yy * Width].CurrentBlock is NoneBlock)
                                        //{
                                        //    // Debug.Log($"early break  {_tiles[x + yy * Width].CurrentBlock}");
                                        //    break;
                                        //}
                                    }
                                }

                                if (IsValidFillTile(x, yy))
                                {
                                    _tileHasMove = true;
                                    SwapTile(x, yy, x, y);
                                    _tiles[x + yy * Width].SetGridPosition(x, yy);
                                    int offsetY = y - yy;
                                    float totalMoveTime = TileAnimationExtensions.TILE_MOVE_TIME * offsetY;
                                    _tiles[x + yy * Width].FallDownToGridPosition(totalMoveTime);
                                    break;
                                }
                            }
                        }
                    }
                    //yield return new WaitForSeconds(0.5f);
                    //Debug.Log(" ------------ ");
                }



                // New tiles
                for (int i = 0; i < _fillBlockIndices.Count; i++)
                {
                    int index = _fillBlockIndices[i];
                    int x = index % Width;
                    int y = index / Width;

                    Tile fillBlockTile = _tiles[x + y * Width];
                    if (IsValidGridTile(x, y - 1))
                    {
                        Tile tileBelow = _tiles[x + (y - 1) * Width];
                        if (tileBelow == null)
                        {
                            _tileHasMove = true;
                            TileID randomTileID = _levelData.AvaiableTiles[Random.Range(0, _levelData.AvaiableTiles.Length)];
                            Tile newTile = AddTile(x, y - 1, randomTileID, BlockID.None, display: true);
                            newTile.SetTileVisualizePosition(x, y);
                            newTile.MoveToGridPosition(TileAnimationExtensions.TILE_FALLDOWN_TIME);
                        }
                        else
                        {
                            tileBelow.FallDownScaleAnimation();
                        }
                    }
                }


#if true   // Sand falling style fill
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 1; y < Height - 1; y++)
                    {
                        Tile currTile = _tiles[x + y * Width];
                        if (currTile != null)
                        {
                            if (IsValidMoveTile(x, y) == false) continue;
                            bool hasFilled = false;
                            // Check bottom
                            if (IsValidGridTile(x, y - 1) && _tiles[x + (y - 1) * Width] == null)
                            {
                                break;
                            }
                            else
                            {
                                int directionX = Random.value < 0.5f ? -1 : 1;
                                for (int i = 0; i < 2 && !hasFilled; i++)
                                {
                                    int dx = directionX;
                                    int expectedFill = directionX == -1 ? 1 : -1;

                                    int checkX = x + dx;
                                    int checkY = y - 1;

                                    if (GetFillType(checkX, checkY) == expectedFill && IsValidFillTile(checkX, checkY))
                                    {
                                        hasFilled = true;
                                        _tileHasMove = true;
                                        SwapTile(checkX, checkY, x, y);
                                        _tiles[checkX + checkY * Width].SetGridPosition(checkX, checkY);
                                        _tiles[checkX + checkY * Width].FallDownToGridPosition(TileAnimationExtensions.TILE_MOVE_TIME);
                                        break;
                                    }

                                    // Flip direction for the second check
                                    directionX *= -1;
                                }

                                // int fillType = GetFillType(x, y);
                                // int botLeftFillType = GetFillType(x - 1, y - 1);
                                // int botRightFillType = GetFillType(x + 1, y - 1);
                                // // Check bottom-left
                                // if (botLeftFillType == 1 && hasFilled == false)
                                // {
                                //     if (IsValidFillTile(x - 1, y - 1))
                                //     {
                                //         Debug.Log("Fill Left");
                                //         hasFilled = true;
                                //         _tileHasMove = true;
                                //         SwapTile(x - 1, y - 1, x, y);
                                //         _tiles[x - 1 + (y - 1) * Width].SetGridPosition(x - 1, y - 1);
                                //         _tiles[x - 1 + (y - 1) * Width].FallDownToGridPosition(TileAnimationExtensions.TILE_MOVE_TIME);
                                //     }
                                // }
                                // if (botRightFillType == -1 && hasFilled == false)
                                // {
                                //     if (IsValidFillTile(x + 1, y - 1))
                                //     {
                                //         Debug.Log($"({x + 1}  {y - 1})     ({x}  {y})   ");
                                //         Debug.Log("Fill right");
                                //         hasFilled = true;
                                //         _tileHasMove = true;
                                //         SwapTile(x + 1, y - 1, x, y);
                                //         _tiles[x + 1 + (y - 1) * Width].SetGridPosition(x + 1, y - 1);
                                //         _tiles[x + 1 + (y - 1) * Width].FallDownToGridPosition(TileAnimationExtensions.TILE_MOVE_TIME);
                                //     }
                                // }
                            }
                        }
                    }
                }
#endif

                yield return new WaitForSeconds(TileAnimationExtensions.TILE_FALLDOWN_TIME);
                if (_tileHasMove == false)
                {
                    break;
                }
            }
            //Debug.Log($"attempts:  {attempts}");
        }






        #region Utilities

        private Tile AddTile(int x, int y, TileID tileID, BlockID blockID, bool display = true)
        {
            // Debug.Log($"AddTile:  {x}  {y}  {tileID}");
            // Tile
            Tile tileInstance = TilePoolManager.Instance.GetTile(tileID);
            tileInstance.Bloom(false);

            tileInstance.Display(display);
            tileInstance.SetSpecialTile(SpecialTileID.None);
#if UNITY_WEBGL
            tileInstance.SetInteractionMask(SpriteMaskInteraction.None);
#else
            tileInstance.SetInteractionMask(SpriteMaskInteraction.VisibleInsideMask);
#endif
            tileInstance.SetGridPosition(x, y);
            _tiles[x + y * Width] = tileInstance;

            // Block
            tileInstance.ChangeBlock(blockID);
            return tileInstance;
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
                        currTile.CurrentBlock is not Lock) continue;

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
                            nbTile.CurrentBlock is Lock))
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
                            (nbTile.CurrentBlock is NoneBlock || nbTile.CurrentBlock is Lock))
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
                            (downTile.CurrentBlock is NoneBlock || downTile.CurrentBlock is Lock) &&
                            (downOfDownTile.CurrentBlock is NoneBlock || downOfDownTile.CurrentBlock is Lock))
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
                            (topTile.CurrentBlock is NoneBlock || topTile.CurrentBlock is Lock) &&
                            (topOfTopTile.CurrentBlock is NoneBlock || topOfTopTile.CurrentBlock is Lock))
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
                            (rightTile.CurrentBlock is NoneBlock || rightTile.CurrentBlock is Lock) &&
                            (rightOfRightTile.CurrentBlock is NoneBlock || rightOfRightTile.CurrentBlock is Lock))
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
                            (leftTile.CurrentBlock is NoneBlock || leftTile.CurrentBlock is Lock) &&
                            (leftOfLeftTile.CurrentBlock is NoneBlock || leftOfLeftTile.CurrentBlock is Lock))
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
            return !(x < 0 || x >= Width || y < 0 || y >= Height);
        }
        private bool IsValidFillTile(int x, int y)
        {
            return IsValidGridTile(x, y) && _tiles[x + y * Width] == null;
        }

        private bool IsValidMoveTile(int x, int y)
        {
            return IsValidGridTile(x, y) && _tiles[x + y * Width] != null && _tiles[x + y * Width].CurrentBlock is NoneBlock;
        }

        private bool IsValidMatchTile(int x, int y)
        {
            return !(x < 0 || x >= Width || y < 0 || y >= Height) &&
                _tiles[x + y * Width] != null &&
                (_tiles[x + y * Width].CurrentBlock is NoneBlock ||
                _tiles[x + y * Width].CurrentBlock is Lock);
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

        private bool IsValidCollectTile(Tile tile)
        {
            if (tile == null) return false;
            if (tile.ID == TileID.None) return false;
            if (tile.SpecialProperties is SpecialTileID.BlastBomb or SpecialTileID.ColorBurst or SpecialTileID.ColumnBomb or SpecialTileID.RowBomb) return false;
            if (tile.CurrentBlock is not NoneBlock and not Lock) return false;
            if (_matchBuffer[tile.X + tile.Y * Width] != MatchID.None) return false;
            return true;
        }


        private int GetFillType(int x, int y)
        {
            Tile closestLeft = null;
            Tile closestRight = null;
            int minLeftDistance = int.MaxValue;
            int minRightDistance = int.MaxValue;

            for (int i = 0; i < _fillBlockIndices.Count; i++)
            {
                int xx = _fillBlockIndices[i] % Width;
                int yy = _fillBlockIndices[i] / Width;

                // Debug.Log($"_fillBlockIndices  {i}  {x}  {y}");

                // Exact vertical match case
                if (y < yy && x == xx)
                {
                    //Debug.Log("case A - exact vertical match");
                    return 0;
                }

                // Check for potential left/right candidates
                if (y < yy)
                {
                    int horizontalDistance = Mathf.Abs(xx - x);

                    // Check left side (xx < x)
                    if (xx < x && horizontalDistance < minLeftDistance)
                    {
                        minLeftDistance = horizontalDistance;
                        closestLeft = _tiles[xx + yy * Width];
                    }
                    // Check right side (xx > x)
                    else if (xx > x && horizontalDistance < minRightDistance)
                    {
                        minRightDistance = horizontalDistance;
                        closestRight = _tiles[xx + yy * Width];
                    }
                }
            }

            // Determine which candidate to return
            if (closestLeft != null || closestRight != null)
            {
                // Prefer the closer one, or left if equal distance
                if (minLeftDistance < minRightDistance)
                {
                    // Debug.Log($"case B - nearest left (distance: {minLeftDistance})");
                    return -1;
                }
                else if (minRightDistance < minLeftDistance)
                {
                    // Debug.Log($"case C - nearest right (distance: {minRightDistance})");
                    return 1;
                }
                else
                {
                    // If equal distance, you could return either - here we choose left
                    // Debug.Log($"case D - equal distance, choosing left");
                    // return closestLeft ?? closestRight;
                    return -1;
                }
            }

            Debug.Log("NOt FOUND CASE");
            return 0;
        }
        #endregion




        #region VFX
        private void PlayClearHorizontalVFX(Tile tile)
        {
            return;
            BaseVisualEffect vfxPrefab = VFXPoolManager.Instance.GetEffect(VisualEffectID.HorizontalRocket);
            vfxPrefab.transform.position = tile.TileTransform.position;
            vfxPrefab.Play(0.5f);

            //if (GameDataManager.Instance.TryGetVfxByID(VisualEffectID.HorizontalRocket, out var vfxPrefab))
            //{
            //    var instance = Instantiate(vfxPrefab, tile.TileTransform.position, Quaternion.identity);
            //    instance.Play(0.5f);
            //    Destroy(instance.gameObject, 0.5f);
            //}
        }

        private void PlayClearVerticalVFX(Tile tile)
        {
            return;
            BaseVisualEffect vfxPrefab = VFXPoolManager.Instance.GetEffect(VisualEffectID.VerticalRocket);
            vfxPrefab.transform.position = tile.TileTransform.position;
            vfxPrefab.Play(0.5f);

            //if(GameDataManager.Instance.TryGetVfxByID(VisualEffectID.VerticalRocket, out var vfxPrefab))
            //{
            //    var instance = Instantiate(vfxPrefab, tile.TileTransform.position, Quaternion.identity);
            //    instance.Play(0.5f);
            //    Destroy(instance.gameObject, 0.5f);
            //}
        }

        private void PlayTilesEffect(List<Tile> tiles, List<int> steps, VisualEffectID visualEffectID)
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                if (tiles[i] == null) continue;
                var vfx = VFXPoolManager.Instance.GetEffect(visualEffectID);
                vfx.transform.position = tiles[i].TileTransform.position;
                vfx.Play(1f / steps[i]);
            }
        }
        private void PlayFlashBombVfx(Tile tile)
        {
            Debug.Log("PlayFlashBombVfx");
            //GameObject vfxPrefab = Resources.Load<GameObject>("Effects/Match5VFX");
            //if (vfxPrefab == null) Debug.LogError("Missing vfx prefab !!!");
            //GameObject vfxInstance = Instantiate(vfxPrefab, (Vector2)tile.transform.position + TileExtension.TileCenter(), Quaternion.identity);

            BaseVisualEffect vfxPrefab = VFXPoolManager.Instance.GetEffect(VisualEffectID.BlastBomb);
            vfxPrefab.transform.position = tile.TileTransform.position;
            vfxPrefab.Play(0.5f);

            CameraShakeManager.Instance.Shake(
                intensity: 0.2f,
                duration: 0.1f);
        }

        private void PlayColorBurstVFX(Tile tile, bool removeCached)
        {
            Debug.Log("PlayColorBurstVFX");
            if (_colorBurstParentDictionary.ContainsKey(tile) == false) return;
            _cachedColorBurstLine.Clear();
            float colorBurstDuration = 1f;
            foreach (var e in _colorBurstParentDictionary)
            {
                Tile t = e.Key;
                ColorBurstFX colorBurstFX = (ColorBurstFX)VFXPoolManager.Instance.GetEffect(VisualEffectID.ColorBurstFX);
                colorBurstFX.transform.position = t.TileTransform.position;
                colorBurstFX.Play(colorBurstDuration);
                colorBurstFX.PlayAnimtion();

                //t.PlayScaleTile(0.8f, 0.2f, Ease.OutBack);
                for (int i = 0; i < e.Value.Count; i++)
                {
                    Tile nb = e.Value[i];
                    nb.PlayShaking(colorBurstDuration);

                    nb.Bloom(true);
                    LightningLine lightningLineFX = (LightningLine)VFXPoolManager.Instance.GetEffect(VisualEffectID.LightingLine);
                    lightningLineFX.transform.position = Vector2.zero;
                    float reachTaretTime = 0.1f;
                    lightningLineFX.ActiveLightningLine((Vector2)t.TileTransform.position, (Vector2)nb.TileTransform.position, reachTaretTime, colorBurstDuration);


                    EndLineColorBurstFX endLineColorBurstFX = (EndLineColorBurstFX)VFXPoolManager.Instance.GetEffect(VisualEffectID.EndLineColorBurstFX);
                    endLineColorBurstFX.transform.position = nb.TileTransform.position;
                    endLineColorBurstFX.Play(colorBurstDuration);
                }
            }

            if (removeCached)
                _colorBurstParentDictionary.Remove(tile);
        }

        private void PlayColorBurstLineFX(Tile colorBurstTile, Tile normalTile, float duration)
        {
            normalTile.PlayShaking(duration);

            normalTile.Bloom(true);
            LightningLine lightningLineFX = (LightningLine)VFXPoolManager.Instance.GetEffect(VisualEffectID.LightingLine);
            lightningLineFX.transform.position = Vector2.zero;
            float reachTaretTime = 0.1f;
            lightningLineFX.ActiveLightningLine((Vector2)colorBurstTile.TileTransform.position, (Vector2)normalTile.TileTransform.position, reachTaretTime, duration);


            EndLineColorBurstFX endLineColorBurstFX = (EndLineColorBurstFX)VFXPoolManager.Instance.GetEffect(VisualEffectID.EndLineColorBurstFX);
            endLineColorBurstFX.transform.position = normalTile.TileTransform.position;
            endLineColorBurstFX.Play(duration);
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
