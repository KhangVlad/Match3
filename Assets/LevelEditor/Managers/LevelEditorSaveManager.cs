using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
namespace Match3.LevelEditor
{
    public class LevelEditorSaveManager : MonoBehaviour
    {
        public static LevelEditorSaveManager Instance { get; private set; }

        [Header("~Runtime")]
        public string CurrentSaveProjectPath = "";
        public string FileName => Path.GetFileName(CurrentSaveProjectPath);
        public bool HasBeenSaved;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            HasBeenSaved = false;
            GridManager.Instance.OnDataHasChanged += OnDataHasChanged_UpdateSaveState;
        }

        private void OnDestroy()
        {
            GridManager.Instance.OnDataHasChanged -= OnDataHasChanged_UpdateSaveState;
        }


        private void SaveLevelData(LevelData levelData, string filePath)
        {
            string detectFileFormat = filePath.Split('.')[^1];  // last index [^1]
            string fileFormat = detectFileFormat.Equals("json") ? "" : ".json";

            //string json = JsonUtility.ToJson(levelData, true);
            string json = JsonConvert.SerializeObject(levelData);
            File.WriteAllText(filePath + fileFormat, json);
        }


        public void Save()
        {
            Debug.Log("Save");
            if (CurrentSaveProjectPath == "")
            {
                Debug.LogError("File not exist yet");
                return;
            }
            //Structure structure = GetStructure(Builder.Instance);
            //SaveStructure(structure, CurrentSaveProjectPath);

            LevelData levelData = GridManager.Instance.GetLevelData();
            SaveLevelData(levelData, CurrentSaveProjectPath);

            UILogHandler.Instance.ShowLogText($"Save successfully: {CurrentSaveProjectPath}", 5f);
            HasBeenSaved = true;
        }

        public void SaveAs(string filePath)
        {
            Debug.Log($"Save as: {filePath}");
            //Structure structure = GetStructure(Builder.Instance);
            //SaveStructure(structure, filePath);

            LevelData levelData = GridManager.Instance.GetLevelData();
            SaveLevelData(levelData, filePath);

            UILogHandler.Instance.ShowLogText($"Save structure successfully: {filePath}", 5f);

            CurrentSaveProjectPath = filePath;
            HasBeenSaved = true;
        }

        public void Load(string filePath, System.Action onCompleted)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);     
                    LevelData levelData = JsonConvert.DeserializeObject<LevelData>(json);
                    GridManager.Instance.LoadLevelData(levelData);
                    UILogHandler.Instance.ShowLogText($"Load structure successfully: {filePath}", 5f);
                    CurrentSaveProjectPath = filePath;
                    //onCompleted?.Invoke();
                }
                catch
                {
                    UILogHandler.Instance.ShowWarningText($"Cannot load file: {filePath}", 5f);
                    return;
                }
            }
            else
            {
                UILogHandler.Instance.ShowWarningText($"File not found: {filePath}", 5f);
            }

            HasBeenSaved = true;
        }

        public void LoadFromJson(string json, System.Action onCompleted)
        {
            try
            {
                LevelData levelData = JsonConvert.DeserializeObject<LevelData>(json);
                GridManager.Instance.LoadLevelData(levelData);
                UILogHandler.Instance.ShowLogText($"Load structure successfully", 5f);
                //onCompleted?.Invoke();
            }
            catch
            {
                UILogHandler.Instance.ShowWarningText($"Cannot load file:", 5f);
                return;
            }
            HasBeenSaved = true;
        }


        public bool IsCurrentWorkingFileExist()
        {
            Debug.Log($"{CurrentSaveProjectPath}   {File.Exists(CurrentSaveProjectPath)}");
            return File.Exists(CurrentSaveProjectPath);
        }
        public string GetProjectFileName()
        {
            return CurrentSaveProjectPath == "" ? "New Project*" : Path.GetFileNameWithoutExtension(CurrentSaveProjectPath);
        }


        private void OnDataHasChanged_UpdateSaveState()
        {
            HasBeenSaved = false;
        }
    }
}
