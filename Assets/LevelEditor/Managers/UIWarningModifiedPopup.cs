using UnityEngine;
using UnityEngine.UI;


namespace Match3.LevelEditor
{
    public class UIWarningModifiedPopup : MonoBehaviour
    {
        [SerializeField] private Button _saveBtn;

        [SerializeField] private Button _dontSaveBtn;

        [SerializeField] private Button _cancelBtn;


        public void OnSaveBtnClick(UnityEngine.Events.UnityAction callback)
        {
            _saveBtn.onClick.RemoveAllListeners();
            _saveBtn.onClick.AddListener(callback);
        }

        public void OnDontSaveBtnClick(UnityEngine.Events.UnityAction callback)
        {
            _dontSaveBtn.onClick.RemoveAllListeners();
            _dontSaveBtn.onClick.AddListener(callback);
        }

        public void OnCancelBtnClick(UnityEngine.Events.UnityAction callback)
        {
            _cancelBtn.onClick.RemoveAllListeners();
            _cancelBtn.onClick.AddListener(callback);
        }
    }
}
