using FunkyCode;
using UnityEngine;
using DG.Tweening;

public class LightStreet : MonoBehaviour
{
    public Light2D light2D;
    public bool IsOn = false;

    public void Toggle()
    {
        IsOn = !IsOn;
        light2D.enabled = IsOn;
    }
}