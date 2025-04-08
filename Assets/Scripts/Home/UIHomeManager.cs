using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHomeManager : MonoBehaviour
{
    private Canvas _canvas;
    [SerializeField] private Button _dailyGiftbtn;
    [SerializeField] private TextMeshProUGUI totalHeartsText;

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
    }


    private void Start()
    {
        _dailyGiftbtn.onClick.AddListener(() => { TownCanvasController.Instance.ActiveDailyGift(true); });
        totalHeartsText.text = UserManager.Instance.TotalHeart.ToString();
      
    }

    private void OnDestroy()
    {
        _dailyGiftbtn.onClick.RemoveAllListeners();
    }


    public void ActiveCanvas(bool active)
    {
        _canvas.enabled = active;
    }
}