using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace Match3.LevelEditor
{
    public class UICopyPaste : MonoBehaviour
    {
        public static UICopyPaste Instance { get; private set; }

        [SerializeField] private RectTransform _panelTransform;

        [Header("Buttons")]
        [SerializeField] private Button _closeBtn;
        [SerializeField] private Button _copyBtn;
        [SerializeField] private Button _pasteBtn;

        // Cached
        public LevelDataV2 CachedLevelData;
        public int CachedIndex;

        private void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            _closeBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                gameObject.SetActive(false);
            });
            _copyBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                gameObject.SetActive(false);
                CopyPasteManager.Instance.Copy(CachedLevelData);
            });
            _pasteBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                gameObject.SetActive(false);
                LevelEditorManager.Instance.SaveLevelData(CachedIndex, CopyPasteManager.Instance.LevelData);
            });

            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _closeBtn.onClick.RemoveAllListeners();
            _copyBtn.onClick.RemoveAllListeners();
            _pasteBtn.onClick.RemoveAllListeners();
        }

        private void OnEnable()
        {
            if (CopyPasteManager.Instance.LevelData == null)
            {
                _pasteBtn.interactable = false;
            }
            else
            {
                _pasteBtn.interactable = true;
            }
        }

        public void UpdatePosition()
        {
            _panelTransform.position = Input.mousePosition + new Vector3(75, 50);
        }
    }
}
