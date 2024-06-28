using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent.Liquid;
using Terraria.GameContent;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Reflection;
using ReLogic.Content;

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
			int totalCount = ModContent.GetInstance<LavaStylesLoader>().TotalCount;
			Array.Resize(ref BiomeLava.instance.lavaBlockTexture, totalCount);
			Array.Resize(ref BiomeLava.instance.lavaSlopeTexture, totalCount);
			Array.Resize(ref BiomeLava.instance.lavaTextures, totalCount);
			Array.Resize(ref BiomeLava.instance.lavaWaterfallTexture, totalCount);
			Array.Resize(ref BiomeLava.lavaLiquidAlpha, totalCount);
			Array.Resize(ref BiomeLava.instance.lavaLightColor, totalCount);

			Array.Resize(ref BiomeLava.instance.lavaBubbleDust, totalCount);
			Array.Resize(ref BiomeLava.instance.lavaDripGore, totalCount);
			Array.Resize(ref BiomeLava.instance.lavafallGlowmask, totalCount);
		}

		private readonly List<ModLavaStyle> _content = [];

		public IReadOnlyList<ModLavaStyle> Content => _content;

		public int VanillaCount => LavaStyleID.Count;

		public int ModCount => _content.Count;

		public int TotalCount => VanillaCount + ModCount;

		public override void Load()
		{
			if (ResizeArrayMethodInfo != null)
			{
				MonoModHooks.Add(ResizeArrayMethodInfo, ResizeArrays);
			}
		}

		public override void PostSetupContent()
		{
			foreach (ModLavaStyle item in ModContent.GetInstance<LavaStylesLoader>().Content)
			{
				int Slot = item.Slot;
				BiomeLava.instance.lavaTextures[Slot] = ModContent.Request<Texture2D>(item.Texture, (AssetRequestMode)2);
				BiomeLava.instance.lavaBlockTexture[Slot] = ModContent.Request<Texture2D>(item.BlockTexture, (AssetRequestMode)2);
				BiomeLava.instance.lavaSlopeTexture[Slot] = ModContent.Request<Texture2D>(item.SlopeTexture, (AssetRequestMode)2);
				BiomeLava.instance.lavaWaterfallTexture[Slot] = ModContent.Request<Texture2D>(item.WaterfallTexture, (AssetRequestMode)2);
				BiomeLava.instance.lavaLightColor[Slot] = Vector3.Zero;

				BiomeLava.instance.lavaBubbleDust[Slot] = item.GetSplashDust();
				BiomeLava.instance.lavaDripGore[Slot] = item.GetDropletGore();
				BiomeLava.instance.lavafallGlowmask[Slot] = item.LavafallGlowmask();
			}
		}

		public ModLavaStyle Get(int type)
		{
			type -= VanillaCount;
			return type >= 0 && type < _content.Count ? _content[type] : null;
		}

		internal int Register(ModLavaStyle instance)
		{
			int type = TotalCount;
			ModTypeLookup<ModLavaStyle>.Register(instance);
			_content.Add(instance);
			return type;
		}

		private delegate void ResizeArray_orig(bool unloading);

		public void UpdateLiquidAlphas()
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
			foreach (ModLavaStyle item in ModContent.GetInstance<LavaStylesLoader>().Content)
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
			ModLavaStyle lavaStyle = LoaderManager.Get<LavaStylesLoader>().Get(type);
			if (lavaStyle != null)
			{
				lavaStyle?.ModifyLight(i, j, ref r, ref g, ref b);
			}
		}

		public static void IsLavaActive()
		{
			foreach (ModLavaStyle item in ModContent.GetInstance<LavaStylesLoader>().Content)
			{
				int type = item.Slot;
				ModLavaStyle lavaStyle = LoaderManager.Get<LavaStylesLoader>().Get(type);
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

		//unimplemented
		//(Mod mod, String LavaName, Asset texture, Asset block, Asset Slope, Asset Waterfall, int DustID, int GoreID, bool Zone), *overload1* Vector3 lightColor), *overload2* bool WaterfallGlowmask), *overload3* int BuffID, bool keepOnFire)
		public static object ModCalledLava(Mod mod, string lavaStyleName, Texture2D texture, Texture2D blockTexture, Texture2D slopeTexture, Texture2D waterfallTexture, int DustID, int GoreID, bool IsActive, Vector3 lightcolor, bool waterfallGlowmask = true, int buffID = BuffID.OnFire, bool keepOnFire = false)
		{
			//turn the parameters into a new ModLavaStyle
			return true;
		}
	}
}
