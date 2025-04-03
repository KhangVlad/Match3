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
        //doscale light2D then turn back to normal
        transform.DOScale(1.2f, 0.2f).OnComplete(() => { transform.DOScale(1f, 0.2f); });
    }
}