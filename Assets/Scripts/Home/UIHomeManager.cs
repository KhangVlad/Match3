using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if !UNITY_WEBGL
public class UIHomeManager : MonoBehaviour
{
    private Canvas _canvas;
    [SerializeField] private Button _dailyGiftbtn;

    [SerializeField] private Button _shopBtn;
    // [SerializeField] private Slider energySlider;
    // [SerializeField] private TextMeshProUGUI energyText; // {current energy}/{max}
    // [SerializeField] private int maxEnergy = 100; // Reference to max energy value
    //
    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
    }

    private void Start()
    {
        _dailyGiftbtn.onClick.AddListener(() => { TownCanvasController.Instance.ActiveDailyGift(true); });
        _shopBtn.onClick.AddListener(() =>
        {
            TownCanvasController.Instance.ActiveShop(true);
        });
    }


    private void OnDestroy()
    {
        _dailyGiftbtn.onClick.RemoveAllListeners();
     
    }
    
   
    
  
    public void ActiveCanvas(bool active)
    {
        _canvas.enabled = active;
    }
}
#endif