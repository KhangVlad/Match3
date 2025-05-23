using UnityEngine;
using UnityEngine.SceneManagement;
using Match3.Shares;

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
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
            

            _levelEditorScene = SceneManager.GetSceneByName(Loader.Scene.LevelEditor.ToString());
        }

        public void DisplaySceneObject(bool enable)
        {
            if(_levelEditorRootSceneObjects == null)
            {
                _levelEditorRootSceneObjects = GameObject.Find("SceneObjects");
            }
            this._levelEditorRootSceneObjects.SetActive(enable);
        }
    }
}
