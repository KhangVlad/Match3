using Match3.Enums;
using UnityEngine;
using DG.Tweening;

namespace Match3.Shares
{
    public class VerticalRocketVfx : BaseVisualEffect
    {
        public const int MAX_ROCKET_DISTANCE = 10;
        private SpriteRenderer _sr;
        private Animator _anim;
        [SerializeField] private Transform _upRocket;
        [SerializeField] private Transform _downRocket;

        private Vector2 _offsetUpRocket;
        private Vector2 _offsetDownRocket;

        private Tween _upMoveTween;
        private Tween _downMoveTween;

        private Transform _targetTransform;

        private void Awake()
        {
            _offsetUpRocket = transform.position - _upRocket.position;
            _offsetDownRocket = transform.position - _downRocket.position;
            _sr = GetComponent<SpriteRenderer>();
            _anim = GetComponent<Animator>();
        }

        private void Update()
        {
            if(_targetTransform != null)
            {
                transform.position = _targetTransform.position;
            }
        }
        public override void Initialize()
        {
            VfxID = VisualEffectID.VerticalRocket;
        }

        public override void Play(float duration = 1)
        {
            base.Play(duration);
            _sr.enabled = false;
            _upRocket.gameObject.SetActive(true);
            _downRocket.gameObject.SetActive(true);
            int targetUp = Mathf.FloorToInt(transform.position.y) + MAX_ROCKET_DISTANCE;
            _upMoveTween = _upRocket.DOMoveY(targetUp, duration).SetEase(Ease.Linear);

            int downTarget = Mathf.FloorToInt(transform.position.y) - MAX_ROCKET_DISTANCE;
            _downMoveTween = _downRocket.DOMoveY(downTarget, duration).SetEase(Ease.Linear);
        }

        public override void ReturnToPool()
        {
            SetTargetTransform(null);
            StopAnimation();
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

        public void PlayAnimtion()
        {
            _upRocket.gameObject.SetActive(false);
            _downRocket.gameObject.SetActive(false);
            _anim?.SetBool("Rotate", true);
        }

        public void StopAnimation()
        {
            _anim?.SetBool("Rotate", false);
        }

        public void SetTargetTransform(Transform transform)
        {
            this._targetTransform = transform;
        }
    }
}
