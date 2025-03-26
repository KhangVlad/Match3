using UnityEngine;

namespace Match3.LevelEditor
{
    public class UILevelEditorManager : MonoBehaviour
    {
        public static UILevelEditorManager Instance { get; private set; }

        public UIMenu UIMenu { get; private set; }
        public UILevelEditor UILevelEditor { get; private set; }

        private void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;

            UIMenu = FindFirstObjectByType<UIMenu>();
            UILevelEditor = FindFirstObjectByType<UILevelEditor>();
        }

        private void Start()
        {
            CloseAll();
        }


        public void CloseAll()
        {
            DisplayUILevelEditor(false);
        }


        public void DisplayUIMenu(bool enable)
        {
            UIMenu.DisplayCanvas(enable);
        }

        public void DisplayUILevelEditor(bool enable)
        {
            UILevelEditor.DisplayCanvas(enable);
        }
    }
}
