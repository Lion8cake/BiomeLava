using System.Diagnostics;
using BiomeLava.Bubbles;
using BiomeLava.Droplets;
using MonoMod.Cil;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.Liquid;
using Terraria.Graphics;
using Terraria.Graphics.Capture;
using Terraria.Graphics.Light;
using static Terraria.GameContent.Liquid.LiquidRenderer;

namespace BiomeLava;

public partial class BiomeLavaMod : Mod
{
    public static BiomeLavaMod Instance => ModContent.GetInstance<BiomeLavaMod>();

    private static bool Active => LavaStyleLoader.Instance.IsStyleActive;
    private static ModLavaStyle Style => LavaStyleLoader.Instance.ActiveStyle;

    // TODO: lavaLiquidAlpha
    private static float[] lavaLiquidAlpha =
    {
        1f,
        1f,
        1f,
        1f,
        1f,
        1f,
        1f,
        1f,
        1f,
        1f,
        1f,
        1f,
        1f,
        1f,
        1f,
        1f,
        1f,
        1f,
        1f,
        1f,
        1f,
        1f,
        1f,
        1f,
        1f
    };

    // TODO: use curried function for ILEdits to reduce code duplication

    #region Preparing IL Edits & Detours

    public override void Load()
    {
        // Looking for IL_LiquidRenderer.DrawNormalLiquids += BlockLavaDrawing?
        // It was removed because it was supposed to block Color and White lava drawing
        // but it made all liquids either disappear or render a few blocks
        // below where they actually were positioned.
        // To find it, check the GitHub commit history.

        // Lava falls
        On_WaterfallManager.DrawWaterfall_int_int_int_float_Vector2_Rectangle_Color_SpriteEffects += DrawLavaFall;
        On_WaterfallManager.AddLight += AddLavaFallLight;
        On_WaterfallManager.StylizeColor += SetLavaFallColor;

        IL_Main.DoDraw += IL_Main_DoDraw;
        IL_Main.RenderWater += IL_Main_RenderWater;
        IL_Main.RenderBackground += IL_Main_RenderBackground;
        IL_Main.DrawCapture += DrawLavatoCapture;

        On_TileDrawing.DrawTile_LiquidBehindTile += BlockLavaDrawingForSlopes;

        IL_TileDrawing.Draw += AddTileLiquidDrawing;

        On_TileLightScanner.ApplyLiquidLight += LavaLightEditor;


        IL_Main.oldDrawWater += BlockRetroLightingLava;
        IL_LiquidRenderer.InternalPrepareDraw += LavaBubbleReplacer;
        IL_Player.Update += SplashPlayerLava;
        IL_NPC.Collision_WaterCollision += SplashNPCLava;
        IL_Projectile.Update += SplashProjectileLava;
        IL_Item.MoveInWorld += SplashItemLava;

        IL_TileDrawing.EmitLiquidDrops += LavaDropletReplacer;
    }

    public override void Unload()
    {
        // Lava falls
        On_WaterfallManager.DrawWaterfall_int_int_int_float_Vector2_Rectangle_Color_SpriteEffects -= DrawLavaFall;
        On_WaterfallManager.AddLight -= AddLavaFallLight;
        On_WaterfallManager.StylizeColor -= SetLavaFallColor;

        IL_Main.DoDraw -= IL_Main_DoDraw;
        IL_Main.RenderWater -= IL_Main_RenderWater;
        IL_Main.RenderBackground -= IL_Main_RenderBackground;
        IL_Main.DrawCapture -= DrawLavatoCapture;

        On_TileDrawing.DrawTile_LiquidBehindTile -= BlockLavaDrawingForSlopes;

        IL_TileDrawing.Draw -= AddTileLiquidDrawing;

        On_TileLightScanner.ApplyLiquidLight -= LavaLightEditor;

        IL_Main.oldDrawWater -= BlockRetroLightingLava;
        IL_LiquidRenderer.InternalPrepareDraw -= LavaBubbleReplacer;
        IL_Player.Update -= SplashPlayerLava;
        IL_NPC.Collision_WaterCollision -= SplashNPCLava;
        IL_Projectile.Update -= SplashProjectileLava;
        IL_Item.MoveInWorld -= SplashItemLava;

        IL_TileDrawing.EmitLiquidDrops -= LavaDropletReplacer;
    }

    #endregion

    #region ILEdits & Detours

    #region Lava Falls

    private static void DrawLavaFall(On_WaterfallManager.orig_DrawWaterfall_int_int_int_float_Vector2_Rectangle_Color_SpriteEffects orig, WaterfallManager self, int waterfallType, int x, int y, float opacity, Vector2 position, Rectangle sourceRect, Color color, SpriteEffects effects)
    {
        if (Active && waterfallType == WaterStyleID.Lava)
        {
            // TODO: lavaLiquidAlpha
            Main.spriteBatch.Draw(Style.LavaFallTexture.Value, position, sourceRect, color, 0f, default, 1f, effects, 0f);
            return;
        }

        orig.Invoke(self, waterfallType, x, y, opacity, position, sourceRect, color, effects);
    }

    private static void AddLavaFallLight(On_WaterfallManager.orig_AddLight orig, int waterfallType, int x, int y)
    {
        if (Active && waterfallType == WaterStyleID.Lava)
        {
            var color = Style.LightColor.ToVector3();
            var r = (color.X + (270 - Main.mouseTextColor) / 900f) * 0.4f;
            var g = r * color.Y;
            var b = r * color.Z;
            Lighting.AddLight(x, y, r, g, b);
            return;
        }

        orig.Invoke(waterfallType, x, y);
    }

    private static Color SetLavaFallColor(On_WaterfallManager.orig_StylizeColor orig, float alpha, int maxSteps, int waterfallType, int y, int s, Tile tileCache, Color aColor)
    {
        if (Active && Style.LavaFallUsesGlowMask)
            aColor = Color.White; // Change the color from the lighting color to solid white

        return orig.Invoke(alpha, maxSteps, waterfallType, y, s, tileCache, aColor);
    }

    #endregion

    private void LavaDropletReplacer(ILContext il)
    {
        var c = new ILCursor(il);
        c.GotoNext(MoveType.After, i => i.MatchLdarg(out _), static i => i.MatchLdcI4(374), i => i.MatchBneUn(out _), static i => i.MatchLdcI4(716));
        c.EmitDelegate<Func<int, int>>(static type => Active ? Style.DropletGoreID : type);
    }


