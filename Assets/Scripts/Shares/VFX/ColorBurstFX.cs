using UnityEngine;
using Match3.Enums;

namespace Match3.Shares
{
    public class ColorBurstFX : BaseVisualEffect
    {
        private Animator _anim;
      
        public override void Initialize()
        {
            VfxID = VisualEffectID.ColorBurstFX;
        }

        private void Awake()
        {
            _anim = GetComponent<Animator>();
        }

        public void PlayAnimtion()
        {
            _anim?.SetBool("Rotate", true);
        }

        public void StopAnimation()
        {
            _anim?.SetBool("Rotate", false);
        }

        public override void ReturnToPool()
        {
            base.ReturnToPool();
            StopAnimation();
        }
    }
}
