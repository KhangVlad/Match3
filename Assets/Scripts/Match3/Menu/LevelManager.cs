using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using System.Linq;

namespace Match3
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        private int _currentLevel = 1;
        public LevelData LevelData { get; private set; }

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


        public LevelData LoadLevelData(int level)
        {
            _currentLevel = level;
            string levelJson = GameDataManager.Instance.GetLevel(level).text;
            LevelData = JsonConvert.DeserializeObject<LevelData>(levelJson);
            LevelData.ApplyRotateMatrix();
            return LevelData;
        }

        public void SetLevelData(LevelData levelData)
        {
            this.LevelData = levelData;
        }



        public int NextLevel()
        {
            _currentLevel = (_currentLevel + 1) % (GameDataManager.Instance.Levels.Length + 1);
            return _currentLevel;
        }
    }
}
