using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Match3
{
    public class UISettings : MonoBehaviour
    {
        private Canvas _canvas;

        [SerializeField] private Button _closeBtn;
        [SerializeField] private Button _homeBtn;
        [SerializeField] private Button _replayBtn;


        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        private void Start()
        {
            _closeBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();

                DisplayCanvas(false);
            });

            _homeBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();

                Loader.Load(Loader.Scene.Town);
            });

            _replayBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();

                Loader.Load(Loader.Scene.GameplayScene);
            });
        }

        private void OnDestroy()
        {
            _closeBtn.onClick.RemoveAllListeners();
            _homeBtn.onClick.RemoveAllListeners();
            _replayBtn.onClick.RemoveAllListeners();
        }


        public void DisplayCanvas(bool enable)
        {
            this._canvas.enabled = enable;
        }
    }
}
