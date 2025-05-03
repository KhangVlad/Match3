using Match3.Enums;
using UnityEngine;
namespace Match3.Shares
{
    public class BlastBombAppearFX : BaseVisualEffect
    {
        private Animator _anim;

        private void Awake()
        {
            _anim = GetComponent<Animator>();
        }

        public override void Initialize()
        {
            VfxID = VisualEffectID.BlastBombAppear;
        }

        public override void Play(float duration = 1)
        {
            base.Play(duration);
            PlayAnimation();
        }

        public override void ReturnToPool()
        {
            StopAnimation();
            base.ReturnToPool();
        }

        public void PlayAnimation()
        {
            _anim.SetBool("Rotate", true);
        }

        public void StopAnimation()
        {
            _anim.SetBool("Rotate", false);
        }
    }
}
