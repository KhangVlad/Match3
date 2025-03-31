using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace Match3.LevelEditor
{
    public class UILevelSelectorSlot : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private Button _addNewBtn;
        [SerializeField] private Button _eidtBtn;
        [SerializeField] private Button _removeBtn;
        [SerializeField] private Image _iconImage;

        public void OnPointerClick(PointerEventData eventData)
        {
            if(eventData.button == PointerEventData.InputButton.Right)
            {
                Debug.Log("Right click");
            }
        }
    }
}
