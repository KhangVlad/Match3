using System;
using UnityEngine;

namespace Match3
{
    public class GameplayManager : MonoBehaviour
    {
        public static GameplayManager Instance { get; private set; }

        // LOW LEVEL
        public static event System.Action OnStateChanged;
        public static event System.Action OnPlaying;
        public static event System.Action OnWin;
        public static event System.Action OnGameOver;


        // HIGH LEVEL
        public event System.Action<int> OnTurnRemaingChanged;
        // public event System.Action<int, TileID, Vector2> OnQuestProgressUpdated;


        public enum GameState
        {
            WAITING,
            PLAYING,
            WIN,
            GAMEOVER,
            PAUSE,
            EXIT,
        }


        [Header("~Runtime")]
        public int TurnRemainingCount { get; private set; }
        public Quest[] Quests;
        //[SerializeField] private int _currentQuestIndex = 0;


        [Space(15)]
        [SerializeField] private GameState _currentState;
        //[SerializeField] private float waitTimeBeforePlaying = 0.5f;


        #region Properties
        public GameState CurrentState { get => _currentState; }
        #endregion


        #region Init & Events
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;


            TurnRemainingCount = LevelManager.Instance.LevelData.MaxTurn;
            Quests = new Quest[LevelManager.Instance.LevelData.Quests.GetLength(0)];
            for (int i = 0; i < LevelManager.Instance.LevelData.Quests.GetLength(0); i++)
            {
                Quests[i] = new Quest()
                {
                    QuestID = (QuestID)LevelManager.Instance.LevelData.Quests[i, 0],
                    Quantity = LevelManager.Instance.LevelData.Quests[i, 1]
                };
            }
        }


        private void Start()
        {
            ChangeGameState(GameState.PLAYING);

            Match3Grid.Instance.OnAfterPlayerMatchInput += OnAfterPlayerMatchInput_UpdateTurnCount;
            Match3Grid.OnEndOfTurn += OnEndOfTurn_UpdateGameState;

            Match3Grid.OnEndOfTurn += OnEndOfTurnQuestTriggered;
            Lock.OnLockMatch += OnLockMatchTriggered;
            Ice.OnIceUnlocked += OnIceUnlockedTriggered;
            Tile.OnMatched += OnTileMatchedTriggered;
        }



        private void OnDestroy()
        {
            Match3Grid.Instance.OnAfterPlayerMatchInput -= OnAfterPlayerMatchInput_UpdateTurnCount;
            Match3Grid.OnEndOfTurn -= OnEndOfTurn_UpdateGameState;

            Match3Grid.OnEndOfTurn -= OnEndOfTurnQuestTriggered;
            Lock.OnLockMatch -= OnLockMatchTriggered;
            Ice.OnIceUnlocked -= OnIceUnlockedTriggered;
            Tile.OnMatched -= OnTileMatchedTriggered;
        }


        #endregion



        public void ChangeGameState(GameState state)
        {
            _currentState = state;
            switch (_currentState)
            {
                default: break;
                case GameState.WAITING:

                    break;
                case GameState.PLAYING:
                    Time.timeScale = 1.0f;

                    OnPlaying?.Invoke();
                    break;
                case GameState.WIN:
                    AudioManager.Instance.PlayWinSfx();
                    UIGameplayManager.Instance.DisplayUIWin(true);
                    OnWin?.Invoke();
                    break;
                case GameState.GAMEOVER:
                    AudioManager.Instance.PlayGameoverSfx();
                    UIGameplayManager.Instance.DisplayUIGameover(true);
                    OnGameOver?.Invoke();
                    break;
                case GameState.PAUSE:
                    Time.timeScale = 0.0f;
                    break;
                case GameState.EXIT:
                    Time.timeScale = 1.0f;
                    break;
            }

            OnStateChanged?.Invoke();
        }




        #region Quest
        private void OnAfterPlayerMatchInput_UpdateTurnCount()
        {
            if(Match3Grid.Instance.UseBoosterThisTurn) return;
            TurnRemainingCount--;
            if (TurnRemainingCount <= 0)
                TurnRemainingCount = 0;

            OnTurnRemaingChanged?.Invoke(TurnRemainingCount);
        }

        private void OnEndOfTurn_UpdateGameState()
        {
            OnBlackMudUpdate();

            //if (IsQuestCompleted(_currentQuestIndex))
            //{
            //    _currentQuestIndex++;
            //}

            if (CheckCompleteAllQuests(out int star))
            {
                Debug.Log($"win star: {star}");
                ChangeGameState(GameState.WIN);
            }
            else
            {
                //Debug.Log($"not win: {star}");
                if (TurnRemainingCount == 0)
                {
                    Debug.Log($"Game over: {star}");
                    ChangeGameState(GameState.GAMEOVER);
                }
            }
        }

        private void OnLockMatchTriggered(Lock @lock)
        {
            for (int i = 0; i < Quests.Length; i++)
            {
                if (Quests[i].QuestID == QuestID.Lock)
                {
                    Quests[i].Quantity--;
                    // OnQuestProgressUpdated?.Invoke(i, TileID.None, @lock.transform.position);
                }
            }
        }


        private void OnBlackMudUpdate()
        {
            for (int i = 0; i < Quests.Length; i++)
            {
                if (Quests[i].QuestID == QuestID.BlackMud)
                {
                    int blackmudCount = 0;
                    for (int j = 0; j < Match3Grid.Instance.Tiles.Length; j++)
                    {
                        Tile tile = Match3Grid.Instance.Tiles[j];
                        if (tile.CurrentBlock is BlackMud)
                        {
                            blackmudCount++;
                        }
                    }
                    Quests[i].Quantity = blackmudCount;
                    // OnQuestProgressUpdated?.Invoke(i, TileID.None, default);
                }
            }
        }

