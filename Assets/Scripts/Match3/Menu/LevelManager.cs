using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using System.Linq;
using Match3.Enums;

namespace Match3
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        private int _currentLevel = 1;
        public CharacterLevelDataV2 CharacterLevelData { get; private set; }
        public LevelDataV2 LevelData { get; private set; }
        public int CurrentLevel => _currentLevel;


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
        }


        public LevelDataV2 LoadLevelData(int level)
        {
            _currentLevel = level;
            string levelJson = GameDataManager.Instance.GetLevel(level).text;
            LevelData = JsonConvert.DeserializeObject<LevelDataV2>(levelJson);
            LevelData.ApplyRotateMatrix();
            return LevelData;
        }

        public LevelDataV2 LoadLevelData(CharacterID characterID, int level)
        {
            _currentLevel = level;
            GameDataManager.Instance.TryGetCharacterLevelDataByID(characterID, out var data);
            CharacterLevelData = data;
            LevelData = CharacterLevelData.Levels[level];
            return LevelData;
        }

        public void SetLevelData(LevelDataV2 levelData)
        {
            this.LevelData = levelData;
        }



        public int NextLevel()
        {
            //_currentLevel = (_currentLevel + 1) % (GameDataManager.Instance.Levels.Length + 1);
            //return _currentLevel;

            _currentLevel = (_currentLevel + 1) % (CharacterLevelData.Levels.Count + 1);
            return _currentLevel;
        }


        public void SetCharacterLevelData(CharacterLevelDataV2 characterLevelData)
        {
            this.CharacterLevelData = characterLevelData;
        }
    }
}
