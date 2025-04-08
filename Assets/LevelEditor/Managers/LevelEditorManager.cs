using UnityEngine;
using System.Collections.Generic;

namespace Match3.LevelEditor
{
    public class LevelEditorManager : MonoBehaviour
    {
        public static LevelEditorManager Instance { get; private set; }
        public event System.Action<string> OnFileNameChanged;
        public event System.Action<CharacterLevelDataV2> OnCharacterLevelDataLoaded;
        public event System.Action<int> OnCharacterLevelDataAdded;
        public event System.Action<int> OnCharacterLevelDataRemoval;
        public event System.Action<int> OnLevelDataSaved;

        public bool ShowCreateNewPanel = true;
        public string FileName;

        public CharacterLevelDataV2 CharacterLevelData;
        [SerializeField] private int _currentSelectLevel;


        #region Properties
        public int CurrentLevel => _currentSelectLevel;
        public LevelDataV2 SelectLevelData => CharacterLevelData.Levels[_currentSelectLevel];
        #endregion


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
        }

        public void SetFileName(string name)
        {
            this.FileName = name;
            OnFileNameChanged?.Invoke(name);
        }

        public void SetSelectLevel(int level)
        {
            this._currentSelectLevel = level;
        }
        
        public CharacterLevelDataV2 InitializeNewChartacterLevelData()
        {
            CharacterLevelData = new();
            LevelDataV2 levelData = new LevelDataV2(0,0);
            CharacterLevelData.Levels.Add(levelData);
            OnCharacterLevelDataLoaded?.Invoke(CharacterLevelData);        
            return CharacterLevelData;
        }
        public void SetCharacterLevelData(CharacterLevelDataV2 characterLevelData)
        {
            this.CharacterLevelData = characterLevelData;
            OnCharacterLevelDataLoaded?.Invoke(CharacterLevelData);
        }


        public void AddNewLevelDataAtIndex(int index)
        {
            LevelDataV2 levelData = new LevelDataV2(0, 0);
            CharacterLevelData.Levels.Insert(index, levelData);
            OnCharacterLevelDataAdded?.Invoke(index);
           
        }

        public void RemoveLevelDataAtIndex(int index)
        {
            CharacterLevelData.Levels.RemoveAt(index);
            OnCharacterLevelDataRemoval?.Invoke(index);
        }

        public void SaveLevelData(int index, LevelDataV2 levelData)
        {
            CharacterLevelData.Levels[index] = levelData;
            OnLevelDataSaved?.Invoke(index);
        }
    }
}
