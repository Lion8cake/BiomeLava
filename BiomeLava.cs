using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.Liquid;
using Terraria.Graphics;
using Terraria.ModLoader;
using MonoMod.Cil;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using System.Reflection;
using ReLogic.Content;
using static Terraria.GameContent.Liquid.LiquidRenderer;
using Terraria.ID;
using Terraria.GameContent.Drawing;
using Terraria.GameContent;
using Terraria.Graphics.Light;
using System.Linq;
using Terraria.Graphics.Capture;
using static Terraria.WaterfallManager;
using BiomeLava.ModLoader;
using BiomeLava.Content.Droplets;
using BiomeLava.Content.Bubbles;
using BiomeLava.Content.Debuffs;
using static Terraria.Localization.NetworkText;
using BiomeLava.ModSupport.AtmosphericLava;

namespace BiomeLava
{
	public class BiomeLava : Mod
	{
		public static int lavaStyle;

		public static float[] lavaLiquidAlpha = new float[7];

		public Asset<Texture2D>[] lavaTextures = new Asset<Texture2D>[7];

		public Asset<Texture2D>[] lavaSlopeTexture = new Asset<Texture2D>[7];

		public Asset<Texture2D>[] lavaBlockTexture = new Asset<Texture2D>[7];

		public Asset<Texture2D>[] lavaWaterfallTexture = new Asset<Texture2D>[7];

		public int[] lavaBubbleDust = new int[7];

		public int[] lavaDripGore = new int[7];

		public int[] lavaExtraDebuff = new int[7];

		public int[] lavaExtraDebuffLength = new int[7];

		public Vector3[] lavaLightColor = new Vector3[7];

		public bool[] lavafallGlowmask = new bool[7];

		public bool[] lavakeepOnFire = new bool[7];

		public int ActiveLavaFountainColor { get; set; }

		public static BiomeLava instance;

		public override void Load()
		{
			instance = this;

			IL_LiquidRenderer.DrawNormalLiquids += BlockLavaDrawing;
			IL_Main.DoDraw += IL_Main_DoDraw;
			IL_Main.RenderWater += IL_Main_RenderWater;
			IL_Main.RenderBackground += IL_Main_RenderBackground;
			IL_Main.DrawCapture += DrawLavatoCapture;

			On_TileDrawing.DrawTile_LiquidBehindTile += BlockLavaDrawingForSlopes;
			On_TileDrawing.DrawPartialLiquid += BlockLavaDrawingForSlopes2;

			IL_TileDrawing.Draw += AddTileLiquidDrawing;
			
			On_TileLightScanner.ApplyLiquidLight += LavaLightEditor;

			On_WaterfallManager.AddLight += LavafallLightEditor;
			On_WaterfallManager.DrawWaterfall_int_int_int_float_Vector2_Rectangle_Color_SpriteEffects += LavafallRemover;
			On_WaterfallManager.Draw += LavaFallRedrawer;
			On_WaterfallManager.StylizeColor += WaterfallGlowmaskEditor;

			IL_Main.oldDrawWater += BlockRetroLightingLava;
			IL_LiquidRenderer.InternalPrepareDraw += LavaBubbleReplacer;
			IL_Player.Update += SplashPlayerLava;
			
			if (ModContent.GetInstance<BiomeLavaConfig>().LavaDebuffs)
			{
				IL_Player.Update += PlayerLavaDebuff;
				IL_NPC.Collision_LavaCollision += LavaDebuffEdits;
			}

			IL_NPC.Collision_WaterCollision += SplashNPCLava;
			IL_Projectile.Update += SplashProjectileLava;
			IL_Item.MoveInWorld += SplashItemLava;

			IL_TileDrawing.EmitLiquidDrops += LavaDropletReplacer;

			if (Terraria.ModLoader.ModLoader.TryGetMod("AtmosphericLava", out _))
			{
				On_Gore.NewGore_IEntitySource_Vector2_Vector2_int_float += replaceGore;
			}

			On_SceneMetrics.Reset += ResetLavaFountainField;
		}

		public override void Unload()
		{
			instance = null;

			IL_LiquidRenderer.DrawNormalLiquids -= BlockLavaDrawing;
			IL_Main.DoDraw -= IL_Main_DoDraw;
			IL_Main.RenderWater -= IL_Main_RenderWater;
			IL_Main.RenderBackground -= IL_Main_RenderBackground;
			IL_Main.DrawCapture -= DrawLavatoCapture;

			On_TileDrawing.DrawTile_LiquidBehindTile -= BlockLavaDrawingForSlopes;
			On_TileDrawing.DrawPartialLiquid -= BlockLavaDrawingForSlopes2;

			IL_TileDrawing.Draw -= AddTileLiquidDrawing;
			
			On_TileLightScanner.ApplyLiquidLight -= LavaLightEditor;

			On_WaterfallManager.AddLight -= LavafallLightEditor;
			On_WaterfallManager.DrawWaterfall_int_int_int_float_Vector2_Rectangle_Color_SpriteEffects -= LavafallRemover;
			On_WaterfallManager.Draw -= LavaFallRedrawer;
			On_WaterfallManager.StylizeColor -= WaterfallGlowmaskEditor;

			IL_Main.oldDrawWater -= BlockRetroLightingLava;
			IL_LiquidRenderer.InternalPrepareDraw -= LavaBubbleReplacer;
			IL_Player.Update -= SplashPlayerLava;
			IL_Player.Update -= PlayerLavaDebuff;
			IL_NPC.Collision_LavaCollision -= LavaDebuffEdits;
			IL_NPC.Collision_WaterCollision -= SplashNPCLava;
			IL_Projectile.Update -= SplashProjectileLava;
			IL_Item.MoveInWorld -= SplashItemLava;

			IL_TileDrawing.EmitLiquidDrops -= LavaDropletReplacer;
			On_Gore.NewGore_IEntitySource_Vector2_Vector2_int_float -= replaceGore;
			On_SceneMetrics.Reset -= ResetLavaFountainField;
		}

		public override void PostSetupContent()
		{
			if (!Main.dedServ)
			{
				for (int i = 0; i < LavaStyleID.Count; i++)
				{
					switch(i)
					{
						case LavaStyleID.Purity:
							lavaTextures[i] = Instance._liquidTextures[WaterStyleID.Lava];
							lavaSlopeTexture[i] = TextureAssets.LiquidSlope[WaterStyleID.Lava];
							lavaBlockTexture[i] = TextureAssets.Liquid[WaterStyleID.Lava];
							lavaBubbleDust[i] = DustID.Lava;
							lavaDripGore[i] = GoreID.LavaDrip;
							lavaLightColor[i] = new Vector3(0.55f, 0.33f, 0.11f);
							lavaWaterfallTexture[i] = Main.instance.waterfallManager.waterfallTexture[WaterfallID.Lava];
							lavafallGlowmask[i] = true;
							lavaExtraDebuff[i] = BuffID.OnFire;
							lavaExtraDebuffLength[i] = 0;
							lavakeepOnFire[i] = true;
							break;
						case LavaStyleID.Corrupt:
							lavaTextures[i] = ModContent.Request<Texture2D>("BiomeLava/Assets/Corruption/CorruptionLava");
							lavaSlopeTexture[i] = ModContent.Request<Texture2D>("BiomeLava/Assets/Corruption/CorruptionLava_Slope");
							lavaBlockTexture[i] = ModContent.Request<Texture2D>("BiomeLava/Assets/Corruption/CorruptionLava_Block");
							lavaBubbleDust[i] = ModContent.DustType<CorruptionLavaDust>();
							lavaDripGore[i] = ModContent.GoreType<CorruptionDroplet>();
							lavaLightColor[i] = new Vector3(0.33f, 0.55f, 0.11f);
							lavaWaterfallTexture[i] = ModContent.Request<Texture2D>("BiomeLava/Assets/Corruption/CorruptionLava_Waterfall");
							lavafallGlowmask[i] = true;
							lavaExtraDebuff[i] = BuffID.CursedInferno;
							lavaExtraDebuffLength[i] = 0;
							lavakeepOnFire[i] = false;
							break;
						case LavaStyleID.Crimson:
							lavaTextures[i] = ModContent.Request<Texture2D>("BiomeLava/Assets/Crimson/CrimsonLava");
							lavaSlopeTexture[i] = ModContent.Request<Texture2D>("BiomeLava/Assets/Crimson/CrimsonLava_Slope");
							lavaBlockTexture[i] = ModContent.Request<Texture2D>("BiomeLava/Assets/Crimson/CrimsonLava_Block");
							lavaBubbleDust[i] = ModContent.DustType<CrimsonLavaDust>();
							lavaDripGore[i] = ModContent.GoreType<CrimsonDroplet>();
							lavaLightColor[i] = new Vector3(0.55f, 0.44f, 0.11f);
							lavaWaterfallTexture[i] = ModContent.Request<Texture2D>("BiomeLava/Assets/Crimson/CrimsonLava_Waterfall");
							lavafallGlowmask[i] = true;
							lavaExtraDebuff[i] = BuffID.Ichor;
							lavaExtraDebuffLength[i] = 60 * 14;
							lavakeepOnFire[i] = true;
							break;
						case LavaStyleID.Hallow:
							lavaTextures[i] = ModContent.Request<Texture2D>("BiomeLava/Assets/Hallow/HallowLava");
							lavaSlopeTexture[i] = ModContent.Request<Texture2D>("BiomeLava/Assets/Hallow/HallowLava_Slope");
							lavaBlockTexture[i] = ModContent.Request<Texture2D>("BiomeLava/Assets/Hallow/HallowLava_Block");
							lavaBubbleDust[i] = ModContent.DustType<HallowLavaDust>();
							lavaDripGore[i] = ModContent.GoreType<HallowDroplet>();
							lavaLightColor[i] = new Vector3(0.33f, 0.77f, 0.99f);
							lavaWaterfallTexture[i] = ModContent.Request<Texture2D>("BiomeLava/Assets/Hallow/HallowLava_Waterfall");
							lavafallGlowmask[i] = true;
							lavaExtraDebuff[i] = ModContent.BuffType<HolyFire>();
							lavaExtraDebuffLength[i] = 0;
							lavakeepOnFire[i] = false;
							break;
						case LavaStyleID.Jungle:
							lavaTextures[i] = ModContent.Request<Texture2D>("BiomeLava/Assets/Jungle/JungleLava");
							lavaSlopeTexture[i] = ModContent.Request<Texture2D>("BiomeLava/Assets/Jungle/JungleLava_Slope");
							lavaBlockTexture[i] = ModContent.Request<Texture2D>("BiomeLava/Assets/Jungle/JungleLava_Block");
							lavaBubbleDust[i] = ModContent.DustType<JungleLavaDust>();
							lavaDripGore[i] = ModContent.GoreType<JungleDroplet>();
							lavaLightColor[i] = new Vector3(0f, 0f, 0f);
							lavaWaterfallTexture[i] = ModContent.Request<Texture2D>("BiomeLava/Assets/Jungle/JungleLava_Waterfall");
							lavafallGlowmask[i] = false;
							lavaExtraDebuff[i] = ModContent.BuffType<Tarred>();
							lavaExtraDebuffLength[i] = 60 * 14;
							lavakeepOnFire[i] = false;
							break;
						case LavaStyleID.Snow:
							lavaTextures[i] = ModContent.Request<Texture2D>("BiomeLava/Assets/Ice/IceLava");
							lavaSlopeTexture[i] = ModContent.Request<Texture2D>("BiomeLava/Assets/Ice/IceLava_Slope");
							lavaBlockTexture[i] = ModContent.Request<Texture2D>("BiomeLava/Assets/Ice/IceLava_Block");
							lavaBubbleDust[i] = ModContent.DustType<IceLavaDust>();
							lavaDripGore[i] = ModContent.GoreType<IceDroplet>();
							lavaLightColor[i] = new Vector3(0.44f, 0.22f, 0.11f);
							lavaWaterfallTexture[i] = ModContent.Request<Texture2D>("BiomeLava/Assets/Ice/IceLava_Waterfall");
							lavafallGlowmask[i] = false;
							lavaExtraDebuff[i] = BuffID.Frostburn;
							lavaExtraDebuffLength[i] = 0;
							lavakeepOnFire[i] = false;
							break;
						case LavaStyleID.Desert:
							lavaTextures[i] = ModContent.Request<Texture2D>("BiomeLava/Assets/Desert/DesertLava");
							lavaSlopeTexture[i] = ModContent.Request<Texture2D>("BiomeLava/Assets/Desert/DesertLava_Slope");
							lavaBlockTexture[i] = ModContent.Request<Texture2D>("BiomeLava/Assets/Desert/DesertLava_Block");
							lavaBubbleDust[i] = ModContent.DustType<DesertLavaDust>();
							lavaDripGore[i] = ModContent.GoreType<IceDroplet>();
							lavaLightColor[i] = new Vector3(0.57f, 0.34f, 0.06f);
							lavaWaterfallTexture[i] = ModContent.Request<Texture2D>("BiomeLava/Assets/Desert/DesertLava_Waterfall");
							lavafallGlowmask[i] = true;
							lavaExtraDebuff[i] = BuffID.OnFire;
							lavaExtraDebuffLength[i] = 0;
							lavakeepOnFire[i] = true;
							break;
					}
				}
				if (Terraria.ModLoader.ModLoader.TryGetMod("AtmosphericLava", out _))
				{
					lavaLightColor[LavaStyleID.Purity] = new Vector3(0.55f, 0.1f, 0.05f);
				}
			}
		}

