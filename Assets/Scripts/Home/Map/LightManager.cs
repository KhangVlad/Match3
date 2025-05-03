using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightManager : MonoBehaviour
{
    public static LightManager Instance { get; private set; }
    private List<Light2D> registeredLights = new List<Light2D>();
    private Dictionary<Light2D, float> lightIntensities = new Dictionary<Light2D, float>();
    [SerializeField] private Light2D dayLight;
    [SerializeField] private Light2D moonLight;
    [SerializeField] private GameObject _nightGameobjects;
    [SerializeField] private GameObject _dayGameobjects;
    public int currentHour;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: keep across scenes
        }
        else
        {
            Destroy(gameObject);
        }
        currentHour = DateTime.Now.Hour;
    }
    
    private bool IsNight()
    {
        return currentHour >= 12 || currentHour < 6;
    }

    private void Start()
    {
        CheckCurrentTime();
    }
    private void CheckCurrentTime()
    {
        if (IsNight())
        {
            _nightGameobjects.SetActive(true);
            _dayGameobjects.SetActive(false);
            if (moonLight != null)
                moonLight.enabled = true;
            if (dayLight != null)
                dayLight.enabled = false; 
        }
        else
        {
            _nightGameobjects?.SetActive(false);
            _dayGameobjects?.SetActive(true);
        
            // Adjust lighting for day
            if (moonLight != null)
                moonLight.enabled = false;
            if (dayLight != null)
                dayLight.enabled = true;
        }
    }
    public void RegisterLight(Light2D light)
    {
        if (light == null)
        {
            Debug.LogWarning("Cannot register null light");
            return;
        }

        if (!registeredLights.Contains(light))
        {
            registeredLights.Add(light);
            lightIntensities[light] = light.intensity;
        }
    }

    public void UnregisterLight(Light2D light)
    {
        if (light == null) return;

        if (registeredLights.Contains(light))
        {
            registeredLights.Remove(light);
            lightIntensities.Remove(light);
        }
    }

    public void ToggleAllLights(bool a)
    {
        foreach (Light2D light in registeredLights)
        {
            if (light != null)
            {
                light.enabled = a;
            }
        }
    }
    public void ToggleDayNight()
    {
        // Add null checks first
        if (_dayGameobjects == null || _nightGameobjects == null)
        {
            Debug.LogError("Day or Night GameObjects are not assigned!");
            return;
        }
    
        bool isDayActive = _dayGameobjects.activeSelf;
        if (isDayActive)
        {
            // Switch to night
            _nightGameobjects.SetActive(true);
            _dayGameobjects.SetActive(false);
        
            if (moonLight != null)
                moonLight.enabled = true;
            if (dayLight != null)
                dayLight.enabled = false;
        }
        else
        {
            // Switch to day
            _nightGameobjects.SetActive(false);
            _dayGameobjects.SetActive(true);
        
            if (moonLight != null)
                moonLight.enabled = false;
            if (dayLight != null)
                dayLight.enabled = true;
        }
    }

    public List<Light2D> GetRegisteredLights()
    {
        return new List<Light2D>(registeredLights);
    }

    // Cleanup when the manager is destroyed
    private void OnDestroy()
    {
        registeredLights.Clear();
        lightIntensities.Clear();
    }
}