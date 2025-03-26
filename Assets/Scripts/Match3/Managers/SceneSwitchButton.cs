using UnityEngine;


#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityToolbarExtender;
#endif
using UnityEngine.SceneManagement;


namespace Match3
{
#if UNITY_EDITOR
    [InitializeOnLoad]
    public class SceneSwitchButton
    {
        static SceneSwitchButton()
        {
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
        }

        private static void OnToolbarGUI()
        {
            GUILayout.FlexibleSpace();
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                fixedHeight = 25,
                fixedWidth = 100
            };

            if (GUILayout.Button(new GUIContent("Level Editor")))
            {
                SceneHelper.StartSceneWithSavePrompt("Assets/Scenes/LevelEditor.unity");
            }
            if (GUILayout.Button(new GUIContent("Menu")))
            {
                SceneHelper.StartSceneWithSavePrompt("Assets/Scenes/MenuScene.unity");
            }
            if( GUILayout.Button(new GUIContent("Town")))
            {
                SceneHelper.StartSceneWithSavePrompt("Assets/Scenes/Town.unity");
            }

            if (GUILayout.Button(new GUIContent("Gameplay")))
            {
                SceneHelper.StartSceneWithSavePrompt("Assets/Scenes/GameplayScene.unity");
            }

            if (GUILayout.Button(new GUIContent("Fast play")))
            {
                SceneHelper.StartSceneWithSavePrompt("Assets/Scenes/Loading.unity", true);
            }
            
            
        }
    }
#endif

    public static class SceneHelper
    {
#if UNITY_EDITOR
        public static void StartScene(string scenePath, bool play = false)
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }

            EditorSceneManager.OpenScene(scenePath);
            EditorApplication.isPlaying = play;
        }

        public static void StartSceneWithSavePrompt(string scenePath, bool play = false)
        {
            // Check if there are unsaved changes in the current scene
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                // User chose to save (or no changes) - load the new scene
                StartScene(scenePath, play);
            }
            else
            {
                // User canceled the save operation - do not load the scene
                Debug.Log("Scene load canceled because user didn't save changes.");
            }
        }
#endif
    }
}
