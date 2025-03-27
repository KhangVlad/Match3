using UnityEngine;

namespace Match3.LevelEditor
{
    public class UIPopupManager : MonoBehaviour
    {
        public static UIPopupManager Instance { get; private set; }

        [SerializeField] private UINameInfoPopup _uiNameInfoPopup;
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
        }


        public void DisplayUINameInfoPopup(bool enable)
        {
            if(enable)
            {
                _uiNameInfoPopup.UpdatePosition();
            }
            _uiNameInfoPopup.gameObject.SetActive(enable);
        }
        public void SetNameInfoPopupContent(string content)
        {
            _uiNameInfoPopup.SetText(content);
        }
    }
}
