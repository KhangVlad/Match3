using UnityEngine;

namespace Match3.LevelEditor
{
    public class UIInventoryManager : MonoBehaviour
    {
        public static UIInventoryManager Instance { get; private set; } 
        private Canvas _canvas;

        [SerializeField] private UITileInventory _uiTileInventory;
        [SerializeField] private UIBlockInventory _uiBlockInventory;
        [SerializeField] private UIQuestInventory _uiQuestInventory;
        [SerializeField] private UICharacterInventory _uiCharacterInventory;

        public bool InventoryBeingDisplayed => _uiTileInventory.gameObject.activeInHierarchy;

        private void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;

            _canvas = GetComponent<Canvas>();
            _uiTileInventory.gameObject.SetActive(true);
            _uiBlockInventory.gameObject.SetActive(true);
            _uiQuestInventory.gameObject.SetActive(true);
            _uiCharacterInventory.gameObject.SetActive(true);
        }


        public void DisplayCanvas(bool enable)
        {
            this._canvas.enabled = enable;
        }

        public void DisplayTileInventory(bool enable)
        {
            _uiTileInventory.gameObject.SetActive(enable);
        }

        public void DisplayBlockInventory(bool enable)
        {
            _uiBlockInventory.gameObject.SetActive(enable);
        }

        public void DisplayQuestInventory(bool enable)
        {
            _uiQuestInventory.gameObject.SetActive(enable);
        }

        public void DisplayCharacterInventory(bool enable)
        {
            _uiCharacterInventory.gameObject.SetActive(enable);
        }
    }
}
