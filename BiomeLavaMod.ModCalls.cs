using System.Runtime.CompilerServices;

namespace BiomeLava;

// This is a static class intended to be copied to a mod wanting to add their own lava styles
// It is just wrapper methods for Mod.Call so you are less likely to make a mistake
public static class BiomeLavaAPI
{
    public const string BiomeLavaModName = "BiomeLava";
    public static bool BiomeLavaModEnabled => ModLoader.HasMod(BiomeLavaModName);
    public static Mod BiomeLavaMod => ModLoader.TryGetMod(BiomeLavaModName, out var mod) ? mod : null;

    public static void AddLavaStyle(
        Asset<Texture2D> lavaTexture,
        Asset<Texture2D> lavaBlockTexture,
        Asset<Texture2D> lavaSlopeTexture,
        Asset<Texture2D> lavaFallTexture,
        bool lavaFallUsesGlowMask,
        int splashDustID,
        int dropletGoreID,
        Color lightColor,
        Func<bool> inZone)
    {
        BiomeLavaMod.Call(nameof(AddLavaStyle), lavaTexture, lavaBlockTexture, lavaSlopeTexture, lavaFallTexture, lavaFallUsesGlowMask, splashDustID, dropletGoreID, lightColor, inZone);
    }
}

// Mod.Call implementation
partial class BiomeLavaMod
{
    private Dictionary<string, Func<object[], object>> commands = new()
    {
        [nameof(BiomeLavaAPI.AddLavaStyle)] = AddLavaStyleCall
    };

    private static bool ValidateArg<T>(object[] args, int index, out T arg)
    {
        arg = default;

        if (args.Length < index + 1)
            return false;

        if (args[index] is not T)
            return false;

        arg = (T)args[index];
        return true;
    }

    public override object Call(params object[] args)
    {
        if (ValidateArg<string>(args, 0, out var commandName) && commands.TryGetValue(commandName, out var command))
            return command(args);

        return "Invalid command";
    }

    private static object AddLavaStyleCall(params object[] args)
    {
        var success = true;
        success &= ValidateArg<Asset<Texture2D>>(args, 1, out var lavaTexture);
        success &= ValidateArg<Asset<Texture2D>>(args, 2, out var lavaBlockTexture);
        success &= ValidateArg<Asset<Texture2D>>(args, 3, out var lavaSlopeTexture);
        success &= ValidateArg<Asset<Texture2D>>(args, 4, out var lavaFallTexture);

        success &= ValidateArg<bool>(args, 5, out var lavaFallUsesGlowMask);
        success &= ValidateArg<int>(args, 6, out var splashDustID);
        success &= ValidateArg<int>(args, 7, out var dropletGoreID);
        success &= ValidateArg<Color>(args, 8, out var lightColor);
        success &= ValidateArg<Func<bool>>(args, 9, out var inZone);

        if (!success)
            return $"Invalid args for {nameof(BiomeLavaAPI.AddLavaStyle)}";

        LavaStyleLoader.Instance.AddLavaStyle(new ModLavaStyle(
            lavaTexture,
            lavaBlockTexture,
            lavaSlopeTexture,
            lavaFallTexture,
            lavaFallUsesGlowMask,
            splashDustID,
            dropletGoreID,
            lightColor,
            inZone
        ));

        return "Successfully added new ModLavaStyle";
    }
}
