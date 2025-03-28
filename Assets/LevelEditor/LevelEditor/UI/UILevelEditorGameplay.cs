using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Match3.LevelEditor
{
    public class UILevelEditorGameplay : MonoBehaviour
    {
        [SerializeField] private Button _backToEditorBtn;

        private void Awake()
        {
            _backToEditorBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                SceneManager.UnloadSceneAsync(LevelEditorSceneLoader.Instance.OtherScene);
                LevelEditorSceneLoader.Instance.DisplaySceneObject(true);
            });
        }

        private void OnDestroy()
        {
            _backToEditorBtn.onClick.RemoveAllListeners();
        }
    }
}
