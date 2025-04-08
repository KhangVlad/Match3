using System.Collections.Generic;
using Match3.Enums;

namespace Match3
{
    [System.Serializable]
    public class CharacterLevelDataV1
    {
        public int Version = 1;
        public List<LevelDataV1> Levels;

        public CharacterLevelDataV1()
        {
            Version = 1;
            Levels = new List<LevelDataV1>();
        }
    }

    [System.Serializable]
    public class CharacterLevelDataV2
    {
        public int Version = 2;
        public CharacterID CharacterID;
        public List<LevelDataV2> Levels;

        public CharacterLevelDataV2()
        {
            Version = 2;
            Levels = new List<LevelDataV2>();
        }
    }
}
