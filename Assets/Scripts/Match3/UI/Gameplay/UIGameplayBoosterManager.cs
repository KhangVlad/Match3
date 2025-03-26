using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Match3
{
    public class UIGameplayBoosterManager : MonoBehaviour
    {
        public static event System.Action<bool> OnUIGameplayBoosterManagerDisplay;

        private Canvas _canvas;

        [SerializeField] private Image _boosterIcon;
        [SerializeField] private TextMeshProUGUI _boosterDescription;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }


        private void Start()
        {
            UserManager.Instance.OnSelectGameplayBooster += OnSelectGameplayBooster_UpdateContext;
            UserManager.Instance.OnUnselectGameplayBooster += OnUnselectGameplayBooster_UpdateContext;
        }


        private void OnDestroy()
        {
            UserManager.Instance.OnSelectGameplayBooster -= OnSelectGameplayBooster_UpdateContext;
            UserManager.Instance.OnUnselectGameplayBooster -= OnUnselectGameplayBooster_UpdateContext;
        }

        public void DisplayCanvas(bool enable)
        {
            this._canvas.enabled = enable;
            OnUIGameplayBoosterManagerDisplay?.Invoke(enable);
        }

        private void OnSelectGameplayBooster_UpdateContext(Booster booster)
        {
            BoosterDataSo boosterData = GameDataManager.Instance.GetBoosterDataByID(booster.BoosterID);
            _boosterIcon.sprite = boosterData.Icon;
            _boosterIcon.SetNativeSize();
            _boosterIcon.rectTransform.ScaleIcon(200, 200);

            _boosterDescription.text = boosterData.Description;
        }

        private void OnUnselectGameplayBooster_UpdateContext()
        {
            DisplayCanvas(false);
        }

    }
}
