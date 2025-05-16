using Match3.Enums;
using UnityEngine;
using DG.Tweening;

namespace Match3.Shares
{
    public class HorizontalRocketVfx : BaseVisualEffect
    {
        public const int MAX_ROCKET_DISTANCE = 10;
        private SpriteRenderer _sr;
        private Animator _anim;
        [SerializeField] private Transform _leftRocket;
        [SerializeField] private Transform _rightRocket;

        private Vector2 _offsetLeftRocket;
        private Vector2 _offsetRightRocket;

        private Tween _leftMoveTween;
        private Tween _rightMoveTween;

        private void Awake()
        {
            _offsetLeftRocket = _leftRocket.position - transform.position;
            _offsetRightRocket = _rightRocket.position - transform.position;
            _sr = GetComponent<SpriteRenderer>();
            _anim = GetComponent<Animator>();
        }
        public override void Initialize()
        {
            VfxID = VisualEffectID.HorizontalRocket;
        }

        public override void Play(float duration = 1)
        {
            // base.Play(duration);

            // _sr.enabled = false;
            // int targetLeft = Mathf.FloorToInt(transform.position.x) - MAX_ROCKET_DISTANCE;
            // _leftMoveTween =_leftRocket.DOMoveX(targetLeft, duration).SetEase(Ease.Linear);

            // int targetRight = Mathf.FloorToInt(transform.position.x) + MAX_ROCKET_DISTANCE;
            // _rightMoveTween =_rightRocket.DOMoveX(targetRight, duration).SetEase(Ease.Linear);     
            base.Play(duration);
            _sr.enabled = false;
            _leftRocket.gameObject.SetActive(true);
            _rightRocket.gameObject.SetActive(true);
            int targetLeft = Mathf.FloorToInt(transform.position.x) - 10;
            int targetRight = Mathf.FloorToInt(transform.position.x) + 10;

            // Initialize rocket scales to zero (for the "pop-in" effect)
            _leftRocket.localScale = Vector3.zero;
            _rightRocket.localScale = Vector3.zero;

            // Animate left rocket: scale in, then launch
            _leftRocket.DOScale(Vector3.one, duration * 0.1f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                _leftMoveTween = _leftRocket.DOMoveX(targetLeft, duration * 0.9f).SetEase(Ease.Linear);
            });

            // Animate right rocket: scale in, then launch
            _rightRocket.DOScale(Vector3.one, duration * 0.1f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                _rightMoveTween = _rightRocket.DOMoveX(targetRight, duration * 0.9f).SetEase(Ease.Linear);
            });
        }

        public override void ReturnToPool()
        {
            StopAnimation();
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

        public void PlayAnimtion()
        {
            _leftRocket.gameObject.SetActive(false);
            _rightRocket.gameObject.SetActive(false);
            _anim?.SetBool("Rotate", true);
        }

        public void StopAnimation()
        {
            _anim?.SetBool("Rotate", false);
        }
    }
}
