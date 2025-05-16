using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using Match3;
using TMPro;

public class SpinItem : MonoBehaviour
{
    private float durationPopup = 1f;
    public Image i;
    private DailyGiftSO data;
    [SerializeField] private TextMeshProUGUI quantity;
    private int Quantity;
    private Vector3 defaultScale;

    private void Awake()
    {
        defaultScale = transform.localScale;
    }

    private void Start()
    {
    }

    public void SetQuantity(int amount)
    {
        Quantity += amount;
        quantity.text = Quantity.ToString();
    }

    public void InitializeItem(DailyGiftSO d)
    {
        this.data = d;
        i.sprite = d.sprite;
        quantity.text = d.quantity.ToString();
        Quantity = d.quantity;
    }


    public void Pick()
    {
        StartCoroutine(DoScaleAnimation());
    }

    private IEnumerator DoScaleAnimation()
    {
        transform.DOScale(defaultScale * 2f, durationPopup / 2)
            .SetEase(Ease.OutQuad);
        yield return new WaitForSeconds(durationPopup);
        transform.DOScale(defaultScale, durationPopup / 2)
            .SetEase(Ease.InQuad);
        yield return new WaitForSeconds(durationPopup / 2);
    }
}