using System;
using UnityEngine;
using System.Collections.Generic;
using Match3.Shares;
using Match3.Enums;
using Newtonsoft.Json;
using System.Linq;
using NUnit.Framework;
using UnityEngine.Tilemaps;
using UnityEngine.Video;

namespace Match3
{
    public class GameDataManager : MonoBehaviour
    {
        public static GameDataManager Instance { get; private set; }
        public event Action OnDataLoaded;

        // [Header("VFX")]
        public Dictionary<VisualEffectID, BaseVisualEffect> VisualEffectDictionary { get; private set; }

        //[Header("Tilebases")]
        private Dictionary<string, TileBase> _tilebaseDictionary;

        [Header("~Runtime")] public Tile[] Tiles;
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

        //character activity data
        // public List<CharacterActivitySO> characterActivities = new();
        public CharacterAppearanceSO characterColor;

        // public List<CharacterDialogueSO> characterDialogues = new();
        public List<CharacterActivitySO> characterActivities = new();

        public DailyGiftSO[] DailyGiftData;
        private Dictionary<ShopItemID, DailyGiftSO> _dailyGiftDict;
        public event System.Action OnCharacterDataLoaded;


        // Level
        private TextAsset[] _levels;
        public TextAsset[] Levels => _levels;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            LoadAllVisualEffect();
            LoadAllTilebases();
            LoadGameData();
        }

        private void OnDestroy()
        {
            for (int i = 0; i < Tiles.Length; i++)
                Tiles[i].SetInteractionMask(SpriteMaskInteraction.None);
        }