        private void OnIceUnlockedTriggered(Ice ice)
        {
            for (int i = 0; i < Quests.Length; i++)
            {
                if (Quests[i].QuestID == QuestID.Ice)
                {
                    Quests[i].Quantity--;
                    // OnQuestProgressUpdated?.Invoke(i, TileID.None, ice.transform.position);
                }
            }
        }




        private void OnTileMatchedTriggered(Tile tile)
        {
            for (int i = 0; i < Quests.Length; i++)
            {
                //int i = _currentQuestIndex;
                switch (Quests[i].QuestID)
                {
                    case QuestID.RedFlower:
                        if (tile is RedFlower)
                        {
                            Quests[i].Quantity--;
                            // OnQuestProgressUpdated?.Invoke(i, tile.ID, tile.transform.position);
                        }
                        break;
                    case QuestID.YellowFlower:
                        if (tile is YellowFlower)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.PurpleFlower:
                        if (tile is PurpleFlower)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.BlueFlower:
                        if (tile is BlueFlower)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.WhiteFlower:
                        if (tile is WhiteFlower)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.RedCandle:
                        if (tile is RedCandle)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.YellowCandle:
                        if (tile is YellowCandle)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.GreenCandle:
                        if (tile is GreenCandle)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.BlueCandle:
                        if (tile is BlueCandle)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.WhiteCandle:
                        if (tile is WhiteCandle)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.RedRibbon:
                        if (tile is RedRibbon)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.YellowRibbon:
                        if (tile is YellowRibbon)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.GreenRibbon:
                        if (tile is GreenRibbon)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.BlueRibbon:
                        if (tile is BlueRibbon)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.PurpleRibbon:
                        if (tile is PurpleRibbon)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.MagnifyingGlass:
                        if (tile is MagnifyingGlass)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.MaxTurn:
                        break;
                    default:
                        Debug.LogError($"Case not found !!! {Quests[i].QuestID}");
                        break;
                }
            }
        }


        private void OnEndOfTurnQuestTriggered()
        {
            for (int i = 0; i < Quests.Length; i++)
            {
                if (Quests[i].QuestID == QuestID.MaxTurn)
                {
                    Quests[i].Quantity--;
                }
            }
        }


        public bool CheckCompleteAllQuests(out int star)
        {
            star = 0;
            int requireStar = 0;
            for (int i = 0; i < Quests.Length; i++)
            {
                if (Quests[i].QuestID != QuestID.MaxTurn)
                {
                    requireStar++;
                }
            }


            for (int i = 0; i < Quests.Length; i++)
            {
                if (Quests[i].QuestID != QuestID.MaxTurn)
                {
                    if (Quests[i].Quantity <= 0)
                    {
                        star++;
                    }
                    else
                    {
                        break;
                    }
                }
            }


            if (star == requireStar)
            {
                for (int i = 0; i < Quests.Length; i++)
                {
                    if (Quests[i].QuestID == QuestID.MaxTurn)
                    {
                        if (Quests[i].Quantity >= 0)
                        {
                            star++;
                            break;
                        }
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsQuestCompleted(int index)
        {
            if (Quests[index].QuestID == QuestID.MaxTurn)
            {
                return Quests[index].Quantity >= 0;
            }
            else
            {
                return Quests[index].Quantity <= 0;
            }
        }

        public void AddTurnRemaingCount(int value)
        {
            this.TurnRemainingCount += value;
            OnTurnRemaingChanged?.Invoke(TurnRemainingCount);
        }

        public bool TryGetQuestIndex(QuestID questID, out int index)
        {
            for (int i = 0; i < Quests.Length; i++)
            {
                if (Quests[i].QuestID == questID)
                {
                    index = i;
                    return true;
                }
            }
            index = -1;
            return false;
        }

        public QuestID GetQuestByTileID(TileID tileID)
        {
            switch (tileID)
            {
                case TileID.RedFlower:
                    return QuestID.RedFlower;
                case TileID.YellowFlower:
                    return QuestID.YellowFlower;
                case TileID.PurpleFlower:
                    return QuestID.PurpleFlower;
                case TileID.BlueFlower:
                    return QuestID.BlueFlower;
                case TileID.WhiteFlower:
                    return QuestID.WhiteFlower;
                case TileID.RedCandle:
                    return QuestID.RedCandle;
                case TileID.YellowCandle:
                    return QuestID.YellowCandle;
                case TileID.GreenCandle:
                    return QuestID.GreenCandle;
                case TileID.BlueCandle:
                    return QuestID.BlueCandle;
                case TileID.WhiteCandle:
                    return QuestID.WhiteCandle;
                case TileID.RedRibbon:
                    return QuestID.RedRibbon;
                case TileID.YellowRibbon:
                    return QuestID.YellowRibbon;
                case TileID.GreenRibbon:
                    return QuestID.GreenRibbon;
                case TileID.BlueRibbon:
                    return QuestID.BlueRibbon;
                case TileID.PurpleRibbon:
                    return QuestID.PurpleRibbon;
                case TileID.MagnifyingGlass:
                    return QuestID.MagnifyingGlass;
                default:
                    return QuestID.None;
            }
        }

        public bool HasTileQuest(Tile tile, out QuestID questID)
        {
            questID = GetQuestByTileID(tile.ID);
            return TryGetQuestIndex(questID, out int index);
        }
        #endregion
    }
}
