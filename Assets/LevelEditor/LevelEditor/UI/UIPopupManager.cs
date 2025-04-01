using UnityEngine;

namespace Match3.LevelEditor
{
    public class UIPopupManager : MonoBehaviour
    {
        public static UIPopupManager Instance { get; private set; }

        [SerializeField] private UINameInfoPopup _uiNameInfoPopup;
        [SerializeField] private UICopyPaste _uiCopyPaste;



        public UICopyPaste UICopyPaste => _uiCopyPaste;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;

            _uiCopyPaste.gameObject.SetActive(true);
        }


        public void DisplayUINameInfoPopup(bool enable, Vector2 offset = default)
        {
            if(enable)
            {
                _uiNameInfoPopup.UpdatePosition(offset);
            }
            _uiNameInfoPopup.gameObject.SetActive(enable);
        }
        public void SetNameInfoPopupContent(string content)
        {
            _uiNameInfoPopup.SetText(content);
        }

        public void DisplayUICopyPaste(bool enable)
        {
            _uiCopyPaste.gameObject.SetActive(enable);
        }

    }
}
