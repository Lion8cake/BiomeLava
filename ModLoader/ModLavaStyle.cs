using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Liquid;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace BiomeLava.ModLoader
{
    public abstract class ModLavaStyle : ModTexturedType
	{
		/// <summary>
		/// The ID of the lava style.
		/// </summary>
		public int Slot { get; private set; } = -1;

		public override string Name => NameCall ?? base.Name;

		public override string Texture => TextureCall ?? base.Texture;

		public virtual string BlockTexture => BlockTextureCall ?? Texture + "_Block";

		public virtual string SlopeTexture => SlopeTextureCall ?? Texture + "_Slope";

		public virtual string WaterfallTexture => WaterfallTextureCall ?? Texture + "_Waterfall";

		protected sealed override void Register()
		{
			Slot = LavaStylesLoader.Register(this);
		}

		public sealed override void SetupContent()
		{
			SetStaticDefaults();
		}

		/// <summary>
		/// Allows you to determine how much light this lava emits.<br />
		/// It can also let you light up the block in front of this lava.<br />
		/// Keep in mind this also effects what color the lavafalls emit <br />
		/// See <see cref="M:Terraria.Graphics.Light.TileLightScanner.ApplyLiquidLight(Terraria.Tile,Microsoft.Xna.Framework.Vector3@)" /> for vanilla tile light values to use as a reference.<br />
		/// </summary>
		/// <param name="i">The x position in tile coordinates.</param>
		/// <param name="j">The y position in tile coordinates.</param>
		/// <param name="r">The red component of light, usually a value between 0 and 1</param>
		/// <param name="g">The green component of light, usually a value between 0 and 1</param>
		/// <param name="b">The blue component of light, usually a value between 0 and 1</param>
		public virtual void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			if (LavaLightCall == null)
			{
				r = 0.55f;
				g = 0.33f;
				b = 0.11f;
			}
			else
			{
				Vector3 call = LavaLightCall.Invoke(i, j, r, g, b);
				r = call.X;
				g = call.Y;
				b = call.Z;
			}
		}

		/// <summary>
		/// The ID of the dust that is created when anything splashes in lava.
		/// </summary>
		public virtual int GetSplashDust()
		{
			if (dustCall == null)
			{
				return DustID.Lava;
			}
			else
			{
				int call = dustCall.Invoke();
				return call;
			}
		}

		/// <summary>
		/// The ID of the gore that represents droplets of lava falling down from a block. Return <see cref="F:Terraria.ID.GoreID.LavaDrip" /> (or another existing droplet gore).
		/// </summary>
		public virtual int GetDropletGore()
		{
			if (goreCall == null)
			{
				return GoreID.LavaDrip;
			}
			else
			{
				int call = goreCall.Invoke();
				return call;
			}
		}

		/// <summary>
		/// Return true if the player is in the correct zone to activate the lava.
		/// </summary>
		/// <returns></returns>
		public virtual bool IsLavaActive()
		{
			if (IsActiveCall == null)
			{
				return false; 
			}
			else
			{
				bool call = IsActiveCall.Invoke();
				return call;
			}
		}

		/// <summary>
		/// Return false if the waterfall made by the lavastyle should have a glowmask
		/// </summary>
		/// <returns></returns>
		public virtual bool LavafallGlowmask()
		{
			if (LavafallGlowmaskCall == null)
			{
				return true;
			}
			else
			{
				bool call = LavafallGlowmaskCall.Invoke();
				return call;
			}
		}

		/// <summary>
		/// Return false if the lava style shouldnt inflict OnFire when an entity enters the lava
		/// </summary>
		/// <returns></returns>
		public virtual bool InflictsOnFire()
		{
			if (InflictsOnFireCall == null)
			{
				return false;
			}
			else
			{
				bool call = InflictsOnFireCall.Invoke();
				return call;
			}
		}

		/// <summary>
		/// The ID of the buff that is given to both the player and NPCs when they enter the lava.
		/// </summary>
		public virtual int GetFlameDebuff()
		{
			if (buffCall == null)
			{
				return BuffID.OnFire;
			}
			else
			{
				int call = buffCall.Invoke();
				return call;
			}
		}

		//Modcall stuff
		internal string NameCall;

		internal string TextureCall;

		internal string BlockTextureCall;

		internal string SlopeTextureCall;

		internal string WaterfallTextureCall;

		internal Func<int> dustCall;

		internal Func<int> goreCall;

		internal Func<bool> IsActiveCall;

		internal Func<int, int, float, float, float, Vector3> LavaLightCall;

		internal Func<bool> LavafallGlowmaskCall;

		internal Func<int> buffCall;

		internal Func<bool> InflictsOnFireCall;
	}
}