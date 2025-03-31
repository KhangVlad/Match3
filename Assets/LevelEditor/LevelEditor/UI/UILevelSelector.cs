using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

namespace Match3.LevelEditor
{
    public class UILevelSelector : MonoBehaviour
    {
        private Canvas _canvas;

        [SerializeField] private TextMeshProUGUI _idText;


        [Space(10)]
        [SerializeField] private Transform _contentParent;
        [SerializeField] private UILevelSelectorSlot _uiLevelSelectorSlotPrefab;
        [SerializeField] private List<UILevelSelectorSlot> _uiSlots;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }


        private void Start()
        {
            LevelEditorManager.Instance.OnFileNameChanged += OnFileNameChanged_UpdateUI;
            LevelEditorManager.Instance.OnCharacterLevelDataLoaded += OnCharacterLevelDataLoaded_InitUI;

            LevelEditorManager.Instance.OnCharacterLevelDataAdded += OnCharacterLevelDataAdded_UpdateUI;
            LevelEditorManager.Instance.OnCharacterLevelDataRemoval += OnCharacterLevelDataRemoval_UpdateUI;

            LevelEditorManager.Instance.OnLevelDataSaved += OnLevelDataSaved_UpdateUI;
        }

     

        private void OnDestroy()
        {
            LevelEditorManager.Instance.OnFileNameChanged -= OnFileNameChanged_UpdateUI;
            LevelEditorManager.Instance.OnCharacterLevelDataLoaded -= OnCharacterLevelDataLoaded_InitUI;


            LevelEditorManager.Instance.OnCharacterLevelDataAdded -= OnCharacterLevelDataAdded_UpdateUI;
            LevelEditorManager.Instance.OnCharacterLevelDataRemoval -= OnCharacterLevelDataRemoval_UpdateUI;

            LevelEditorManager.Instance.OnLevelDataSaved -= OnLevelDataSaved_UpdateUI;
        }
    
        public void DisplayCanvas(bool enable)
        {
            this._canvas.enabled = enable;
        }

        private void OnFileNameChanged_UpdateUI(string fileName)
        {
            _idText.text = fileName;
        }

        private void OnCharacterLevelDataLoaded_InitUI(CharacterLevelData data)
        {   
            if(_uiSlots != null)
            {
                for(int i = 0; i < _uiSlots.Count; i++)
                {
                    Destroy(_uiSlots[i].gameObject);
                }
                _uiSlots = null;
            }


            _uiSlots = new(data.Levels.Count);
            for (int i = 0; i < data.Levels.Count; i++)
            {
                UILevelSelectorSlot newSlot = Instantiate(_uiLevelSelectorSlotPrefab, _contentParent);

                LevelData levelData = data.Levels[i];
                newSlot.SetData(levelData, i);
                Texture2D iconTexture = LevelPreviewManager.Instance.GetLevelTexture(levelData);
                newSlot.SetIcon(Utilities.ConvertTextureToSprite(iconTexture));

                _uiSlots.Add(newSlot);
            }

            DisplayCanvas(true);
        }


        private void OnCharacterLevelDataAdded_UpdateUI(int index)
        {
            UILevelSelectorSlot newSlot = Instantiate(_uiLevelSelectorSlotPrefab, _contentParent);
            newSlot.SetData(LevelEditorManager.Instance.CharacterLevelData.Levels[index], index);
            _uiSlots.Insert(index, newSlot);
            for (int i = index + 1; i < LevelEditorManager.Instance.CharacterLevelData.Levels.Count; i++)
            {
                LevelData levelData = LevelEditorManager.Instance.CharacterLevelData.Levels[i];
                _uiSlots[i].SetData(levelData, i);
            }

            UpdateGameObjectSlotsOrders();
        }

        private void OnCharacterLevelDataRemoval_UpdateUI(int index)
        {
            Destroy(_uiSlots[index].gameObject);
            _uiSlots.RemoveAt(index);
            for(int i = index; i < LevelEditorManager.Instance.CharacterLevelData.Levels.Count; i++)
            {
                LevelData levelData = LevelEditorManager.Instance.CharacterLevelData.Levels[i];
                _uiSlots[i].SetData(levelData, i);
            }

            UpdateGameObjectSlotsOrders();
        }

        private void UpdateGameObjectSlotsOrders()
        {
            for (int i = 0; i < _uiSlots.Count; i++)
            {
                _uiSlots[i].transform.SetSiblingIndex(i);
            }
        }

        private void OnLevelDataSaved_UpdateUI(int index)
        {
            LevelData levelData = LevelEditorManager.Instance.CharacterLevelData.Levels[index];
            Texture2D iconTexture = LevelPreviewManager.Instance.GetLevelTexture(levelData);
            _uiSlots[index].SetIcon(Utilities.ConvertTextureToSprite(iconTexture));
        }
    }
}
