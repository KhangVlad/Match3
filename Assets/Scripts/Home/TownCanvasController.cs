using Match3;
using UnityEngine;

#if !UNITY_WEBGL
public class TownCanvasController : MonoBehaviour
{
    public static TownCanvasController Instance { get; private set; }
    public UISettingManager uiSettingManager { get; private set; }
    public UILevelDesignManager uiLevelDesignManager { get; private set; }
    public UIDailyGiftManager UIDailyGiftManager { get; private set; }
    public UIHomeManager uiHomeManager { get; private set; }
    public UIShop uiShop { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        uiSettingManager = GetComponentInChildren<UISettingManager>();
        uiLevelDesignManager = GetComponentInChildren<UILevelDesignManager>();
        UIDailyGiftManager = GetComponentInChildren<UIDailyGiftManager>();
        uiHomeManager = GetComponentInChildren<UIHomeManager>();
        uiShop = GetComponentInChildren<UIShop>();
        CloseAllCanvas();
    }

    private void CloseAllCanvas()
    {
        ActiveLevelDesign(false);
        ActiveDailyGift(false);
        ActiveShop(false);
    }


    public void ActiveLevelDesign(bool active)
    {
        uiLevelDesignManager.ActiveCanvas(active);
        uiHomeManager.ActiveCanvas(!active);
        uiSettingManager.ActiveCanvas(!active);
    }


    public void ActiveDailyGift(bool active)
    {
        UIDailyGiftManager.ActiveCanvas(active);
        uiHomeManager.ActiveCanvas(!active);
    }

    public void ActiveShop(bool active)
    {
        uiShop.ActiveCanvas(active);
    }
}
#endif