    private void SplashItemLava(ILContext il)
    {
        var c = new ILCursor(il);
        c.GotoNext(MoveType.After, i => i.MatchStloc(15), i => i.MatchBr(out _), i => i.MatchLdarg0(), i => i.MatchLdflda<Entity>("position"),
            i => i.MatchLdfld<Vector2>("X"), i => i.MatchLdcR4(6), i => i.MatchSub(), i => i.MatchLdarg0(),
            i => i.MatchLdflda<Entity>("position"), i => i.MatchLdfld<Vector2>("Y"), i => i.MatchLdarg0(), i => i.MatchLdfld<Entity>("height"), i => i.MatchLdcI4(2),
            i => i.MatchDiv(), i => i.MatchConvR4(), i => i.MatchAdd(), i => i.MatchLdcR4(8),
            i => i.MatchSub(), i => i.MatchNewobj(out _), i => i.MatchLdarg0(), i => i.MatchLdfld<Entity>("width"), i => i.MatchLdcI4(12), i => i.MatchAdd(),
            i => i.MatchLdcI4(24), i => i.MatchLdcI4(35));
        c.EmitDelegate<Func<int, int>>(type => Active ? Style.SplashDustID : type);
        c.GotoNext(MoveType.After, i => i.MatchStloc(23), i => i.MatchBr(out _), i => i.MatchLdarg0(), i => i.MatchLdflda<Entity>("position"),
            i => i.MatchLdfld<Vector2>("X"), i => i.MatchLdcR4(6), i => i.MatchSub(), i => i.MatchLdarg0(),
            i => i.MatchLdflda<Entity>("position"), i => i.MatchLdfld<Vector2>("Y"), i => i.MatchLdarg0(), i => i.MatchLdfld<Entity>("height"), i => i.MatchLdcI4(2),
            i => i.MatchDiv(), i => i.MatchConvR4(), i => i.MatchAdd(), i => i.MatchLdcR4(8),
            i => i.MatchSub(), i => i.MatchNewobj(out _), i => i.MatchLdarg0(), i => i.MatchLdfld<Entity>("width"), i => i.MatchLdcI4(12), i => i.MatchAdd(),
            i => i.MatchLdcI4(24), i => i.MatchLdcI4(35));
        c.EmitDelegate<Func<int, int>>(type => Active ? Style.SplashDustID : type);
    }

    private void SplashProjectileLava(ILContext il)
    {
        var c = new ILCursor(il);
        c.GotoNext(MoveType.After, i => i.MatchStloc(22), i => i.MatchBr(out _), i => i.MatchLdarg0(), i => i.MatchLdflda<Entity>("position"),
            i => i.MatchLdfld<Vector2>("X"), i => i.MatchLdcR4(6), i => i.MatchSub(), i => i.MatchLdarg0(),
            i => i.MatchLdflda<Entity>("position"), i => i.MatchLdfld<Vector2>("Y"), i => i.MatchLdarg0(), i => i.MatchLdfld<Entity>("height"), i => i.MatchLdcI4(2),
            i => i.MatchDiv(), i => i.MatchConvR4(), i => i.MatchAdd(), i => i.MatchLdcR4(8),
            i => i.MatchSub(), i => i.MatchNewobj(out _), i => i.MatchLdarg0(), i => i.MatchLdfld<Entity>("width"), i => i.MatchLdcI4(12), i => i.MatchAdd(),
            i => i.MatchLdcI4(24), i => i.MatchLdcI4(35));
        c.EmitDelegate<Func<int, int>>(type => Active ? Style.SplashDustID : type);
        c.GotoNext(MoveType.After, i => i.MatchStloc(30), i => i.MatchBr(out _), i => i.MatchLdarg0(), i => i.MatchLdflda<Entity>("position"),
            i => i.MatchLdfld<Vector2>("X"), i => i.MatchLdcR4(6), i => i.MatchSub(), i => i.MatchLdarg0(),
            i => i.MatchLdflda<Entity>("position"), i => i.MatchLdfld<Vector2>("Y"), i => i.MatchLdarg0(), i => i.MatchLdfld<Entity>("height"), i => i.MatchLdcI4(2),
            i => i.MatchDiv(), i => i.MatchConvR4(), i => i.MatchAdd(), i => i.MatchLdcR4(8),
            i => i.MatchSub(), i => i.MatchNewobj(out _), i => i.MatchLdarg0(), i => i.MatchLdfld<Entity>("width"), i => i.MatchLdcI4(12), i => i.MatchAdd(),
            i => i.MatchLdcI4(24), i => i.MatchLdcI4(35));
        c.EmitDelegate<Func<int, int>>(type => Active ? Style.SplashDustID : type);
    }

    private void SplashNPCLava(ILContext il)
    {
        var c = new ILCursor(il);
        c.GotoNext(MoveType.After, i => i.MatchStloc(10), i => i.MatchBr(out _), i => i.MatchLdarg0(), i => i.MatchLdflda<Entity>("position"),
            i => i.MatchLdfld<Vector2>("X"), i => i.MatchLdcR4(6), i => i.MatchSub(), i => i.MatchLdarg0(),
            i => i.MatchLdflda<Entity>("position"), i => i.MatchLdfld<Vector2>("Y"), i => i.MatchLdarg0(), i => i.MatchLdfld<Entity>("height"), i => i.MatchLdcI4(2),
            i => i.MatchDiv(), i => i.MatchConvR4(), i => i.MatchAdd(), i => i.MatchLdcR4(8),
            i => i.MatchSub(), i => i.MatchNewobj(out _), i => i.MatchLdarg0(), i => i.MatchLdfld<Entity>("width"), i => i.MatchLdcI4(12), i => i.MatchAdd(),
            i => i.MatchLdcI4(24), i => i.MatchLdcI4(35));
        c.EmitDelegate<Func<int, int>>(type => Active ? Style.SplashDustID : type);
        c.GotoNext(MoveType.After, i => i.MatchStloc(19), i => i.MatchBr(out _), i => i.MatchLdarg0(), i => i.MatchLdflda<Entity>("position"),
            i => i.MatchLdfld<Vector2>("X"), i => i.MatchLdcR4(6), i => i.MatchSub(), i => i.MatchLdarg0(),
            i => i.MatchLdflda<Entity>("position"), i => i.MatchLdfld<Vector2>("Y"), i => i.MatchLdarg0(), i => i.MatchLdfld<Entity>("height"), i => i.MatchLdcI4(2),
            i => i.MatchDiv(), i => i.MatchConvR4(), i => i.MatchAdd(), i => i.MatchLdcR4(8),
            i => i.MatchSub(), i => i.MatchNewobj(out _), i => i.MatchLdarg0(), i => i.MatchLdfld<Entity>("width"), i => i.MatchLdcI4(12), i => i.MatchAdd(),
            i => i.MatchLdcI4(24), i => i.MatchLdcI4(35));
        c.EmitDelegate<Func<int, int>>(type => Active ? Style.SplashDustID : type);
    }

