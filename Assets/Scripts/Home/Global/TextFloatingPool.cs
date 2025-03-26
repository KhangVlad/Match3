using System.Collections.Generic;
using UnityEngine;

public class TextFloatingPool : MonoBehaviour
{
    public static TextFloatingPool Instance { get; private set; }
    public TextFloating textPrefab;
    public int initialPoolSize = 5;

    private Queue<TextFloating> textPool = new Queue<TextFloating>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializePool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            TextFloating textInstance = Instantiate(textPrefab);
            textInstance.gameObject.SetActive(false);
            textPool.Enqueue(textInstance);
        }
    }

    public TextFloating GetFromPool(Canvas canvas)
    {
        TextFloating textInstance;
        if (textPool.Count > 0)
        {
            textInstance = textPool.Dequeue();
            textInstance.transform.SetParent(canvas.transform, false);
            textInstance.gameObject.SetActive(true);
        }
        else
        {
            textInstance = Instantiate(textPrefab, canvas.transform);
        }

        return textInstance;
    }

    public void ReturnToPool(TextFloating textInstance)
    {
        textInstance.Reset();
        textInstance.gameObject.SetActive(false);
        textPool.Enqueue(textInstance);
    }
}