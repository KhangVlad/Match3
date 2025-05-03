using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering.Universal;

public class LightStreet : MonoBehaviour
{
    public Light2D light2D;
    private bool IsOn = false; // Start with lights on by default
    
    public float toggleDuration = 0.3f;
    public Ease toggleEase = Ease.OutQuad;

    private float originalIntensity;
    private bool isTransitioning = false;
    

    public void Toggle()
    {
        if (isTransitioning) return; 
        IsOn = !IsOn;
        AnimateToggle();
    }

    public void TurnOn(bool animated = true)
    {
        if (IsOn || isTransitioning) return;
        
        IsOn = true;
        if (animated)
            AnimateToggle();
        else
            light2D.enabled = true;
    }

    public void TurnOff(bool animated = true)
    {
        if (!IsOn || isTransitioning) return;
        
        IsOn = false;
        if (animated)
            AnimateToggle();
        else
            light2D.enabled = false;
    }

    private void AnimateToggle()
    {
        if (light2D == null) return;
        
        isTransitioning = true;
        
        if (IsOn)
        {
            // Turning on: enable first, then fade in
            light2D.enabled = true;
            light2D.intensity = 0f;
            light2D.DOIntensity(originalIntensity, toggleDuration)
                .SetEase(toggleEase)
                .OnComplete(() => isTransitioning = false);
        }
        else
        {
            // Turning off: fade out, then disable
            light2D.DOIntensity(0f, toggleDuration)
                .SetEase(toggleEase)
                .OnComplete(() => 
                {
                    light2D.enabled = false;
                    light2D.intensity = originalIntensity; // Reset for next use
                    isTransitioning = false;
                });
        }
    }

    private void Start()
    {
        if (light2D != null)
        {
            originalIntensity = light2D.intensity;
            light2D.enabled = IsOn;
        }
        else
        {
            Debug.LogError($"Light2D is not assigned in {gameObject.name}!");
        }
        RegisterWithManager();
    }

    private void RegisterWithManager()
    {
        if (LightManager.Instance != null && light2D != null)
        {
            LightManager.Instance.RegisterLight(light2D);
        }
       
    }

    private void OnDestroy()
    {
        if (LightManager.Instance != null && light2D != null)
        {
            LightManager.Instance.UnregisterLight(light2D);
        }
    }

}