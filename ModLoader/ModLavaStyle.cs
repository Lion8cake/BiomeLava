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

		public virtual string BlockTexture => Texture + "_Block";

		public virtual string SlopeTexture => Texture + "_Slope";

		public virtual string WaterfallTexture => Texture + "_Waterfall";

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
			r = 0.55f;
			g = 0.33f;
			b = 0.11f;
		}

		/// <summary>
		/// The ID of the dust that is created when anything splashes in lava.
		/// </summary>
		public abstract int GetSplashDust();

		/// <summary>
		/// The ID of the gore that represents droplets of lava falling down from a block. Return <see cref="F:Terraria.ID.GoreID.LavaDrip" /> (or another existing droplet gore).
		/// </summary>
		public abstract int GetDropletGore();

		/// <summary>
		/// Return true if the player is in the correct zone to activate the lava.
		/// </summary>
		/// <returns></returns>
		public abstract bool IsLavaActive();

		/// <summary>
		/// Return false if the waterfall made by the lavastyle should have a glowmask
		/// </summary>
		/// <returns></returns>
		public virtual bool LavafallGlowmask()
		{
			return true; 
		}

		/// <summary>
		/// Return false if the lava style shouldnt inflict OnFire when an entity enters the lava
		/// </summary>
		/// <returns></returns>
		public virtual bool InflictsOnFire()
		{
			return true; 
		}

		/// <summary>
		/// The ID of the buff that is given to both the player and NPCs when they enter the lava.
		/// </summary>
		public virtual int GetFlameDebuff()
		{
			return BuffID.OnFire;
		}
	}
}