using UnityEngine;

namespace Match3.LevelEditor
{
    public class UILevelEditorManager : MonoBehaviour
    {
        public static UILevelEditorManager Instance { get; private set; }

        public UIMenu UIMenu { get; private set; }
        public UILevelEditor UILevelEditor { get; private set; }
        public UILevelSelector UILevelSelector { get; private set; }
        public UIPopupManager UIPopupManager { get; private set; }

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
            UILevelSelector = FindFirstObjectByType<UILevelSelector>();
            UIPopupManager = FindFirstObjectByType<UIPopupManager>();
        }

        private void Start()
        {
            CloseAll();
            DisplayUIPopupManager(true);
        }


        public void CloseAll()
        {
            DisplayUILevelEditor(false);
            DisplayUILevelSelector(false);
        }


        public void DisplayUIMenu(bool enable)
        {
            UIMenu.DisplayCanvas(enable);
        }

        public void DisplayUILevelEditor(bool enable)
        {
            UILevelEditor.DisplayCanvas(enable);
        }

        public void DisplayUILevelSelector(bool enable)
        {
            UILevelSelector.DisplayCanvas(enable);
        }

        public void DisplayUIPopupManager(bool enable)
        {
            UIPopupManager.DisplayCanvas(enable);
        }
    }
}
