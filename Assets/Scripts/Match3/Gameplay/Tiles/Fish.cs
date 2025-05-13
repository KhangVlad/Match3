using Match3.Shares;
using Match3.Enums;

namespace Match3
{
    public class Fish : Tile
    {
        public override void Initialize()
        {
            base.Initialize();
            this.ID = TileID.Fish;
        }

        protected override void Awake()
        {
            base.Awake();
        }

        public override void Match(Tile[] grid, int width, MatchID matchID)
        {
            base.Match(grid, width, matchID);
        }

        public override void PlayMatchVFX( MatchID matchID)
        {
           
        }
    }
}