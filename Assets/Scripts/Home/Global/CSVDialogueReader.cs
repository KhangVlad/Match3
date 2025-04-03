//
//
//
// using System;
// using System.Collections.Generic;
// using System.IO;
// using UnityEngine;
// using UnityEditor;
//
// public class CSVDialogueReader : MonoBehaviour
// {
//     private string FolderPath = "Assets/Resources/CSVData/Dialogues";
//
//     [ContextMenu("Read CSV And Create SO")]
//     public void ReadCSVAndCreateSO()
//     {
//         string[] files = Directory.GetFiles(FolderPath, "*.csv");
//
// #if UNITY_EDITOR
//         foreach (string file in files)
//         {
//             string fileName = Path.GetFileNameWithoutExtension(file);
//             string[] lines = File.ReadAllLines(file);
//
//             // Determine the language based on the file name prefix
//             string language = fileName.StartsWith("vi_") ? "VI" : "EN";
//             string characterId = fileName.Substring(3); // Remove the language prefix
//
//             // Ensure directory exists
//             string directoryPath = $"Assets/Resources/DataSO/CharacterDialogues/{language}/";
//             if (!Directory.Exists(directoryPath))
//                 Directory.CreateDirectory(directoryPath);
//
//             // Create the ScriptableObject for dialogue data
//             CharacterDialogueSO dialogueSO = ScriptableObject.CreateInstance<CharacterDialogueSO>();
//             dialogueSO.id = (CharacterID)Enum.Parse(typeof(CharacterID), characterId);
//
//             // Dictionary to hold LevelDialogueData by level
//             Dictionary<int, List<string>> levelDialogues = new Dictionary<int, List<string>>();
//
//             // Skip the header row and iterate through the lines
//             for (int i = 1; i < lines.Length; i++)
//             {
//                 string[] columns = lines[i].Split(',');
//
//                 // Make sure the row has enough columns
//                 if (columns.Length >= 3)
//                 {
//                     int level = int.Parse(columns[0].Trim()); // Level (GroupLevel)
//                     string dialog = columns[2].Trim(); // Dialogue text
//
//                     // If the level is already in the dictionary, add the dialogue to it
//                     if (!levelDialogues.ContainsKey(level))
//                     {
//                         levelDialogues[level] = new List<string>();
//                     }
//
//                     levelDialogues[level].Add(dialog);
//                 }
//             }
//
//             // Convert the dictionary into the LevelDialogueData array
//             List<LevelDialogueData> levelDataList = new List<LevelDialogueData>();
//             foreach (var levelEntry in levelDialogues)
//             {
//                 LevelDialogueData data = new LevelDialogueData
//                 {
//                     levelDialogs = levelEntry.Value.ToArray(),
//                 
//                 };
//                 levelDataList.Add(data);
//             }
//
//             // Set the dialogue data in the SO
//             dialogueSO.data = levelDataList.ToArray();
//
//             // Create the asset in the directory
//             AssetDatabase.CreateAsset(dialogueSO, $"{directoryPath}{characterId}_Dialogue.asset");
//         }
//
//         // Save and refresh assets
//         AssetDatabase.SaveAssets();
//         AssetDatabase.Refresh();
//         Debug.Log("Finished creating ScriptableObjects.");
// #else
//         Debug.Log("ScriptableObject creation is only supported in the Unity Editor.");
// #endif
//     }
// }

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class CSVDialogueReader : MonoBehaviour
{
    private string FolderPath = "Assets/Resources/CSVData/Dialogues";

    [ContextMenu("Read CSV And Create SO")]
    public void ReadCSVAndCreateSO()
    {
        string[] files = Directory.GetFiles(FolderPath, "*.csv");

#if UNITY_EDITOR
        foreach (string file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            string[] lines = File.ReadAllLines(file);

            // Determine the language based on the file name prefix
            string language = fileName.StartsWith("vi_") ? "VI" : "EN";
            string characterId = fileName.Substring(3); // Remove the language prefix

            // Ensure directory exists
            string directoryPath = $"Assets/Resources/DataSO/CharacterDialogues/{language}/";
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            // Create the ScriptableObject for dialogue data
            CharacterDialogueSO dialogueSO = ScriptableObject.CreateInstance<CharacterDialogueSO>();
            dialogueSO.id = (CharacterID)Enum.Parse(typeof(CharacterID), characterId);

            // Dictionary to hold LevelDialogueData by level
            Dictionary<int, List<string>> levelDialogues = new Dictionary<int, List<string>>();
            List<string> greetingDialogs = new List<string>(); // Store greeting dialogues

            // Skip the header row and iterate through the lines
            for (int i = 1; i < lines.Length; i++)
            {
                string[] columns = lines[i].Split(',');

                // Make sure the row has enough columns
                if (columns.Length >= 4)
                {
                    int level = int.Parse(columns[0].Trim()); // Level (GroupLevel)
                    string dialog = columns[2].Trim(); // Dialogue text
                    string greeting = columns[3].Trim(); // Greeting dialogue

                    // Store greeting dialogue
                    if (!string.IsNullOrEmpty(greeting))
                    {
                        greetingDialogs.Add(greeting);
                    }

                    // Store level dialogue
                    if (!levelDialogues.ContainsKey(level))
                    {
                        levelDialogues[level] = new List<string>();
                    }

                    levelDialogues[level].Add(dialog);
                }
            }

            // Convert the dictionary into the LevelDialogueData array
            List<LevelDialogueData> levelDataList = new List<LevelDialogueData>();
            foreach (var levelEntry in levelDialogues)
            {
                LevelDialogueData data = new LevelDialogueData
                {
                    levelDialogs = levelEntry.Value.ToArray(),
                };
                levelDataList.Add(data);
            }

            // Set dialogues in the SO
            dialogueSO.data = levelDataList.ToArray();
            dialogueSO.greetingDialogs = greetingDialogs.ToArray(); // Assign greeting dialogues

            // Create the asset in the directory
            AssetDatabase.CreateAsset(dialogueSO, $"{directoryPath}{characterId}_Dialogue.asset");
        }

        // Save and refresh assets
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Finished creating ScriptableObjects.");
#else
        Debug.Log("ScriptableObject creation is only supported in the Unity Editor.");
#endif
    }
}
