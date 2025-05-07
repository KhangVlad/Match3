using UnityEngine;
using DG.Tweening;

namespace Match3.Shares
{
    public class SpecialTileMergeGhostFX : BaseVisualEffect
    {
        private SpriteRenderer _sr;
        private Tween _changeColorTween;
    
        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();   
        }

        public override void Initialize()
        {
            VfxID = Enums.VisualEffectID.SpecialTileMergeGhostFX;
        }


        public override void Play(float duration = 1)
        {
            base.Play(duration);
            PlayAnimation(duration);
        }

        public override void ReturnToPool()
        {
            ResetFX();
            base.ReturnToPool();
        }

        public void PlayAnimation(float duration)
        {
            float startAlpha = _sr.color.a;

            _changeColorTween = DOTween.To(() => startAlpha, x =>
            {
                startAlpha = x;
                Color c = _sr.color;
                c.a = x;
                _sr.color = c;
            }, 0f, duration); // fade to 0 over 1 second
        }

        private void ResetFX()
        {
            if (_changeColorTween != null && _changeColorTween.IsActive())
            {
                _changeColorTween.Kill();
            }

            Color c = _sr.color;
            c.a = 0.5f;
            _sr.color = c;
        }
    }
}
