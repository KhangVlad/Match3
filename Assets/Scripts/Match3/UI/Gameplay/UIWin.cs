using UnityEngine;
using UnityEngine.UI;

namespace Match3
{
    public class UIWin : MonoBehaviour
    {
        private Canvas _canvas;

        [SerializeField] private Button _homeBtn;
        [SerializeField] private Button _replayBtn;
        [SerializeField] private Button _nextBtn;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        private void Start()
        {
            _homeBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();

                Loader.Load(Loader.Scene.MenuScene);
            });

            _replayBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();

                Loader.Load(Loader.Scene.GameplayScene);
            });

            _nextBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();

                int nextLevel = LevelManager.Instance.NextLevel();
                LevelManager.Instance.LoadLevelData(nextLevel);
                Loader.Load(Loader.Scene.GameplayScene);
            });
        }

        private void OnDestroy()
        {
            _homeBtn.onClick.RemoveAllListeners();
            _replayBtn.onClick.RemoveAllListeners();
            _nextBtn.onClick.RemoveAllListeners();
        }

        public void DisplayCanvas(bool enable)
        {
            this._canvas.enabled = enable;
        }
    }
}