		#region ILEditsandDetours
		private void ResetLavaFountainField(On_SceneMetrics.orig_Reset orig, SceneMetrics self)
		{
			orig.Invoke(self);
			ActiveLavaFountainColor = -1;
		}

		private int replaceGore(On_Gore.orig_NewGore_IEntitySource_Vector2_Vector2_int_float orig, Terraria.DataStructures.IEntitySource source, Vector2 Position, Vector2 Velocity, int Type, float Scale)
		{
			Terraria.ModLoader.ModLoader.TryGetMod("AtmosphericLava", out Mod mod);
			if (Type == mod.Find<ModGore>("LavaBubbles").Type)
			{
				Type = ModContent.GoreType<LavaBubbles>();
			}
			return orig.Invoke(source, Position, Velocity, Type, Scale);
		}

		private Color WaterfallGlowmaskEditor(On_WaterfallManager.orig_StylizeColor orig, float alpha, int maxSteps, int waterfallType, int y, int s, Tile tileCache, Color aColor)
		{
			if (lavafallGlowmask[lavaStyle] == false)
			{
				return aColor;
			}
			else
			{
				return orig.Invoke(alpha, maxSteps, waterfallType, y, s, tileCache, aColor);
			}
		}

		private void LavafallLightEditor(On_WaterfallManager.orig_AddLight orig, int waterfallType, int x, int y)
		{
			if (waterfallType == 1)
			{
				float r = lavaLightColor[lavaStyle].X;
				float g = lavaLightColor[lavaStyle].Y;
				float b = lavaLightColor[lavaStyle].Z;
				LavaStylesLoader.ModifyLight(x, y, lavaStyle, ref r, ref g, ref b);
				if (!(r == 0 && g == 0 && b == 0))
				{
					float r8;
					float num3 = (r8 = (r + (float)(270 - Main.mouseTextColor) / 900f) * 0.4f);
					float g8 = num3 * g;
					float b8 = num3 * b;
					Lighting.AddLight(x, y, r8, g8, b8);
				}
				return;
			}
			orig.Invoke(waterfallType, x, y);
		}

		private void LavaFallRedrawer(On_WaterfallManager.orig_Draw orig, WaterfallManager self, SpriteBatch spriteBatch)
		{
			orig.Invoke(self, spriteBatch);
			InitialDrawLavafall(self);
		}

		private void LavafallRemover(On_WaterfallManager.orig_DrawWaterfall_int_int_int_float_Vector2_Rectangle_Color_SpriteEffects orig, WaterfallManager self, int waterfallType, int x, int y, float opacity, Vector2 position, Rectangle sourceRect, Color color, SpriteEffects effects)
		{
			if (waterfallType == 1)
			{
				return;
			}
			orig.Invoke(self, waterfallType, x, y, opacity, position, sourceRect, color, effects);
		}

		private void LavaDropletReplacer(ILContext il)
		{
			ILCursor c = new ILCursor(il);
			c.GotoNext(MoveType.After, i => i.MatchLdarg(out _), i => i.MatchLdcI4(374), i => i.MatchBneUn(out _), i => i.MatchLdcI4(716));
			c.EmitDelegate<Func<int, int>>(type => lavaDripGore[lavaStyle]);
		}

		private void SplashItemLava(ILContext il)
		{
			ILCursor c = new ILCursor(il);
			c.GotoNext(MoveType.After, i => i.MatchStloc(15), i => i.MatchBr(out _), i => i.MatchLdarg0(), i => i.MatchLdflda<Entity>("position"), i => i.MatchLdfld<Vector2>("X"), i => i.MatchLdcR4(6), i => i.MatchSub(), i => i.MatchLdarg0(),
				i => i.MatchLdflda<Entity>("position"), i => i.MatchLdfld<Vector2>("Y"), i => i.MatchLdarg0(), i => i.MatchLdfld<Entity>("height"), i => i.MatchLdcI4(2), i => i.MatchDiv(), i => i.MatchConvR4(), i => i.MatchAdd(), i => i.MatchLdcR4(8),
				i => i.MatchSub(), i => i.MatchNewobj(out _), i => i.MatchLdarg0(), i => i.MatchLdfld<Entity>("width"), i => i.MatchLdcI4(12), i => i.MatchAdd(), i => i.MatchLdcI4(24), i => i.MatchLdcI4(35));
			c.EmitDelegate<Func<int, int>>(type => lavaBubbleDust[lavaStyle]);
			c.GotoNext(MoveType.After, i => i.MatchStloc(23), i => i.MatchBr(out _), i => i.MatchLdarg0(), i => i.MatchLdflda<Entity>("position"), i => i.MatchLdfld<Vector2>("X"), i => i.MatchLdcR4(6), i => i.MatchSub(), i => i.MatchLdarg0(),
				i => i.MatchLdflda<Entity>("position"), i => i.MatchLdfld<Vector2>("Y"), i => i.MatchLdarg0(), i => i.MatchLdfld<Entity>("height"), i => i.MatchLdcI4(2), i => i.MatchDiv(), i => i.MatchConvR4(), i => i.MatchAdd(), i => i.MatchLdcR4(8),
				i => i.MatchSub(), i => i.MatchNewobj(out _), i => i.MatchLdarg0(), i => i.MatchLdfld<Entity>("width"), i => i.MatchLdcI4(12), i => i.MatchAdd(), i => i.MatchLdcI4(24), i => i.MatchLdcI4(35));
			c.EmitDelegate<Func<int, int>>(type2 => lavaBubbleDust[lavaStyle]);
		}

		private void SplashProjectileLava(ILContext il)
		{
			ILCursor c = new ILCursor(il);
			c.GotoNext(MoveType.After, i => i.MatchStloc(22), i => i.MatchBr(out _), i => i.MatchLdarg0(), i => i.MatchLdflda<Entity>("position"), i => i.MatchLdfld<Vector2>("X"), i => i.MatchLdcR4(6), i => i.MatchSub(), i => i.MatchLdarg0(),
				i => i.MatchLdflda<Entity>("position"), i => i.MatchLdfld<Vector2>("Y"), i => i.MatchLdarg0(), i => i.MatchLdfld<Entity>("height"), i => i.MatchLdcI4(2), i => i.MatchDiv(), i => i.MatchConvR4(), i => i.MatchAdd(), i => i.MatchLdcR4(8),
				i => i.MatchSub(), i => i.MatchNewobj(out _), i => i.MatchLdarg0(), i => i.MatchLdfld<Entity>("width"), i => i.MatchLdcI4(12), i => i.MatchAdd(), i => i.MatchLdcI4(24), i => i.MatchLdcI4(35));
			c.EmitDelegate<Func<int, int>>(type => lavaBubbleDust[lavaStyle]);
			c.GotoNext(MoveType.After, i => i.MatchStloc(30), i => i.MatchBr(out _), i => i.MatchLdarg0(), i => i.MatchLdflda<Entity>("position"), i => i.MatchLdfld<Vector2>("X"), i => i.MatchLdcR4(6), i => i.MatchSub(), i => i.MatchLdarg0(),
				i => i.MatchLdflda<Entity>("position"), i => i.MatchLdfld<Vector2>("Y"), i => i.MatchLdarg0(), i => i.MatchLdfld<Entity>("height"), i => i.MatchLdcI4(2), i => i.MatchDiv(), i => i.MatchConvR4(), i => i.MatchAdd(), i => i.MatchLdcR4(8),
				i => i.MatchSub(), i => i.MatchNewobj(out _), i => i.MatchLdarg0(), i => i.MatchLdfld<Entity>("width"), i => i.MatchLdcI4(12), i => i.MatchAdd(), i => i.MatchLdcI4(24), i => i.MatchLdcI4(35));
			c.EmitDelegate<Func<int, int>>(type2 => lavaBubbleDust[lavaStyle]);
		}

		private void SplashNPCLava(ILContext il)
		{
			ILCursor c = new ILCursor(il);
			c.GotoNext(MoveType.After, i => i.MatchStloc(10), i => i.MatchBr(out _), i => i.MatchLdarg0(), i => i.MatchLdflda<Entity>("position"), i => i.MatchLdfld<Vector2>("X"), i => i.MatchLdcR4(6), i => i.MatchSub(), i => i.MatchLdarg0(),
				i => i.MatchLdflda<Entity>("position"), i => i.MatchLdfld<Vector2>("Y"), i => i.MatchLdarg0(), i => i.MatchLdfld<Entity>("height"), i => i.MatchLdcI4(2), i => i.MatchDiv(), i => i.MatchConvR4(), i => i.MatchAdd(), i => i.MatchLdcR4(8),
				i => i.MatchSub(), i => i.MatchNewobj(out _), i => i.MatchLdarg0(), i => i.MatchLdfld<Entity>("width"), i => i.MatchLdcI4(12), i => i.MatchAdd(), i => i.MatchLdcI4(24), i => i.MatchLdcI4(35));
			c.EmitDelegate<Func<int, int>>(type => lavaBubbleDust[lavaStyle]);
			c.GotoNext(MoveType.After, i => i.MatchStloc(19), i => i.MatchBr(out _), i => i.MatchLdarg0(), i => i.MatchLdflda<Entity>("position"), i => i.MatchLdfld<Vector2>("X"), i => i.MatchLdcR4(6), i => i.MatchSub(), i => i.MatchLdarg0(),
				i => i.MatchLdflda<Entity>("position"), i => i.MatchLdfld<Vector2>("Y"), i => i.MatchLdarg0(), i => i.MatchLdfld<Entity>("height"), i => i.MatchLdcI4(2), i => i.MatchDiv(), i => i.MatchConvR4(), i => i.MatchAdd(), i => i.MatchLdcR4(8),
				i => i.MatchSub(), i => i.MatchNewobj(out _), i => i.MatchLdarg0(), i => i.MatchLdfld<Entity>("width"), i => i.MatchLdcI4(12), i => i.MatchAdd(), i => i.MatchLdcI4(24), i => i.MatchLdcI4(35));
			c.EmitDelegate<Func<int, int>>(type2 => lavaBubbleDust[lavaStyle]);
		}

