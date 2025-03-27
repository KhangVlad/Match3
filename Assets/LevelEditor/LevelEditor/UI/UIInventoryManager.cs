using UnityEngine;

namespace Match3.LevelEditor
{
    public class UIInventoryManager : MonoBehaviour
    {
        public static UIInventoryManager Instance { get; private set; } 
        private Canvas _canvas;

        [SerializeField] private UITileInventory _tileInventory;

        public bool InventoryBeingDisplayed => _tileInventory.gameObject.activeInHierarchy;

        private void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;

            _canvas = GetComponent<Canvas>();
            _tileInventory.gameObject.SetActive(true);
        }


        public void DisplayCanvas(bool enable)
        {
            this._canvas.enabled = enable;
        }

        public void DisplayTileInventory(bool enable)
        {
            _tileInventory.gameObject.SetActive(enable);
        }
    }
}
