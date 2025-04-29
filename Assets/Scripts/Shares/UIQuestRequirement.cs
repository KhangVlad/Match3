namespace Match3.Shares
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class UIQuestRequirement : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _quantity;
        [SerializeField] private Image icon;

        public void Initialize(Sprite s, int q)
        {
            icon.sprite = s;
            _quantity.text = q.ToString();
        }
    }
    
    
}