    private void SplashPlayerLava(ILContext il)
    {
        var c = new ILCursor(il);
        c.GotoNext(MoveType.After, i => i.MatchStloc(172), i => i.MatchBr(out _), i => i.MatchLdarg0(), i => i.MatchLdflda<Entity>("position"),
            i => i.MatchLdfld<Vector2>("X"), i => i.MatchLdcR4(6), i => i.MatchSub(), i => i.MatchLdarg0(),
            i => i.MatchLdflda<Entity>("position"), i => i.MatchLdfld<Vector2>("Y"), i => i.MatchLdarg0(), i => i.MatchLdfld<Entity>("height"), i => i.MatchLdcI4(2),
            i => i.MatchDiv(), i => i.MatchConvR4(), i => i.MatchAdd(), i => i.MatchLdcR4(8),
            i => i.MatchSub(), i => i.MatchNewobj(out _), i => i.MatchLdarg0(), i => i.MatchLdfld<Entity>("width"), i => i.MatchLdcI4(12), i => i.MatchAdd(),
            i => i.MatchLdcI4(24), i => i.MatchLdcI4(35));
        c.EmitDelegate<Func<int, int>>(type => Active ? Style.SplashDustID : type);
        c.GotoNext(MoveType.After, i => i.MatchStloc(180), i => i.MatchBr(out _), i => i.MatchLdarg0(), i => i.MatchLdflda<Entity>("position"),
            i => i.MatchLdfld<Vector2>("X"), i => i.MatchLdcR4(6), i => i.MatchSub(), i => i.MatchLdarg0(),
            i => i.MatchLdflda<Entity>("position"), i => i.MatchLdfld<Vector2>("Y"), i => i.MatchLdarg0(), i => i.MatchLdfld<Entity>("height"), i => i.MatchLdcI4(2),
            i => i.MatchDiv(), i => i.MatchConvR4(), i => i.MatchAdd(), i => i.MatchLdcR4(8),
            i => i.MatchSub(), i => i.MatchNewobj(out _), i => i.MatchLdarg0(), i => i.MatchLdfld<Entity>("width"), i => i.MatchLdcI4(12), i => i.MatchAdd(),
            i => i.MatchLdcI4(24), i => i.MatchLdcI4(35));
        c.EmitDelegate<Func<int, int>>(type => Active ? Style.SplashDustID : type);
    }

    private void DrawLavatoCapture(ILContext il)
    {
        var c = new ILCursor(il);
        c.GotoNext(MoveType.After, i => i.MatchLdsfld<Main>("liquidAlpha"), i => i.MatchCall(out _), i => i.MatchStloc2());
        var alphaSave = lavaLiquidAlpha.ToArray();
        c.EmitDelegate(() => { alphaSave = lavaLiquidAlpha.ToArray(); });
        c.GotoNext(MoveType.Before, i => i.MatchLdcI4(0), i => i.MatchStloc(34), i => i.MatchBr(out _), i => i.MatchLdloc(34), i => i.MatchLdcI4(1),
            i => i.MatchBeq(out _));
        c.EmitLdloc(8);
        c.EmitDelegate((CaptureBiome biome) =>
        {
            // TODO: lavaLiquid alpha either 0f or 1f here
        });
        c.GotoNext(MoveType.After, i => i.MatchLdarg0(), i => i.MatchLdcI4(1), i => i.MatchLdsfld<Main>("waterStyle"), i => i.MatchLdcR4(1), i => i.MatchLdcI4(1),
            i => i.MatchCall<Main>("DrawLiquid"));
        c.EmitDelegate(() => { DrawLiquid(true, Style); });
        c.GotoNext(MoveType.After, i => i.MatchLdarg0(), i => i.MatchLdcI4(1), i => i.MatchLdsfld<Main>("bloodMoon"), i => i.MatchBrtrue(out _), i => i.MatchLdloc(8),
            i => i.MatchLdfld<CaptureBiome>("WaterStyle"), i => i.MatchBr(out _), i => i.MatchLdcI4(9), i => i.MatchLdcR4(1), i => i.MatchLdcI4(1),
            i => i.MatchCall<Main>("DrawLiquid"));
        c.EmitDelegate(() => { DrawLiquid(true, Style); });
        c.GotoNext(MoveType.After, i => i.MatchLdarg0(), i => i.MatchLdcI4(0), i => i.MatchLdsfld<Main>("waterStyle"), i => i.MatchLdcR4(1), i => i.MatchLdcI4(1),
            i => i.MatchCall<Main>("DrawLiquid"));
        c.EmitDelegate(() => { DrawLiquid(false, Style); });
        c.GotoNext(MoveType.After, i => i.MatchLdarg0(), i => i.MatchLdcI4(0), i => i.MatchLdloc(8), i => i.MatchLdfld<CaptureBiome>("WaterStyle"), i => i.MatchLdcR4(1),
            i => i.MatchLdcI4(1), i => i.MatchCall<Main>("DrawLiquid"));
        c.EmitDelegate(() => { DrawLiquid(false, Style); });
        c.GotoNext(MoveType.After, i => i.MatchLdloc2(), i => i.MatchStsfld<Main>("liquidAlpha"));
        c.EmitDelegate(() => { lavaLiquidAlpha = alphaSave; });
    }

    private void LavaLightEditor(On_TileLightScanner.orig_ApplyLiquidLight orig, TileLightScanner self, Tile tile, ref Vector3 lightColor)
    {
        orig.Invoke(self, tile, ref lightColor);
        if (tile.LiquidType == LiquidID.Lava)
        {
            var num = Style.LightColor.R / 255f;
            var num2 = Style.LightColor.G / 255f;
            var num3 = Style.LightColor.B / 255f;
            var colorManipulator = (float)(270 - Main.mouseTextColor) / 900f;
            num += colorManipulator;
            num2 += colorManipulator;
            num3 += colorManipulator;
            if (lightColor.X < num) lightColor.X = num;

            if (lightColor.Y < num2) lightColor.Y = num2;

            if (lightColor.Z < num3) lightColor.Z = num3;
        }
    }

    private void LavaBubbleReplacer(ILContext il)
    {
        var c = new ILCursor(il);
        c.GotoNext(MoveType.After, i => i.MatchLdcI4(16), i => i.MatchLdcI4(16), i => i.MatchLdcI4(35));
        c.EmitDelegate<Func<int, int>>(type => Active ? Style.SplashDustID : type);
        c.GotoNext(MoveType.After, i => i.MatchLdcI4(16), i => i.MatchLdcI4(8), i => i.MatchLdcI4(35));
        c.EmitDelegate<Func<int, int>>(type => Active ? Style.SplashDustID : type);
    }

