using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class SpinItem : MonoBehaviour
{
    private float durationPopup = 1f;
    public Image i;
    
    private Vector3 defaultScale;
    
    private void Awake()
    {
        // Store the default scale when the object is created
        defaultScale = transform.localScale;
    }
    
    public void InitializeItem(Sprite sprite)
    {
        i.sprite = sprite;
    }

    public void Pick()
    {
        // Call the animation when the item is picked
        StartCoroutine(DoScaleAnimation());
    }
    
    private IEnumerator DoScaleAnimation()
    {
        transform.DOScale(defaultScale * 2f, durationPopup/2)
            .SetEase(Ease.OutQuad);
        yield return new WaitForSeconds(durationPopup);
        transform.DOScale(defaultScale, durationPopup/2)
            .SetEase(Ease.InQuad);
        yield return new WaitForSeconds(durationPopup/2);
    }
}