
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if !UNITY_WEBGL
public class UIStatusBarManager : MonoBehaviour
{
    private Canvas _canvas;
    [SerializeField] private Slider energySlider;
    [SerializeField] private TextMeshProUGUI energyText; // {current energy}/{max}
    [SerializeField] private int maxEnergy = 100; // Reference to max energy value
    [SerializeField] private TextMeshProUGUI goldText;
    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
    }

    private void Start()
    {
        UserManager.Instance.OnEnergyChanged += UpdateEnergyUI;
        UserManager.Instance.OnGoldChanged += UpdateGoldUI;
        UserManager.Instance.OnUserDataLoaded += InitializeEnergyAndGoldUI;
        InitializeEnergyAndGoldUI();
    }


    private void OnDestroy()
    {
        UserManager.Instance.OnEnergyChanged -= UpdateEnergyUI;
        UserManager.Instance.OnGoldChanged -= UpdateGoldUI;
        UserManager.Instance.OnUserDataLoaded -= InitializeEnergyAndGoldUI;
    }
    
    
    private void InitializeEnergyAndGoldUI()
    {
        if (UserManager.Instance != null && UserManager.Instance.UserData != null)
        {
            energySlider.maxValue = maxEnergy;
            UpdateEnergyUI(UserManager.Instance.UserData.Energy);
            UpdateGoldUI(UserManager.Instance.UserData.Gold);
        }
    }

    private void UpdateGoldUI(float gold)
    {
        goldText.text = gold.ToString();
    }
    
    private void UpdateEnergyUI(int currentEnergy)
    {
        energySlider.value = currentEnergy;
        energyText.text = $"{currentEnergy}/{maxEnergy}";
    }

    public void ActiveCanvas(bool active)
    {
        _canvas.enabled = active;
    }
}
#endif