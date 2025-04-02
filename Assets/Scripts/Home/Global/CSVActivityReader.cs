using UnityEngine;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CSVActivityReader : MonoBehaviour
{
//     public TextAsset csvFile;
//     private string spritePath = "Sprites/CharactersAvatar/";
//
//     private Dictionary<CharacterID, Vector2Int> homePos = new Dictionary<CharacterID, Vector2Int>
//     {
//         { CharacterID.Lina, new Vector2Int(1897, 1762) },
//         { CharacterID.John, new Vector2Int(2490, 2401) },
//         { CharacterID.Mary, new Vector2Int(1508, 1448) },
//         { CharacterID.Tom, new Vector2Int(0, 0) },
//         { CharacterID.Sarah, new Vector2Int(0, 0) },
//         { CharacterID.Shiba, new Vector2Int(0, 0) }
//     };
//
//
//     private Dictionary<CharacterID, int[]> heartRequired = new Dictionary<CharacterID, int[]>
//     {
//         { CharacterID.Lina, new int[] { 30, 70, 150, 300, 450, 700 } },
//         { CharacterID.John, new int[] { 30, 70, 150, 300, 450, 700 } },
//         { CharacterID.Mary, new int[] { 30, 70, 150, 300, 450, 700 } },
//         { CharacterID.Tom, new int[] { 30, 70, 150, 300, 450, 700 } },
//         { CharacterID.Sarah, new int[] { 30, 70, 150, 300, 450, 700 } },
//         { CharacterID.Shiba, new int[] { 30, 70, 150, 300, 450, 700 } }
//     };
//
//     public Dictionary<CharacterID, DayInWeek> dayOff = new Dictionary<CharacterID, DayInWeek>
//     {
//         { CharacterID.Lina, DayInWeek.None },
//         { CharacterID.John, DayInWeek.Sunday },
//         { CharacterID.Mary, DayInWeek.None },
//         { CharacterID.Tom, DayInWeek.None },
//         { CharacterID.Sarah, DayInWeek.None },
//         { CharacterID.Shiba, DayInWeek.None }
//     };
//
//
//     [ContextMenu("Read CSV And Create SO")]
//     public void ReadCSVAndCreateSO()
//     {
//         if (csvFile == null)
//         {
//             Debug.LogError("CSV file not assigned!");
//             return;
//         }
//
//         string[] lines = csvFile.text.Split('\n');
//         Dictionary<int, List<ActivityInfo>> characterActivities = new Dictionary<int, List<ActivityInfo>>();
//
//         for (int i = 1; i < lines.Length; i++) // Skip header
//         {
//             if (string.IsNullOrWhiteSpace(lines[i]))
//                 continue;
//
//             string[] values = lines[i].Split(','); // Assuming comma-separated CSV
//             if (values.Length < 6)
//             {
//                 Debug.LogWarning($"Skipping malformed line: {lines[i]}");
//                 continue;
//             }
//
//             try
//             {
//                 int charId = int.Parse(values[0]);
//                 DayInWeek day = (DayInWeek)int.Parse(values[1]);
//                 int startTime = int.Parse(values[2]);
//                 int endTime = int.Parse(values[3]);
//                 Vector2Int appearPosition = new Vector2Int(int.Parse(values[4]), int.Parse(values[5]));
//
//                 ActivityInfo activityInfo = new ActivityInfo
//                 {
//                     startTime = startTime,
//                     endTime = endTime,
//                     appearPosition = appearPosition,
//                     dayOfWeek = day,
//                 };
//
//                 if (!characterActivities.ContainsKey(charId))
//                     characterActivities[charId] = new List<ActivityInfo>();
//
//                 characterActivities[charId].Add(activityInfo);
//             }
//             catch (System.Exception e)
//             {
//                 Debug.LogError($"Failed to parse line {i}: {lines[i]}. Error: {e.Message}");
//             }
//         }
//
// #if UNITY_EDITOR
//         // Ensure directory exists
//         string directoryPath = "Assets/Resources/DataSO/CharacterActivities/";
//         if (!Directory.Exists(directoryPath))
//             Directory.CreateDirectory(directoryPath);
//
//         foreach (var kvp in characterActivities)
//         {
//             CreateCharacterActivitySO(kvp.Key, kvp.Value, homePos[(CharacterID)kvp.Key]);
//         }
//
//         AssetDatabase.Refresh();
//         Debug.Log("Finished creating ScriptableObjects.");
// #else
//         Debug.Log("ScriptableObject creation is only supported in the Unity Editor.");
// #endif
//     }
//
// #if UNITY_EDITOR
//     private void CreateCharacterActivitySO(int charId, List<ActivityInfo> activities, Vector2Int pos)
//     {
//         CharacterActivitySO characterActivitySO = ScriptableObject.CreateInstance<CharacterActivitySO>();
//         characterActivitySO.id = (CharacterID)charId; // Ensure CharacterID enum matches charId
//         characterActivitySO.activityInfos = activities.ToArray();
//         characterActivitySO.homePosition = pos;
//         characterActivitySO.sprite =
//             AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/CharactersAvatar/c_{(int)charId}.png");
//         characterActivitySO.sympathyRequired = heartRequired[(CharacterID)charId];
//         if (characterActivitySO.sprite == null)
//         {
//             Debug.LogError($"Sprite not found at path: Assets/Sprites/CharactersAvatar/c_{(int)charId}.png");
//         }
//         characterActivitySO.dayOff = dayOff[(CharacterID)charId];
//
//         string path = $"Assets/Resources/DataSO/CharacterActivities/{charId}.asset";
//         AssetDatabase.CreateAsset(characterActivitySO, path);
//     }
// #endif
}