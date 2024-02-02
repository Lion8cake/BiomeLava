using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BiomeLava
{
    public class ModLavaStyle
    {
		internal static List<int>? moddedLavaStyles;

		//(Asset texture, Asset block, Asset Slope, Asset Waterfall, int DustID, int GoreID, int Zone), *overload1* Vector3 lightColor), *overload2* bool WaterfallGlowmask), *overload3* int BuffID, bool keepOnFire)
		internal static void LavaStyle(object texture, object block, object slope, object waterfall, object dustID, object goreID, object zone)
		{
			if (texture is not Texture2D LiquidTexture)
			{
				throw new ArgumentException("texture is not a Texture2D!");
			}

			if (block is not Texture2D LiquidBlockTexture)
			{
				throw new ArgumentException("block texture is not a Texture2D!");
			}

			if (slope is not Texture2D LiquidSlopeTexture)
			{
				throw new ArgumentException("slope texture is not a Texture2D!");
			}

			if (waterfall is not Texture2D LiquidWaterfallTexture)
			{
				throw new ArgumentException("waterfall texture is not a Texture2D!");
			}

			if (dustID is not int LiquidBubble)
			{
				throw new ArgumentException("dust ID is not a Int!");
			}

			if (goreID is not int LiquidDroplet)
			{
				throw new ArgumentException("gore ID is not a Int!");
			}

			if (zone is not Func<Player, bool> ZoneRegistration)
			{
				throw new ArgumentException("zone is not a Func<Player, bool>!");
			}

			moddedLavaStyles.Add(LiquidBubble);
		}

		internal static void LavaStyle(object texture, object block, object slope, object waterfall, object dustID, object goreID, object zone, object lightColor)
		{
			if (texture is not Texture2D LiquidTexture)
			{
				throw new ArgumentException("texture is not a Texture2D!");
			}

			if (block is not Texture2D LiquidBlockTexture)
			{
				throw new ArgumentException("block texture is not a Texture2D!");
			}

			if (slope is not Texture2D LiquidSlopeTexture)
			{
				throw new ArgumentException("slope texture is not a Texture2D!");
			}

			if (waterfall is not Texture2D LiquidWaterfallTexture)
			{
				throw new ArgumentException("waterfall texture is not a Texture2D!");
			}

			if (dustID is not int LiquidBubble)
			{
				throw new ArgumentException("dust ID is not a Int!");
			}

			if (goreID is not int LiquidDroplet)
			{
				throw new ArgumentException("gore ID is not a Int!");
			}

			if (zone is not Func<Player, bool> ZoneRegistration)
			{
				throw new ArgumentException("zone is not a Func<Player, bool>!");
			}

			if (lightColor is not Vector3 LiquidLightColor)
			{
				throw new ArgumentException("light Color is not a Vector3!");
			}

			moddedLavaStyles.Add(LiquidBubble);
		}

		internal static void LavaStyle(object texture, object block, object slope, object waterfall, object dustID, object goreID, object zone, object lightColor, object waterfallGlowmask)
		{
			if (texture is not Texture2D LiquidTexture)
			{
				throw new ArgumentException("texture is not a Texture2D!");
			}

			if (block is not Texture2D LiquidBlockTexture)
			{
				throw new ArgumentException("block texture is not a Texture2D!");
			}

			if (slope is not Texture2D LiquidSlopeTexture)
			{
				throw new ArgumentException("slope texture is not a Texture2D!");
			}

			if (waterfall is not Texture2D LiquidWaterfallTexture)
			{
				throw new ArgumentException("waterfall texture is not a Texture2D!");
			}

			if (dustID is not int LiquidBubble)
			{
				throw new ArgumentException("dust ID is not a Int!");
			}

			if (goreID is not int LiquidDroplet)
			{
				throw new ArgumentException("gore ID is not a Int!");
			}

			if (zone is not Func<Player, bool> ZoneRegistration)
			{
				throw new ArgumentException("zone is not a Func<Player, bool>!");
			}

			if (lightColor is not Vector3 LiquidLightColor)
			{
				throw new ArgumentException("light Color is not a Vector3!");
			}

			if (waterfallGlowmask is not bool LiquidWaterfallGlowmask)
			{
				throw new ArgumentException("waterfall Glowmask is not a Bool!");
			}

			moddedLavaStyles.Add(LiquidBubble);
		}

		internal static void LavaStyle(object texture, object block, object slope, object waterfall, object dustID, object goreID, object zone, object lightColor, object waterfallGlowmask, object buffID, object keepOnFire)
		{
			if (texture is not Texture2D LiquidTexture)
			{
				throw new ArgumentException("texture is not a Texture2D!");
			}

			if (block is not Texture2D LiquidBlockTexture)
			{
				throw new ArgumentException("block texture is not a Texture2D!");
			}

			if (slope is not Texture2D LiquidSlopeTexture)
			{
				throw new ArgumentException("slope texture is not a Texture2D!");
			}

			if (waterfall is not Texture2D LiquidWaterfallTexture)
			{
				throw new ArgumentException("waterfall texture is not a Texture2D!");
			}

			if (dustID is not int LiquidBubble)
			{
				throw new ArgumentException("dust ID is not a Int!");
			}

			if (goreID is not int LiquidDroplet)
			{
				throw new ArgumentException("gore ID is not a Int!");
			}

			if (zone is not Func<Player, bool> ZoneRegistration)
			{
				throw new ArgumentException("zone is not a Func<Player, bool>!");
			}

			if (lightColor is not Vector3 LiquidLightColor)
			{
				throw new ArgumentException("light Color is not a Vector3!");
			}

			if (waterfallGlowmask is not bool LiquidWaterfallGlowmask)
			{
				throw new ArgumentException("waterfall Glowmask is not a Bool!");
			}

			if (buffID is not int LiquidDebuff)
			{
				throw new ArgumentException("buff ID is not a Int!");
			}

			if (keepOnFire is not bool LiquidOnFire)
			{
				throw new ArgumentException("keep On Fire is not a Bool!");
			}

			moddedLavaStyles.Add(LiquidBubble);
		}
	}
}