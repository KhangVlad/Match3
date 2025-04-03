using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace Match3.LevelEditor
{
    public class UINameInfoPopup : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _contentText;
        [SerializeField] private RectTransform _rectTransform;

        private Vector2 _offset;

        public void SetText(string text)
        {
            _contentText.text = text;   
        }

        private void Update()
        {
            UpdatePosition(_offset);
        }

        public void UpdatePosition(Vector2 offset = default)
        {
            _offset = offset;
            _rectTransform.position = Input.mousePosition + new Vector3(0, 100) + (Vector3)_offset;
        }
    }
}
