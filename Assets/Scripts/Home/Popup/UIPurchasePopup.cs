#if !UNITY_WEBGL
    using System;
    using Match3;
    using UnityEngine;
    using UnityEngine.UI;

    public class UIPurchasePopup : MonoBehaviour
    {
        [SerializeField] private Button closePopup;
        public UIShopPack UIShopPack;

        private void Start()
        {
            closePopup.onClick.AddListener(ClosePopup);
        }


        private void OnDestroy()
        {
            closePopup.onClick.RemoveAllListeners();
        }

        private void ClosePopup()
        {
            Destroy(this.gameObject);
        }


        public void Initialize(ShopItemPack d)
        {
            if(d == null)return;
            UIShopPack.SetPackData(d);
        }
        
    }
#endif