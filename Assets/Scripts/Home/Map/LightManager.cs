using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightManager : MonoBehaviour
{
    public static LightManager Instance { get; private set; }
    private List<Light2D> registeredLights = new List<Light2D>();
    private Dictionary<Light2D, float> lightIntensities = new Dictionary<Light2D, float>();
    [SerializeField] private GameObject _nightGameobjects;
    [SerializeField] private GameObject _dayGameobjects;
    [SerializeField] private Light2D enviromentLight;
    public int currentHour;
    public Color nightColor = new Color(); 
    public Color dayColor = new Color(); 
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        currentHour = DateTime.Now.Hour;
    }

    private bool IsNight()
    {
        return currentHour >= 9;
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
            if (enviromentLight != null)
                enviromentLight.color = nightColor;
        }
        else
        {
            _nightGameobjects?.SetActive(false);
            _dayGameobjects?.SetActive(true);
            if (enviromentLight != null)
                enviromentLight.color = dayColor;
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

    [ContextMenu("ToggleDayNight")]
    public void ToggleDayNight()
    {
        // Add null checks first
        if (_dayGameobjects == null || _nightGameobjects == null || enviromentLight == null)
        {
            Debug.LogError("Day/Night GameObjects or Environment Light are not assigned!");
            return;
        }

        bool isDayActive = _dayGameobjects.activeSelf;
        if (isDayActive)
        {
            // Switch to night
            _nightGameobjects.SetActive(true);
            _dayGameobjects.SetActive(false);
            enviromentLight.color = nightColor; // Set environment light to night color
        }
        else
        {
            // Switch to day
            _nightGameobjects.SetActive(false);
            _dayGameobjects.SetActive(true);
            enviromentLight.color = dayColor; // Set environment light to day color
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