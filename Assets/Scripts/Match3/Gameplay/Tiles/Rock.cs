using Match3.Shares;
using Match3.Enums;

namespace Match3
{
    public class Rock : Tile
    {
        public override void Initialize()
        {
            base.Initialize();
            this.ID = TileID.Rock;
        }

        protected override void Awake()
        {
            base.Awake();
        }
        
        public override void PlayMatchVFX(MatchID matchID)
        {
            BaseVisualEffect effect = VFXPoolManager.Instance.GetEffect(VisualEffectID.WhiteFlowerDestroy);
            effect.transform.position = TileTransform.position;
            effect.Play();
        }
    }



   

   

    

    

    
    
   

   

   


   

    

    

    

    

   

   

    

   

   

   

    
}
