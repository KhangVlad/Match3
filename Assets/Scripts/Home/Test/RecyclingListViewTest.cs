using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecyclingListViewTest : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contentPanel;
    [SerializeField] private UITest itemPrefab;
    [SerializeField] private int itemCount = 1000; // Total number of items
    
    // Function to set item count dynamically at runtime
    public void SetItemCount(int count)
    {
        itemCount = count;
        if (contentPanel != null)
        {
            // Update content panel height to accommodate the new item count
            contentPanel.sizeDelta = new Vector2(contentPanel.sizeDelta.x, itemHeight * itemCount);
            // Force update of visible items
            currentTopItemIndex = -1;
            UpdateItems();
        }
    }
    
    private readonly List<RectTransform> pooledItems = new List<RectTransform>();
    private float itemHeight;
    private int itemsPerScreen;
    private int currentTopItemIndex = -1; // Index of the first visible item
    private int poolSize;
    
    void Start()
    {
        // Calculate item height from the prefab
        var itemRectTransform = itemPrefab.GetComponent<RectTransform>();
        itemHeight = itemRectTransform.rect.height;
        
        // Determine how many items fit on screen (plus buffer)
        var viewportHeight = scrollRect.viewport.rect.height;
        itemsPerScreen = Mathf.CeilToInt(viewportHeight / itemHeight) + 2; // +2 as buffer
        
        poolSize = itemsPerScreen + 4;
        contentPanel.sizeDelta = new Vector2(contentPanel.sizeDelta.x, itemHeight * itemCount);
        
        CreateItemPool();
        
        scrollRect.onValueChanged.AddListener(OnScrollChanged);
        
        UpdateItems();
    }
    
    private void CreateItemPool()
    {
        // Clear existing items
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }
        pooledItems.Clear();
        
        for (int i = 0; i < poolSize; i++)
        {
            UITest item = Instantiate(itemPrefab, contentPanel);
            item.SetText(i);
            RectTransform rt = item.GetComponent<RectTransform>();
            rt.gameObject.SetActive(false);
            pooledItems.Add(rt);
        }
    }
    
    private void OnScrollChanged(Vector2 normalizedPosition)
    {
        UpdateItems();
    }
    
    private void UpdateItems()
    {
        // Calculate the first visible item index
        float scrollPos = contentPanel.anchoredPosition.y;
        int newTopItemIndex = Mathf.FloorToInt(scrollPos / itemHeight);
        
        // Clamp to valid range
        newTopItemIndex = Mathf.Clamp(newTopItemIndex, 0, itemCount - 1);
        
        // Skip update if the position hasn't changed enough
        if (newTopItemIndex == currentTopItemIndex)
            return;
            
        currentTopItemIndex = newTopItemIndex;
        
        for (int i = 0; i < pooledItems.Count; i++)
        {
            int itemIndex = currentTopItemIndex + i;
            RectTransform item = pooledItems[i];
            
            if (itemIndex >= 0 && itemIndex < itemCount)
            {
                item.anchoredPosition = new Vector2(0, -itemIndex * itemHeight);
                item.gameObject.SetActive(true);
            }
            else
            {
                item.gameObject.SetActive(false);
            }
        }
    }
 
    // Optional: Create a method to refresh all visible items (useful when data changes)
    public void RefreshItems()
    {
        currentTopItemIndex = -1; // Force full refresh
        UpdateItems();
    }
}