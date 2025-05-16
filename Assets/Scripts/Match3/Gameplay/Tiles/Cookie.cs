using System;
using Match3.Shares;
using Match3.Enums;

namespace Match3
{
    public class Cookie : Tile
    {
        public override void Initialize()
        {
            base.Initialize();
            this.ID = TileID.Cookie;
        }

        protected override void Awake()
        {
            base.Awake();
        }
    }
}