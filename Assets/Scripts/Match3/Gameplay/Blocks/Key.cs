﻿namespace Match3
{
    public class Key : Block
    {
         public override void Initialize()
        {
            BlockID = BlockID.Key;
        }

        public override void Match(Tile tile, Tile[] grid, int width)
        {
        }

        public override void Unlock(Tile tile)
        {
        }
    }
}
