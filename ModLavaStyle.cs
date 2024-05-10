﻿namespace BiomeLava;

// TODO: allow lava styles to add different buffs
// TODO: extend from ModWaterStyle and ModWaterfallStyle
public readonly record struct ModLavaStyle(
    Asset<Texture2D> LavaTexture,
    Asset<Texture2D> LavaBlockTexture,
    Asset<Texture2D> LavaSlopeTexture,
    Asset<Texture2D> LavaFallTexture,
    bool LavaFallUsesGlowMask,
    int SplashDustID,
    int DropletGoreID,
    Color LightColor,
    Func<bool> InZone
)
{
    // TODO: make types better
    public int Type => LavaStyleLoader.Instance.LavaStyles.ToList().IndexOf(this);
}