    private void BlockLavaDrawingForSlopes(On_TileDrawing.orig_DrawTile_LiquidBehindTile orig, TileDrawing self, bool solidLayer, bool inFrontOfPlayers,
        int waterStyleOverride, Vector2 screenPosition, Vector2 screenOffset, int tileX, int tileY, Tile tileCache)
    {
        var tile = Main.tile[tileX + 1, tileY];
        var tile2 = Main.tile[tileX - 1, tileY];
        var tile3 = Main.tile[tileX, tileY - 1];
        var tile4 = Main.tile[tileX, tileY + 1];
        if (tileCache.LiquidType == LiquidID.Lava || tile.LiquidType == LiquidID.Lava || tile2.LiquidType == LiquidID.Lava || tile3.LiquidType == LiquidID.Lava ||
            tile4.LiquidType == LiquidID.Lava)
            return;

        orig.Invoke(self, solidLayer, inFrontOfPlayers, waterStyleOverride, screenPosition, screenOffset, tileX, tileY, tileCache);
    }

    private void AddTileLiquidDrawing(ILContext il)
    {
        var c = new ILCursor(il);
        c.GotoNext(MoveType.After, i => i.MatchLdarg0(), i => i.MatchLdarg1(), i => i.MatchLdcI4(0), i => i.MatchLdarg(out var waterStyleOverride), i => i.MatchLdloc1(),
            i => i.MatchLdloc2(), i => i.MatchLdloc(12), i => i.MatchLdloc(13), i => i.MatchLdloc(14), i => i.MatchCall<TileDrawing>("DrawTile_LiquidBehindTile"));
        c.EmitLdloc1();
        c.EmitLdloc2();
        c.EmitLdloc(12);
        c.EmitLdloc(13);
        c.EmitLdloc(14);
        c.EmitDelegate((Vector2 unscaledPosition, Vector2 vector, int j, int i, Tile tile) => { DrawTile_LiquidBehindTile(false, false, Style, unscaledPosition, vector, j, i, tile); });
    }

    private void BlockRetroLightingLava(ILContext il)
    {
        var c = new ILCursor(il);
        ILLabel l = null;
        c.GotoNext(MoveType.After, i => i.MatchCgt(), i => i.MatchLdarg1(), i => i.MatchOr(), i => i.MatchBrfalse(out l));
        if (l == null) return;
        c.EmitLdloc(12);
        c.EmitLdloc(11);
        c.EmitDelegate((int i, int j) => { return Main.tile[i, j].LiquidType == LiquidID.Lava; });
        c.EmitBrtrue(l);
    }

    private void BlockLavaDrawing(ILContext il)
    {
        var c = new ILCursor(il);
        var l = c.DefineLabel();
        c.GotoNext(MoveType.After, i => i.MatchLdloc2(), i => i.MatchLdfld<LiquidDrawCache>("Type"), i => i.MatchStloc(8));
        c.EmitLdloc3();
        c.EmitLdloc(4);
        c.EmitDelegate((int i, int j) => { return Main.tile[i, j].LiquidType == 1; });
        c.EmitBrtrue(l);
        c.GotoNext(MoveType.Before, i => i.MatchLdloc(4), i => i.MatchLdcI4(1), i => i.MatchAdd(), i => i.MatchStloc(4));
        l.Target = c.Next;
    }

    private void IL_Main_DoDraw(ILContext il)
    {
        var c = new ILCursor(il);
        c.GotoNext(MoveType.After, i => i.MatchLdsfld<Main>("drawToScreen"), i => i.MatchBrfalse(out _), i => i.MatchLdarg0(), i => i.MatchLdcI4(1),
            i => i.MatchCall<Main>("DrawWaters"));
        c.EmitDelegate(() => { DrawLavas(true); });
        c.GotoNext(MoveType.After, i => i.MatchLdsfld<Main>("drawToScreen"), i => i.MatchBrfalse(out _), i => i.MatchLdarg0(), i => i.MatchLdcI4(0),
            i => i.MatchCall<Main>("DrawWaters"));
        c.EmitDelegate(() => { DrawLavas(); });
    }

    private void IL_Main_RenderWater(ILContext il)
    {
        var c = new ILCursor(il);
        c.GotoNext(MoveType.After, i => i.MatchLdarg0(), i => i.MatchLdcI4(0), i => i.MatchCall<Main>("DrawWaters"));
        c.EmitDelegate(() => { DrawLavas(); });
    }

    private void IL_Main_RenderBackground(ILContext il)
    {
        var c = new ILCursor(il);
        c.GotoNext(MoveType.After, i => i.MatchLdarg0(), i => i.MatchLdcI4(1), i => i.MatchCall<Main>("DrawWaters"));
        c.EmitDelegate(() => { DrawLavas(true); });
    }

    #endregion

    #region Lava Drawing

    private void DrawLavas(bool isBackground = false)
    {
        Main.drewLava = false;
        if (!isBackground)
        {
            // TODO: lavaLiquidAlpha
            /*
            for (var i = 0; i < 7; i++)
            {
                if (lavaStyle != i)
                    lavaLiquidAlpha[i] = Math.Max(lavaLiquidAlpha[i] - 0.2f, 0f);
                else
                    lavaLiquidAlpha[i] = Math.Min(lavaLiquidAlpha[i] + 0.2f, 1f);
            }//*/
        }

        if (!Main.drawToScreen && !isBackground)
        {
            var vector = (Vector2)(Main.drawToScreen ? Vector2.Zero : new Vector2((float)Main.offScreenRange, (float)Main.offScreenRange));
            var val = (int)((Main.Camera.ScaledPosition.X - vector.X) / 16f - 1f);
            var val2 = (int)((Main.Camera.ScaledPosition.X + Main.Camera.ScaledSize.X + vector.X) / 16f) + 2;
            var val3 = (int)((Main.Camera.ScaledPosition.Y - vector.Y) / 16f - 1f);
            var val4 = (int)((Main.Camera.ScaledPosition.Y + Main.Camera.ScaledSize.Y + vector.Y) / 16f) + 5;
            val = Math.Max(val, 5) - 2;
            val3 = Math.Max(val3, 5);
            val2 = Math.Min(val2, Main.maxTilesX - 5) + 2;
            val4 = Math.Min(val4, Main.maxTilesY - 5) + 4;
            Rectangle drawArea = new(val, val3, val2 - val, val4 - val3);
            //LiquidRenderer.Instance.PrepareDraw(drawArea);
        }

        DrawLiquid(isBackground, Style, 1f); // TODO: lavaLiquidStyle
    }

    protected internal void DrawLiquid(bool bg = false, ModLavaStyle lavaStyle = default, float Alpha = 1f, bool drawSinglePassLiquids = true)
    {
        if (!Lighting.NotRetro)
        {
            oldDrawWater(bg, lavaStyle, Alpha);
            return;
        }

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var drawOffset = (Vector2)(Main.drawToScreen ? Vector2.Zero : new Vector2((float)Main.offScreenRange, (float)Main.offScreenRange)) - Main.screenPosition;
        if (bg) DrawLiquidBehindTiles(lavaStyle);

        DrawLava(Main.spriteBatch, drawOffset, lavaStyle, Alpha, bg);
        if (!bg) TimeLogger.DrawTime(4, stopwatch.Elapsed.TotalMilliseconds);
    }

