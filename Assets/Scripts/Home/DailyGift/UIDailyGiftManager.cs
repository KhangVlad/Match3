using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

#if !UNITY_WEBGL
public class UIDailyGiftManager : MonoBehaviour
{
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    [SerializeField] private Button _closebtn;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private SpinWheel spin;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
    }

    private void Start()
    {
        _closebtn.onClick.AddListener(() => TownCanvasController.Instance.ActiveDailyGift(false));
    }

    private void OnDestroy()
    {
        _closebtn.onClick.RemoveAllListeners();
        DOTween.Kill(canvasGroup);
    }

    public void ActiveCanvas(bool active)
    {
        if (active)
        {
            spin.enabled = true;
            canvas.enabled = true;
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, fadeDuration);
        }
        else
        {
            spin.enabled = false;
            canvasGroup.DOFade(0f, fadeDuration).OnComplete(() => canvas.enabled = false);
        }
    }
}
#endif