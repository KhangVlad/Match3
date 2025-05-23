using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Match3.Shares;

namespace Match3
{
    public class UISettingsGamePlay : MonoBehaviour
    {
        private Canvas _canvas;

        [SerializeField] private Button _closeBtn;
        [SerializeField] private Button _homeBtn;
        [SerializeField] private Button _replayBtn;
            
        public event Action OnHomeBtnClick;
        public event Action OnReplayBtnClick;

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
              LevelManager.Instance.BackToTownAdditive();
            });

            _replayBtn.onClick.AddListener(() =>
            {
               LevelManager.Instance.ReloadScene();
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
   