    public unsafe void DrawLava(SpriteBatch spriteBatch, Vector2 drawOffset, ModLavaStyle LavaStyle, float globalAlpha, bool isBackgroundDraw)
    {
        Main.tileBatch.End();
        var drawArea = LiquidRenderer.Instance._drawArea;
        Main.tileBatch.Begin();
        fixed (LiquidDrawCache* ptr3 = &LiquidRenderer.Instance._drawCache[0])
        {
            var ptr2 = ptr3;
            for (var i = drawArea.X; i < drawArea.X + drawArea.Width; i++)
            {
                for (var j = drawArea.Y; j < drawArea.Y + drawArea.Height; j++)
                {
                    if (ptr2->IsVisible && ptr2->Type == 1)
                    {
                        var sourceRectangle = ptr2->SourceRectangle;
                        if (ptr2->IsSurfaceLiquid)
                            sourceRectangle.Y = 1280;
                        else
                            sourceRectangle.Y += LiquidRenderer.Instance._animationFrame * 80;

                        var liquidOffset = ptr2->LiquidOffset;
                        var num = globalAlpha;
                        var num2 = LavaStyle;
                        Lighting.GetCornerColors(i, j, out var vertices);
                        ref var bottomLeftColor = ref vertices.BottomLeftColor;
                        bottomLeftColor *= num;
                        ref var bottomRightColor = ref vertices.BottomRightColor;
                        bottomRightColor *= num;
                        ref var topLeftColor = ref vertices.TopLeftColor;
                        topLeftColor *= num;
                        ref var topRightColor = ref vertices.TopRightColor;
                        topRightColor *= num;
                        Main.DrawTileInWater(drawOffset, i, j);
                        Main.tileBatch.Draw(LavaStyle.LavaTexture.Value, new Vector2((float)(i << 4), (float)(j << 4)) + drawOffset + liquidOffset, sourceRectangle,
                            vertices, Vector2.Zero, 1f, (SpriteEffects)0);
                    }

                    ptr2++;
                }
            }
        }

        Main.tileBatch.End();
    }

