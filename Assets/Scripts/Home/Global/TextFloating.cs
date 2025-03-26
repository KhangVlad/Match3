using TMPro;
using UnityEngine;
using DG.Tweening;

public class TextFloating : MonoBehaviour
{
    public TextMeshProUGUI text;
    private float floatDuration = 0.4f;
    private Vector3 floatOffset = new Vector3(0, 5, 0); // Only move up on the y-axis

    private Vector3 initialPosition;

    private void OnEnable()
    {
        initialPosition = transform.position;
        AnimateFloating();
    }

    private void AnimateFloating()
    {
        transform.DOMoveY(initialPosition.y + floatOffset.y, floatDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() => TextFloatingPool.Instance.ReturnToPool(this));
    }

    public void SetText(string message, float size)
    {
        text.text = message;
        text.fontSize = size;
    }

    public void Reset()
    {
        text.text = "";
        transform.position = initialPosition;
    }
}