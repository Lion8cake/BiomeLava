using BiomeLava.Bubbles;
using BiomeLava.Droplets;

namespace BiomeLava;

// TODO: lavaLiquidAlpha
public class LavaStyleLoader : ModSystem
{
    public static LavaStyleLoader Instance => ModContent.GetInstance<LavaStyleLoader>();

    // TODO: priorities, transitions
    public bool IsStyleActive => ActiveStyles.Any();
    public ModLavaStyle ActiveStyle => ActiveStyles.First();

    public IEnumerable<ModLavaStyle> ActiveStyles
        => _lavaStyles
            .Where(l => LavaAlpha[l.Type] > 0f)
            .OrderByDescending(l => LavaAlpha[l.Type]);

    public List<float> LavaAlpha = new();

    public int LavaStyleCount { get; internal set; }
    public IReadOnlyList<ModLavaStyle> LavaStyles => _lavaStyles.AsReadOnly();
    private readonly List<ModLavaStyle> _lavaStyles = new();

    public void AddLavaStyle(ModLavaStyle style)
    {
        _lavaStyles.Add(style);
        LavaAlpha.Add(0f);
        LavaStyleCount++;
    }

    public override void PostSetupContent()
    {
        // Add default lava styles
        if (Main.dedServ)
            return;

        #region Built-In Styles

        AddBuiltInLavaStyle(
            "Corruption",
            ModContent.DustType<CorruptionLavaDust>(),
            ModContent.GoreType<CorruptionDroplet>(),
            true,
            new(0.33f, 0.55f, 0.11f),
            static () => Main.LocalPlayer.ZoneCorrupt && !Main.LocalPlayer.ZoneUnderworldHeight && BiomeLavaConfig.Instance.CorruptionLava
        );

        AddBuiltInLavaStyle(
            "Crimson",
            ModContent.DustType<CrimsonLavaDust>(),
            ModContent.GoreType<CrimsonDroplet>(),
            true,
            new(0.55f, 0.44f, 0.11f),
            static () => Main.LocalPlayer.ZoneCrimson && !Main.LocalPlayer.ZoneUnderworldHeight && BiomeLavaConfig.Instance.CrimsonLava
        );

        AddBuiltInLavaStyle(
            "Hallow",
            ModContent.DustType<HallowLavaDust>(),
            ModContent.GoreType<HallowDroplet>(),
            true,
            new(0.33f, 0.77f, 0.99f),
            static () => Main.LocalPlayer.ZoneHallow && !Main.LocalPlayer.ZoneUnderworldHeight && BiomeLavaConfig.Instance.HallowLava
        );

        AddBuiltInLavaStyle(
            "Jungle",
            ModContent.DustType<JungleLavaDust>(),
            ModContent.GoreType<JungleDroplet>(),
            false,
            new(0.22f, 0.22f, 0.11f),
            static () => Main.LocalPlayer.ZoneJungle && !Main.LocalPlayer.ZoneUnderworldHeight && BiomeLavaConfig.Instance.JungleLava
        );

        AddBuiltInLavaStyle(
            "Ice",
            ModContent.DustType<IceLavaDust>(),
            ModContent.GoreType<IceDroplet>(),
            false,
            new(0.44f, 0.22f, 0.11f),
            static () => Main.LocalPlayer.ZoneSnow && !Main.LocalPlayer.ZoneUnderworldHeight && BiomeLavaConfig.Instance.IceLava
        );

        AddBuiltInLavaStyle(
            "Desert",
            ModContent.DustType<DesertLavaDust>(),
            ModContent.GoreType<DesertDroplet>(),
            true,
            new(0.77f, 0.44f, 0.11f),
            static () => Main.LocalPlayer.ZoneDesert && !Main.LocalPlayer.ZoneUnderworldHeight && BiomeLavaConfig.Instance.DesertLava
        );

        #endregion

        return;

        void AddBuiltInLavaStyle(string name, int splashDustType, int dropletGoreType, bool glowMask, Color lightColor, Func<bool> condition)
        {
            AddLavaStyle(
                new(
                    ModContent.Request<Texture2D>($"BiomeLava/Assets/{name}/{name}Lava"),
                    ModContent.Request<Texture2D>($"BiomeLava/Assets/{name}/{name}Lava_Block"),
                    ModContent.Request<Texture2D>($"BiomeLava/Assets/{name}/{name}Lava_Slope"),
                    ModContent.Request<Texture2D>($"BiomeLava/Assets/{name}/{name}Lava_Waterfall"),
                    glowMask,
                    splashDustType,
                    dropletGoreType,
                    lightColor,
                    condition
                )
            );
        }
    }
}
