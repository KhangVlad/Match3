using UnityEngine;

namespace Match3.LevelEditor
{
    public class CopyPasteManager : MonoBehaviour
    {
        public static CopyPasteManager Instance { get; private set; }
        public event System.Action<LevelData> OnCopy;


        [field: SerializeField] public LevelData LevelData { get; private set; }

        private void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
        }

        public void Copy(LevelData levelData)
        {
            this.LevelData = levelData;
            OnCopy?.Invoke(levelData);
        }
    }
}
