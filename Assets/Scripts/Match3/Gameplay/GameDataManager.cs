using UnityEngine;
using System.Collections.Generic;

namespace Match3
{
    public class GameDataManager : MonoBehaviour
    {
        public static GameDataManager Instance { get; private set; }   

        [Header("~Runtime")]
        public Tile[] Tiles;
        private Dictionary<TileID, Tile> _tileDict;

        public Block[] Blocks;
        private Dictionary<BlockID, Block> _blockDict;

        public QuestDataSO[] QuestDataSos;
        private Dictionary<QuestID, QuestDataSO> _questDataDict;

        public BoosterDataSo[] BoosterDataSos;
        private Dictionary<BoosterID, BoosterDataSo> _boosterDataDict;
        public event System.Action OnDataLoaded;  

        // Level
        private TextAsset[] _levels;


        public TextAsset[] Levels => _levels;

        private void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
            LoadGameData();
        }

        
        
        private void LoadGameData()
        {
            // Load all tiles
            Tiles = Resources.LoadAll<Tile>("Tiles/");

            _tileDict = new();
            for(int i = 0; i < Tiles.Length; i++)
            {
                _tileDict.Add(Tiles[i].ID, Tiles[i]);
            }

            // Load all blocks
            _blockDict = new();
            Blocks = Resources.LoadAll<Block>("Blocks/");
            for (int i = 0; i < Blocks.Length; i++)
            {
                _blockDict.Add(Blocks[i].BlockID, Blocks[i]);
            }

            // Load levels
            _levels = Resources.LoadAll<TextAsset>("Levels/");

            // Load quest data
            LoadQuestData();

            // Load booster data
            LoadBoosterData();
            OnDataLoaded?.Invoke();
        }


        private void LoadQuestData()
        {
            QuestDataSos = Resources.LoadAll<QuestDataSO>("Data/Quests/");
            _questDataDict = new();
            for(int i = 0; i < QuestDataSos.Length; i++)
            {
                QuestDataSO questData = QuestDataSos[i];
                _questDataDict.Add(questData.QuestID, questData);
            }
        }

        private void LoadBoosterData()
        {
            BoosterDataSos = Resources.LoadAll<BoosterDataSo>("Data/Boosters/");
            _boosterDataDict = new();
            for (int i = 0; i < BoosterDataSos.Length; i++)
            {
                BoosterDataSo questData = BoosterDataSos[i];
                _boosterDataDict.Add(questData.ID, questData);
            }
        }
        public BoosterDataSo GetBoosterDataByID(BoosterID id)
        {
            return _boosterDataDict[id];
        }

        #region Extensions
        //public Tile GetRandomTile(bool withTileNone = false)
        //{
        //    if(withTileNone)
        //    {
        //        return Tiles[Random.Range(0, Tiles.Length)];
        //    }
        //    else
        //    {
        //        int attempts = 0;
        //        while (true)
        //        {
        //            Tile t = Tiles[Random.Range(0, Tiles.Length)];
        //            if(t.ID != TileID.None)
        //            {
        //                return t;
        //            }


        //            if(attempts++ > 100)
        //            {
        //                Debug.LogError("Something went wrong");
        //                break;
        //            }
        //        }
        //        return null;
        //    }         
        //}

        public Tile GetTileByID(TileID tileID)
        {
            return _tileDict[tileID];
        }  

        public Block GetBlockByID(BlockID blockID)
        {
            return _blockDict[blockID];
        }

        public TextAsset GetLevel(int level)
        {
            if(level < 1 || level > _levels.Length)
            {
                Debug.Log("========= Level out of range ==========");
                return _levels[0];
            }
            return _levels[level-1];
        }


        public QuestDataSO GetQuestDataByID(QuestID questID)
        {
            return _questDataDict[questID];
        }
        #endregion
    }
}
