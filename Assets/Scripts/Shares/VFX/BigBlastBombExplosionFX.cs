using Match3.Enums;
using UnityEngine;
using DG.Tweening;

namespace Match3.Shares
{
    public class BigBlastBombExplosionFX : BaseVisualEffect
    {

        private Animator _anim;
        private Tween _scaleTween;


        private void Awake()
        {
            _anim = GetComponent<Animator>();
        }

        public override void Initialize()
        {
            VfxID = VisualEffectID.BigBlastBombExplosionFX;
        }

        public override void Play(float duration = 1)
        {
            PlayAnimation(duration);
            base.Play(duration);
        }

        public override void ReturnToPool()
        {
            // Reset
            transform.localScale = Vector3.one;
            _anim.SetBool("Explosion", false);
            if(_scaleTween != null && _scaleTween.IsActive())
            {
                _scaleTween.Kill();
            }

            base.ReturnToPool();
        }

        private void PlayAnimation(float duration)
        {
            _scaleTween = transform.DOScale(2.5f, duration * 0.2f).SetEase(Ease.InBack).OnComplete(()=>
            {
                _anim.SetBool("Explosion", true);
            });
        }
    }
}