		private void SplashPlayerLava(ILContext il)
		{
			ILCursor c = new ILCursor(il);
			//Dusts
			c.GotoNext(MoveType.After, i => i.MatchStloc(172), i => i.MatchBr(out _), i => i.MatchLdarg0(), i => i.MatchLdflda<Entity>("position"), i => i.MatchLdfld<Vector2>("X"), i => i.MatchLdcR4(6), i => i.MatchSub(), i => i.MatchLdarg0(), 
				i => i.MatchLdflda<Entity>("position"), i => i.MatchLdfld<Vector2>("Y"), i => i.MatchLdarg0(), i => i.MatchLdfld<Entity>("height"), i => i.MatchLdcI4(2), i => i.MatchDiv(), i => i.MatchConvR4(), i => i.MatchAdd(), i => i.MatchLdcR4(8), 
				i => i.MatchSub(), i => i.MatchNewobj(out _), i => i.MatchLdarg0(), i => i.MatchLdfld<Entity>("width"), i => i.MatchLdcI4(12), i => i.MatchAdd(), i => i.MatchLdcI4(24), i => i.MatchLdcI4(35));
			c.EmitDelegate<Func<int, int>>(type => lavaBubbleDust[lavaStyle]);
			c.GotoNext(MoveType.After, i => i.MatchStloc(180), i => i.MatchBr(out _), i => i.MatchLdarg0(), i => i.MatchLdflda<Entity>("position"), i => i.MatchLdfld<Vector2>("X"), i => i.MatchLdcR4(6), i => i.MatchSub(), i => i.MatchLdarg0(),
				i => i.MatchLdflda<Entity>("position"), i => i.MatchLdfld<Vector2>("Y"), i => i.MatchLdarg0(), i => i.MatchLdfld<Entity>("height"), i => i.MatchLdcI4(2), i => i.MatchDiv(), i => i.MatchConvR4(), i => i.MatchAdd(), i => i.MatchLdcR4(8),
				i => i.MatchSub(), i => i.MatchNewobj(out _), i => i.MatchLdarg0(), i => i.MatchLdfld<Entity>("width"), i => i.MatchLdcI4(12), i => i.MatchAdd(), i => i.MatchLdcI4(24), i => i.MatchLdcI4(35));
			c.EmitDelegate<Func<int, int>>(type2 => lavaBubbleDust[lavaStyle]);
		}

		private void PlayerLavaDebuff(ILContext il)
		{
			ILCursor c = new ILCursor(il);
			c.GotoNext(MoveType.Before, i => i.MatchLdarg0(), i => i.MatchLdcI4(24), i => i.MatchLdloc(161), i => i.MatchLdcI4(1), i => i.MatchLdcI4(0), i => i.MatchCall<Player>("AddBuff"));
			c.EmitLdarg0();
			c.EmitLdloc(161);
			c.EmitDelegate((Player player, int onFiretime) =>
			{
				LavaStylesLoader.InflictDebuff(player, null, lavaStyle, onFiretime);
				if (lavaStyle < LavaStyleID.Count && lavaExtraDebuff[lavaStyle] != BuffID.OnFire)
				{
					player.AddBuff(lavaExtraDebuff[lavaStyle], lavaExtraDebuffLength[lavaStyle] != 0 ? lavaExtraDebuffLength[lavaStyle] : onFiretime);
				}
			});
			//Onfire tamporing
			c.GotoNext(MoveType.Before, i => i.MatchLdloc(161), i => i.MatchLdcI4(1), i => i.MatchLdcI4(0), i => i.MatchCall<Player>("AddBuff"));
			c.EmitDelegate<Func<int, int>>(type => lavakeepOnFire[lavaStyle] ? type : 0);
		}

		private void LavaDebuffEdits(ILContext il)
		{
			ILCursor c = new(il);
			c.GotoNext(MoveType.Before, i => i.MatchLdarg0(), i => i.MatchLdfld<NPC>("immune"), i => i.MatchLdcI4(255), i => i.MatchLdcI4(30), i => i.MatchStelemI4());
			c.EmitLdarg0();
			c.EmitDelegate((NPC npc) =>
			{
				if (Main.netMode == NetmodeID.SinglePlayer)
				{
					LavaStylesLoader.InflictDebuff(null, npc, lavaStyle, Main.remixWorld && !npc.friendly ? 180 : 420);
					if (lavaStyle < LavaStyleID.Count && lavaExtraDebuff[lavaStyle] != BuffID.OnFire)
					{
						if (Main.remixWorld && !npc.friendly)
						{
							npc.AddBuff(lavaExtraDebuff[lavaStyle], lavaExtraDebuffLength[lavaStyle] != 0 ? lavaExtraDebuffLength[lavaStyle] : 180);
						}
						else
						{
							npc.AddBuff(lavaExtraDebuff[lavaStyle], lavaExtraDebuffLength[lavaStyle] != 0 ? lavaExtraDebuffLength[lavaStyle] : 420);
						}
					}
				}
			});
			//Onfire Tamporing
			c.GotoNext(MoveType.Before, i => i.MatchLdcI4(180), i => i.MatchLdcI4(0), i => i.MatchCall<NPC>("AddBuff"), i => i.MatchBr(out _));
			c.EmitDelegate<Func<int, int>>(type => lavakeepOnFire[lavaStyle] || Main.netMode != NetmodeID.SinglePlayer ? type : 0);
			c.GotoNext(MoveType.Before, i => i.MatchLdcI4(420), i => i.MatchLdcI4(0), i => i.MatchCall<NPC>("AddBuff"), i => i.MatchLdarg0());
			c.EmitDelegate<Func<int, int>>(type => lavakeepOnFire[lavaStyle] || Main.netMode != NetmodeID.SinglePlayer ? type : 0);
		}

		private static float[] alphaSave = lavaLiquidAlpha.ToArray();

		private void DrawLavatoCapture(ILContext il)
		{
			ILCursor c = new ILCursor(il);
			c.GotoNext(MoveType.After, i => i.MatchLdsfld<Main>("liquidAlpha"), i => i.MatchCall(out _), i => i.MatchStloc2());
			c.EmitDelegate(() => {
				alphaSave = lavaLiquidAlpha.ToArray();
			});
			c.GotoNext(MoveType.Before, i => i.MatchLdcI4(0), i => i.MatchStloc(34), i => i.MatchBr(out _), i => i.MatchLdloc(34), i => i.MatchLdcI4(1), i => i.MatchBeq(out _));
			c.EmitLdloc(8);
			c.EmitDelegate((CaptureBiome biome) => {
				for (int i = 0; i < 7; i++)
				{
					if (i != 1)
					{
						lavaLiquidAlpha[i] = ((i == lavaStyle) ? 1f : 0f);
					}
				}
			});
			c.GotoNext(MoveType.After, i => i.MatchLdarg0(), i => i.MatchLdcI4(1), i => i.MatchLdsfld<Main>("waterStyle"), i => i.MatchLdcR4(1), i => i.MatchLdcI4(1), i => i.MatchCall<Main>("DrawLiquid"));
			c.EmitDelegate(() => {
				DrawLiquid(bg: true, lavaStyle);
			});
			c.GotoNext(MoveType.After, i => i.MatchLdarg0(), i => i.MatchLdcI4(1), i => i.MatchLdsfld<Main>("bloodMoon"), i => i.MatchBrtrue(out _), i => i.MatchLdloc(8), i => i.MatchLdfld<CaptureBiome>("WaterStyle"), i => i.MatchBr(out _), i => i.MatchLdcI4(9), i => i.MatchLdcR4(1), i => i.MatchLdcI4(1), i => i.MatchCall<Main>("DrawLiquid"));
			c.EmitDelegate(() => {
				DrawLiquid(bg: true, lavaStyle);
			});
			c.GotoNext(MoveType.After, i => i.MatchLdarg0(), i => i.MatchLdcI4(0), i => i.MatchLdsfld<Main>("waterStyle"), i => i.MatchLdcR4(1), i => i.MatchLdcI4(1), i => i.MatchCall<Main>("DrawLiquid"));
			c.EmitDelegate(() => {
				DrawLiquid(bg: false, lavaStyle);
			});
			c.GotoNext(MoveType.After, i => i.MatchLdarg0(), i => i.MatchLdcI4(0), i => i.MatchLdloc(8), i => i.MatchLdfld<CaptureBiome>("WaterStyle"), i => i.MatchLdcR4(1), i => i.MatchLdcI4(1), i => i.MatchCall<Main>("DrawLiquid"));
			c.EmitDelegate(() => {
				DrawLiquid(bg: false, lavaStyle);
			});
			c.GotoNext(MoveType.After, i => i.MatchLdloc2(), i => i.MatchStsfld<Main>("liquidAlpha"));
			c.EmitDelegate(() => {
				lavaLiquidAlpha = alphaSave;
			});
		}

		private void LavaLightEditor(On_TileLightScanner.orig_ApplyLiquidLight orig, TileLightScanner self, Tile tile, ref Vector3 lightColor)
		{
			if (tile.LiquidAmount <= 0)
			{
				return;
			}
			if (tile.LiquidType == LiquidID.Lava)
			{
				float num = lavaLightColor[lavaStyle].X;
				float num2 = lavaLightColor[lavaStyle].Y;
				float num3 = lavaLightColor[lavaStyle].Z;
				LavaStylesLoader.ModifyLight(tile.X(), tile.Y(), lavaStyle, ref num, ref num2, ref num3);
				for (int j = 0; j < LavaStylesLoader.TotalCount; j++)
				{
					if (lavaLiquidAlpha[j] > 0f && j != lavaStyle)
					{
						float r = lavaLightColor[j].X;
						float g = lavaLightColor[j].Y;
						float b = lavaLightColor[j].Z;
						LavaStylesLoader.ModifyLight(tile.X(), tile.Y(), j, ref r, ref g, ref b);
						float r2 = lavaLightColor[lavaStyle].X;
						float g2 = lavaLightColor[lavaStyle].Y;
						float b2 = lavaLightColor[lavaStyle].Z;
						LavaStylesLoader.ModifyLight(tile.X(), tile.Y(), lavaStyle, ref r2, ref g2, ref b2);
						num = Single.Lerp(r, r2, lavaLiquidAlpha[lavaStyle]);
						num2 = Single.Lerp(g, g2, lavaLiquidAlpha[lavaStyle]);
						num3 = Single.Lerp(b, b2, lavaLiquidAlpha[lavaStyle]);
					}
				}
				if (!(num == 0 && num2 == 0 && num3 == 0))
				{
					float colorManipulator = (float)(270 - Main.mouseTextColor) / 900f;
					num += colorManipulator;
					num2 += colorManipulator;
					num3 += colorManipulator;
				}
				if (lightColor.X < num)
				{
					lightColor.X = num;
				}
				if (lightColor.Y < num2)
				{
					lightColor.Y = num2;
				}
				if (lightColor.Z < num3)
				{
					lightColor.Z = num3;
				}
				return;
			}
			orig.Invoke(self, tile, ref lightColor);
		}

		private void LavaBubbleReplacer(ILContext il)
		{
			ILCursor c = new ILCursor(il);
			c.GotoNext(MoveType.After, i => i.MatchLdcI4(16), i => i.MatchLdcI4(16), i => i.MatchLdcI4(35));
			c.EmitDelegate<Func<int, int>>(type => lavaBubbleDust[lavaStyle]);
			c.GotoNext(MoveType.After, i => i.MatchLdcI4(16), i => i.MatchLdcI4(8), i => i.MatchLdcI4(35));
			c.EmitDelegate<Func<int, int>>(type2 => lavaBubbleDust[lavaStyle]);
		}

