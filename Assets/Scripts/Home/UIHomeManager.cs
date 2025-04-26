using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHomeManager : MonoBehaviour
{
    private Canvas _canvas;
    [SerializeField] private Button _dailyGiftbtn;
    [SerializeField] private Slider energySlider;
    [SerializeField] private TextMeshProUGUI energyText; // {current energy}/{max}
    [SerializeField] private int maxEnergy = 100; // Reference to max energy value
    
    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
    }

    private void Start()
    {
        _dailyGiftbtn.onClick.AddListener(() => { TownCanvasController.Instance.ActiveDailyGift(true); });
        UserManager.Instance.OnEnergyChanged += UpdateEnergyUI;
        UserManager.Instance.OnUserDataLoaded += InitializeEnergyUI;
        InitializeEnergyUI();
    }


    private void OnDestroy()
    {
        _dailyGiftbtn.onClick.RemoveAllListeners();
        
        UserManager.Instance.OnEnergyChanged -= UpdateEnergyUI;
        UserManager.Instance.OnUserDataLoaded -= InitializeEnergyUI;
    }
    
    private void InitializeEnergyUI()
    {
        if (UserManager.Instance != null && UserManager.Instance.UserData != null)
        {
            // Set max value for slider
            energySlider.maxValue = maxEnergy;
            
            // Update UI with current energy
            UpdateEnergyUI(UserManager.Instance.UserData.Energy);
        }
    }
    
    private void UpdateEnergyUI(int currentEnergy)
    {
        // Update slider value
        energySlider.value = currentEnergy;
        
        // Update text display
        energyText.text = $"{currentEnergy}/{maxEnergy}";
    }

    public void ActiveCanvas(bool active)
    {
        _canvas.enabled = active;
    }
}