using UnityEngine;
using DG.Tweening;
using Match3.Shares;

namespace Match3
{
    public class UINoMorePossibleMove : MonoBehaviour
    {
        public static UINoMorePossibleMove Instance { get; private set; }

        private Canvas _canvas;

        [Header("No More Possible Move")]
        [SerializeField] private Transform _noMorePossibleMovePanel;
        private Vector3 _defaultNoMorePossibleMovePanelPosition;

        private Tween _moveTween;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
            _canvas = GetComponent<Canvas>();
        }

        private void Start()
        {
            Utilities.WaitAfterEndOfFrame(() =>
            {
                _defaultNoMorePossibleMovePanelPosition = _noMorePossibleMovePanel.transform.position;
                _noMorePossibleMovePanel.gameObject.SetActive(false);
            });
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                DisplayNoMorePossibleMove(true);
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                DisplayNoMorePossibleMove(false);
            }
        }

        public void DisplayNoMorePossibleMove(bool enable)
        {
            if (_moveTween != null && _moveTween.IsActive())
            {
                _moveTween.Kill();
            }

            if (enable)
            {
                _noMorePossibleMovePanel.transform.position = _defaultNoMorePossibleMovePanelPosition + new Vector3(0, 900);
                _noMorePossibleMovePanel.gameObject.SetActive(true);
                _moveTween = _noMorePossibleMovePanel.DOMove(_defaultNoMorePossibleMovePanelPosition, 0.5f).SetEase(Ease.InBack);
            }
            else
            {
                _moveTween = _noMorePossibleMovePanel.DOMove(_defaultNoMorePossibleMovePanelPosition + new Vector3(0, 900), 0.5f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    _noMorePossibleMovePanel.gameObject.SetActive(false);
                });
            }
        }

        public void DisplayCanvas(bool enable)
        {
            Debug.Log($"Display canvas:  {enable}");
            this._canvas.enabled = enable;
        }
    }
}
