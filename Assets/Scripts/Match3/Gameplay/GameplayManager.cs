using System;
using FMOD.Studio;
using Match3.Enums;
using Match3.Shares;
using UnityEngine;

namespace Match3
{
    public class GameplayManager : MonoBehaviour
    {
        public static GameplayManager Instance { get; private set; }

        // LOW LEVEL
        public static event System.Action OnStateChanged;
        public static event System.Action OnPlaying;
        public static event System.Action<CharacterID, int> OnWin;
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

#if UNITY_EDITOR
            Utilities.WaitAfter(0.2f, () =>
            {
                Utilities.ClearEdiorLog();
            });
#endif
        }


        private void Start()
        {
            ChangeGameState(GameState.PLAYING);

            Match3Grid.OnEndOfTurn += OnEndOfTurn_UpdateTurnCount;
            Match3Grid.OnEndOfTurn += OnEndOfTurn_UpdateGameState;
            Match3Grid.OnEndOfTurn += OnEndOfTurnQuestTriggered;
            Lock.OnLockMatch += OnLockMatchTriggered;
            Ice.OnIceUnlocked += OnIceUnlockedTriggered;
            Tile.OnMatched += OnTileMatchedTriggered;
            GameplayVideo.Instance.InitializeCharacterVideo(LevelManager.Instance.CharacterLevelData.CharacterID); //khang add

        }



