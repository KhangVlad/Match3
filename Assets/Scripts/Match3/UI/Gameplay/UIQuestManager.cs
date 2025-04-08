using UnityEngine;

namespace Match3
{
    public class UIQuestManager : MonoBehaviour
    {
        [Header("Quests")]
        [SerializeField] private UIQuest _uiQuestPrefab;
        [SerializeField] private Transform _questContentParent;
        [SerializeField] private UIQuest[] _uiQuestSlots;


        private void Start()
        {
                 Match3Grid.OnEndOfTurn += OnEndOfTurn_UpdateQuestUI;
            LoadAllQuestsUI();
        }

        private void OnDestroy()
        {
                 Match3Grid.OnEndOfTurn -= OnEndOfTurn_UpdateQuestUI;
        }

        private void LoadAllQuestsUI()
        {
            _uiQuestSlots = new UIQuest[GameplayManager.Instance.Quests.Length];
            for (int i = 0; i < GameplayManager.Instance.Quests.Length; i++)
            {
                Quest quest = GameplayManager.Instance.Quests[i];
                QuestDataSO questData = GameDataManager.Instance.GetQuestDataByID(quest.QuestID);

                UIQuest uiQuest = Instantiate(_uiQuestPrefab, _questContentParent);
                uiQuest.SetData(questData.Icon, quest.Quantity);
                _uiQuestSlots[i] = uiQuest;
            }
        }


        private void OnEndOfTurn_UpdateQuestUI()
        {
            for (int i = 0; i < _uiQuestSlots.Length; i++)
            {
                UIQuest uiQuest = _uiQuestSlots[i];
                Quest quest = GameplayManager.Instance.Quests[i];

                uiQuest.UpdateQuest(quest);
            }
        }
    }
}