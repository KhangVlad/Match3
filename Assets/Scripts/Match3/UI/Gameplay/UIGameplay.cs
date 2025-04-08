using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

namespace Match3
{
    public class UIGameplay : MonoBehaviour
    {
        private Canvas _canvas;

        [SerializeField] private TextMeshProUGUI _turnCountText;

        [Header("Buttons")]
        [SerializeField] private Button _settingsBtn;

        [Header("Progress")]
        [SerializeField] private Slider _progressSlider;
        [SerializeField] private Image _start1;
        [SerializeField] private Image _start2;
        [SerializeField] private Image _start3;
        [SerializeField] private Sprite _activeSprite;
        [SerializeField] private Sprite _unactiveSprite;


        [Header("Boosters")]
        [SerializeField] private UIGameplayBooster _uiGameplayBoosterPrefab;
        [SerializeField] private Transform _boosterContentParent;


        [Header("Avatars")]
        [SerializeField] private Image _characterAvatar;


        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        private void Start()
        {
            _characterAvatar.sprite = GameDataManager.Instance.GetCharacterDataByID(LevelManager.Instance.CharacterLevelData.CharacterID).sprite;
            _turnCountText.text = GameplayManager.Instance.TurnRemainingCount.ToString();
            LoadUIBoosters();

            _settingsBtn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonSfx();

                UIGameplayManager.Instance.DisplayUISettings(true);
            });

#if WEBGL_BUILD
            _settingsBtn.gameObject.SetActive(false);
#endif

            GameplayManager.Instance.OnTurnRemaingChanged += OnTurnRemaingChanged_UpdateUI;
            UIGameplayBoosterManager.OnUIGameplayBoosterManagerDisplay += OnUIGameplayBoosterManagerDisplay_UpdateUI;

        }


        private void OnDestroy()
        {
            _settingsBtn.onClick.RemoveAllListeners();
            GameplayManager.Instance.OnTurnRemaingChanged -= OnTurnRemaingChanged_UpdateUI;
            UIGameplayBoosterManager.OnUIGameplayBoosterManagerDisplay -= OnUIGameplayBoosterManagerDisplay_UpdateUI;
        }


        public void DisplayCanvas(bool enable)
        {
            this._canvas.enabled = enable;
        }


        private void LoadUIBoosters()
        {
            for (int i = 0; i < GameplayUserManager.Instance.GameplayBoosters.Count; i++)
            {
                Booster booster = GameplayUserManager.Instance.GameplayBoosters[i];

                UIGameplayBooster uiBooster = Instantiate(_uiGameplayBoosterPrefab, _boosterContentParent);
                uiBooster.SetBoosterData(booster);
            }
        }
        private void OnTurnRemaingChanged_UpdateUI(int value)
        {
            _turnCountText.text = value.ToString();
        }

        private void OnUIGameplayBoosterManagerDisplay_UpdateUI(bool enable)
        {
            _settingsBtn.interactable = !enable;
        }
    }

}