    public void oldDrawWater(bool bg = false, ModLavaStyle Style = default, float Alpha = 1f)
    {
        var num = 0f;
        var num12 = 99999f;
        var num23 = 99999f;
        var num27 = -1;
        var num28 = -1;
        Vector2 vector = new((float)Main.offScreenRange, (float)Main.offScreenRange);
        if (Main.drawToScreen) vector = Vector2.Zero;

        _ = new Color[4];
        var num29 = (int)(255f * (1f - Main.gfxQuality) + 40f * Main.gfxQuality);
        _ = Main.gfxQuality;
        _ = Main.gfxQuality;
        var num30 = (int)((Main.screenPosition.X - vector.X) / 16f - 1f);
        var num31 = (int)((Main.screenPosition.X + (float)Main.screenWidth + vector.X) / 16f) + 2;
        var num32 = (int)((Main.screenPosition.Y - vector.Y) / 16f - 1f);
        var num2 = (int)((Main.screenPosition.Y + (float)Main.screenHeight + vector.Y) / 16f) + 5;
        if (num30 < 5) num30 = 5;

        if (num31 > Main.maxTilesX - 5) num31 = Main.maxTilesX - 5;

        if (num32 < 5) num32 = 5;

        if (num2 > Main.maxTilesY - 5) num2 = Main.maxTilesY - 5;

        Vector2 vector2;
        Rectangle value;
        Color newColor;
        for (var i = num32; i < num2 + 4; i++)
        {
            for (var j = num30 - 2; j < num31 + 2; j++)
            {
                if (Main.tile[j, i].LiquidAmount <= 0 ||
                    (Main.tile[j, i].HasUnactuatedTile && Main.tileSolid[Main.tile[j, i].TileType] && !Main.tileSolidTop[Main.tile[j, i].TileType]) ||
                    !(Lighting.Brightness(j, i) > 0f || bg) || Main.tile[j, i].LiquidType != LiquidID.Lava)
                    continue;

                var color = Lighting.GetColor(j, i);
                float num3 = 256 - Main.tile[j, i].LiquidAmount;
                num3 /= 32f;
                var num4 = 0;
                if (Main.tile[j, i].LiquidType == LiquidID.Lava)
                {
                    if (Main.drewLava) continue;

                    var num5 = Math.Abs((float)(j * 16 + 8) - (Main.screenPosition.X + (float)(Main.screenWidth / 2)));
                    var num6 = Math.Abs((float)(i * 16 + 8) - (Main.screenPosition.Y + (float)(Main.screenHeight / 2)));
                    if (num5 < (float)(Main.screenWidth * 2) && num6 < (float)(Main.screenHeight * 2))
                    {
                        var num7 = (float)Math.Sqrt(num5 * num5 + num6 * num6);
                        var num8 = 1f - num7 / ((float)Main.screenWidth * 0.75f);
                        if (num8 > 0f) num += num8;
                    }

                    if (num5 < num12)
                    {
                        num12 = num5;
                        num27 = j * 16 + 8;
                    }

                    if (num6 < num23)
                    {
                        num23 = num5;
                        num28 = i * 16 + 8;
                    }
                }

                if (Main.drewLava) continue;

                var num9 = 0.5f;
                if (bg) num9 = 1f;

                if (num4 != 1 && num4 != 11) num9 *= Alpha;

                Main.DrawTileInWater(-Main.screenPosition + vector, j, i);
                vector2 = new Vector2((float)(j * 16), (float)(i * 16 + (int)num3 * 2));
                value = new Rectangle(0, 0, 16, 16 - (int)num3 * 2);
                var flag2 = true;
                if (Main.tile[j, i + 1].LiquidAmount < 245 && (!Main.tile[j, i + 1].HasUnactuatedTile || !Main.tileSolid[Main.tile[j, i + 1].TileType] ||
                                                               Main.tileSolidTop[Main.tile[j, i + 1].TileType]))
                {
                    float num10 = 256 - Main.tile[j, i + 1].LiquidAmount;
                    num10 /= 32f;
                    num9 = 0.5f * (8f - num3) / 4f;
                    if ((double)num9 > 0.55) num9 = 0.55f;

                    if ((double)num9 < 0.35) num9 = 0.35f;

                    var num11 = num3 / 2f;
                    if (Main.tile[j, i + 1].LiquidAmount < 200)
                    {
                        if (bg) continue;

                        if (Main.tile[j, i - 1].LiquidAmount > 0 && Main.tile[j, i - 1].LiquidAmount > 0)
                        {
                            value = new Rectangle(0, 4, 16, 16);
                            num9 = 0.5f;
                        }
                        else if (Main.tile[j, i - 1].LiquidAmount > 0)
                        {
                            vector2 = new Vector2((float)(j * 16), (float)(i * 16 + 4));
                            value = new Rectangle(0, 4, 16, 12);
                            num9 = 0.5f;
                        }
                        else if (Main.tile[j, i + 1].LiquidAmount > 0)
                        {
                            vector2 = new Vector2((float)(j * 16), (float)(i * 16 + (int)num3 * 2 + (int)num10 * 2));
                            value = new Rectangle(0, 4, 16, 16 - (int)num3 * 2);
                        }
                        else
                        {
                            vector2 = new Vector2((float)(j * 16 + (int)num11), (float)(i * 16 + (int)num11 * 2 + (int)num10 * 2));
                            value = new Rectangle(0, 4, 16 - (int)num11 * 2, 16 - (int)num11 * 2);
                        }
                    }
                    else
                    {
                        num9 = 0.5f;
                        value = new Rectangle(0, 4, 16, 16 - (int)num3 * 2 + (int)num10 * 2);
                    }
                }
                else if (Main.tile[j, i - 1].LiquidAmount > 32)
                {
                    value = new Rectangle(0, 4, value.Width, value.Height);
                }
                else if (num3 < 1f && Main.tile[j, i - 1].HasUnactuatedTile && Main.tileSolid[Main.tile[j, i - 1].TileType] &&
                         !Main.tileSolidTop[Main.tile[j, i - 1].TileType])
                {
                    vector2 = new Vector2((float)(j * 16), (float)(i * 16));
                    value = new Rectangle(0, 4, 16, 16);
                }
                else
                {
                    for (var k = i + 1;
                         k < i + 6 && (!Main.tile[j, k].HasUnactuatedTile || !Main.tileSolid[Main.tile[j, k].TileType] || Main.tileSolidTop[Main.tile[j, k].TileType]);
                         k++)
                    {
                        if (Main.tile[j, k].LiquidAmount < 200)
                        {
                            flag2 = false;
                            break;
                        }
                    }

                    if (!flag2)
                    {
                        num9 = 0.5f;
                        value = new Rectangle(0, 4, 16, 16);
                    }
                    else if (Main.tile[j, i - 1].LiquidAmount > 0)
                    {
                        value = new Rectangle(0, 2, value.Width, value.Height);
                    }
                }

                if ((color.R > 20 || color.B > 20 || color.G > 20) && value.Y < 4)
                {
                    int num13 = color.R;
                    if (color.G > num13) num13 = color.G;

                    if (color.B > num13) num13 = color.B;

                    num13 /= 30;
                    if (Main.rand.Next(20000) < num13)
                    {
                        newColor = new Color(255, 255, 255);
                        var num14 = Dust.NewDust(new Vector2((float)(j * 16), vector2.Y - 2f), 16, 8, 43, 0f, 0f, 254, newColor, 0.75f);
                        var obj = Main.dust[num14];
                        obj.velocity *= 0f;
                    }
                }

                if (Main.tile[j, i].LiquidType == LiquidID.Lava)
                {
                    num9 *= 1.8f;
                    if (num9 > 1f) num9 = 1f;

                    if (Main.instance.IsActive && !Main.gamePaused && Dust.lavaBubbles < 200)
                    {
                        if (Main.tile[j, i].LiquidAmount > 200 && Main.rand.Next(700) == 0) Dust.NewDust(new Vector2((float)(j * 16), (float)(i * 16)), 16, 16, 35);

                        if (value.Y == 0 && Main.rand.Next(350) == 0)
                        {
                            var num15 = Dust.NewDust(new Vector2((float)(j * 16), (float)(i * 16) + num3 * 2f - 8f), 16, 8, Style.SplashDustID, 0f, 0f, 50,
                                default, 1.5f);
                            var obj2 = Main.dust[num15];
                            obj2.velocity *= 0.8f;
                            Main.dust[num15].velocity.X *= 2f;
                            Main.dust[num15].velocity.Y -= (float)Main.rand.Next(1, 7) * 0.1f;
                            if (Main.rand.Next(10) == 0) Main.dust[num15].velocity.Y *= Main.rand.Next(2, 5);

                            Main.dust[num15].noGravity = true;
                        }
                    }
                }

                var num16 = (float)(int)color.R * num9;
                var num17 = (float)(int)color.G * num9;
                var num18 = (float)(int)color.B * num9;
                var num19 = (float)(int)color.A * num9;
                color = new Color((int)(byte)num16, (int)(byte)num17, (int)(byte)num18, (int)(byte)num19);
                if (Lighting.NotRetro && !bg)
                {
                    var color2 = color;
                    if (num4 != 1 && ((double)(int)color2.R > (double)num29 * 0.6 || (double)(int)color2.G > (double)num29 * 0.65 ||
                                      (double)(int)color2.B > (double)num29 * 0.7))
                        for (var l = 0; l < 4; l++)
                        {
                            var num20 = 0;
                            var num21 = 0;
                            var width = 8;
                            var height = 8;
                            var color3 = color2;
                            var color4 = Lighting.GetColor(j, i);
                            if (l == 0)
                            {
                                color4 = Lighting.GetColor(j - 1, i - 1);
                                if (value.Height < 8) height = value.Height;
                            }

                            if (l == 1)
                            {
                                color4 = Lighting.GetColor(j + 1, i - 1);
                                num20 = 8;
                                if (value.Height < 8) height = value.Height;
                            }

                            if (l == 2)
                            {
                                color4 = Lighting.GetColor(j - 1, i + 1);
                                num21 = 8;
                                height = 8 - (16 - value.Height);
                            }

                            if (l == 3)
                            {
                                color4 = Lighting.GetColor(j + 1, i + 1);
                                num20 = 8;
                                num21 = 8;
                                height = 8 - (16 - value.Height);
                            }

                            num16 = (float)(int)color4.R * num9;
                            num17 = (float)(int)color4.G * num9;
                            num18 = (float)(int)color4.B * num9;
                            num19 = (float)(int)color4.A * num9;
                            color4 = new Color((int)(byte)num16, (int)(byte)num17, (int)(byte)num18, (int)(byte)num19);
                            color3.R = (byte)(color2.R * 3 + color4.R * 2 / 5);
                            color3.G = (byte)(color2.G * 3 + color4.G * 2 / 5);
                            color3.B = (byte)(color2.B * 3 + color4.B * 2 / 5);
                            color3.A = (byte)(color2.A * 3 + color4.A * 2 / 5);
                            Main.spriteBatch.Draw(Style.LavaBlockTexture.Value, vector2 - Main.screenPosition + new Vector2((float)num20, (float)num21) + vector,
                                (Rectangle?)new Rectangle(value.X + num20, value.Y + num21, width, height), color3, 0f, default, 1f, (SpriteEffects)0, 0f);
                        }
                    else
                        Main.spriteBatch.Draw(Style.LavaBlockTexture.Value, vector2 - Main.screenPosition + vector, (Rectangle?)value, color, 0f, default, 1f,
                            (SpriteEffects)0, 0f);
                }
                else
                {
                    if (value.Y < 4) value.X += (int)(Main.wFrame * 18f);

                    Main.spriteBatch.Draw(Style.LavaBlockTexture.Value, vector2 - Main.screenPosition + vector, (Rectangle?)value, color, 0f, default, 1f,
                        (SpriteEffects)0, 0f);
                }

                if (!Main.tile[j, i + 1].IsHalfBlock) continue;

                color = Lighting.GetColor(j, i + 1);
                num16 = (float)(int)color.R * num9;
                num17 = (float)(int)color.G * num9;
                num18 = (float)(int)color.B * num9;
                num19 = (float)(int)color.A * num9;
                color = new Color((int)(byte)num16, (int)(byte)num17, (int)(byte)num18, (int)(byte)num19);
                vector2 = new Vector2((float)(j * 16), (float)(i * 16 + 16));
                Main.spriteBatch.Draw(Style.LavaBlockTexture.Value, vector2 - Main.screenPosition + vector, (Rectangle?)new Rectangle(0, 4, 16, 8), color, 0f,
                    default, 1f, (SpriteEffects)0, 0f);
                var num22 = 6f;
                var num24 = 0.75f;
                num22 = 4f;
                num24 = 0.5f;
                for (var m = 0; (float)m < num22; m++)
                {
                    var num25 = i + 2 + m;
                    if (WorldGen.SolidTile(j, num25)) break;

                    var num26 = 1f - (float)m / num22;
                    num26 *= num24;
                    vector2 = new Vector2((float)(j * 16), (float)(num25 * 16 - 2));
                    Main.spriteBatch.Draw(Style.LavaBlockTexture.Value, vector2 - Main.screenPosition + vector, (Rectangle?)new Rectangle(0, 18, 16, 16), color * num26,
                        0f, default, 1f, (SpriteEffects)0, 0f);
                }
            }
        }

        if (!Main.drewLava)
        {
            Main.ambientLavaX = num27;
            Main.ambientLavaY = num28;
            Main.ambientLavaStrength = num;
        }

        Main.drewLava = true;
    }

