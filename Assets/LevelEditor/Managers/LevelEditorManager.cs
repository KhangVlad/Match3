using UnityEngine;
using System.Collections.Generic;

namespace Match3.LevelEditor
{
    public class LevelEditorManager : MonoBehaviour
    {
        public static LevelEditorManager Instance { get; private set; }
        public event System.Action<string> OnFileNameChanged;
        public event System.Action<CharacterLevelData> OnCharacterLevelDataInitialized;
        public event System.Action<int> OnCharacterLevelDataAdded;
        public event System.Action<int> OnCharacterLevelDataRemoval;

        public bool ShowCreateNewPanel = true;
        public string FileName;

        public CharacterLevelData CharacterLevelData;
        [SerializeField] private int _currentSelectLevel;


        #region Properties
        public int CurrentLevel => _currentSelectLevel;
        public LevelData SelectLevelData => CharacterLevelData.Levels[_currentSelectLevel];
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
        
        public CharacterLevelData InitializeNewChartacterLevelData()
        {
            CharacterLevelData = new();
            LevelData levelData = new LevelData(0,0);
            CharacterLevelData.Levels.Add(levelData);
            OnCharacterLevelDataInitialized?.Invoke(CharacterLevelData);        
            return CharacterLevelData;
        }

        public void AddNewLevelDataAtIndex(int index)
        {
            LevelData levelData = new LevelData(0, 0);
            CharacterLevelData.Levels.Insert(index, levelData);
            OnCharacterLevelDataAdded?.Invoke(index);
           
        }

        public void RemoveLevelDataAtIndex(int index)
        {
            CharacterLevelData.Levels.RemoveAt(index);
            OnCharacterLevelDataRemoval?.Invoke(index);
        }
    }
}
