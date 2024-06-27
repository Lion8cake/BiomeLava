using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent.Liquid;
using Terraria.GameContent;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace BiomeLava.ModLoader
{
	public class LavaStylesLoader : SceneEffectLoader<ModLavaStyle>
	{
		public LavaStylesLoader()
		{
			InitializeLava(LavaStyleID.Count);
		}

		internal int TotalLavaCount { get; set; }

		internal List<ModLavaStyle> lavaList = new List<ModLavaStyle>();

		/// <summary>
		/// Initializes the loader based on the vanilla count of the ModType.
		/// </summary>
		internal void InitializeLava(int vanillaCount)
		{
			VanillaCount = vanillaCount;
			TotalLavaCount = vanillaCount;
		}

		protected int ReserveLava()
		{
			return TotalLavaCount++;
		}

		public int RegisterLava(ModLavaStyle obj)
		{
			int result = ReserveLava();
			ModTypeLookup<ModLavaStyle>.Register(obj);
			lavaList.Add(obj);
			return result;
		}

		internal void Unload()
		{
			TotalLavaCount = VanillaCount;
		}

		internal void ResizeArrays()
		{
			Array.Resize(ref BiomeLava.instance.lavaBlockTexture, TotalLavaCount + 1);
			Array.Resize(ref BiomeLava.instance.lavaSlopeTexture, TotalLavaCount + 1);
			Array.Resize(ref BiomeLava.instance.lavaTextures, TotalLavaCount + 1);
			Array.Resize(ref BiomeLava.instance.lavaWaterfallTexture, TotalLavaCount + 1);
			Array.Resize(ref BiomeLava.lavaLiquidAlpha, TotalLavaCount + 1);
			Array.Resize(ref BiomeLava.instance.lavaLightColor, TotalLavaCount + 1);

			Array.Resize(ref BiomeLava.instance.lavaBubbleDust, TotalLavaCount + 1);
			Array.Resize(ref BiomeLava.instance.lavaDripGore, TotalLavaCount + 1);
			Array.Resize(ref BiomeLava.instance.lavafallGlowmask, TotalLavaCount + 1);
		}

		public void UpdateLiquidAlphas()
		{
			if (BiomeLava.lavaStyle >= base.VanillaCount)
			{
				for (int i = 0; i < base.VanillaCount; i++)
				{
					BiomeLava.lavaLiquidAlpha[i] -= 0.2f;
					if (BiomeLava.lavaLiquidAlpha[i] < 0f)
					{
						BiomeLava.lavaLiquidAlpha[i] = 0f;
					}
				}
			}
			foreach (ModLavaStyle item in list)
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
			foreach (ModLavaStyle item in LoaderManager.Get<LavaStylesLoader>().lavaList)
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

		//(Mod mod, String LavaName, Asset texture, Asset block, Asset Slope, Asset Waterfall, int DustID, int GoreID, bool Zone), *overload1* Vector3 lightColor), *overload2* bool WaterfallGlowmask), *overload3* int BuffID, bool keepOnFire)
		public static object ModCalledLava(Mod mod, String lavaStyleName, Texture2D texture, Texture2D blockTexture, Texture2D slopeTexture, Texture2D waterfallTexture, int DustID, int GoreID, bool IsActive, Vector3 lightcolor, bool waterfallGlowmask = true, int buffID = BuffID.OnFire, bool keepOnFire = false)
		{
			//turn the parameters into a new ModLavaStyle
			return true;
		}
	}
}
