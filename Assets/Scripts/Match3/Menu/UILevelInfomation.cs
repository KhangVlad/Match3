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

            GameplayUserManager.Instance.OnAvaiableBoostersQuantityChanged += OnAvaiableBoostersQuantityChanged_UpdateUI;
        }
   

        private void OnDestroy()
        {
            _closeBtn.onClick.RemoveAllListeners();
            _playBtn.onClick.RemoveAllListeners();

            GameplayUserManager.Instance.OnAvaiableBoostersQuantityChanged -= OnAvaiableBoostersQuantityChanged_UpdateUI;
        }


        public void LoadLevelData(LevelDataV1 levelData, int level)
        {
            _levelText.text = $"Level {level}";
        }


        public void DisplayCanvas(bool enable)
        {
            this._canvas.enabled = enable;
        }

        private void LoadUIBoosters()
        {
            UIBoosters = new UIBooster[GameplayUserManager.Instance.AvaiableBoosters.Count];
            for (int i = 0; i < GameplayUserManager.Instance.AvaiableBoosters.Count; i++)
            {
                Booster booster = GameplayUserManager.Instance.AvaiableBoosters[i];
                UIBooster uibooster = Instantiate(_uiBoosterPrefab, _boosterContentParent);
                uibooster.SetBoosterData(booster);

                UIBoosters[i] = uibooster;
            }
        }

        private void OnAvaiableBoostersQuantityChanged_UpdateUI(Booster booster)
        {
            for (int i = 0; i < GameplayUserManager.Instance.AvaiableBoosters.Count; i++)
            {
                if (UIBoosters[i].CachedBooster.BoosterID == booster.BoosterID)
                {
                    UIBoosters[i].SetBoosterData(booster);
                }         
            }
        }
    }
}