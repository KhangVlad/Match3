using Match3.Shares;
using Match3.Enums;

namespace Match3
{
    public class MeatBall : Tile
    {
        public override void Initialize()
        {
            base.Initialize();
            this.ID = TileID.MeetBall;
        }

        protected override void Awake()
        {
            base.Awake();
        }
    }
}