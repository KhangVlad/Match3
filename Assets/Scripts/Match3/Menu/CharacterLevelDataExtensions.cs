using UnityEngine;
using System.Collections.Generic;
using Match3.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Match3
{
    public static class CharacterLevelDataExtensions
    {
        public static int DetectVersion(string json)
        {
            Debug.Log($"DetectVersion: ");
            try
            {
                var jsonObject = JObject.Parse(json);

                if (jsonObject["Version"] != null && int.TryParse(jsonObject["Version"].ToString(), out int version))
                {
                    return version;
                }
                else
                {
                    Debug.LogWarning("Version field missing or invalid.");
                    return 1;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to parse JSON: {ex.Message}");
                return 1;
            }

            //var jsonObject = JObject.Parse(json);
            //int version = (int)jsonObject["Version"];

            //Debug.Log($"v: {version}");

            //return 1;
            //    return 1;
            //var temp = JsonUtility.FromJson<Dictionary<string, object>>(json);
            //foreach(var e in temp)
            //{
            //    Debug.Log(e.Key);
            //}
            //Debug.Log($"DetectVersion: {temp.Count}");
            //return temp.ContainsKey("Version") ? System.Convert.ToInt32(temp["Version"]) : 1;
        }

        public static LevelDataV2 UpgradeV1ToV2(this LevelDataV1 levelDataV1)
        {
            int width = levelDataV1.Blocks.GetLength(0);
            int height = levelDataV1.Blocks.GetLength(1);

            return new LevelDataV2(width, height)
            {
                MaxTurn = levelDataV1.MaxTurn,
                Blocks = levelDataV1.Blocks,
                Tiles = levelDataV1.Tiles,
                AvaiableTiles = levelDataV1.AvaiableTiles,
                Quests = levelDataV1.Quests,
                Heart = levelDataV1.Unlock[1],
                Energy = 0
            };
        }

        public static CharacterLevelDataV2 UpgradeV1ToV2(this CharacterLevelDataV1 characterLevelDataV1)
        {
            List<LevelDataV2> levels = new List<LevelDataV2>();
            for(int i = 0; i < characterLevelDataV1.Levels.Count; i++)
            {
                LevelDataV2 newLevelData = characterLevelDataV1.Levels[i].UpgradeV1ToV2();
                levels.Add(newLevelData);
            }

            return new CharacterLevelDataV2()
            {
                Version = 2,
                CharacterID =  (CharacterID)characterLevelDataV1.Levels[0].Unlock[0],
                Levels = levels
            };
        }



        public static LevelDataV1 DowngradeV2ToV1(this LevelDataV2 levelDataV2, CharacterID characterID)
        {
            int width = levelDataV2.Blocks.GetLength(0);
            int height = levelDataV2.Blocks.GetLength(1);
    
            return new LevelDataV1(width, height)
            { 
                MaxTurn = levelDataV2.MaxTurn,
                Blocks = levelDataV2.Blocks,
                Tiles = levelDataV2.Tiles,
                AvaiableTiles = levelDataV2.AvaiableTiles,
                Quests = levelDataV2.Quests,
                Unlock = new int[] { (int)characterID, levelDataV2.Heart }
            };
        }
        public static CharacterLevelDataV1 DowngradeV2ToV1(this CharacterLevelDataV2 characterLevelDataV2)
        {
            List<LevelDataV1> levels = new List<LevelDataV1>();
            for (int i = 0; i < characterLevelDataV2.Levels.Count; i++)
            {
                LevelDataV1 newLevelData = characterLevelDataV2.Levels[i].DowngradeV2ToV1(characterLevelDataV2.CharacterID);
                levels.Add(newLevelData);
            }

            return new CharacterLevelDataV1()
            {
                Version = 1,
                Levels = levels
            };
        }
    }
}
