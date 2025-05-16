using UnityEngine;
using Match3.Enums;
using DG.Tweening;
namespace Match3.Shares
{
    public class ColorBurstAppearFX : BaseVisualEffect
    {
        private Animator _anim;
        private Transform _target;

        private Tween _scaleTween;

        public override void Initialize()
        {
            VfxID = VisualEffectID.ColorBurstAppearFX;
        }

        private void Awake()
        {
            _anim = GetComponent<Animator>();
        }

        private void Update()
        {
            if (_target == null) return;
            transform.position = _target.transform.position;
        }

        public void PlayAnimtion()
        {
            _anim?.SetBool("Rotate", true);
        }

        public void StopAnimation()
        {
            _anim?.SetBool("Rotate", false);
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        public void AppearPopupAnimation()
        {
            transform.localScale = Vector3.zero; // Start invisible
            _scaleTween = transform.DOScale(Vector3.one, 0.5f) // Scale up to full size
                   .SetEase(Ease.OutBack);     // Nice bouncy ease
        }

        public override void ReturnToPool()
        {
            base.ReturnToPool();
            SetTarget(null);
            StopAnimation();

            if (_scaleTween != null && _scaleTween.IsActive())
            {
                _scaleTween.Kill();
                transform.localScale = Vector3.one;
            }
        }
    }
}
