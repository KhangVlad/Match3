namespace Match3
{
    public enum TileID : byte
    {
        None = 0,
        RedFlower = 1,
        YellowFlower = 2,
        PurpleFlower = 3,
        BlueFlower = 4,
        WhiteFlower = 5,
        RedCandle = 6,
        YellowCandle = 7,
        GreenCandle = 8,
        BlueCandle = 9,
        WhiteCandle = 10,
        RedRibbon = 11,
        YellowRibbon = 12,
        GreenRibbon = 13,
        BlueRibbon = 14,
        PurpleRibbon = 15,
        MagnifyingGlass = 16,
    }

    public enum SpecialTileID 
    {
        None = 0,
        ColumnBomb = 1,
        RowBomb = 2,
        BlastBomb = 3,
        ColorBurst = 4,
    }
}
