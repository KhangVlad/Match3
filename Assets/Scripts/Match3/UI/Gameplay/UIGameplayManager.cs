using UnityEngine;

namespace Match3
{
    public class UIGameplayManager : MonoBehaviour
    {
        public static UIGameplayManager Instance { get; private set; }

        public UIGameplay UIGameplay { get; private set; }
        public UISettingsGamePlay UISettingsGamePlay { get; private set; }
        public UIWin UIWin { get; private set; }
        public UIGameover UIGameover { get; private set; }
        public UIGameplayBoosterManager UIGameplayBoosterManager { get; private set; }
        public UINoMorePossibleMove UINoMorePossibleMove { get; private set; }


        private void Awake()
        {
            Debug.Log("AAAAA");
            // Debug.Break();
            // if (Instance != null && Instance != this)
            // {
            //     Destroy(this.gameObject);
            //     return;
            // }
            if(Instance == null)
            {
                Instance = this;
            }
            Debug.Log("BBB");
            Instance = this;

            UIGameplay = FindFirstObjectByType<UIGameplay>();
            UISettingsGamePlay = FindFirstObjectByType<UISettingsGamePlay>();
            UIWin = FindFirstObjectByType<UIWin>();
            UIGameover = FindFirstObjectByType<UIGameover>();
            UIGameplayBoosterManager = FindFirstObjectByType<UIGameplayBoosterManager>();
            UINoMorePossibleMove = FindFirstObjectByType<UINoMorePossibleMove>();
        }

        private void Start()
        {

            CloseAll();
            DisplayUIGameplay(true);
            DisplayUINoMorePossibleMove(true);
        }

        public void CloseAll()
        {
            DisplayUIGameplay(false);
            DisplayUISettings(false);
            DisplayUIWin(false);
            DisplayUIGameover(false);
            DisplayUIGameplayBoosterManager(false);
            DisplayUINoMorePossibleMove(false);
        }


        public void DisplayUIGameplay(bool enable)
        {
            UIGameplay.DisplayCanvas(enable);
        }

        public void DisplayUISettings(bool enable)
        {
            UISettingsGamePlay.DisplayCanvas(enable);
        }

        public void DisplayUIWin(bool enable)
        {
            UIWin.DisplayCanvas(enable);
        }

        public void DisplayUIGameover(bool enable)
        {
            UIGameover.DisplayCanvas(enable);
        }

        public void DisplayUIGameplayBoosterManager(bool enable)
        {
            UIGameplayBoosterManager.DisplayCanvas(enable);
        }

        public void DisplayUINoMorePossibleMove(bool enable)
        {
            UINoMorePossibleMove.DisplayCanvas(enable);
        }
    }
}
