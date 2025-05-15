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
            if (_scaleTween != null && _scaleTween.IsActive())
            {
                _scaleTween.Kill();
            }

            base.ReturnToPool();
        }

        private void PlayAnimation(float duration)
        {
            // _scaleTween = transform.DOScale(2.5f, duration * 0.2f).SetEase(Ease.InBack).OnComplete(()=>
            // {
            //     _anim.SetBool("Explosion", true);
            // });

            _anim.SetBool("Explosion", true);
            // Ensure default scale
            transform.localScale = Vector3.one * 2.5f;

            // One boom cycle duration (scale out and back)
            float cycleDuration = 0.25f;

            // Calculate how many cycles fit in the full duration
            int loopCount = Mathf.FloorToInt(duration / cycleDuration);
            _scaleTween = transform.DOScale(3.5f, cycleDuration / 2f)
                .SetEase(Ease.OutBack)
                .SetLoops(loopCount * 2, LoopType.Yoyo) // Multiply by 2 because each cycle has in and out
                .OnComplete(() =>
                {
                    // Return to default scale in case it's slightly off
                    transform.localScale = Vector3.one * 2.5f;
                });
        }
    }
}
