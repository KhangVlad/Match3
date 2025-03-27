using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Match3
{
    public class UILevelInfomation : MonoBehaviour
    {
        public static UILevelInfomation Instance { get; private set; }

        private Canvas _canvas;

        [SerializeField] private Button _closeBtn;
        [SerializeField] private Button _playBtn;
        [SerializeField] private TextMeshProUGUI _levelText;

        [Header("Booster")] [SerializeField] private UIBooster _uiBoosterPrefab;
        [SerializeField] private Transform _boosterContentParent;
        public UIBooster[] UIBoosters;
        public event Action<Loader.Scene> OnSceneSwitch;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            Instance = this;

            _canvas = GetComponent<Canvas>();
        }


        private void Start()
        {
            _closeBtn.onClick.AddListener(() =>
            {
                Debug.Log("Close");
                AudioManager.Instance.PlayButtonSfx();
                DisplayCanvas(false);
            });

            _playBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();
                Loader.Load(Loader.Scene.GameplayScene);
            });

            LoadUIBoosters();
        }

        private void OnDestroy()
        {
            _closeBtn.onClick.RemoveAllListeners();
            _playBtn.onClick.RemoveAllListeners();
        }


        public void LoadLevelData(LevelData levelData, int level)
        {
            _levelText.text = $"Level {level}";
        }


        public void DisplayCanvas(bool enable)
        {
            this._canvas.enabled = enable;
        }

        private void LoadUIBoosters()
        {
            for (int i = 0; i < UserManager.Instance.AvaiableBoosters.Count; i++)
            {
                Booster booster = UserManager.Instance.AvaiableBoosters[i];
                UIBooster uibooster = Instantiate(_uiBoosterPrefab, _boosterContentParent);
                uibooster.SetBoosterData(booster);
            }
        }
    }
}