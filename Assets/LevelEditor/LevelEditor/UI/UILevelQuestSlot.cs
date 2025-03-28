using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Match3.LevelEditor
{
    public class UILevelQuestSlot : MonoBehaviour
    {
        [SerializeField] private Button _removeBtn;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_InputField _inputField;

        private int _slotIndex;

        private void Start()
        {
            _removeBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                GridManager.Instance.RemoveQuest(_slotIndex);
            });

            _inputField.onValueChanged.AddListener((value) =>
            {
                Quest quest = GridManager.Instance.Quests[_slotIndex];
                GridManager.Instance.Quests[_slotIndex] = new Quest()
                {
                    QuestID = quest.QuestID,
                    Quantity = int.Parse(value)
                };
            });
        }

        private void OnDestroy()
        {
            _removeBtn.onClick.RemoveAllListeners();
            _inputField.onValueChanged.RemoveAllListeners();
        }

        public void SetData(Quest quest, int slotIndex)
        {
            QuestDataSO questData = GameDataManager.Instance.GetQuestDataByID(quest.QuestID);
            _iconImage.sprite = questData.Icon;
            this._slotIndex = slotIndex;

            _inputField.text = quest.Quantity.ToString();
        }
    }
}