		private void BlockLavaDrawingForSlopes(On_TileDrawing.orig_DrawTile_LiquidBehindTile orig, TileDrawing self, bool solidLayer, bool inFrontOfPlayers, int waterStyleOverride, Vector2 screenPosition, Vector2 screenOffset, int tileX, int tileY, Tile tileCache)
		{
			Tile tile = Main.tile[tileX + 1, tileY];
			Tile tile2 = Main.tile[tileX - 1, tileY];
			Tile tile3 = Main.tile[tileX, tileY - 1];
			Tile tile4 = Main.tile[tileX, tileY + 1];
			if (tileCache.LiquidType == LiquidID.Lava || tile.LiquidType == LiquidID.Lava || tile2.LiquidType == LiquidID.Lava || tile3.LiquidType == LiquidID.Lava || tile4.LiquidType == LiquidID.Lava)
			{
				return;
			}
			orig.Invoke(self, solidLayer, inFrontOfPlayers, waterStyleOverride, screenPosition, screenOffset, tileX, tileY, tileCache);
		}

		private void BlockLavaDrawingForSlopes2(On_TileDrawing.orig_DrawPartialLiquid orig, TileDrawing self, bool behindBlocks, Tile tileCache, ref Vector2 position, ref Rectangle liquidSize, int liquidType, ref VertexColors colors)
		{
			if (liquidType == 1)
			{
				return;
			}
			orig.Invoke(self, behindBlocks, tileCache, ref position, ref liquidSize, liquidType, ref colors);
		}

		private void AddTileLiquidDrawing(ILContext il)
		{
			ILCursor c = new ILCursor(il);
			c.GotoNext(MoveType.After, i => i.MatchLdarg0(), i => i.MatchLdarg1(), i => i.MatchLdcI4(0), i => i.MatchLdarg(out int waterStyleOverride), i => i.MatchLdloc1(), i => i.MatchLdloc2(), i => i.MatchLdloc(12), i => i.MatchLdloc(13), i => i.MatchLdloc(14), i => i.MatchCall<TileDrawing>("DrawTile_LiquidBehindTile"));
			c.EmitLdloc1();
			c.EmitLdloc2();
			c.EmitLdloc(12);
			c.EmitLdloc(13);
			c.EmitLdloc(14);
			c.EmitDelegate((Microsoft.Xna.Framework.Vector2 unscaledPosition, Microsoft.Xna.Framework.Vector2 vector, int j, int i, Terraria.Tile tile) => {
				DrawTile_LiquidBehindTile(solidLayer: false, inFrontOfPlayers: false, -1, unscaledPosition, vector, j, i, tile);
			});
		}

		private void BlockRetroLightingLava(ILContext il)
		{
			ILCursor c = new ILCursor(il);
			ILLabel l = null;
			c.GotoNext(MoveType.After, i => i.MatchCgt(), i => i.MatchLdarg1(), i => i.MatchOr(), i => i.MatchBrfalse(out l));
			if (l == null) return;
			c.EmitLdloc(12);
			c.EmitLdloc(11);
			c.EmitDelegate((int i, int j) => {
				return Main.tile[i, j].LiquidType == LiquidID.Lava;
			});
			c.EmitBrtrue(l);
		}

		private void BlockLavaDrawing(ILContext il)
		{
			ILCursor c = new ILCursor(il);
			ILLabel IL_0000 = c.DefineLabel();
			c.GotoNext(MoveType.After, i => i.MatchLdloc2(), i => i.MatchLdfld<LiquidRenderer.LiquidDrawCache>("Type"), i => i.MatchStloc(8));
			c.EmitLdloc3();
			c.EmitLdloc(4);
			c.EmitDelegate((int i, int j) => {
				return Main.tile[i, j].LiquidType == 1;
			});
			c.EmitBrtrue(IL_0000);
			c.GotoNext(MoveType.Before, i => i.MatchLdloc(2), i => i.MatchSizeof(typeof(LiquidRenderer).GetNestedType("LiquidDrawCache", BindingFlags.NonPublic)), i => i.MatchAdd(), i => i.MatchStloc(2));
			c.MarkLabel(IL_0000);
		}

		private void IL_Main_DoDraw(ILContext il)
		{
			ILCursor c = new ILCursor(il);
			c.GotoNext(MoveType.After, i => i.MatchLdsfld<Main>("drawToScreen"), i => i.MatchBrfalse(out _), i => i.MatchLdarg0(), i => i.MatchLdcI4(1), i => i.MatchCall<Main>("DrawWaters"));
			c.EmitDelegate(() => {
				DrawLavas(isBackground: true);
			});
			c.GotoNext(MoveType.After, i => i.MatchLdsfld<Main>("drawToScreen"), i => i.MatchBrfalse(out _), i => i.MatchLdarg0(), i => i.MatchLdcI4(0), i => i.MatchCall<Main>("DrawWaters"));
			c.EmitDelegate(() => {
				DrawLavas();
			});
		}

		private void IL_Main_RenderWater(ILContext il)
		{
			ILCursor c = new ILCursor(il);
			c.GotoNext(MoveType.After, i => i.MatchLdarg0(), i => i.MatchLdcI4(0), i => i.MatchCall<Main>("DrawWaters"));
			c.EmitDelegate(() => {
				DrawLavas();
			});
		}

		private void IL_Main_RenderBackground(ILContext il)
		{
			ILCursor c = new ILCursor(il);
			c.GotoNext(MoveType.After, i => i.MatchLdarg0(), i => i.MatchLdcI4(1), i => i.MatchCall<Main>("DrawWaters"));
			c.EmitDelegate(() => {
				DrawLavas(isBackground: true);
			});
		}
		#endregion

		#region ModCalls
		//(Mod mod, String LavaName, Asset texture, Asset block, Asset Slope, Asset Waterfall, int DustID, int GoreID, Vector3 lightColor, bool Zone), *overload1* bool WaterfallGlowmask), *overload2* int BuffID, bool keepOnFire)
		public override object Call(params object[] args)
		{
			return args switch
			{
				["ModLavaStyle", Mod mod, string lavaName, string texture, string block, string slope, string waterfall, Func<int> DustID, Func<int> GoreID, Func<int, int, float, float, float, Vector3> lightColor, Func<bool> IsActive] => LavaStylesLoader.ModCalledLava(mod, lavaName, texture, block, slope, waterfall, DustID, GoreID, lightColor, IsActive, null, null, null),
				["ModLavaStyle", Mod mod, string lavaName, string texture, string block, string slope, string waterfall, Func<int> DustID, Func<int> GoreID, Func<int, int, float, float, float, Vector3> lightColor, Func<bool> IsActive, Func<bool> fallMask] => LavaStylesLoader.ModCalledLava(mod, lavaName, texture, block, slope, waterfall, DustID, GoreID, lightColor, IsActive, fallMask, null, null),
				["ModLavaStyle", Mod mod, string lavaName, string texture, string block, string slope, string waterfall, Func<int> DustID, Func<int> GoreID, Func<int, int, float, float, float, Vector3> lightColor, Func<bool> IsActive, Func<bool> fallMask, Func<Player, NPC, int, Action> BuffID, Func<bool> OnFire] => LavaStylesLoader.ModCalledLava(mod, lavaName, texture, block, slope, waterfall, DustID, GoreID, lightColor, IsActive, fallMask, BuffID, OnFire),
				["ModLavaStyle", Mod mod, string lavaName, string texture, string block, string slope, string waterfall, Func<int> DustID, Func<int> GoreID, Func<int, int, float, float, float, Vector3> lightColor, Func<bool> IsActive, Func<bool> fallMask, Action<Player, NPC, int> BuffID, Func<bool> OnFire] => LavaStylesLoader.ModCalledLava(mod, lavaName, texture, block, slope, waterfall, DustID, GoreID, lightColor, IsActive, fallMask, BuffID, OnFire),
				["GetActiveLavaFountainColor", ] => ActiveLavaFountainColor,
				["SetActiveLavaFountainColor", int lavaStyleID] => ActiveLavaFountainColor = lavaStyleID,
				_ => throw new Exception("BiomeLava: Unknown mod call, make sure you have the correct amount of parameters and those parameters are the correct object!")
			};
		}
		#endregion

		public static int CalculateLavaStyle()
		{
			if (!Main.LocalPlayer.ZoneUnderworldHeight)
			{
				if (instance.ActiveLavaFountainColor >= 0)
				{
					return instance.ActiveLavaFountainColor;
				}
				/*if (Main.SceneMetrics.ActiveFountainColor == WaterStyleID.Corrupt)
				{
					return LavaStyleID.Corrupt; //corrupt lava
				}
				else if (Main.SceneMetrics.ActiveFountainColor == WaterStyleID.Crimson)
				{
					return LavaStyleID.Crimson; //crimson lava
				}
				else if (Main.SceneMetrics.ActiveFountainColor == WaterStyleID.Hallow)
				{
					return LavaStyleID.Hallow; //hallow lava
				}
				else if (Main.SceneMetrics.ActiveFountainColor == WaterStyleID.Jungle)
				{
					return LavaStyleID.Jungle; //Jungle lava
				}
				else if (Main.SceneMetrics.ActiveFountainColor == WaterStyleID.Snow)
				{
					return LavaStyleID.Snow; //Ice lava
				}
				else if (Main.SceneMetrics.ActiveFountainColor == WaterStyleID.Desert || Main.SceneMetrics.ActiveFountainColor == WaterStyleID.UndergroundDesert)
				{
					return LavaStyleID.Desert; //Desert lava
				}*/
				switch (Main.bgStyle)
				{
					case 1:
						return LavaStyleID.Corrupt;
					case 5:
						if (Main.SceneMetrics.EvilTileCount > Main.SceneMetrics.HolyTileCount && Main.SceneMetrics.EvilTileCount > Main.SceneMetrics.BloodTileCount)
						{
							return LavaStyleID.Corrupt;
						}
						if (Main.SceneMetrics.HolyTileCount > Main.SceneMetrics.BloodTileCount)
						{
							return LavaStyleID.Hallow;
						}
						return LavaStyleID.Crimson;
					case 3:
						return LavaStyleID.Jungle;
					case 8:
						return LavaStyleID.Crimson;
					case 6:
						return LavaStyleID.Hallow;
					case 7:
						return LavaStyleID.Snow;
					case 2:
						return LavaStyleID.Desert;
				}
			}
			return LavaStyleID.Purity;
		}

