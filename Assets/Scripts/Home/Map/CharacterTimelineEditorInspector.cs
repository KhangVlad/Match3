using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Match3.Enums;
using Match3.Shares;
#if UNITY_EDITOR
[CustomEditor(typeof(TimeLineManager))]
public class CharacterTimelineEditorInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TimeLineManager myScript = (TimeLineManager)target;

        if (GUILayout.Button("Save Object"))
        {
            // Define the folder and asset path.
            string assetFolder = "Assets/Resources/DataSO/CharacterActivities/";
            // Construct an asset file name based on the character ID (casting to int if needed).
            string assetPath = assetFolder + "c_" + (int)myScript.EditorCharacterID + ".asset";

            // Try to load an existing asset at the path.
            CharacterActivitySO activitySO = AssetDatabase.LoadAssetAtPath<CharacterActivitySO>(assetPath);

            // Create a new ActivityInfo based on editor settings.
            ActivityInfo newActivity = new ActivityInfo
            {
                startTime = myScript.StartTime,
                endTime = myScript.EndTime,
                appearPosition = myScript.AppearPosition,
                dayOfWeek = myScript.currentDay
            };

            if (activitySO != null)
            {
                // If the asset already exists, add the new activity info.
                List<ActivityInfo> activityList =
                    new List<ActivityInfo>(activitySO.activityInfos ?? new ActivityInfo[0]);
                activityList.Add(newActivity);
                activitySO.activityInfos = activityList.ToArray();
                activitySO.homePosition = myScript.HomePos;
                activitySO.sprite = myScript.CharacterSprite;
                EditorUtility.SetDirty(activitySO);
                AssetDatabase.SaveAssets();
                Debug.Log("Activity added to existing asset.");
            }
            else
            {
                // If the asset does not exist, create a new one.
                activitySO = ScriptableObject.CreateInstance<CharacterActivitySO>();
                activitySO.id = myScript.EditorCharacterID;
                activitySO.homePosition = myScript.HomePos;
                activitySO.sprite = myScript.CharacterSprite;

                activitySO.activityInfos = new ActivityInfo[] { newActivity };

                // Create and save the new asset.
                AssetDatabase.CreateAsset(activitySO, assetPath);
                AssetDatabase.SaveAssets();
                Debug.Log("New asset created and activity added.");
            }
        }

        if (GUILayout.Button("PlusCurrentHour"))
        {
            myScript.PlusCurrentHour();
        }

        if (GUILayout.Button("MinusCurrentHour"))
        {
            myScript.MinusCurrentHour();
        }
    }
}
#endif