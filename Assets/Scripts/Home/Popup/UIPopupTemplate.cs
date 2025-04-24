using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIPopupTemplate : MonoBehaviour
{
    public TextMeshProUGUI text;
    
    public void SetText(string message)
    {
        if (text != null)
        {
            text.text = message;
        }
    }

}