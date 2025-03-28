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

            Match3Grid.OnAfterPlayerMatchInput += OnAfterPlayerMatchInput_UpdateTurnCount;
            Match3Grid.OnEndOfTurn += OnEndOfTurn_UpdateGameState;

            Match3Grid.OnEndOfTurn += OnEndOfTurnQuestTriggered;
            Lock.OnLockMatch += OnLockMatchTriggered;
            Ice.OnIceUnlocked += OnIceUnlockedTriggered;
            Stone.OnStoneMatch += OnStoneMatchTriggered;
            Tile.OnMatched += OnTileMatchedTriggered;
        }



        private void OnDestroy()
        {
            Match3Grid.OnAfterPlayerMatchInput -= OnAfterPlayerMatchInput_UpdateTurnCount;
            Match3Grid.OnEndOfTurn -= OnEndOfTurn_UpdateGameState;

            Match3Grid.OnEndOfTurn -= OnEndOfTurnQuestTriggered;
            Lock.OnLockMatch -= OnLockMatchTriggered;
            Ice.OnIceUnlocked -= OnIceUnlockedTriggered;
            Stone.OnStoneMatch -= OnStoneMatchTriggered;
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
            TurnRemainingCount--;
            if (TurnRemainingCount <= 0)
                TurnRemainingCount = 0;

            OnTurnRemaingChanged?.Invoke(TurnRemainingCount);
        }

        private void OnEndOfTurn_UpdateGameState()
        {
            OnBlackMudUpdate();

            if (CheckCompleteAllQuests())
            {
                ChangeGameState(GameState.WIN);
            }
            else
            {
                if (TurnRemainingCount == 0)
                {
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
                }
            }


            //if (CheckCompleteAllQuests())
            //{
            //    Debug.Log("All quest completed");
            //}
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
                }
            }
        }


        private void OnStoneMatchTriggered(Stone stone)
        {
            for (int i = 0; i < Quests.Length; i++)
            {
                if (Quests[i].QuestID == QuestID.Stone)
                {
                    Quests[i].Quantity--;
                }
            }
        }

        private void OnTileMatchedTriggered(Tile tile)
        {
            for (int i = 0; i < Quests.Length; i++)
            {
                switch (Quests[i].QuestID)
                {
                    case QuestID.RedFlower:
                        if (tile is RedFlower)
                            Quests[i].Quantity--;
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
                    default:
                        Debug.Log($"Case not found !!!");
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


        public bool CheckCompleteAllQuests()
        {
            for (int i = 0; i < Quests.Length; i++)
            {
                if (Quests[i].QuestID == QuestID.MaxTurn)
                {
                    if (Quests[i].Quantity < 0)
                    {
                        return false;
                    }
                }
                else
                {
                    if (Quests[i].Quantity > 0)
                    {
                        return false;
                    }
                }
            }

            return true;
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
        #endregion
    }
}
