using DG.Tweening;
using UnityEngine;
using System.Collections;

public static class UILayoutAnimationExtensions
{

    public static IEnumerator AnimateLayoutItems(this Transform layoutTransform, LayoutAnimationSettings settings)
    {
        yield return null;

        for (int i = 0; i < layoutTransform.childCount; i++)
        {
            Transform item = layoutTransform.GetChild(i);
            if (!item.gameObject.activeSelf)
                continue;

            RectTransform rectTransform = item.GetComponent<RectTransform>();
            if (rectTransform == null)
                continue;
            Vector2 targetPosition = rectTransform.anchoredPosition;
            rectTransform.anchoredPosition = new Vector2(targetPosition.x + settings.offsetX, targetPosition.y);
            float delay = i * settings.delay;
            rectTransform.DOAnchorPos(targetPosition, settings.duration)
                .SetDelay(delay)
                .SetEase(settings.easeType);
        }
        
    }
    public static IEnumerator AnimateLayoutItemsDefault(this Transform layoutTransform)
    {
        return AnimateLayoutItems(layoutTransform, new LayoutAnimationSettings());
    }
    public static IEnumerator AnimateSingleItem(this Transform item, LayoutAnimationSettings settings, int index, Transform parent = null)
    {
        if (!item.gameObject.activeSelf)
            yield break;
        
        RectTransform rectTransform = item.GetComponent<RectTransform>();
        if (rectTransform == null)
            yield break;
        
        Vector2 targetPosition = rectTransform.anchoredPosition;
    
        rectTransform.anchoredPosition = new Vector2(targetPosition.x + settings.offsetX, targetPosition.y);
    
        if (parent != null && item.parent != parent)
        {
            item.SetParent(parent, false);
        }
    
        float delay = index * settings.delay;
        Tweener positionTween = rectTransform.DOAnchorPos(targetPosition, settings.duration)
            .SetDelay(delay)
            .SetEase(settings.easeType);
        
        yield return positionTween.WaitForCompletion();
    }
    
  
}

[System.Serializable]
public class LayoutAnimationSettings
{
    public float duration = 0.3f;
    public float delay = 0.1f;
    public float offsetX = -300f;
    public Ease easeType = Ease.OutBack;
    public bool fadeIn = true;
}
