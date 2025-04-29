using Match3.Enums;
using UnityEngine;
using DG.Tweening;

namespace Match3.Shares
{
    public class HorizontalRocketVfx : BaseVisualEffect
    {
        private SpriteRenderer _sr;
        [SerializeField] private Transform _leftRocket;
        [SerializeField] private Transform _rightRocket;

        private Vector2 _offsetLeftRocket;
        private Vector2 _offsetRightRocket;

        private Tween _leftMoveTween;
        private Tween _rightMoveTween;

        private void Awake()
        {
            _offsetLeftRocket = transform.position - _leftRocket.position;
            _offsetRightRocket = transform.position - _rightRocket.position;
            _sr = GetComponent<SpriteRenderer>();
        }
        public override void Initialize()
        {
            VfxID = VisualEffectID.HorizontalRocket;
        }

        public override void Play(float duration = 1)
        {
            base.Play(duration);

            _sr.enabled = false;
            int targetLeft = Mathf.FloorToInt(transform.position.x) - 10;
            _leftMoveTween =_leftRocket.DOMoveX(targetLeft, duration).SetEase(Ease.Linear);

            int targetRight = Mathf.FloorToInt(transform.position.x) + 10;
            _rightMoveTween =_rightRocket.DOMoveX(targetRight, duration).SetEase(Ease.Linear);
        }

        public override void ReturnToPool()
        {
            base.ReturnToPool();

            // Reset
            if (_leftMoveTween != null && _leftMoveTween.IsActive())
            {
                _leftMoveTween.Kill();
            }
            if (_rightMoveTween != null && _rightMoveTween.IsActive())
            {
                _rightMoveTween.Kill();
            }

            _leftRocket.transform.position = (Vector2)transform.position + _offsetLeftRocket;
            _rightRocket.transform.position = (Vector2)transform.position + _offsetRightRocket;
            _sr.enabled = true;
        }
    }
}
