using System;
using UnityEngine;
using Newtonsoft.Json;
using Match3.Enums;
using UnityEngine.SceneManagement;
using Match3.Shares;

namespace Match3
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        private int _currentLevelIndex = 1;
        public CharacterLevelDataV2 CharacterLevelData { get; private set; }
        public LevelDataV2 LevelData { get; private set; }
        public int CurrentLevelIndex => _currentLevelIndex;
        [SerializeField] private GameObject sceneGameobjects;
        public Scene OtherScene;
        public event Action OnBackScene;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }


        public LevelDataV2 LoadLevelData(int level)
        {
            _currentLevelIndex = level;
            string levelJson = GameDataManager.Instance.GetLevel(level).text;
            LevelData = JsonConvert.DeserializeObject<LevelDataV2>(levelJson);
            LevelData.ApplyRotateMatrix();
            return LevelData;
        }

        public LevelDataV2 LoadLevelData(CharacterID characterID, int level)
        {
            _currentLevelIndex = level;
            GameDataManager.Instance.TryGetCharacterLevelDataByID(characterID, out var data);
            CharacterLevelData = data;
            LevelData = CharacterLevelData.Levels[level];
            return LevelData;
        }

        public void SetLevelData(LevelDataV2 levelData)
        {
            this.LevelData = levelData;
        }


        public void SetCurrentLevelIndex(int levelIndex)
        {
            _currentLevelIndex = levelIndex;
        }


        public int NextLevel()
        {
            //_currentLevel = (_currentLevel + 1) % (GameDataManager.Instance.Levels.Length + 1);
            //return _currentLevel;

            _currentLevelIndex = (_currentLevelIndex + 1) % (CharacterLevelData.Levels.Count + 1);
            return _currentLevelIndex;
        }


        public void SetCharacterLevelData(CharacterLevelDataV2 characterLevelData)
        {
            this.CharacterLevelData = characterLevelData;
        }

        public void ActiveGameObject(bool a)
        {
            sceneGameobjects.SetActive(a);
            if (a)
            {
                OnBackScene?.Invoke();
            }
        }


        public void ReloadScene()
        {
            SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(Loader.Scene.GameplayScene.ToString()));
            StartCoroutine(Loader.LoadSceneAsyncCoroutine(Loader.Scene.GameplayScene, LoadSceneMode.Additive, 0f,
                () => { OtherScene = SceneManager.GetSceneByName(Loader.Scene.GameplayScene.ToString()); }));
        }
    }
}