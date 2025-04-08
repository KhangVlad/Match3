using UnityEngine;
using UnityEngine.UI;
using Match3.Shares;
using System;

namespace Match3.LevelEditor
{
    public class UICharacterInventory : MonoBehaviour
    {
        public static UICharacterInventory Instance { get; private set; }

        [Header("Buttons")]
        [SerializeField] private Button _closeBtn;
        [SerializeField] private Button _selectBtn;

        [SerializeField] private Transform _contentsParent;
        [SerializeField] private UICharacterSlot _uiCharacterSlotPrefab;
        [SerializeField] private UICharacterSlot[] _uiSlots;


        [Header("~Runtime")]
        [SerializeField] private int _currentSelectSlotIndex;


        // Cached
        private UICharacterSelectedSlot _uiCharacterSelectedSlotCached;

        private void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
        }


        private void Start()
        {
            _closeBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();

                this.gameObject.SetActive(false);
            });

            _selectBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                this.gameObject.SetActive(false);


                LevelEditorManager.Instance.CharacterLevelData.CharacterID = GameDataManager.Instance.CharacterDataSos[_currentSelectSlotIndex].id;
                //GridManager.Instance.Heart = (int)GameDataManager.Instance.CharacterDataSos[_currentSelectSlotIndex].id;
                _uiCharacterSelectedSlotCached.SetData(GameDataManager.Instance.CharacterDataSos[_currentSelectSlotIndex]);
            });

            LoadAllUICharacters();

            UICharacterSlot.OnClicked += OnCharacterSlotClickTriggered;
            UICharacterSelectedSlot.OnClicked += OnUICharacterSelectedSlotClickTriggered;




            this.gameObject.SetActive(false);     
        }

        private void OnDestroy()
        {
            _closeBtn.onClick.RemoveAllListeners();
            _selectBtn.onClick.RemoveAllListeners();

            UICharacterSlot.OnClicked -= OnCharacterSlotClickTriggered;
            UICharacterSelectedSlot.OnClicked -= OnUICharacterSelectedSlotClickTriggered;
        }

        private void LoadAllUICharacters()
        {
            _uiSlots = new UICharacterSlot[GameDataManager.Instance.CharacterDataSos.Length];
            for (int i = 0; i < GameDataManager.Instance.CharacterDataSos.Length; i++)
            {
                CharacterDataSO characterData = GameDataManager.Instance.CharacterDataSos[i];

                UICharacterSlot slot = Instantiate(_uiCharacterSlotPrefab, _contentsParent);
                slot.SetData(characterData, i);
                _uiSlots[i] = slot;
            }
        }

        private void OnCharacterSlotClickTriggered(UICharacterSlot slot)
        {
            _currentSelectSlotIndex = slot.SlotIndex;
        }


        private void OnUICharacterSelectedSlotClickTriggered(UICharacterSelectedSlot slot)
        {
            _uiCharacterSelectedSlotCached = slot;
        }

    }
}
