using UnityEngine;
using UnityEngine.UI;
using Match3.Shares;

namespace Match3
{
    public class UIGameover : MonoBehaviour
    {
        private Canvas _canvas;

        [SerializeField] private Button _homeBtn;
        [SerializeField] private Button _replayBtn;


        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        private void Start()
        {
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

#if WEBGL_BUILD
            _homeBtn.gameObject.SetActive(false);
            _replayBtn.gameObject.SetActive(false);
#endif
        }

        private void OnDestroy()
        {
            _homeBtn.onClick.RemoveAllListeners();
            _replayBtn.onClick.RemoveAllListeners();
        }


        public void DisplayCanvas(bool enable)
        {
            this._canvas.enabled = enable;
        }

    }
}
