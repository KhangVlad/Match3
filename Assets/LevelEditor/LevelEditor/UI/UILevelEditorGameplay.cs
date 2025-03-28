using UnityEngine;
using UnityEngine.UI;

namespace Match3.LevelEditor
{
    public class UILevelEditorGameplay : MonoBehaviour
    {
        [SerializeField] private Button _backToEditorBtn;

        private void Awake()
        {
            _backToEditorBtn.onClick.AddListener(() =>
            {

            });
        }

        private void OnDestroy()
        {
            _backToEditorBtn.onClick.RemoveAllListeners();
        }
    }
}
