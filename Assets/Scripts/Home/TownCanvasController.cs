using UnityEngine;

public class TownCanvasController : MonoBehaviour
{
    public static TownCanvasController Instance { get; private set; }
    public UISettingManager uiSettingManager { get; private set; }
    public UILevelDesignManager uiLevelDesignManager { get; private set; }

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
        CloseAllCanvas();
    }

    private void CloseAllCanvas()
    {
        ActiveLevelDesign(false);
    }


    public void ActiveLevelDesign(bool active)
    {
        uiLevelDesignManager.ActiveCanvas(active);
        ActiveSetting(!active);
    }

    public void ActiveSetting(bool active)
    {
        uiSettingManager.ActiveCanvas(active);
    }
}