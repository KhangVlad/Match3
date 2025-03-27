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
        // Ensure directory exists
        string directoryPath = "Assets/Resources/DataSO/CharacterDialogues/";
        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

        foreach (string file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            string[] lines = File.ReadAllLines(file);
            List<string> dialogues = new List<string>();

            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    dialogues.Add(line.Trim('"'));
                }
            }

            CharacterDialogueSO dialogueSO = ScriptableObject.CreateInstance<CharacterDialogueSO>();
            dialogueSO.id = (CharacterID)System.Enum.Parse(typeof(CharacterID), fileName);
            dialogueSO.levelDialogues = dialogues.ToArray();

            AssetDatabase.CreateAsset(dialogueSO, $"{directoryPath}{fileName}_Dialogue.asset");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Finished creating ScriptableObjects.");
#else
        Debug.Log("ScriptableObject creation is only supported in the Unity Editor.");
#endif
    }
}