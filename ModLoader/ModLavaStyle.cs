using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BiomeLava.ModLoader {
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
		public virtual int GetSplashDust()
		{
			return DustID.Lava;
		}

		/// <summary>
		/// The ID of the gore that represents droplets of lava falling down from a block. Return <see cref="F:Terraria.ID.GoreID.LavaDrip" /> (or another existing droplet gore).
		/// </summary>
		public virtual int GetDropletGore()
		{
			return GoreID.LavaDrip;
		}

		/// <summary>
		/// Return true if the player is in the correct zone to activate the lava.
		/// </summary>
		/// <returns></returns>
		public virtual bool IsLavaActive()
		{
			return false;
		}

		/// <summary>
		/// Return false if the waterfall made by the lavastyle should have a glowmask
		/// </summary>
		/// <returns></returns>
		public virtual bool LavafallGlowmask()
		{
			return true;
		}

		/// <summary>
		/// Return false if the lava style shouldnt inflict OnFire when an entity enters the lava while this style is active
		/// </summary>
		/// <returns></returns>
		public virtual bool InflictsOnFire()
		{
			return false;
		}

		/// <summary>
		/// Allows your lavastyle to inflict debuffs to Players and NPCs when they enter your lava style <br />
		/// Only runs when the BiomeLava Config Lava Style Debuffs is active. Otherwise this method wont run. <br />
		/// Check for if Player or NPC is null before doing each other's Add debuff code. <br />
		/// For NPCS only: <br />
		/// Npc debuff code is only ran client side to prevent massive issues with detecting if an NPC is allowed to have What lava style. <br />
		/// all code is called ONLY in singleplayer for NPCs.
		/// </summary>
		/// <param name="player">The Player that is inflicted with the debuff apon entering the lavastyle</param>
		/// <param name="npc">The NPC that is inflicted with the debuff apon entering the lavastyle</param>
		/// <param name="onfireDuration">The duration of the OnFire! debuff. This allows for easy replacement of OnFire</param>
		public virtual void InflictDebuff(Player player, NPC npc, int onfireDuration)
		{
		}
	}
}