        private void OnDestroy()
        {
            Match3Grid.OnEndOfTurn -= OnEndOfTurn_UpdateTurnCount;
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
        private void OnEndOfTurn_UpdateTurnCount()
        {
            // Debug.Log($"End of turn: {Match3Grid.Instance.UseBoosterThisTurn}");
            if (Match3Grid.Instance.UseBoosterThisTurn) return;
            if (Match3Grid.Instance.SwapTileHasMatched == false) return;
            TurnRemainingCount--;
            if (TurnRemainingCount <= 0)
                TurnRemainingCount = 0;

            OnTurnRemaingChanged?.Invoke(TurnRemainingCount);
        }

        private void OnEndOfTurn_UpdateGameState()
        {
            //if (IsQuestCompleted(_currentQuestIndex))
            //{
            //    _currentQuestIndex++;
            //}

            if (CheckCompleteAllQuests(out int star))
            {
                Debug.Log($"Game win: {star}");
                OnWin?.Invoke(CharacterID.Alice, star);
                ChangeGameState(GameState.WIN);
            }
            else
            {
                //Debug.Log($"not win: {star}");
                if (TurnRemainingCount == 0)
                {
                    Debug.Log($"Game over: {star}");
                    if (star == 0)
                    {
                        ChangeGameState(GameState.GAMEOVER);
                    }
                    else
                    {
                        ChangeGameState(GameState.WIN);
                    }

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


        // private void OnBlackMudUpdate()
        // {
        //     for (int i = 0; i < Quests.Length; i++)
        //     {
        //         if (Quests[i].QuestID == QuestID.BlackMud)
        //         {
        //             int blackmudCount = 0;
        //             for (int j = 0; j < Match3Grid.Instance.Tiles.Length; j++)
        //             {
        //                 Tile tile = Match3Grid.Instance.Tiles[j];
        //                 if (tile.CurrentBlock is BlackMud)
        //                 {
        //                     blackmudCount++;
        //                 }
        //             }
        //             Quests[i].Quantity = blackmudCount;
        //             // OnQuestProgressUpdated?.Invoke(i, TileID.None, default);
        //         }
        //     }
        // }

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
                    case QuestID.BlueFeather:
                        if (tile is BlueFeather)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.GreenFeather:
                        if (tile is GreenFeather)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.PurpleFeather:
                        if (tile is PurpleFeather)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.RedFeather:
                        if (tile is RedFeather)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.WhiteFeather:
                        if (tile is WhiteFeather)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.YellowFeather:
                        if (tile is YellowFeather)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.BlueMushroom:
                        if (tile is BlueMushroom)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.GreenMushroom:
                        if (tile is GreenMushroom)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.PurpleMushroom:
                        if (tile is PurpleMushroom)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.RedMushroom:
                        if (tile is RedMushroom)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.YellowMushroom:
                        if (tile is YellowMushroom)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.GreenBut:
                        if (tile is GreenBut)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.PurpleBut:
                        if (tile is PurpleBut)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.RedBut:
                        if (tile is RedBut)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.WhiteBut:
                        if (tile is WhiteBut)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.YellowBut:
                        if (tile is YellowBut)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.Rock:
                        if (tile is Rock)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.Bell:
                        if (tile is Bell)
                            Quests[i].Quantity--;
                        break;

                    case QuestID.BlueBallon:
                        if (tile is BlueBallon)
                            Quests[i].Quantity--;
                        break;

                    case QuestID.BlueWool:
                        if (tile is BlueWool)
                            Quests[i].Quantity--;
                        break;

                    case QuestID.BrownMushRoom:
                        if (tile is BrownMushRoom)
                            Quests[i].Quantity--;
                        break;

                    case QuestID.BrownRock:
                        if (tile is BrownRock)
                            Quests[i].Quantity--;
                        break;

                    case QuestID.Charcoal:
                        if (tile is Charcoal)
                            Quests[i].Quantity--;
                        break;

                    case QuestID.Flyfire:
                        if (tile is Flyfire)
                            Quests[i].Quantity--;
                        break;

                    case QuestID.GreenBallon:
                        if (tile is GreenBallon)
                            Quests[i].Quantity--;
                        break;

                    case QuestID.GreenWool:
                        if (tile is GreenWool)
                            Quests[i].Quantity--;
                        break;

                    case QuestID.Lantern:
                        if (tile is Lantern)
                            Quests[i].Quantity--;
                        break;

                    case QuestID.Onion:
                        if (tile is Onion)
                            Quests[i].Quantity--;
                        break;

                    case QuestID.PurpleBallon:
                        if (tile is PurpleBallon)
                            Quests[i].Quantity--;
                        break;

                    case QuestID.PurpleWool:
                        if (tile is PurpleWool)
                            Quests[i].Quantity--;
                        break;

                    case QuestID.RedBallon:
                        if (tile is RedBallon)
                            Quests[i].Quantity--;
                        break;

                    case QuestID.RedWool:
                        if (tile is RedWool)
                            Quests[i].Quantity--;
                        break;

                    case QuestID.Shushi_1:
                        if (tile is Shushi_1)
                            Quests[i].Quantity--;
                        break;

                    case QuestID.Shushi_2:
                        if (tile is Shushi_2)
                            Quests[i].Quantity--;
                        break;

                    case QuestID.Shushi_3:
                        if (tile is Shushi_3)
                            Quests[i].Quantity--;
                        break;

                    case QuestID.Shushi_4:
                        if (tile is Shushi_4)
                            Quests[i].Quantity--;
                        break;

                    case QuestID.Shushi_5:
                        if (tile is Shushi_5)
                            Quests[i].Quantity--;
                        break;

                    case QuestID.Shushi_6:
                        if (tile is Shushi_6)
                            Quests[i].Quantity--;
                        break;

                    case QuestID.Shushi_7:
                        if (tile is Shushi_7)
                            Quests[i].Quantity--;
                        break;

                    case QuestID.Shushi_9:
                        if (tile is Shushi_9)
                            Quests[i].Quantity--;
                        break;

                    case QuestID.Shushi_8:
                        if (tile is Shushi_8)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.Tape:
                        if (tile is Tape)
                            Quests[i].Quantity--;
                        break;

                    case QuestID.WhiteBallon:
                        if (tile is WhiteBallon)
                            Quests[i].Quantity--;
                        break;

                    case QuestID.WhiteWool:
                        if (tile is WhiteWool)
                            Quests[i].Quantity--;
                        break;

                    case QuestID.WoodLog:
                        if (tile is WoodLog)
                            Quests[i].Quantity--;
                        break;

                    case QuestID.YellowBallon:
                        if (tile is YellowBallon)
                            Quests[i].Quantity--;
                        break;

                    case QuestID.YellowWool:
                        if (tile is YellowWool)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.Meat_Ball:
                        if (tile is MeatBall)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.Cheese:
                        if (tile is Cheese)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.Fish:
                        if (tile is Fish)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.Corn:
                        if (tile is Corn)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.Duck:
                        if (tile is Duck)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.Fire:
                        if (tile is Fire)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.Raw_Meat:
                        if (tile is RawMeat)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.Red_Chillie:
                        if (tile is RedChilie)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.Tomato:
                        if (tile is Tomato)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.Soap:
                        if (tile is Soap)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.Cookie:
                        if (tile is Cookie)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.Egg:
                        if (tile is Egg)
                            Quests[i].Quantity--;
                        break;
                    case QuestID.Green_Vegetable:
                        if (tile is GreenVegetable)
                            Quests[i].Quantity--;
                        break;
                    default:
                        Debug.LogError($"Case not found !!! {Quests[i].QuestID}");
                        break;
                }
            }
        }


        private void OnEndOfTurnQuestTriggered()
        {
            if (Match3Grid.Instance.SwapTileHasMatched == false) return;
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

        public bool HasTileQuest(TileID tileID)
        {
            QuestID questID = GameplayManager.Instance.GetQuestByTileID(tileID);
            return TryGetQuestIndex(questID, out int questIndex);
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
