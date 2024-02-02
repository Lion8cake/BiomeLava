using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace BiomeLava;

public class BiomeLavaConfig : ModConfig
{
    public static BiomeLavaConfig Instance => ModContent.GetInstance<BiomeLavaConfig>();
    public override ConfigScope Mode => ConfigScope.ServerSide;

    [Header("LavaStyles")]
    [DefaultValue(true)]
    public bool CorruptionLava;

    [DefaultValue(true)]
    public bool CrimsonLava;

    [DefaultValue(true)]
    public bool DesertLava;

    [DefaultValue(true)]
    public bool HallowLava;

    [DefaultValue(true)]
    public bool IceLava;

    [DefaultValue(true)]
    public bool JungleLava;
}
