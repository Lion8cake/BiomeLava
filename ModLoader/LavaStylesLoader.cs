﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using System.Reflection;
using ReLogic.Content;
using Terraria.Localization;
using Terraria;

namespace BiomeLava.ModLoader
{
	public class LavaStylesLoader : ModSystem
	{
		private static readonly MethodInfo ResizeArrayMethodInfo;

		static LavaStylesLoader()
		{
			ResizeArrayMethodInfo = typeof(ModContent).GetMethod("ResizeArrays", BindingFlags.NonPublic | BindingFlags.Static);
		}

		private static void ResizeArrays(ResizeArray_orig orig, bool unloading)
		{
			orig.Invoke(unloading);
			int totalCount = TotalCount;
			Array.Resize(ref BiomeLava.instance.lavaBlockTexture, totalCount);
			Array.Resize(ref BiomeLava.instance.lavaSlopeTexture, totalCount);
			Array.Resize(ref BiomeLava.instance.lavaTextures, totalCount);
			Array.Resize(ref BiomeLava.instance.lavaWaterfallTexture, totalCount);
			Array.Resize(ref BiomeLava.lavaLiquidAlpha, totalCount);
			Array.Resize(ref BiomeLava.instance.lavaData, totalCount);
		}

		private static readonly List<ModLavaStyle> _content = [];

		public static IReadOnlyList<ModLavaStyle> Content => _content;

		public static int VanillaCount => LavaStyleID.Count;

		public static int ModCount => _content.Count;

		public static int TotalCount => VanillaCount + ModCount;

		public override void Load()
		{
			if (ResizeArrayMethodInfo != null)
			{
				MonoModHooks.Add(ResizeArrayMethodInfo, ResizeArrays);
			}
		}

		public override void PostSetupContent()
		{
			foreach (ModLavaStyle item in Content)
			{
				int Slot = item.Slot;
				BiomeLava.instance.lavaTextures[Slot] = ModContent.Request<Texture2D>(item.Texture, (AssetRequestMode)2);
				BiomeLava.instance.lavaBlockTexture[Slot] = ModContent.Request<Texture2D>(item.BlockTexture, (AssetRequestMode)2);
				BiomeLava.instance.lavaSlopeTexture[Slot] = ModContent.Request<Texture2D>(item.SlopeTexture, (AssetRequestMode)2);
				BiomeLava.instance.lavaWaterfallTexture[Slot] = ModContent.Request<Texture2D>(item.WaterfallTexture, (AssetRequestMode)2);

				BiomeLava.instance.lavaData[Slot] = BiomeLava.instance.lavaData[Slot] with {
					LightColor = Vector3.Zero,
					BubbleDust = item.GetSplashDust(),
					DripGore = item.GetDropletGore(),
					LavafallGlowmask = item.LavafallGlowmask(),
					KeepOnFire = item.InflictsOnFire()
				};
			}
		}

		public static ModLavaStyle Get(int type)
		{
			type -= VanillaCount;
			return type >= 0 && type < _content.Count ? _content[type] : null;
		}

		internal static int Register(ModLavaStyle instance)
		{
			int type = TotalCount;
			ModTypeLookup<ModLavaStyle>.Register(instance);
			_content.Add(instance);
			return type;
		}

		private delegate void ResizeArray_orig(bool unloading);

		public static void UpdateLiquidAlphas()
		{
			if (BiomeLava.lavaStyle >= VanillaCount)
			{
				for (int i = 0; i < VanillaCount; i++)
				{
					BiomeLava.lavaLiquidAlpha[i] -= 0.2f;
					if (BiomeLava.lavaLiquidAlpha[i] < 0f)
					{
						BiomeLava.lavaLiquidAlpha[i] = 0f;
					}
				}
			}
			foreach (ModLavaStyle item in Content)
			{
				int type = item.Slot;
				if (BiomeLava.lavaStyle == type)
				{
					BiomeLava.lavaLiquidAlpha[type] += 0.2f;
					if (BiomeLava.lavaLiquidAlpha[type] > 1f)
					{
						BiomeLava.lavaLiquidAlpha[type] = 1f;
					}
				}
				else
				{
					BiomeLava.lavaLiquidAlpha[type] -= 0.2f;
					if (BiomeLava.lavaLiquidAlpha[type] < 0f)
					{
						BiomeLava.lavaLiquidAlpha[type] = 0f;
					}
				}
			}
		}

		public static void ModifyLight(int i, int j, int type, ref float r, ref float g, ref float b)
		{
			ModLavaStyle lavaStyle = Get(type);
			if (lavaStyle != null)
			{
				lavaStyle?.ModifyLight(i, j, ref r, ref g, ref b);
			}
		}

		public static void InflictDebuff(Player player, NPC npc, int type, int onfireDuration)
		{
			ModLavaStyle lavaStyle = Get(type);
			if (lavaStyle != null)
			{
				lavaStyle?.InflictDebuff(player, npc, onfireDuration);
			}
		}

		public static void IsLavaActive()
		{
			foreach (ModLavaStyle item in Content)
			{
				int type = item.Slot;
				ModLavaStyle lavaStyle = Get(type);
				if (lavaStyle != null)
				{
					bool? flag = lavaStyle?.IsLavaActive();
					if (flag != null && flag == true)
					{
						BiomeLava.lavaStyle = lavaStyle.Slot;
					}
				}
			}
		}

		//Mod calls
		//(Mod mod, String LavaName, Asset texture, Asset block, Asset Slope, Asset Waterfall, int DustID, int GoreID, Vector3 lightColor, bool Zone), *overload1* bool WaterfallGlowmask), *overload2* int BuffID, bool keepOnFire)
		public static object ModCalledLava(Mod mod, string lavaStyleName, string texture, string blockTexture, string slopeTexture, string waterfallTexture, Func<int> DustID, Func<int> GoreID, Func<int, int, float, float, float, Vector3> lightcolor, Func<bool> IsActive, Func<bool> waterfallGlowmask, Func<Player, NPC, int, Action> buffID, Func<bool> keepOnFire)
		{
			ArgumentNullException.ThrowIfNull(mod);
			ArgumentNullException.ThrowIfNull(lavaStyleName);
			ArgumentNullException.ThrowIfNull(texture);
			ArgumentNullException.ThrowIfNull(blockTexture);
			ArgumentNullException.ThrowIfNull(slopeTexture);
			ArgumentNullException.ThrowIfNull(waterfallTexture);

			return mod.AddContent(new ModCallModLavaStyle
			{
				NameCall = lavaStyleName,
				TextureCall = texture,
				BlockTextureCall = blockTexture,
				SlopeTextureCall = slopeTexture,
				WaterfallTextureCall = waterfallTexture,
				dustCall = DustID,
				goreCall = GoreID,
				IsActiveCall = IsActive,
				LavaLightCall = lightcolor,
				LavafallGlowmaskCall = waterfallGlowmask,
				buffCall = buffID,
				InflictsOnFireCall = keepOnFire,
			});
		}
	}
}