    public void DrawLiquidBehindTiles(ModLavaStyle style)
    {
        var unscaledPosition = Main.Camera.UnscaledPosition;
        Vector2 vector = new((float)Main.offScreenRange, (float)Main.offScreenRange);
        if (Main.drawToScreen) vector = Vector2.Zero;

        GetScreenDrawArea(unscaledPosition, vector + (Main.Camera.UnscaledPosition - Main.Camera.ScaledPosition), out var firstTileX, out var lastTileX,
            out var firstTileY, out var lastTileY);
        for (var i = firstTileY; i < lastTileY + 4; i++)
        {
            for (var j = firstTileX - 2; j < lastTileX + 2; j++)
            {
                var tile = Main.tile[j, i];
                if (tile != null) DrawTile_LiquidBehindTile(false, false, style, unscaledPosition, vector, j, i, tile);
            }
        }
    }

    private void GetScreenDrawArea(Vector2 screenPosition, Vector2 offSet, out int firstTileX, out int lastTileX, out int firstTileY, out int lastTileY)
    {
        firstTileX = (int)((screenPosition.X - offSet.X) / 16f - 1f);
        lastTileX = (int)((screenPosition.X + (float)Main.screenWidth + offSet.X) / 16f) + 2;
        firstTileY = (int)((screenPosition.Y - offSet.Y) / 16f - 1f);
        lastTileY = (int)((screenPosition.Y + (float)Main.screenHeight + offSet.Y) / 16f) + 5;
        if (firstTileX < 4) firstTileX = 4;

        if (lastTileX > Main.maxTilesX - 4) lastTileX = Main.maxTilesX - 4;

        if (firstTileY < 4) firstTileY = 4;

        if (lastTileY > Main.maxTilesY - 4) lastTileY = Main.maxTilesY - 4;

        if (Main.sectionManager.AnyUnfinishedSections)
        {
            TimeLogger.DetailedDrawReset();
            WorldGen.SectionTileFrameWithCheck(firstTileX, firstTileY, lastTileX, lastTileY);
            TimeLogger.DetailedDrawTime(5);
        }

        if (Main.sectionManager.AnyNeedRefresh) WorldGen.RefreshSections(firstTileX, firstTileY, lastTileX, lastTileY);
    }

