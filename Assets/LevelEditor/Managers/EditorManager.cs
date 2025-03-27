using UnityEngine;

namespace Match3.LevelEditor
{
    public class EditorManager : MonoBehaviour
    {
        public static EditorManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
        }
    }
}
