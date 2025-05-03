using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Match3
{
    public class UIQuestManager : MonoBehaviour
    {
        public static UIQuestManager Instance { get; private set; }

        [Header("Quests")]
        private GridLayoutGroup _gridLayoutGroup;
        [SerializeField] private UIQuest _uiQuestPrefab;
        [SerializeField] private Transform _questContentParent;
        [SerializeField] private UIQuest[] _uiQuestSlots;
        public event Action OnCollect;
    
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
            _gridLayoutGroup = _questContentParent.GetComponent<GridLayoutGroup>();
            if (_gridLayoutGroup == null)
            {
                Debug.LogError("Missing GridLayoutGroup components !!!");
            }
        }


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
            float uiScale = 1f;
            int questCount = GameplayManager.Instance.Quests.Length;
            if (questCount == 1) uiScale = 1.75f;
            else uiScale = 1f;

            _uiQuestSlots = new UIQuest[GameplayManager.Instance.Quests.Length];
            for (int i = 0; i < GameplayManager.Instance.Quests.Length; i++)
            {
                Quest quest = GameplayManager.Instance.Quests[i];
                QuestDataSO questData = GameDataManager.Instance.GetQuestDataByID(quest.QuestID);

                UIQuest uiQuest = Instantiate(_uiQuestPrefab, _questContentParent);
                uiQuest.SetData(questData.Icon, quest.Quantity);
                _uiQuestSlots[i] = uiQuest;
                _uiQuestSlots[i].transform.localScale = new Vector3(uiScale, uiScale, 1f);
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

        public Vector2 GetUIQuestSSPosition(int questIndex)
        {
            if (questIndex < 0 || questIndex > GameplayManager.Instance.Quests.Length - 1)
            {
                Debug.LogError("quest index out of range!");
                return default;
            }

            Vector2 uiPosition = _uiQuestSlots[questIndex].IconImage.transform.position;
            return uiPosition;
        }

        public void PlayQuestCollectAnimation(int questIndex)
        {
            if (questIndex < 0 || questIndex > GameplayManager.Instance.Quests.Length - 1)
            {
                Debug.LogError("quest index out of range!");
                return;
            }
            _uiQuestSlots[questIndex].PlayCollectAnimation();
            OnCollect?.Invoke();
        }
    }
}