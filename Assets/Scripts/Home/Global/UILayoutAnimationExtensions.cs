using DG.Tweening;
using UnityEngine;
using System.Collections;

public static class UILayoutAnimationExtensions
{

    public static IEnumerator AnimateLayoutItems(this Transform layoutTransform, LayoutAnimationSettings settings)
    {
        yield return null;

        // Animate each child item
        for (int i = 0; i < layoutTransform.childCount; i++)
        {
            Transform item = layoutTransform.GetChild(i);

            // Skip inactive items
            if (!item.gameObject.activeSelf)
                continue;

            RectTransform rectTransform = item.GetComponent<RectTransform>();
            if (rectTransform == null)
                continue;

            Vector2 targetPosition = rectTransform.anchoredPosition;

            // Set initial position (off-screen to the left)
            rectTransform.anchoredPosition = new Vector2(targetPosition.x + settings.offsetX, targetPosition.y);

            // Animate to final position with delay based on index
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
        
        // Store the final position that was set by the layout group
        Vector2 targetPosition = rectTransform.anchoredPosition;
    
        // Set initial position (off-screen to the left)
        rectTransform.anchoredPosition = new Vector2(targetPosition.x + settings.offsetX, targetPosition.y);
    
        // If parent is provided, add the item to it
        if (parent != null && item.parent != parent)
        {
            item.SetParent(parent, false); // false to keep world position
        }
    
        // Animate to final position with delay based on index
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
