using UnityEngine;
using System.Collections.Generic;
using Match3.Shares;
using Match3.Enums;
using Newtonsoft.Json;
namespace Match3
{
    public class GameDataManager : MonoBehaviour
    {
        public static GameDataManager Instance { get; private set; }
        public event System.Action OnDataLoaded;


        [Header("~Runtime")]
        public Tile[] Tiles;
        private Dictionary<TileID, Tile> _tileDict;

        public Block[] Blocks;
        private Dictionary<BlockID, Block> _blockDict;

        public QuestDataSO[] QuestDataSos;
        private Dictionary<QuestID, QuestDataSO> _questDataDict;

        public BoosterDataSo[] BoosterDataSos;
        private Dictionary<BoosterID, BoosterDataSo> _boosterDataDict;


        // Character Data
        public CharacterDataSO[] CharacterDataSos;
        private Dictionary<CharacterID, CharacterDataSO> _characterDataDict;

        // Character level data
        public CharacterLevelDataV2[] CharacterLevelDatas;
        private Dictionary<CharacterID, CharacterLevelDataV2> _characterLevelDataDict;


        // Shop Data
        public ShopItemDataSO[] ShopItemDataSos;
        private Dictionary<ShopItemID, ShopItemDataSO> _shopItemDataDict;

        // Level
        private TextAsset[] _levels;


        public TextAsset[] Levels => _levels;

        private void Awake()
        {
            if (Instance != null && Instance != this)
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
            for (int i = 0; i < Tiles.Length; i++)
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

            // Character data
            LoadCharacterData();

            // Character level data
            LoadCharactereLevelData();


            // Shop data
            LoadShopData();

            OnDataLoaded?.Invoke();
        }


        private void LoadQuestData()
        {
            QuestDataSos = Resources.LoadAll<QuestDataSO>("Data/Quests/");
            _questDataDict = new();
            for (int i = 0; i < QuestDataSos.Length; i++)
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



        private void LoadCharacterData()
        {
            CharacterDataSos = Resources.LoadAll<CharacterDataSO>("DataSO/Characters/");
            _characterDataDict = new();
            for (int i = 0; i < CharacterDataSos.Length; i++)
            {
                CharacterDataSO characterData = CharacterDataSos[i];
                _characterDataDict.Add(characterData.id, characterData);
            }
        }
        public CharacterDataSO GetCharacterDataByID(CharacterID id)
        {
            return _characterDataDict[id];
        }



        private void LoadCharactereLevelData()
        {
            TextAsset[] allCharacterLevelData = Resources.LoadAll<TextAsset>("CharacterLevelData/");
            CharacterLevelDatas = new CharacterLevelDataV2[allCharacterLevelData.Length];
            for (int i = 0; i < allCharacterLevelData.Length; i++)
            {
                string json = allCharacterLevelData[i].text;

                int version = CharacterLevelDataExtensions.DetectVersion(allCharacterLevelData[i].text);
                if (version == 1)
                {
                    CharacterLevelDatas[i] = JsonConvert.DeserializeObject<CharacterLevelDataV1>(json).UpgradeV1ToV2();
                }
                else if (version == 2)
                {
                    CharacterLevelDatas[i] = JsonConvert.DeserializeObject<CharacterLevelDataV2>(json);   
                }
                else
                {
                    Debug.LogError("Version not found!!!");
                }
            }

            _characterLevelDataDict = new();
            for(int i = 0; i < CharacterLevelDatas.Length; i++)
            {
                _characterLevelDataDict.Add(CharacterLevelDatas[i].CharacterID, CharacterLevelDatas[i]);
            }
        }

        public bool TryGetCharacterLevelDataByID(CharacterID id, out CharacterLevelDataV2 characterLevelData)
        {
            if(_characterLevelDataDict.ContainsKey(id))
            {
                characterLevelData = _characterLevelDataDict[id];
                return true;
            }
           else
            {
                characterLevelData = null;
                return false;
            }
        }



        private void LoadShopData()
        {
            ShopItemDataSos = Resources.LoadAll<ShopItemDataSO>("Data/ShopItem/");
            _shopItemDataDict = new();
            for (int i = 0; i < ShopItemDataSos.Length; i++)
            {
                ShopItemDataSO shopItemData = ShopItemDataSos[i];
                _shopItemDataDict.Add(shopItemData.ShopItemID, shopItemData);
            }
        }
        public ShopItemDataSO GetShopItemDataByID(ShopItemID shopItemID)
        {
            return _shopItemDataDict[shopItemID];
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
            if (level < 1 || level > _levels.Length)
            {
                Debug.Log("========= Level out of range ==========");
                return _levels[0];
            }
            return _levels[level - 1];
        }


        public QuestDataSO GetQuestDataByID(QuestID questID)
        {
            return _questDataDict[questID];
        }
        #endregion
    }
}