		#region LavaDrawing
		private void DrawLavas(bool isBackground = false)
		{
			Main.drewLava = false;
			if (!isBackground)
			{
				lavaStyle = CalculateLavaStyle();
				LavaStylesLoader.IsLavaActive();
				for (int i = 0; i < LavaStyleID.Count; i++)
				{
					if (lavaStyle != i)
					{
						lavaLiquidAlpha[i] = Math.Max(lavaLiquidAlpha[i] - 0.2f, 0f);
					}
					else
					{
						lavaLiquidAlpha[i] = Math.Min(lavaLiquidAlpha[i] + 0.2f, 1f);
					}
				}
				LavaStylesLoader.UpdateLiquidAlphas();
			}
			/*if (!Main.drawToScreen && !isBackground) //already called through DrawWaters
			{
				Vector2 vector = (Vector2)(Main.drawToScreen ? Vector2.Zero : new Vector2((float)Main.offScreenRange, (float)Main.offScreenRange));
				int val = (int)((Main.Camera.ScaledPosition.X - vector.X) / 16f - 1f);
				int val2 = (int)((Main.Camera.ScaledPosition.X + Main.Camera.ScaledSize.X + vector.X) / 16f) + 2;
				int val3 = (int)((Main.Camera.ScaledPosition.Y - vector.Y) / 16f - 1f);
				int val4 = (int)((Main.Camera.ScaledPosition.Y + Main.Camera.ScaledSize.Y + vector.Y) / 16f) + 5;
				val = Math.Max(val, 5) - 2;
				val3 = Math.Max(val3, 5);
				val2 = Math.Min(val2, Main.maxTilesX - 5) + 2;
				val4 = Math.Min(val4, Main.maxTilesY - 5) + 4;
				Rectangle drawArea = new(val, val3, val2 - val, val4 - val3);
				//LiquidRenderer.Instance.PrepareDraw(drawArea); //already called
			}*/
			bool flag = false;
			for (int j = 0; j < LavaStylesLoader.TotalCount; j++)
			{
				if (lavaLiquidAlpha[j] > 0f && j != lavaStyle)
				{
					DrawLiquid(isBackground, j, isBackground ? 1f : lavaLiquidAlpha[j], drawSinglePassLiquids: false);
					flag = true;
				}
			}
			DrawLiquid(isBackground, lavaStyle, flag ? lavaLiquidAlpha[lavaStyle] : 1f);
		}

		protected internal void DrawLiquid(bool bg = false, int lavaStyle = 0, float Alpha = 1f, bool drawSinglePassLiquids = true)
		{
			if (!Lighting.NotRetro)
			{
				oldDrawLava(bg, lavaStyle, Alpha);
				return;
			}
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			Vector2 drawOffset = (Vector2)(Main.drawToScreen ? Vector2.Zero : new Vector2((float)Main.offScreenRange, (float)Main.offScreenRange)) - Main.screenPosition;
			if (bg)
			{
				DrawLiquidBehindTiles(lavaStyle);
			}
			DrawLava(Main.spriteBatch, drawOffset, lavaStyle, Alpha, bg);
			if (!bg)
			{
				TimeLogger.DrawTime(4, stopwatch.Elapsed.TotalMilliseconds);
			}
		}

		public unsafe void DrawLava(SpriteBatch spriteBatch, Vector2 drawOffset, int LavaStyle, float globalAlpha, bool isBackgroundDraw)
		{
			Main.tileBatch.End();
			Rectangle drawArea = Instance._drawArea;
			Main.tileBatch.Begin();
			fixed (LiquidDrawCache* ptr3 = &Instance._drawCache[0])
			{
				LiquidDrawCache* ptr2 = ptr3;
				for (int i = drawArea.X; i < drawArea.X + drawArea.Width; i++)
				{
					for (int j = drawArea.Y; j < drawArea.Y + drawArea.Height; j++)
					{
						if (ptr2->IsVisible && ptr2->Type == LiquidID.Lava)
						{
							Rectangle sourceRectangle = ptr2->SourceRectangle;
							if (ptr2->IsSurfaceLiquid)
							{
								sourceRectangle.Y = 1280;
							}
							else
							{
								sourceRectangle.Y += Instance._animationFrame * 80;
							}
							Vector2 liquidOffset = ptr2->LiquidOffset;
							float num = ptr2->Opacity * (isBackgroundDraw ? 1f : DEFAULT_OPACITY[ptr2->Type]);
							int num2 = LavaStyle;
							num *= globalAlpha;
							num = Math.Min(1f, num);
							Lighting.GetCornerColors(i, j, out var vertices);
							ref Color bottomLeftColor = ref vertices.BottomLeftColor;
							bottomLeftColor *= num;
							ref Color bottomRightColor = ref vertices.BottomRightColor;
							bottomRightColor *= num;
							ref Color topLeftColor = ref vertices.TopLeftColor;
							topLeftColor *= num;
							ref Color topRightColor = ref vertices.TopRightColor;
							topRightColor *= num;
							Main.DrawTileInWater(drawOffset, i, j);
							Main.tileBatch.Draw(lavaTextures[num2].Value, new Vector2((float)(i << 4), (float)(j << 4)) + drawOffset + liquidOffset, sourceRectangle, vertices, Vector2.Zero, 1f, (SpriteEffects)0);
						}
						ptr2++;
					}
				}
			}
			Main.tileBatch.End();
		}

