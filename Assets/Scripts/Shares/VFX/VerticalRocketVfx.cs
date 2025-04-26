using Match3.Enums;
using UnityEngine;
using DG.Tweening;

namespace Match3.Shares
{
    public class VerticalRocketVfx : BaseVisualEffect
    {
        private SpriteRenderer _sr;
        [SerializeField] private Transform _upRocket;
        [SerializeField] private Transform _downRocket;

        private Vector2 _offsetUpRocket;
        private Vector2 _offsetDownRocket;

        private Tween _upMoveTween;
        private Tween _downMoveTween;

        private void Awake()
        {
            _offsetUpRocket = transform.position - _upRocket.position;
            _offsetDownRocket = transform.position - _downRocket.position;
            _sr = GetComponent<SpriteRenderer>();
        }
        public override void Initialize()
        {
            VfxID = VisualEffectID.VerticalRocket;
        }

        public override void Play(float duration = 1)
        {
            base.Play(duration);

            _sr.enabled = false;
            int targetUp = Mathf.FloorToInt(transform.position.y) + 10;
            _upMoveTween = _upRocket.DOMoveY(targetUp, duration).SetEase(Ease.Linear);

            int downTarget = Mathf.FloorToInt(transform.position.y) - 10;
            _downMoveTween = _downRocket.DOMoveY(downTarget, duration).SetEase(Ease.Linear);
        }

        public override void ReturnToPool()
        {
            base.ReturnToPool();

            // Reset
            if (_upMoveTween != null && _upMoveTween.IsActive())
            {
                _upMoveTween.Kill();
            }
            if (_downMoveTween != null && _downMoveTween.IsActive())
            {
                _downMoveTween.Kill();
            }

            _upRocket.transform.position = (Vector2)transform.position + _offsetUpRocket;
            _downRocket.transform.position = (Vector2)transform.position + _offsetDownRocket;
            _sr.enabled = true;
        }
    }
}
