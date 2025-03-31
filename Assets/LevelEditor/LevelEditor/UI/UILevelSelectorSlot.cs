using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace Match3.LevelEditor
{
    public class UILevelSelectorSlot : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private Button _addNewBtn;
        [SerializeField] private Button _eidtBtn;
        [SerializeField] private Button _removeBtn;
        [SerializeField] private Image _iconImage;


        [Header("~Runtime")]
        [SerializeField] private int _level;


        private void Start()
        {
            _addNewBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                LevelEditorManager.Instance.AddNewLevelDataAtIndex(_level + 1);
            });
            _eidtBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                UILevelEditorManager.Instance.DisplayUILevelSelector(false);
                UILevelEditorManager.Instance.DisplayUILevelEditor(true);
            });
            _removeBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                LevelEditorManager.Instance.RemoveLevelDataAtIndex(_level);
            });
        }

        private void OnDestroy()
        {
            _addNewBtn.onClick.RemoveAllListeners();
            _eidtBtn.onClick.RemoveAllListeners();
            _removeBtn.onClick.RemoveAllListeners();
        }

        public void SetData(LevelData levelData, int level)
        {
            _level = level;
            _levelText.text = $"Level {level + 1}";
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                Debug.Log("Right click");
            }
        }
    }
}
