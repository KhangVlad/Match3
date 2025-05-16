using UnityEngine;

namespace Match3
{
    public class UIHomeManager : MonoBehaviour
    {
        public static UIHomeManager Instance { get; private set; }

        public UILevelInfomation UILevelInfomation { get; private set; }
        // public UIShop UIShop { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;

            UILevelInfomation = FindFirstObjectByType<UILevelInfomation>();
            // UIShop = FindFirstObjectByType<UIShop>();
        }


        private void Start()
        {
            CloseAll();
        
        }

        public void CloseAll()
        {
            DisplayUILevelInfomation(false);
            DisplayUIShop(false);
        }

        public void DisplayUILevelInfomation(bool enable)
        {
            this.UILevelInfomation.DisplayCanvas(enable);
        }

        public void DisplayUIShop(bool enable)
        {
            // this.UIShop.ActiveCanvas(enable);
        }
    }

}
