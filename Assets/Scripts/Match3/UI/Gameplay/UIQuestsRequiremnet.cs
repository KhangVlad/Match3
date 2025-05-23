
    using Match3;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    public class UIQuestsRequiremnet : MonoBehaviour
    {
        [SerializeField] private Image questIcon;
        [SerializeField] private TextMeshProUGUI quantityText;

        public void Initialize(QuestDataSO data ,int quantity)
        {
            questIcon.sprite = data.Icon;
            this.quantityText.text = quantity.ToString();
        }
    }
