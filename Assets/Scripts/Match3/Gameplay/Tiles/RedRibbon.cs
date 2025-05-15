using Match3.Shares;
using Match3.Enums;
using UnityEngine;

namespace Match3
{
    public class RedRibbon : Tile
    {
          public override void Initialize()
        {
            base.Initialize();
            this.ID = TileID.RedRibbon;
        }

        protected override void Awake()
        {
            base.Awake();
        }

        public override void PlayMatchVFX(MatchID matchID)
        {
            BaseVisualEffect effect = VFXPoolManager.Instance.GetEffect(VisualEffectID.RedRibbonDestroy);
            effect.transform.position = TileTransform.position;

               switch (matchID)
            {
                case MatchID.BlastBomb:
                    effect.SetParticleQuantity(50);
                    effect.SetLifeTime(1.5f);
                    effect.SetSpeed();
                    break;
                default:
                    effect.SetParticleQuantity(5);
                    effect.SetLifeTime(0.65f);
                    effect.SetSpeed();
                    break;
            }
            
            effect.Play();
        }
    }
}
