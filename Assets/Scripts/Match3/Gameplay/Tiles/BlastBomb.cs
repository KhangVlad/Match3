using Match3.Shares;
using Match3.Enums;
using UnityEngine;

namespace Match3
{
    public class BlastBomb : Tile
    {
        [SerializeField] private SpriteRenderer _fireSR;

        public override void Initialize()
        {
            base.Initialize();
            this.ID = TileID.BlastBomb;
        }

        protected override void Awake()
        {
            base.Awake();
        }

        public override void PlayMatchVFX(MatchID matchID)
        {
            // BaseVisualEffect effect = VFXPoolManager.Instance.GetEffect(VisualEffectID.RedFlowerDestroy);
            // effect.transform.position = TileTransform.position;
            // effect.Play();
        }

        public override void Display(bool enable)
        {
            base.Display(enable);
            _fireSR.enabled = enable;
        }
    }
}
