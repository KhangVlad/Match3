using UnityEngine;
using System.Collections.Generic;
using Match3.Enums;

namespace Match3
{
    public static class CharacterLevelDataExtensions
    {
        public static int DetectVersion(string json)
        {
            var temp = JsonUtility.FromJson<Dictionary<string, object>>(json);
            return temp.ContainsKey("Version") ? System.Convert.ToInt32(temp["Version"]) : 1;
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
                Heart = levelDataV1.Unlock[1]
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
    }
}
