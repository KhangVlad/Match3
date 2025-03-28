using UnityEngine;
using UnityEngine.SceneManagement;

namespace Match3.LevelEditor
{
    public class LevelEditorSceneLoader : MonoBehaviour
    {
        public static LevelEditorSceneLoader Instance { get; private set; }
        private Scene _levelEditorScene;
        public Scene OtherScene;
        [SerializeField] private GameObject _levelEditorRootSceneObjects;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            

            _levelEditorScene = SceneManager.GetSceneByName(Loader.Scene.LevelEditor.ToString());
        }

        public void DisplaySceneObject(bool enable)
        {
            this._levelEditorRootSceneObjects.SetActive(enable);
        }
    }
}
