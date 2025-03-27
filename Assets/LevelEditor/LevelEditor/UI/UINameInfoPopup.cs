using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace Match3.LevelEditor
{
    public class UINameInfoPopup : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _contentText;
        [SerializeField] private RectTransform _rectTransform;

    
        public void SetText(string text)
        {
            _contentText.text = text;   
        }

        private void Update()
        {
            UpdatePosition();
        }

        public void UpdatePosition()
        {
            _rectTransform.position = Input.mousePosition + new Vector3(0, 100);
        }
    }
}