    private void DrawTile_LiquidBehindTile(bool solidLayer, bool inFrontOfPlayers, ModLavaStyle style, Vector2 screenPosition, Vector2 screenOffset, int tileX,
        int tileY, Tile tileCache)
    {
        var tile = Main.tile[tileX + 1, tileY];
        var tile2 = Main.tile[tileX - 1, tileY];
        var tile3 = Main.tile[tileX, tileY - 1];
        var tile4 = Main.tile[tileX, tileY + 1];
        if (tileCache.LiquidType != LiquidID.Lava && tile.LiquidType != LiquidID.Lava && tile2.LiquidType != LiquidID.Lava && tile3.LiquidType != LiquidID.Lava &&
            tile4.LiquidType != LiquidID.Lava)
            return;

        //bool[] _tileSolidTop = (bool[])typeof(TileDrawing).GetField("_tileSolidTop", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance).GetValue(null);
        if (!tileCache.HasTile || tileCache.IsActuated /*|| _tileSolidTop[tileCache.TileType]*/ ||
            (tileCache.IsHalfBlock && (tile2.LiquidAmount > 160 || tile.LiquidAmount > 160) && Main.instance.waterfallManager.CheckForWaterfall(tileX, tileY)) ||
            (TileID.Sets.BlocksWaterDrawingBehindSelf[tileCache.TileType] && tileCache.Slope == 0))
            return;

        var num = 0;
        var flag = false;
        var flag2 = false;
        var flag3 = false;
        var flag4 = false;
        var flag5 = false;
        var num2 = 0;
        var flag6 = true;
        var num3 = (int)tileCache.Slope;
        var num4 = (int)tileCache.BlockType;
        if (tileCache.TileType == 546 && tileCache.LiquidAmount > 0)
        {
            flag5 = true;
            flag4 = true;
            flag = true;
            flag2 = true;
            num = tileCache.LiquidAmount;
        }
        else
        {
            if (tileCache.LiquidAmount > 0 && num4 != 0 && (num4 != 1 || tileCache.LiquidAmount > 160))
            {
                flag5 = true;
                if (tileCache.LiquidAmount > num) num = tileCache.LiquidAmount;
            }

            if (tile2.LiquidAmount > 0 && num3 != 1 && num3 != 3)
            {
                flag = true;
                if (tile2.LiquidAmount > num) num = tile2.LiquidAmount;
            }

            if (tile.LiquidAmount > 0 && num3 != 2 && num3 != 4)
            {
                flag2 = true;
                if (tile.LiquidAmount > num) num = tile.LiquidAmount;
            }

            if (tile3.LiquidAmount > 0 && num3 != 3 && num3 != 4) flag3 = true;

            if (tile4.LiquidAmount > 0 && num3 != 1 && num3 != 2)
                if (tile4.LiquidAmount > 240)
                    flag4 = true;
        }

        if (!flag3 && !flag4 && !flag && !flag2 && !flag5) return;

        Lighting.GetCornerColors(tileX, tileY, out var vertices);
        Vector2 vector = new((float)(tileX * 16), (float)(tileY * 16));
        Rectangle liquidSize = new(0, 4, 16, 16);
        if (flag4 && (flag || flag2))
        {
            flag = true;
            flag2 = true;
        }

        if (tileCache.HasTile && (Main.tileSolidTop[tileCache.TileType] || !Main.tileSolid[tileCache.TileType])) return;

        if ((!flag3 || !(flag || flag2)) && !(flag4 && flag3))
        {
            if (flag3)
            {
                liquidSize = new Rectangle(0, 4, 16, 4);
                if (tileCache.IsHalfBlock || tileCache.Slope != 0) liquidSize = new Rectangle(0, 4, 16, 12);
            }
            else if (flag4 && !flag && !flag2)
            {
                vector = new Vector2((float)(tileX * 16), (float)(tileY * 16 + 12));
                liquidSize = new Rectangle(0, 4, 16, 4);
            }
            else
            {
                var num8 = (float)(256 - num) / 32f;
                var y = 4;
                if (tile3.LiquidAmount == 0 && (num4 != 0 || !WorldGen.SolidTile(tileX, tileY - 1))) y = 0;

                var num5 = (int)num8 * 2;
                if (tileCache.Slope != 0)
                {
                    vector = new Vector2((float)(tileX * 16), (float)(tileY * 16 + num5));
                    liquidSize = new Rectangle(0, num5, 16, 16 - num5);
                }
                else if ((flag && flag2) || tileCache.IsHalfBlock)
                {
                    vector = new Vector2((float)(tileX * 16), (float)(tileY * 16 + num5));
                    liquidSize = new Rectangle(0, y, 16, 16 - num5);
                }
                else if (flag)
                {
                    vector = new Vector2((float)(tileX * 16), (float)(tileY * 16 + num5));
                    liquidSize = new Rectangle(0, y, 4, 16 - num5);
                }
                else
                {
                    vector = new Vector2((float)(tileX * 16 + 12), (float)(tileY * 16 + num5));
                    liquidSize = new Rectangle(0, y, 4, 16 - num5);
                }
            }
        }

        var position = vector - screenPosition + screenOffset;
        var num6 = 1f;
        if ((double)tileY <= Main.worldSurface || num6 > 1f)
        {
            num6 = 1f;
            if (tileCache.WallType == 21)
                num6 = 0.9f;
            else if (tileCache.WallType > 0) num6 = 0.6f;
        }

        if (tileCache.IsHalfBlock && tile3.LiquidAmount > 0 && tileCache.WallType > 0) num6 = 0f;

        if (num3 == 4 && tile2.LiquidAmount == 0 && !WorldGen.SolidTile(tileX - 1, tileY)) num6 = 0f;

        if (num3 == 3 && tile.LiquidAmount == 0 && !WorldGen.SolidTile(tileX + 1, tileY)) num6 = 0f;

        ref var bottomLeftColor = ref vertices.BottomLeftColor;
        bottomLeftColor *= num6;
        ref var bottomRightColor = ref vertices.BottomRightColor;
        bottomRightColor *= num6;
        ref var topLeftColor = ref vertices.TopLeftColor;
        topLeftColor *= num6;
        ref var topRightColor = ref vertices.TopRightColor;
        topRightColor *= num6;
        var flag7 = false;
        if (flag6)
            for (var i = 0; i < 7 /*LoaderManager.Get<WaterStylesLoader>().TotalCount*/; i++)
            {
                if (lavaLiquidAlpha[i] > 0f && i != num2)
                {
                    DrawPartialLiquid(!solidLayer, tileCache, ref position, ref liquidSize, i, ref vertices);
                    flag7 = true;
                    break;
                }
            }

        var colors = vertices;
        var num7 = flag7 ? lavaLiquidAlpha[num2] : 1f;
        ref var bottomLeftColor2 = ref colors.BottomLeftColor;
        bottomLeftColor2 *= num7;
        ref var bottomRightColor2 = ref colors.BottomRightColor;
        bottomRightColor2 *= num7;
        ref var topLeftColor2 = ref colors.TopLeftColor;
        topLeftColor2 *= num7;
        ref var topRightColor2 = ref colors.TopRightColor;
        topRightColor2 *= num7;
        DrawPartialLiquid(!solidLayer, tileCache, ref position, ref liquidSize, num2, ref colors);
    }

    private void DrawPartialLiquid(bool behindBlocks, Tile tileCache, ref Vector2 position, ref Rectangle liquidSize, int liquidType, ref VertexColors colors)
    {
        var num = (int)tileCache.Slope;
        var flag = !TileID.Sets.BlocksWaterDrawingBehindSelf[tileCache.TileType];
        if (!behindBlocks) flag = false;

        if (flag || num == 0)
        {
            Main.tileBatch.Draw(Style.LavaBlockTexture.Value, position, liquidSize, colors, default, 1f, (SpriteEffects)0);
            return;
        }

        liquidSize.X += 18 * (num - 1);
        switch (num)
        {
            case 1:
                Main.tileBatch.Draw(Style.LavaBlockTexture.Value, position, liquidSize, colors, Vector2.Zero, 1f, (SpriteEffects)0);
                break;
            case 2:
                Main.tileBatch.Draw(Style.LavaBlockTexture.Value, position, liquidSize, colors, Vector2.Zero, 1f, (SpriteEffects)0);
                break;
            case 3:
                Main.tileBatch.Draw(Style.LavaBlockTexture.Value, position, liquidSize, colors, Vector2.Zero, 1f, (SpriteEffects)0);
                break;
            case 4:
                Main.tileBatch.Draw(Style.LavaBlockTexture.Value, position, liquidSize, colors, Vector2.Zero, 1f, (SpriteEffects)0);
                break;
        }
    }

    #endregion
}
