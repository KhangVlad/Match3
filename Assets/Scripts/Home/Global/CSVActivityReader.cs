using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Match3.Enums;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CSVActivityReader : MonoBehaviour
{
    public TextAsset csvFile;

    [ContextMenu("Read CSV And Create SO")]
    public void ReadCSVAndCreateSO()
    {
        if (csvFile == null)
        {
            Debug.LogError("CSV file not assigned!");
            return;
        }

        string[] lines = csvFile.text.Split('\n');
        Dictionary<int, List<ActivityInfo>> characterActivities = new Dictionary<int, List<ActivityInfo>>();

        for (int i = 1; i < lines.Length; i++) // Skip header
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            string[] values = lines[i].Split(','); // Assuming comma-separated CSV
            if (values.Length < 6)
            {
                Debug.LogWarning($"Skipping malformed line: {lines[i]}");
                continue;
            }

            try
            {
                int charId = int.Parse(values[0]);
                DayInWeek day = (DayInWeek)int.Parse(values[1]);
                int startTime = int.Parse(values[2]);
                int endTime = int.Parse(values[3]);
                Vector2Int appearPosition = new Vector2Int(int.Parse(values[4]), int.Parse(values[5]));

                ActivityInfo activityInfo = new ActivityInfo
                {
                    startTime = startTime,
                    endTime = endTime,
                    appearPosition = appearPosition,
                    dayOfWeek = day
                };

                if (!characterActivities.ContainsKey(charId))
                    characterActivities[charId] = new List<ActivityInfo>();

                characterActivities[charId].Add(activityInfo);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to parse line {i}: {lines[i]}. Error: {e.Message}");
            }
        }

#if UNITY_EDITOR
        // Ensure directory exists
        string directoryPath = "Assets/Resources/DataSO/CharacterActivities/";
        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

        foreach (var kvp in characterActivities)
        {
            CreateCharacterActivitySO(kvp.Key, kvp.Value);
        }

        AssetDatabase.Refresh();
        Debug.Log("Finished creating ScriptableObjects.");
#else
        Debug.Log("ScriptableObject creation is only supported in the Unity Editor.");
#endif
    }

#if UNITY_EDITOR
    private void CreateCharacterActivitySO(int charId, List<ActivityInfo> activities)
    {
        CharacterActivitySO characterActivitySO = ScriptableObject.CreateInstance<CharacterActivitySO>();
        characterActivitySO.id = (CharacterID)charId; // Ensure CharacterID enum matches charId
        characterActivitySO.activityInfos = activities.ToArray();

        string path = $"Assets/Resources/DataSO/CharacterActivities/CharacterActivity_{charId}.asset";
        AssetDatabase.CreateAsset(characterActivitySO, path);
    }
#endif
}