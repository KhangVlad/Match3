using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Match3.LevelEditor
{
    public class UITileInventory : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _closeBtn;
        [SerializeField] private Button _selectBtn;

        [SerializeField] private UITileSlot _uiTileSlotPrefab;

       

        private void Start()
        {
            _closeBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
            });

            _selectBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
            });
        }

        private void OnDestroy()
        {
            _closeBtn.onClick.RemoveAllListeners();
            _selectBtn.onClick.RemoveAllListeners();
        }
    }
}
