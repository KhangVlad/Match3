using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Match3.LevelEditor
{
    public class UILevelSelector : MonoBehaviour
    {
        private Canvas _canvas;

        [SerializeField] private TextMeshProUGUI _idText;


        [Space(10)]
        [SerializeField] private Transform _contentParent;
        [SerializeField] private UILevelSelectorSlot _uiLevelSelectorSlotPrefab;


        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }


        public void DisplayCanvas(bool enable)
        {
            this._canvas.enabled = enable;
        }
    }
}
