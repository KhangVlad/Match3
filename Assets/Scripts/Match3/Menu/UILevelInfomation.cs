using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Match3.Shares;
using UnityEngine.SceneManagement;


namespace Match3
{
    public class UILevelInfomation : MonoBehaviour
    {
        public static UILevelInfomation Instance { get; private set; }

        private Canvas _canvas;

        [SerializeField] private Button _closeBtn;
        [SerializeField] private Button _playBtn;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _quest;
        [Header("Booster")] [SerializeField] private UIBooster _uiBoosterPrefab;
        [SerializeField] private Transform _boosterContentParent;
        [SerializeField] private UIQuest uiQuest;
        public UIBooster[] UIBoosters;
        [SerializeField] private UIQuestRequirement requirementPrefab;
        [SerializeField] private Transform requirementParent;
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
                DisplayCanvas(false);
                // AudioManager.Instance.PlayButtonSfx();
                // Loader.Load(Loader.Scene.GameplayScene);
                StartCoroutine(Loader.LoadSceneAsyncCoroutine(Loader.Scene.GameplayScene, LoadSceneMode.Additive, 0f, () =>
                {
                    // LevelEditorSceneLoader.Instance.OtherScene = SceneManager.GetSceneByName(Loader.Scene.GameplayScene.ToString());
                    // LevelEditorSceneLoader.Instance.DisplaySceneObject(false);
                    LevelManager.Instance.OtherScene =  SceneManager.GetSceneByName(Loader.Scene.GameplayScene.ToString());
                    LevelManager.Instance.ActiveGameObject(false);
                }));
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


        public void LoadLevelData(LevelDataV2 levelData, int level)
        {
            _levelText.text = $"Level {level + 1}";
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


        public void SetQuest(int[,] quest)
        {
            // Clear existing requirements
            foreach (Transform child in requirementParent)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < quest.GetLength(0); i++)
            {
                int id = quest[i, 0];
                int quantity = quest[i, 1];
                QuestDataSO questData = GameDataManager.Instance.GetQuestDataByID((QuestID)id);
                Sprite iconSprite = questData.Icon;
                UIQuestRequirement req = Instantiate(requirementPrefab, requirementParent);
                req.Initialize(iconSprite, quantity);
            }
        }

    }
}