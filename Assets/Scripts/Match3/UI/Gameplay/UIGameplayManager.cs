using UnityEngine;

namespace Match3
{
    public class UIGameplayManager : MonoBehaviour
    {
        public static UIGameplayManager Instance { get; private set; }

        public UIGameplay UIGameplay { get; private set; }
        public UISettings UISettings { get; private set; }
        public UIWin UIWin { get; private set; }    
        public UIGameover UIGameover { get; private set; }    
        public UIGameplayBoosterManager UIGameplayBoosterManager { get; private set; }  



        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;

            UIGameplay = FindFirstObjectByType<UIGameplay>();
            UISettings = FindFirstObjectByType<UISettings>();
            UIWin = FindFirstObjectByType<UIWin>();
            UIGameover = FindFirstObjectByType<UIGameover>();
            UIGameplayBoosterManager = FindFirstObjectByType<UIGameplayBoosterManager>();
        }

        private void Start()
        {

            CloseAll();
            DisplayUIGameplay(true);
        }

        public void CloseAll()
        {
            DisplayUIGameplay(false);
            DisplayUISettings(false);
            DisplayUIWin(false);
            DisplayUIGameover(false);
            DisplayUIGameplayBoosterManager(false);
        }


        public void DisplayUIGameplay(bool enable)
        {
            UIGameplay.DisplayCanvas(enable);
        }

        public void DisplayUISettings(bool enable)
        {
            UISettings.DisplayCanvas(enable);
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
    }
}
