using System;
using UnityEngine;
using UnityEngine.UI;
using Match3.Shares;
using UnityEngine.SceneManagement;
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
                LevelManager.Instance.ActiveGameObject(true);
                SceneManager.UnloadSceneAsync(LevelManager.Instance.OtherScene);
            });

            _replayBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                LevelManager.Instance.ReloadScene();
            });

#if WEBGL_BUILD 
            _homeBtn.gameObject.SetActive(false);
            _replayBtn.gameObject.SetActive(false);
#endif
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.U))
            {
                Debug.Log("U");
                SceneManager.UnloadSceneAsync(LevelManager.Instance.OtherScene);
            }
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
