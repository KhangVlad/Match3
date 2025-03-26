using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace Match3.LevelEditor
{
    public class LevelEditorSaveManager : MonoBehaviour
    {
        public static LevelEditorSaveManager Instance { get; private set; }

        [Header("~Runtime")]
        public string CurrentSaveProjectPath = "";
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
            //Builder.Instance.OnDataHasChanged += OnDataHasChanged_UpdateSaveState;
        }

        private void OnDestroy()
        {
            //Builder.Instance.OnDataHasChanged -= OnDataHasChanged_UpdateSaveState;
        }


        //private Structure GetStructure(Builder builder)
        //{
        //    Structure structure = new Structure();

        //    foreach (var chunk in builder.Chunks.Values)
        //    {
        //        for (int z = 0; z < chunk.Dimensions[2]; z++)
        //        {
        //            for (int y = 0; y < chunk.Dimensions[1]; y++)
        //            {
        //                for (int x = 0; x < chunk.Dimensions[0]; x++)
        //                {
        //                    BlockID blockID = chunk.GetBlock(x, y, z);
        //                    if (blockID == BlockID.Air) continue;

        //                    StructureNode node = new StructureNode()
        //                    {
        //                        Position = chunk.GlobalPosition + new Vector3Int(x, y, z),
        //                        BlockID = blockID
        //                    };

        //                    structure.Nodes.Add(node);
        //                }
        //            }
        //        }
        //    }
        //    return structure;
        //}


        //private void SaveStructure(Structure structure, string filePath)
        //{
        //    string detectFileFormat = filePath.Split('.')[^1];  // last index [^1]
        //    string fileFormat = detectFileFormat.Equals("json") ? "" : ".json";

        //    string json = JsonUtility.ToJson(structure, true);
        //    File.WriteAllText(filePath + fileFormat, json);
        //}


        public void Save()
        {
        //    Debug.Log("Save");
        //    if (CurrentSaveProjectPath == "")
        //    {
        //        Debug.LogError("File not exist yet");
        //        return;
        //    }
        //    Structure structure = GetStructure(Builder.Instance);
        //    SaveStructure(structure, CurrentSaveProjectPath);
        //    UILogHandler.Instance.ShowLogText($"Save structure successfully: {CurrentSaveProjectPath}", 5f);
        //    HasBeenSaved = true;
        //}

        //public void SaveAs(string filePath)
        //{
        //    Debug.Log($"Save as: {filePath}");
        //    Structure structure = GetStructure(Builder.Instance);
        //    SaveStructure(structure, filePath);
        //    UILogHandler.Instance.ShowLogText($"Save structure successfully: {filePath}", 5f);

        //    CurrentSaveProjectPath = filePath;
        //    HasBeenSaved = true;
        }

        public async void Load(string filePath, System.Action onCompleted)
        {
            //if (File.Exists(filePath))
            //{
            //    try
            //    {
            //        string json = File.ReadAllText(filePath);
            //        Structure structure = JsonUtility.FromJson<Structure>(json);
            //        await BuilderChunkGeneration.Instance.LoadStructureTask(structure);
            //        UILogHandler.Instance.ShowLogText($"Load structure successfully: {filePath}", 5f);

            //        CurrentSaveProjectPath = filePath;
            //        onCompleted?.Invoke();
            //    }
            //    catch
            //    {
            //        UILogHandler.Instance.ShowWarningText($"Cannot load file: {filePath}", 5f);
            //        return;
            //    }
            //}
            //else
            //{
            //    UILogHandler.Instance.ShowWarningText($"File not found: {filePath}", 5f);
            //}

            //HasBeenSaved = true;
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
