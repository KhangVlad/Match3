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


        private void SaveCharacterLevelData(CharacterLevelDataV2 characterLevelData, string filePath)
        {
            string detectFileFormat = filePath.Split('.')[^1];  // last index [^1]
            string fileFormat = detectFileFormat.Equals("json") ? "" : ".json";
            string json = JsonConvert.SerializeObject(characterLevelData);   
            File.WriteAllText(filePath + fileFormat, json);
            Debug.Log(filePath);
        }



        public void SaveAs(string filePath)
        {
            Debug.Log($"Save as: {filePath}");
 
            LevelDataV2 levelData = GridManager.Instance.GetLevelData();
            int index = LevelEditorManager.Instance.CurrentLevel;
            LevelEditorManager.Instance.SaveLevelData(index, levelData);

            SaveCharacterLevelData(LevelEditorManager.Instance.CharacterLevelData, filePath);
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
                    //Debug.Log("version: " + CharacterLevelDataExtensions.DetectVersion(json));
                    int version = CharacterLevelDataExtensions.DetectVersion(json);
                    Debug.Log($"version: {version}");
                    switch(version)
                    {
                        case 1:
                            CharacterLevelDataV1 characterLevelDataV1 = JsonConvert.DeserializeObject<CharacterLevelDataV1>(json);
                            CharacterLevelDataV2 characterDataInVersion2 = characterLevelDataV1.UpgradeV1ToV2();
                            LevelEditorManager.Instance.SetCharacterLevelData(characterDataInVersion2);
                            break;
                        case 2:
                            CharacterLevelDataV2 characterLevelDataV2 = JsonConvert.DeserializeObject<CharacterLevelDataV2>(json);
                            LevelEditorManager.Instance.SetCharacterLevelData(characterLevelDataV2);
                            break;
                        default:
                            Debug.LogError("Version not found");
                            break;
                    }
                    UILogHandler.Instance.ShowLogText($"Load structure successfully: {filePath}", 5f);
                    CurrentSaveProjectPath = filePath;
                    onCompleted?.Invoke();
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
                CharacterLevelDataV2 characterLevelData = JsonConvert.DeserializeObject<CharacterLevelDataV2>(json);
                LevelEditorManager.Instance.SetCharacterLevelData(characterLevelData);
                //LevelData levelData = JsonConvert.DeserializeObject<LevelData>(json);
                //GridManager.Instance.LoadLevelData(levelData);
                UILogHandler.Instance.ShowLogText($"Load structure successfully", 5f);

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
