using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Match3.Shares;

public class UIMenuTabBtn : MonoBehaviour
{
    [Header("~Runtime")]
    [SerializeField] private Button _button;
    [SerializeField] private Image _icon;

    private float _cachedIconPositionY = 120f;
    public Button Button => _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _icon = transform.GetChild(0).GetComponent<Image>();
        _cachedIconPositionY = _icon.rectTransform.position.y;
    }

    public void Select()
    {
        _icon.transform.DOScale(1.2f, 0.1f).SetEase(Ease.InBack);
         _icon.rectTransform.DOLocalMoveY(75, 0.1f).SetEase(Ease.Linear);
         _icon.transform.DORotate(new Vector3(0,0,-15),0.1f, RotateMode.Fast);
    }
    public void Unselect()
    {
        _icon.transform.DOScale(1.0f, 0.1f).SetEase(Ease.InBack);
        _icon.rectTransform.DOLocalMoveY(0, 0.1f).SetEase(Ease.Linear);
         _icon.transform.DORotate(new Vector3(0,0,0),0.0f, RotateMode.Fast);
    }
}
