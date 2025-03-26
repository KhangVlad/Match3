using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Match3
{
    public class UIQuest : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _quantityText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private GameObject _completeMarkObject;

        public void SetData(Sprite sprite, int quantity)
        {
            this._iconImage.sprite = sprite;
            this._quantityText.text = quantity.ToString();
        }

        public void UpdateQuest(int quantity)
        {
            if(quantity <= 0)
            {
                _completeMarkObject.gameObject.SetActive(true);
                _quantityText.text = "";
            }
            else
            {
                _completeMarkObject.gameObject.SetActive(false);
                _quantityText.text = quantity.ToString();
            }
        }
    }
}
