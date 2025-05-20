using UnityEngine;
namespace Match3.Enums
{
    public enum VisualEffectID : byte
    {
        None = 0,
        // Tiles
        RedFlowerDestroy = 1,
        YellowFlowerDestroy = 2,
        PurpleFlowerDestroy = 3,
        BlueFlowerDestroy = 4,
        WhiteFlowerDestroy = 5,
        RedCandleDestroy = 6,
        YellowCandleDestroy = 7,
        GreenCandleDestroy = 8,
        BlueCandleDestroy = 9,
        WhiteCandleDestroy = 10,
        RedRibbonDestroy = 11,
        YellowRibbonDestroy = 12,
        GreenRibbonDestroy = 13,
        BlueRibbonDestroy = 14,
        PurpleRibbonDestroy = 15,



        // Special Tiles
        ExplosionHorizontalFX = 51,
        ExplosionVerticalFX = 52,
        ColorBurstFX = 53,
        ColorBurstLineFX = 54,
        LightingLine = 55,
        EndLineColorBurstFX = 56,
        VerticalRocket = 57,
        HorizontalRocket = 58,
        BlastBomb = 59,
        BlastBombAppear = 60,
        SpecialTileMergeGhostFX = 61,
        BigBlastBombExplosionFX = 62,
        ColorBurstAppearFX = 63,
        DoubleColorBurstHorizontalCharge = 64,
        DoubleColorBurstVerticalCharge = 65,
        DoubleColorBurstExplosion = 66,


        Slash = 100,
    }
}