        private void LoadGameData()
        {
            // Load all tiles
            Tiles = Resources.LoadAll<Tile>("Tiles/");
            for (int i = 0; i < Tiles.Length; i++)
            {
                Tiles[i].Initialize();
#if UNITY_WEBGL
                Tiles[i].SetInteractionMask(SpriteMaskInteraction.None);
#else
                Tiles[i].SetInteractionMask(SpriteMaskInteraction.VisibleInsideMask);
#endif
            }

            _tileDict = new();
            for (int i = 0; i < Tiles.Length; i++)
            {
                if (_tileDict.ContainsKey(Tiles[i].ID) == false)
                {
                    _tileDict.Add(Tiles[i].ID, Tiles[i]);
                }
                else
                {
                    Debug.LogError($"Tile ID {Tiles[i].ID} already added to tile dictionry");
                }
            }

            // Load all blocks
            _blockDict = new();
            Blocks = Resources.LoadAll<Block>("Blocks/");
            for (int i = 0; i < Blocks.Length; i++)
            {
                Blocks[i].Initialize();
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
            characterActivities = Resources.LoadAll<CharacterActivitySO>("DataSO/CharacterActivities").ToList();
            LoadDailyGift();

            // Shop data
            LoadShopData();
            OnDataLoaded?.Invoke();
        }


        private void LoadDailyGift()
        {
            DailyGiftData = Resources.LoadAll<DailyGiftSO>("DataSO/DailyGift/");
            _dailyGiftDict = new Dictionary<ShopItemID, DailyGiftSO>();
            for (int i = 0; i < DailyGiftData.Length; i++)
            {
                DailyGiftSO data = DailyGiftData[i];
                _dailyGiftDict.Add(data.id, data);
            }
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


        #region Characters

        public CharacterAppearance GetCharacterAppearanceData(CharacterID id)
        {
            return characterColor.AppearancesInfo.Find(x => x.id == id);
        }

        public Color GetHeartColor(int level, out Color nextLevelColor)
        {
            if (level + 1 < characterColor.heartColors.Length)
            {
                nextLevelColor = characterColor.heartColors[level + 1];
            }
            else
            {
                nextLevelColor = characterColor.heartColors[level];
            }

            return characterColor.heartColors[level];
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

            OnCharacterDataLoaded?.Invoke();
        }

        public CharacterDataSO GetCharacterDataSOByID(CharacterID id)
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
            for (int i = 0; i < CharacterLevelDatas.Length; i++)
            {
                _characterLevelDataDict.Add(CharacterLevelDatas[i].CharacterID, CharacterLevelDatas[i]);
            }
        }

        public bool TryGetCharacterLevelDataByID(CharacterID id, out CharacterLevelDataV2 characterLevelData)
        {
            if (_characterLevelDataDict.ContainsKey(id))
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

        public RunTimeDialogData ReadDialogueData(CharacterID characterId, LanguageType language)
        {
            string resourcePath = "CSVData/Dialogues";
            string fileName = $"{language.ToString().ToLower()}_{(int)characterId}";
            TextAsset csvFile = Resources.Load<TextAsset>($"{resourcePath}/{fileName}");

            if (csvFile == null)
            {
                Debug.LogError($"Failed to load dialogue file: {fileName}");
                return null;
            }

            return ParseCSVData(csvFile.text, characterId);
        }

        private RunTimeDialogData ParseCSVData(string csvContent, CharacterID characterId)
        {
            string[] lines = csvContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Dictionary to hold LevelDialogueData by level
            Dictionary<int, List<string>> levelDialogues = new Dictionary<int, List<string>>();
            List<string> greetingDialogs = new List<string>();
            List<string> lowSympathyDialogs = new List<string>();

            // Skip the header row and iterate through the lines
            for (int i = 1; i < lines.Length; i++)
            {
                string[] columns = lines[i].Split(',');

                // Make sure the row has enough columns
                if (columns.Length >= 5)
                {
                    int level = int.Parse(columns[0].Trim());
                    string dialog = columns[2].Trim();
                    string greeting = columns[3].Trim();
                    string lowSympathy = columns[4].Trim();

                    // Store greeting dialogue
                    if (!string.IsNullOrEmpty(greeting))
                    {
                        greetingDialogs.Add(greeting);
                    }

                    if (!string.IsNullOrEmpty(lowSympathy))
                    {
                        lowSympathyDialogs.Add(lowSympathy);
                    }

                    if (!levelDialogues.ContainsKey(level))
                    {
                        levelDialogues[level] = new List<string>();
                    }

                    levelDialogues[level].Add(dialog);
                }
            }

            // Convert the dictionary into the LevelDialogueData array
            List<LevelDialogueData> levelDataList = new List<LevelDialogueData>();
            foreach (var levelEntry in levelDialogues)
            {
                LevelDialogueData data = new LevelDialogueData
                {
                    levelDialogs = levelEntry.Value.ToArray(),
                };
                levelDataList.Add(data);
            }

            // Create and return the RunTimeDialogData
            RunTimeDialogData runTimeData = new RunTimeDialogData
            {
                id = characterId,
                data = levelDataList.ToArray(),
                greetingDialogs = greetingDialogs.ToArray(),
                lowSympathyDialogs = lowSympathyDialogs.ToArray()
            };

            return runTimeData;
        }

        public List<CharacterActivitySO> GetCharacterActive(DayInWeek day) //current day
        {
            List<CharacterActivitySO> a = new List<CharacterActivitySO>();
            foreach (var characterActivity in characterActivities)
            {
                //Debug.Log(characterActivity.id);
                if (characterActivity.dayOff == day) continue;
                if (characterActivity.activityInfos.Any(info => info.dayOfWeek == day))
                {
                    a.Add(characterActivity);
                }
            }

            return a;
        }

        #endregion


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


        public VideoClip GetVideoByName(string name, CharacterID id)
        {
            string path = $"Characters/{(int)id}/{name}";
            return Resources.Load<VideoClip>(path);
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


        #region Tilebase

        private void LoadAllTilebases()
        {
            TileBase[] tilebases = Resources.LoadAll<TileBase>("Tilebases");
            _tilebaseDictionary = new();
            // Debug.Log(tilebases.Length);

            for (int i = 0; i < tilebases.Length; i++)
            {
                _tilebaseDictionary.Add(tilebases[i].name, tilebases[i]);
            }
        }

        public bool TryGetTilebaseByName(string tileName, out TileBase tilebase)
        {
            if (_tilebaseDictionary.TryGetValue(tileName, out tilebase))
            {
                return true;
            }
            else
            {
                Debug.LogError("Tile base not found: " + tileName);
                return false;
            }
        }

        #endregion


        #region Visual Effect

        private void LoadAllVisualEffect()
        {
            BaseVisualEffect[] vfxs = Resources.LoadAll<BaseVisualEffect>("Effects/");
            VisualEffectDictionary = new();
            for (int i = 0; i < vfxs.Length; i++)
            {
                BaseVisualEffect vfx = vfxs[i];
                vfx.Initialize();
                VisualEffectDictionary.Add(vfx.VfxID, vfx);
            }
        }

        public bool TryGetVfxByID(VisualEffectID visualEffectID, out BaseVisualEffect vfxPrefab)
        {
            if (VisualEffectDictionary.TryGetValue(visualEffectID, out vfxPrefab))
            {
                return true;
            }
            else
            {
                Debug.Log($"====== Missing vfx effect at id: {visualEffectID}");
                return false;
            }
        }

        #endregion
    }
}