using System;
using UnityEngine;
using UnityEngine.UI;

#if !UNITY_WEBGL
public class UIDailyGiftManager : MonoBehaviour
{
    private Canvas canvas;
    [SerializeField] private Button _closebtn;


    private void Start()
    {
        _closebtn.onClick.AddListener(() => { TownCanvasController.Instance.ActiveDailyGift(false); });
    }


    private void OnDestroy()
    {
        _closebtn.onClick.RemoveAllListeners();
    }

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
    }

    public void ActiveCanvas(bool active)
    {
        canvas.enabled = active;
    }
}
#endif