		public void oldDrawLava(bool bg = false, int Style = 0, float Alpha = 1f)
		{
			float num = 0f;
			float num12 = 99999f;
			float num23 = 99999f;
			int num27 = -1;
			int num28 = -1;
			Vector2 vector = new((float)Main.offScreenRange, (float)Main.offScreenRange);
			if (Main.drawToScreen)
			{
				vector = Vector2.Zero;
			}
			_ = new Color[4];
			int num29 = (int)(255f * (1f - Main.gfxQuality) + 40f * Main.gfxQuality);
			_ = Main.gfxQuality;
			_ = Main.gfxQuality;
			int num30 = (int)((Main.screenPosition.X - vector.X) / 16f - 1f);
			int num31 = (int)((Main.screenPosition.X + (float)Main.screenWidth + vector.X) / 16f) + 2;
			int num32 = (int)((Main.screenPosition.Y - vector.Y) / 16f - 1f);
			int num2 = (int)((Main.screenPosition.Y + (float)Main.screenHeight + vector.Y) / 16f) + 5;
			if (num30 < 5)
			{
				num30 = 5;
			}
			if (num31 > Main.maxTilesX - 5)
			{
				num31 = Main.maxTilesX - 5;
			}
			if (num32 < 5)
			{
				num32 = 5;
			}
			if (num2 > Main.maxTilesY - 5)
			{
				num2 = Main.maxTilesY - 5;
			}
			Vector2 vector2;
			Rectangle value;
			Color newColor;
			for (int i = num32; i < num2 + 4; i++)
			{
				for (int j = num30 - 2; j < num31 + 2; j++)
				{
					if (Main.tile[j, i].LiquidAmount <= 0 || (Main.tile[j, i].HasUnactuatedTile && Main.tileSolid[Main.tile[j, i].TileType] && !Main.tileSolidTop[Main.tile[j, i].TileType]) || !(Lighting.Brightness(j, i) > 0f || bg) || Main.tile[j, i].LiquidType != LiquidID.Lava)
					{
						continue;
					}
					Color color = Lighting.GetColor(j, i);
					float num3 = 256 - Main.tile[j, i].LiquidAmount;
					num3 /= 32f;
					bool flag = false;
					int num4 = 0;
					if (Main.tile[j, i].LiquidType == LiquidID.Lava)
					{
						/*if (Main.drewLava) //disallows the back liquid to not draw until its alpha hits 1f apparently
						{
							continue;
						}*/
						float num5 = Math.Abs((float)(j * 16 + 8) - (Main.screenPosition.X + (float)(Main.screenWidth / 2)));
						float num6 = Math.Abs((float)(i * 16 + 8) - (Main.screenPosition.Y + (float)(Main.screenHeight / 2)));
						if (num5 < (float)(Main.screenWidth * 2) && num6 < (float)(Main.screenHeight * 2))
						{
							float num7 = (float)Math.Sqrt(num5 * num5 + num6 * num6);
							float num8 = 1f - num7 / ((float)Main.screenWidth * 0.75f);
							if (num8 > 0f)
							{
								num += num8;
							}
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
					if (num4 == 0)
					{
						num4 = Style;
					}
					if (Main.drewLava)
					{
						continue;
					}
					float num9 = 0.5f;
					if (bg)
					{
						num9 = 1f;
					}
					num9 *= Alpha;
					Main.DrawTileInWater(-Main.screenPosition + vector, j, i); //lily pads
					vector2 = new((float)(j * 16), (float)(i * 16 + (int)num3 * 2));
					value = new(0, 0, 16, 16 - (int)num3 * 2);
					bool flag2 = true;
					if (Main.tile[j, i + 1].LiquidAmount < 245 && (!Main.tile[j, i + 1].HasUnactuatedTile || !Main.tileSolid[Main.tile[j, i + 1].TileType] || Main.tileSolidTop[Main.tile[j, i + 1].TileType]))
					{
						float num10 = 256 - Main.tile[j, i + 1].LiquidAmount;
						num10 /= 32f;
						num9 = 0.5f * (8f - num3) / 4f;
						if ((double)num9 > 0.55)
						{
							num9 = 0.55f;
						}
						if ((double)num9 < 0.35)
						{
							num9 = 0.35f;
						}
						float num11 = num3 / 2f;
						if (Main.tile[j, i + 1].LiquidAmount < 200)
						{
							if (bg)
							{
								continue;
							}
							if (Main.tile[j, i - 1].LiquidAmount > 0 && Main.tile[j, i - 1].LiquidAmount > 0)
							{
								value = new(0, 4, 16, 16);
								num9 = 0.5f;
							}
							else if (Main.tile[j, i - 1].LiquidAmount > 0)
							{
								vector2 = new((float)(j * 16), (float)(i * 16 + 4));
								value = new(0, 4, 16, 12);
								num9 = 0.5f;
							}
							else if (Main.tile[j, i + 1].LiquidAmount > 0)
							{
								vector2 = new((float)(j * 16), (float)(i * 16 + (int)num3 * 2 + (int)num10 * 2));
								value = new(0, 4, 16, 16 - (int)num3 * 2);
							}
							else
							{
								vector2 = new((float)(j * 16 + (int)num11), (float)(i * 16 + (int)num11 * 2 + (int)num10 * 2));
								value = new(0, 4, 16 - (int)num11 * 2, 16 - (int)num11 * 2);
							}
						}
						else
						{
							num9 = 0.5f;
							value = new(0, 4, 16, 16 - (int)num3 * 2 + (int)num10 * 2);
						}
					}
					else if (Main.tile[j, i - 1].LiquidAmount > 32)
					{
						value = new(0, 4, value.Width, value.Height);
					}
					else if (num3 < 1f && Main.tile[j, i - 1].HasUnactuatedTile && Main.tileSolid[Main.tile[j, i - 1].TileType] && !Main.tileSolidTop[Main.tile[j, i - 1].TileType])
					{
						vector2 = new((float)(j * 16), (float)(i * 16));
						value = new(0, 4, 16, 16);
					}
					else
					{
						for (int k = i + 1; k < i + 6 && (!Main.tile[j, k].HasUnactuatedTile || !Main.tileSolid[Main.tile[j, k].TileType] || Main.tileSolidTop[Main.tile[j, k].TileType]); k++)
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
							value = new(0, 4, 16, 16);
						}
						else if (Main.tile[j, i - 1].LiquidAmount > 0)
						{
							value = new(0, 2, value.Width, value.Height);
						}
					}
					if ((color.R > 20 || color.B > 20 || color.G > 20) && value.Y < 4)
					{
						int num13 = color.R;
						if (color.G > num13)
						{
							num13 = color.G;
						}
						if (color.B > num13)
						{
							num13 = color.B;
						}
						num13 /= 30;
						if (Main.rand.Next(20000) < num13)
						{
							newColor = new(255, 255, 255);
							int num14 = Dust.NewDust(new Vector2((float)(j * 16), vector2.Y - 2f), 16, 8, DustID.TintableDustLighted, 0f, 0f, 254, newColor, 0.75f);
							Dust obj = Main.dust[num14];
							obj.velocity *= 0f;
						}
					}
					if (Main.tile[j, i].LiquidType == LiquidID.Lava)
					{
						num9 *= 1.8f;
						if (num9 > 1f)
						{
							num9 = 1f;
						}
						if (Main.instance.IsActive && !Main.gamePaused && Dust.lavaBubbles < 200)
						{
							if (Main.tile[j, i].LiquidAmount > 200 && Main.rand.NextBool(700))
							{
								Dust.NewDust(new Vector2((float)(j * 16), (float)(i * 16)), 16, 16, lavaBubbleDust[num4]);
							}
							if (value.Y == 0 && Main.rand.NextBool(350))
							{
								int num15 = Dust.NewDust(new Vector2((float)(j * 16), (float)(i * 16) + num3 * 2f - 8f), 16, 8, lavaBubbleDust[num4], 0f, 0f, 50, default(Color), 1.5f);
								Dust obj2 = Main.dust[num15];
								obj2.velocity *= 0.8f;
								Main.dust[num15].velocity.X *= 2f;
								Main.dust[num15].velocity.Y -= (float)Main.rand.Next(1, 7) * 0.1f;
								if (Main.rand.NextBool(10))
								{
									Main.dust[num15].velocity.Y *= Main.rand.Next(2, 5);
								}
								Main.dust[num15].noGravity = true;
							}
						}
					}
					float num16 = (float)(int)color.R * num9;
					float num17 = (float)(int)color.G * num9;
					float num18 = (float)(int)color.B * num9;
					float num19 = (float)(int)color.A * num9;
					color = new((int)(byte)num16, (int)(byte)num17, (int)(byte)num18, (int)(byte)num19);
					if (flag)
					{
						color = new(color.ToVector4() * LiquidRenderer.GetShimmerBaseColor(j, i));
					}
					if (Lighting.NotRetro && !bg)
					{
						Color color2 = color;
						if (((double)(int)color2.R > (double)num29 * 0.6 || (double)(int)color2.G > (double)num29 * 0.65 || (double)(int)color2.B > (double)num29 * 0.7))
						{
							for (int l = 0; l < 4; l++)
							{
								int num20 = 0;
								int num21 = 0;
								int width = 8;
								int height = 8;
								Color color3 = color2;
								Color color4 = Lighting.GetColor(j, i);
								if (l == 0)
								{
									color4 = Lighting.GetColor(j - 1, i - 1);
									if (value.Height < 8)
									{
										height = value.Height;
									}
								}
								if (l == 1)
								{
									color4 = Lighting.GetColor(j + 1, i - 1);
									num20 = 8;
									if (value.Height < 8)
									{
										height = value.Height;
									}
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
								color4 = new((int)(byte)num16, (int)(byte)num17, (int)(byte)num18, (int)(byte)num19);
								color3.R = (byte)((color2.R * 3 + color4.R * 2) / 5);
								color3.G = (byte)((color2.G * 3 + color4.G * 2) / 5);
								color3.B = (byte)((color2.B * 3 + color4.B * 2) / 5);
								color3.A = (byte)((color2.A * 3 + color4.A * 2) / 5);
								if (flag)
								{
									color3 = new(color3.ToVector4() * LiquidRenderer.GetShimmerBaseColor(j, i));
								}
								Main.spriteBatch.Draw(lavaBlockTexture[num4].Value, vector2 - Main.screenPosition + new Vector2((float)num20, (float)num21) + vector, (Rectangle?)new Rectangle(value.X + num20, value.Y + num21, width, height), color3, 0f, default(Vector2), 1f, (SpriteEffects)0, 0f);
							}
						}
						else
						{
							Main.spriteBatch.Draw(lavaBlockTexture[num4].Value, vector2 - Main.screenPosition + vector, (Rectangle?)value, color, 0f, default(Vector2), 1f, (SpriteEffects)0, 0f);
						}
					}
					else
					{
						if (value.Y < 4)
						{
							value.X += (int)(Main.wFrame * 18f);
						}
						Main.spriteBatch.Draw(lavaBlockTexture[num4].Value, vector2 - Main.screenPosition + vector, (Rectangle?)value, color, 0f, default(Vector2), 1f, (SpriteEffects)0, 0f);
					}
					if (!Main.tile[j, i + 1].IsHalfBlock)
					{
						continue;
					}
					color = Lighting.GetColor(j, i + 1);
					num16 = (float)(int)color.R * num9;
					num17 = (float)(int)color.G * num9;
					num18 = (float)(int)color.B * num9;
					num19 = (float)(int)color.A * num9;
					color = new((int)(byte)num16, (int)(byte)num17, (int)(byte)num18, (int)(byte)num19);
					vector2 = new((float)(j * 16), (float)(i * 16 + 16));
					Main.spriteBatch.Draw(lavaBlockTexture[num4].Value, vector2 - Main.screenPosition + vector, (Rectangle?)new Rectangle(0, 4, 16, 8), color, 0f, default(Vector2), 1f, (SpriteEffects)0, 0f);
					float num22 = 6f;
					float num24 = 0.75f;
					num22 = 4f;
					num24 = 0.5f;
					for (int m = 0; (float)m < num22; m++)
					{
						int num25 = i + 2 + m;
						if (WorldGen.SolidTile(j, num25))
						{
							break;
						}
						float num26 = 1f - (float)m / num22;
						num26 *= num24;
						vector2 = new((float)(j * 16), (float)(num25 * 16 - 2));
						Main.spriteBatch.Draw(lavaBlockTexture[num4].Value, vector2 - Main.screenPosition + vector, (Rectangle?)new Rectangle(0, 18, 16, 16), color * num26, 0f, default(Vector2), 1f, (SpriteEffects)0, 0f);
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

		public void DrawLiquidBehindTiles(int lavaStyleOverride = -1)
		{
			Vector2 unscaledPosition = Main.Camera.UnscaledPosition;
			Vector2 vector = new((float)Main.offScreenRange, (float)Main.offScreenRange);
			if (Main.drawToScreen)
			{
				vector = Vector2.Zero;
			}
			GetScreenDrawArea(unscaledPosition, vector + (Main.Camera.UnscaledPosition - Main.Camera.ScaledPosition), out var firstTileX, out var lastTileX, out var firstTileY, out var lastTileY);
			for (int i = firstTileY; i < lastTileY + 4; i++)
			{
				for (int j = firstTileX - 2; j < lastTileX + 2; j++)
				{
					Tile tile = Main.tile[j, i];
					if (tile != null)
					{
						DrawTile_LiquidBehindTile(solidLayer: false, inFrontOfPlayers: false, lavaStyleOverride, unscaledPosition, vector, j, i, tile);
					}
				}
			}
		}

		private void GetScreenDrawArea(Vector2 screenPosition, Vector2 offSet, out int firstTileX, out int lastTileX, out int firstTileY, out int lastTileY)
		{
			firstTileX = (int)((screenPosition.X - offSet.X) / 16f - 1f);
			lastTileX = (int)((screenPosition.X + (float)Main.screenWidth + offSet.X) / 16f) + 2;
			firstTileY = (int)((screenPosition.Y - offSet.Y) / 16f - 1f);
			lastTileY = (int)((screenPosition.Y + (float)Main.screenHeight + offSet.Y) / 16f) + 5;
			if (firstTileX < 4)
			{
				firstTileX = 4;
			}
			if (lastTileX > Main.maxTilesX - 4)
			{
				lastTileX = Main.maxTilesX - 4;
			}
			if (firstTileY < 4)
			{
				firstTileY = 4;
			}
			if (lastTileY > Main.maxTilesY - 4)
			{
				lastTileY = Main.maxTilesY - 4;
			}
			if (Main.sectionManager.AnyUnfinishedSections)
			{
				TimeLogger.DetailedDrawReset();
				WorldGen.SectionTileFrameWithCheck(firstTileX, firstTileY, lastTileX, lastTileY);
				TimeLogger.DetailedDrawTime(5);
			}
			if (Main.sectionManager.AnyNeedRefresh)
			{
				WorldGen.RefreshSections(firstTileX, firstTileY, lastTileX, lastTileY);
			}
		}

		private void DrawTile_LiquidBehindTile(bool solidLayer, bool inFrontOfPlayers, int waterStyleOverride, Vector2 screenPosition, Vector2 screenOffset, int tileX, int tileY, Tile tileCache)
		{
			Tile tile = Main.tile[tileX + 1, tileY];
			Tile tile2 = Main.tile[tileX - 1, tileY];
			Tile tile3 = Main.tile[tileX, tileY - 1];
			Tile tile4 = Main.tile[tileX, tileY + 1];
			if ((tileCache.LiquidType != LiquidID.Lava && tile.LiquidType != LiquidID.Lava && tile2.LiquidType != LiquidID.Lava && tile3.LiquidType != LiquidID.Lava && tile4.LiquidType != LiquidID.Lava))
			{
				return;
			}
			//bool[] _tileSolidTop = (bool[])typeof(TileDrawing).GetField("_tileSolidTop", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance).GetValue(null);
			if (!tileCache.HasTile || tileCache.IsActuated /*|| _tileSolidTop[tileCache.TileType]*/ || (tileCache.IsHalfBlock && (tile2.LiquidAmount > 160 || tile.LiquidAmount > 160) && Main.instance.waterfallManager.CheckForWaterfall(tileX, tileY)) || (TileID.Sets.BlocksWaterDrawingBehindSelf[tileCache.TileType] && tileCache.Slope == 0))
			{
				return;
			}
			int num = 0;
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			bool flag5 = false;
			int num2 = 0;
			bool flag6 = true;
			int num3 = (int)tileCache.Slope;
			int num4 = (int)tileCache.BlockType;
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
					if (tileCache.LiquidAmount > num)
					{
						num = tileCache.LiquidAmount;
					}
				}
				if (tile2.LiquidAmount > 0 && num3 != 1 && num3 != 3)
				{
					flag = true;
					if (tile2.LiquidAmount > num)
					{
						num = tile2.LiquidAmount;
					}
				}
				if (tile.LiquidAmount > 0 && num3 != 2 && num3 != 4)
				{
					flag2 = true;
					if (tile.LiquidAmount > num)
					{
						num = tile.LiquidAmount;
					}
				}
				if (tile3.LiquidAmount > 0 && num3 != 3 && num3 != 4)
				{
					flag3 = true;
				}
				if (tile4.LiquidAmount > 0 && num3 != 1 && num3 != 2)
				{
					if (tile4.LiquidAmount > 240)
					{
						flag4 = true;
					}
				}
			}
			if (!flag3 && !flag4 && !flag && !flag2 && !flag5)
			{
				return;
			}
			if (waterStyleOverride != -1)
			{
				lavaStyle = waterStyleOverride;
			}
			if (num2 == 0)
			{
				num2 = lavaStyle;
			}
			Lighting.GetCornerColors(tileX, tileY, out var vertices);
			Vector2 vector = new((float)(tileX * 16), (float)(tileY * 16));
			Rectangle liquidSize = new(0, 4, 16, 16);
			if (flag4 && (flag || flag2))
			{
				flag = true;
				flag2 = true;
			}
			if (tileCache.HasTile && (Main.tileSolidTop[tileCache.TileType] || !Main.tileSolid[tileCache.TileType]))
			{
				return;
			}
			if ((!flag3 || !(flag || flag2)) && !(flag4 && flag3))
			{
				if (flag3)
				{
					liquidSize = new(0, 4, 16, 4);
					if (tileCache.IsHalfBlock || tileCache.Slope != 0)
					{
						liquidSize = new(0, 4, 16, 12);
					}
				}
				else if (flag4 && !flag && !flag2)
				{
					vector = new((float)(tileX * 16), (float)(tileY * 16 + 12));
					liquidSize = new(0, 4, 16, 4);
				}
				else
				{
					float num8 = (float)(256 - num) / 32f;
					int y = 4;
					if (tile3.LiquidAmount == 0 && (num4 != 0 || !WorldGen.SolidTile(tileX, tileY - 1)))
					{
						y = 0;
					}
					int num5 = (int)num8 * 2;
					if (tileCache.Slope != 0)
					{
						vector = new((float)(tileX * 16), (float)(tileY * 16 + num5));
						liquidSize = new(0, num5, 16, 16 - num5);
					}
					else if ((flag && flag2) || tileCache.IsHalfBlock)
					{
						vector = new((float)(tileX * 16), (float)(tileY * 16 + num5));
						liquidSize = new(0, y, 16, 16 - num5);
					}
					else if (flag)
					{
						vector = new((float)(tileX * 16), (float)(tileY * 16 + num5));
						liquidSize = new(0, y, 4, 16 - num5);
					}
					else
					{
						vector = new((float)(tileX * 16 + 12), (float)(tileY * 16 + num5));
						liquidSize = new(0, y, 4, 16 - num5);
					}
				}
			}
			Vector2 position = vector - screenPosition + screenOffset;
			float num6 = 1f;
			if ((double)tileY <= Main.worldSurface || num6 > 1f)
			{
				num6 = 1f;
				if (tileCache.WallType == 21)
				{
					num6 = 0.9f;
				}
				else if (tileCache.WallType > 0)
				{
					num6 = 0.6f;
				}
			}
			if (tileCache.IsHalfBlock && tile3.LiquidAmount > 0 && tileCache.WallType > 0)
			{
				num6 = 0f;
			}
			if (num3 == 4 && tile2.LiquidAmount == 0 && !WorldGen.SolidTile(tileX - 1, tileY))
			{
				num6 = 0f;
			}
			if (num3 == 3 && tile.LiquidAmount == 0 && !WorldGen.SolidTile(tileX + 1, tileY))
			{
				num6 = 0f;
			}
			ref Color bottomLeftColor = ref vertices.BottomLeftColor;
			bottomLeftColor *= num6;
			ref Color bottomRightColor = ref vertices.BottomRightColor;
			bottomRightColor *= num6;
			ref Color topLeftColor = ref vertices.TopLeftColor;
			topLeftColor *= num6;
			ref Color topRightColor = ref vertices.TopRightColor;
			topRightColor *= num6;
			bool flag7 = false;
			if (flag6)
			{
				for (int i = 0; i < LavaStylesLoader.TotalCount; i++)
				{
					if (lavaLiquidAlpha[i] > 0f && i != num2)
					{
						DrawPartialLiquid(!solidLayer, tileCache, ref position, ref liquidSize, i, ref vertices);
						flag7 = true;
						break;
					}
				}
			}
			VertexColors colors = vertices;
			float num7 = (flag7 ? lavaLiquidAlpha[num2] : 1f);
			ref Color bottomLeftColor2 = ref colors.BottomLeftColor;
			bottomLeftColor2 *= num7;
			ref Color bottomRightColor2 = ref colors.BottomRightColor;
			bottomRightColor2 *= num7;
			ref Color topLeftColor2 = ref colors.TopLeftColor;
			topLeftColor2 *= num7;
			ref Color topRightColor2 = ref colors.TopRightColor;
			topRightColor2 *= num7;
			DrawPartialLiquid(!solidLayer, tileCache, ref position, ref liquidSize, num2, ref colors);
		}

		private void DrawPartialLiquid(bool behindBlocks, Tile tileCache, ref Vector2 position, ref Rectangle liquidSize, int liquidType, ref VertexColors colors)
		{
			int num = (int)tileCache.Slope;
			bool flag = !TileID.Sets.BlocksWaterDrawingBehindSelf[tileCache.TileType];
			if (!behindBlocks)
			{
				flag = false;
			}
			if (flag || num == 0)
			{
				Main.tileBatch.Draw(lavaBlockTexture[liquidType].Value, position, liquidSize, colors, default(Vector2), 1f, (SpriteEffects)0);
				return;
			}
			liquidSize.X += 18 * (num - 1);
			switch (num)
			{
				case 1:
					Main.tileBatch.Draw(lavaSlopeTexture[liquidType].Value, position, liquidSize, colors, Vector2.Zero, 1f, (SpriteEffects)0);
					break;
				case 2:
					Main.tileBatch.Draw(lavaSlopeTexture[liquidType].Value, position, liquidSize, colors, Vector2.Zero, 1f, (SpriteEffects)0);
					break;
				case 3:
					Main.tileBatch.Draw(lavaSlopeTexture[liquidType].Value, position, liquidSize, colors, Vector2.Zero, 1f, (SpriteEffects)0);
					break;
				case 4:
					Main.tileBatch.Draw(lavaSlopeTexture[liquidType].Value, position, liquidSize, colors, Vector2.Zero, 1f, (SpriteEffects)0);
					break;
			}
		}

		public void InitialDrawLavafall(WaterfallManager waterfallManager)
		{
			for (int i = 0; i < LavaStylesLoader.TotalCount; i++)
			{
				if (lavaLiquidAlpha[i] > 0f)
				{
					DrawLavafall(waterfallManager, i, lavaLiquidAlpha[i]);
				}
			}
		}

		internal void DrawLavafall(WaterfallManager waterfallManager, int Style = 0, float Alpha = 1f)
		{
			int waterfallDist = (int)typeof(WaterfallManager).GetField("waterfallDist", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance).GetValue(waterfallManager);
			int rainFrameForeground = (int)typeof(WaterfallManager).GetField("rainFrameForeground", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance).GetValue(waterfallManager);
			int rainFrameBackground = (int)typeof(WaterfallManager).GetField("rainFrameBackground", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance).GetValue(waterfallManager);
			int snowFrameForeground = (int)typeof(WaterfallManager).GetField("snowFrameForeground", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance).GetValue(waterfallManager);
			WaterfallData[] waterfalls = (WaterfallData[])typeof(WaterfallManager).GetField("waterfalls", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance).GetValue(waterfallManager);
			int currentMax = (int)typeof(WaterfallManager).GetField("currentMax", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance).GetValue(waterfallManager);
			int slowFrame = (int)typeof(WaterfallManager).GetField("slowFrame", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance).GetValue(waterfallManager);
			Main.tileSolid[546] = false;
			float num = 0f;
			float num12 = 99999f;
			float num23 = 99999f;
			int num34 = -1;
			int num45 = -1;
			float num47 = 0f;
			float num48 = 99999f;
			float num49 = 99999f;
			int num50 = -1;
			int num2 = -1;
			Rectangle value = default(Rectangle);
			Rectangle value2 = default(Rectangle);
			Vector2 origin = default(Vector2);
			for (int i = 0; i < currentMax; i++)
			{
				if (waterfalls[i].type != 1)
				{
					continue;
				}
				int num3 = 0;
				int num4 = Style;
				int num5 = waterfalls[i].x;
				int num6 = waterfalls[i].y;
				int num7 = 0;
				int num8 = 0;
				int num9 = 0;
				int num10 = 0;
				int num11 = 0;
				int num13 = 0;
				int num14;
				int num15;
				if (waterfalls[i].stopAtStep == 0)
				{
					continue;
				}
				num14 = 32 * slowFrame;
				int num22 = 0;
				num15 = waterfallDist;
				Color color4 = Color.White;
				for (int k = 0; k < num15; k++)
				{
					if (num22 >= 2)
					{
						break;
					}
					typeof(WaterfallManager).GetMethod("AddLight", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Invoke(null, new object[] { 1, num5, num6 });
					Tile tile3 = Main.tile[num5, num6];
					if (tile3.HasUnactuatedTile && Main.tileSolid[tile3.TileType] && !Main.tileSolidTop[tile3.TileType] && !TileID.Sets.Platforms[tile3.TileType] && tile3.BlockType == 0)
					{
						break;
					}
					Tile tile4 = Main.tile[num5 - 1, num6];
					Tile tile5 = Main.tile[num5, num6 + 1];
					Tile tile6 = Main.tile[num5 + 1, num6];
					if (WorldGen.SolidTile(tile5) && !tile3.IsHalfBlock)
					{
						num3 = 8;
					}
					else if (num8 != 0)
					{
						num3 = 0;
					}
					int num24 = 0;
					int num25 = num10;
					int num26 = 0;
					int num27 = 0;
					bool flag2 = false;
					if (tile5.TopSlope && !tile3.IsHalfBlock && tile5.TileType != 19)
					{
						flag2 = true;
						if (tile5.Slope == (SlopeType)1)
						{
							num24 = 1;
							num26 = 1;
							num9 = 1;
							num10 = num9;
						}
						else
						{
							num24 = -1;
							num26 = -1;
							num9 = -1;
							num10 = num9;
						}
						num27 = 1;
					}
					else if ((!WorldGen.SolidTile(tile5) && !tile5.BottomSlope && !tile3.IsHalfBlock) || (!tile5.HasTile && !tile3.IsHalfBlock))
					{
						num22 = 0;
						num27 = 1;
						num26 = 0;
					}
					else if ((WorldGen.SolidTile(tile4) || tile4.TopSlope || tile4.LiquidAmount > 0) && !WorldGen.SolidTile(tile6) && tile6.LiquidAmount == 0)
					{
						if (num9 == -1)
						{
							num22++;
						}
						num26 = 1;
						num27 = 0;
						num9 = 1;
					}
					else if ((WorldGen.SolidTile(tile6) || tile6.TopSlope || tile6.LiquidAmount > 0) && !WorldGen.SolidTile(tile4) && tile4.LiquidAmount == 0)
					{
						if (num9 == 1)
						{
							num22++;
						}
						num26 = -1;
						num27 = 0;
						num9 = -1;
					}
					else if (((!WorldGen.SolidTile(tile6) && !tile3.TopSlope) || tile6.LiquidAmount == 0) && !WorldGen.SolidTile(tile4) && !tile3.TopSlope && tile4.LiquidAmount == 0)
					{
						num27 = 0;
						num26 = num9;
					}
					else
					{
						num22++;
						num27 = 0;
						num26 = 0;
					}
					if (num22 >= 2)
					{
						num9 *= -1;
						num26 *= -1;
					}
					Color color5 = Lighting.GetColor(num5, num6);
					if (k > 50)
					{
						typeof(WaterfallManager).GetMethod("TrySparkling", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Invoke(null, new object[] { num5, num6, num9, color5 });
					}
					float alpha = GetLavafallAlpha(Alpha, num15, num6, k, tile3);
					color5 = (Color)typeof(WaterfallManager).GetMethod("StylizeColor", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Invoke(null, new object[] { alpha, num15, 1, num6, k, tile3, color5 });
					float num33 = Math.Abs((float)(num5 * 16 + 8) - (Main.screenPosition.X + (float)(Main.screenWidth / 2)));
					float num35 = Math.Abs((float)(num6 * 16 + 8) - (Main.screenPosition.Y + (float)(Main.screenHeight / 2)));
					if (num33 < (float)(Main.screenWidth * 2) && num35 < (float)(Main.screenHeight * 2))
					{
						float num36 = (float)Math.Sqrt(num33 * num33 + num35 * num35);
						float num37 = 1f - num36 / ((float)Main.screenWidth * 0.75f);
						if (num37 > 0f)
						{
							num += num37;
						}
					}
					if (num33 < num12)
					{
						num12 = num33;
						num34 = num5 * 16 + 8;
					}
					if (num35 < num23)
					{
						num23 = num33;
						num45 = num6 * 16 + 8;
					}
					int num38 = tile3.LiquidAmount / 16;
					if (flag2 && num9 != num25)
					{
						int num39 = 2;
						if (num25 == 1)
						{
							DrawLavafall(num4, num5, num6, alpha, new Vector2((float)(num5 * 16 - 16), (float)(num6 * 16 + 16 - num39)) - Main.screenPosition, new Rectangle(num14, 24, 32, 16 - num38 - num39), color5, (SpriteEffects)1);
						}
						else
						{
							DrawLavafall(num4, num5, num6, alpha, new Vector2((float)(num5 * 16), (float)(num6 * 16 + 16 - num39)) - Main.screenPosition, new Rectangle(num14, 24, 32, 16 - num38 - num39), color5, (SpriteEffects)0);
						}
					}
					if (num7 == 0 && num24 != 0 && num8 == 1 && num9 != num10)
					{
						num24 = 0;
						num9 = num10;
						color5 = Color.White;
						if (num9 == 1)
						{
							DrawLavafall(num4, num5, num6, alpha, new Vector2((float)(num5 * 16 - 16), (float)(num6 * 16 + 16)) - Main.screenPosition, new Rectangle(num14, 24, 32, 16 - num38), color5, (SpriteEffects)1);
						}
						else
						{
							DrawLavafall(num4, num5, num6, alpha, new Vector2((float)(num5 * 16 - 16), (float)(num6 * 16 + 16)) - Main.screenPosition, new Rectangle(num14, 24, 32, 16 - num38), color5, (SpriteEffects)1);
						}
					}
					if (num11 != 0 && num26 == 0 && num27 == 1)
					{
						if (num9 == 1)
						{
							if (num13 != num4)
							{
								DrawLavafall(num4, num5, num6, alpha, new Vector2((float)(num5 * 16), (float)(num6 * 16 + num3 + 8)) - Main.screenPosition, new Rectangle(num14, 0, 16, 16 - num38 - 8), color4, (SpriteEffects)1);
							}
							else
							{
								DrawLavafall(num4, num5, num6, alpha, new Vector2((float)(num5 * 16), (float)(num6 * 16 + num3 + 8)) - Main.screenPosition, new Rectangle(num14, 0, 16, 16 - num38 - 8), color5, (SpriteEffects)1);
							}
						}
						else
						{
							DrawLavafall(num4, num5, num6, alpha, new Vector2((float)(num5 * 16), (float)(num6 * 16 + num3 + 8)) - Main.screenPosition, new Rectangle(num14, 0, 16, 16 - num38 - 8), color5, (SpriteEffects)0);
						}
					}
					if (num3 == 8 && num8 == 1 && num11 == 0)
					{
						if (num10 == -1)
						{
							if (num13 != num4)
							{
								DrawLavafall(num13, num5, num6, alpha, new Vector2((float)(num5 * 16), (float)(num6 * 16)) - Main.screenPosition, new Rectangle(num14, 24, 32, 8), color4, (SpriteEffects)0);
							}
							else
							{
								DrawLavafall(num4, num5, num6, alpha, new Vector2((float)(num5 * 16), (float)(num6 * 16)) - Main.screenPosition, new Rectangle(num14, 24, 32, 8), color5, (SpriteEffects)0);
							}
						}
						else if (num13 != num4)
						{
							DrawLavafall(num13, num5, num6, alpha, new Vector2((float)(num5 * 16 - 16), (float)(num6 * 16)) - Main.screenPosition, new Rectangle(num14, 24, 32, 8), color4, (SpriteEffects)1);
						}
						else
						{
							DrawLavafall(num4, num5, num6, alpha, new Vector2((float)(num5 * 16 - 16), (float)(num6 * 16)) - Main.screenPosition, new Rectangle(num14, 24, 32, 8), color5, (SpriteEffects)1);
						}
					}
					if (num24 != 0 && num7 == 0)
					{
						if (num25 == 1)
						{
							if (num13 != num4)
							{
								DrawLavafall(num13, num5, num6, alpha, new Vector2((float)(num5 * 16 - 16), (float)(num6 * 16)) - Main.screenPosition, new Rectangle(num14, 24, 32, 16 - num38), color4, (SpriteEffects)1);
							}
							else
							{
								DrawLavafall(num4, num5, num6, alpha, new Vector2((float)(num5 * 16 - 16), (float)(num6 * 16)) - Main.screenPosition, new Rectangle(num14, 24, 32, 16 - num38), color5, (SpriteEffects)1);
							}
						}
						else if (num13 != num4)
						{
							DrawLavafall(num13, num5, num6, alpha, new Vector2((float)(num5 * 16), (float)(num6 * 16)) - Main.screenPosition, new Rectangle(num14, 24, 32, 16 - num38), color4, (SpriteEffects)0);
						}
						else
						{
							DrawLavafall(num4, num5, num6, alpha, new Vector2((float)(num5 * 16), (float)(num6 * 16)) - Main.screenPosition, new Rectangle(num14, 24, 32, 16 - num38), color5, (SpriteEffects)0);
						}
					}
					if (num27 == 1 && num24 == 0 && num11 == 0)
					{
						if (num9 == -1)
						{
							if (num8 == 0)
							{
								DrawLavafall(num4, num5, num6, alpha, new Vector2((float)(num5 * 16), (float)(num6 * 16 + num3)) - Main.screenPosition, new Rectangle(num14, 0, 16, 16 - num38), color5, (SpriteEffects)0);
							}
							else if (num13 != num4)
							{
								DrawLavafall(num13, num5, num6, alpha, new Vector2((float)(num5 * 16), (float)(num6 * 16)) - Main.screenPosition, new Rectangle(num14, 24, 32, 16 - num38), color4, (SpriteEffects)0);
							}
							else
							{
								DrawLavafall(num4, num5, num6, alpha, new Vector2((float)(num5 * 16), (float)(num6 * 16)) - Main.screenPosition, new Rectangle(num14, 24, 32, 16 - num38), color5, (SpriteEffects)0);
							}
						}
						else if (num8 == 0)
						{
							DrawLavafall(num4, num5, num6, alpha, new Vector2((float)(num5 * 16), (float)(num6 * 16 + num3)) - Main.screenPosition, new Rectangle(num14, 0, 16, 16 - num38), color5, (SpriteEffects)1);
						}
						else if (num13 != num4)
						{
							DrawLavafall(num13, num5, num6, alpha, new Vector2((float)(num5 * 16 - 16), (float)(num6 * 16)) - Main.screenPosition, new Rectangle(num14, 24, 32, 16 - num38), color4, (SpriteEffects)1);
						}
						else
						{
							DrawLavafall(num4, num5, num6, alpha, new Vector2((float)(num5 * 16 - 16), (float)(num6 * 16)) - Main.screenPosition, new Rectangle(num14, 24, 32, 16 - num38), color5, (SpriteEffects)1);
						}
					}
					else
					{
						switch (num26)
						{
							case 1:
								if (Main.tile[num5, num6].LiquidAmount > 0 && !Main.tile[num5, num6].IsHalfBlock)
								{
									break;
								}
								if (num24 == 1)
								{
									for (int m = 0; m < 8; m++)
									{
										int num43 = m * 2;
										int num44 = 14 - m * 2;
										int num46 = num43;
										num3 = 8;
										if (num7 == 0 && m < 2)
										{
											num46 = 4;
										}
										DrawLavafall(num4, num5, num6, alpha, new Vector2((float)(num5 * 16 + num43), (float)(num6 * 16 + num3 + num46)) - Main.screenPosition, new Rectangle(16 + num14 + num44, 0, 2, 16 - num3), color5, (SpriteEffects)1);
									}
								}
								else
								{
									int height2 = 16;
									if (TileID.Sets.BlocksWaterDrawingBehindSelf[Main.tile[num5, num6].TileType])
									{
										height2 = 8;
									}
									else if (TileID.Sets.BlocksWaterDrawingBehindSelf[Main.tile[num5, num6 + 1].TileType])
									{
										height2 = 8;
									}
									DrawLavafall(num4, num5, num6, alpha, new Vector2((float)(num5 * 16), (float)(num6 * 16 + num3)) - Main.screenPosition, new Rectangle(16 + num14, 0, 16, height2), color5, (SpriteEffects)1);
								}
								break;
							case -1:
								if (Main.tile[num5, num6].LiquidAmount > 0 && !Main.tile[num5, num6].IsHalfBlock)
								{
									break;
								}
								if (num24 == -1)
								{
									for (int l = 0; l < 8; l++)
									{
										int num40 = l * 2;
										int num41 = l * 2;
										int num42 = 14 - l * 2;
										num3 = 8;
										if (num7 == 0 && l > 5)
										{
											num42 = 4;
										}
										DrawLavafall(num4, num5, num6, alpha, new Vector2((float)(num5 * 16 + num40), (float)(num6 * 16 + num3 + num42)) - Main.screenPosition, new Rectangle(16 + num14 + num41, 0, 2, 16 - num3), color5, (SpriteEffects)1);
									}
								}
								else
								{
									int height = 16;
									if (TileID.Sets.BlocksWaterDrawingBehindSelf[Main.tile[num5, num6].TileType])
									{
										height = 8;
									}
									else if (TileID.Sets.BlocksWaterDrawingBehindSelf[Main.tile[num5, num6 + 1].TileType])
									{
										height = 8;
									}
									DrawLavafall(num4, num5, num6, alpha, new Vector2((float)(num5 * 16), (float)(num6 * 16 + num3)) - Main.screenPosition, new Rectangle(16 + num14, 0, 16, height), color5, (SpriteEffects)0);
								}
								break;
							case 0:
								if (num27 == 0)
								{
									if (Main.tile[num5, num6].LiquidAmount <= 0 || Main.tile[num5, num6].IsHalfBlock)
									{
										DrawLavafall(num4, num5, num6, alpha, new Vector2((float)(num5 * 16), (float)(num6 * 16 + num3)) - Main.screenPosition, new Rectangle(16 + num14, 0, 16, 16), color5, (SpriteEffects)0);
									}
									k = 1000;
								}
								break;
						}
					}
					if (tile3.LiquidAmount > 0 && !tile3.IsHalfBlock)
					{
						k = 1000;
					}
					num8 = num27;
					num10 = num9;
					num7 = num26;
					num5 += num26;
					num6 += num27;
					num11 = num24;
					color4 = color5;
					if (num13 != num4)
					{
						num13 = num4;
					}
					if ((tile4.HasTile && (tile4.TileType == 189 || tile4.TileType == 196)) || (tile6.HasTile && (tile6.TileType == 189 || tile6.TileType == 196)) || (tile5.HasTile && (tile5.TileType == 189 || tile5.TileType == 196)))
					{
						num15 = (int)(40f * ((float)Main.maxTilesX / 4200f) * Main.gfxQuality);
					}
				}
			}
			Main.ambientWaterfallX = num34;
			Main.ambientWaterfallY = num45;
			Main.ambientWaterfallStrength = num;
			Main.ambientLavafallX = num50;
			Main.ambientLavafallY = num2;
			Main.ambientLavafallStrength = num47;
			Main.tileSolid[546] = true;
		}

		private void DrawLavafall(int waterfallType, int x, int y, float opacity, Vector2 position, Rectangle sourceRect, Color color, SpriteEffects effects)
		{
			Texture2D value = lavaWaterfallTexture[waterfallType].Value;
			Main.spriteBatch.Draw(value, position, (Rectangle?)sourceRect, color, 0f, default(Vector2), 1f, effects, 0f);
		}

		private static float GetLavafallAlpha(float Alpha, int maxSteps, int y, int s, Tile tileCache)
		{
			float num = (tileCache.WallType != 0 || !((double)y < Main.worldSurface)) ? (1f * Alpha) : Alpha;
			if (s > maxSteps - 10)
			{
				num *= (float)(maxSteps - s) / 10f;
			}
			return num;
		}
		#endregion
	}
}