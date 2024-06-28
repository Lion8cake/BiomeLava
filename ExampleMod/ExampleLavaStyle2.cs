using BiomeLava.ModLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;

namespace BiomeLava.ExampleMod
{
	public class ExampleLavaStyle2 : ModLavaStyle
	{
		public override string Texture => "BiomeLava/ExampleMod/ExampleLavaStyle";

		public override int GetSplashDust()
		{
			return DustID.Honey;
		}

		public override int GetDropletGore()
		{
			return GoreID.HoneyDrip;
		}

		public override bool IsLavaActive()
		{
			return Main.LocalPlayer.ZoneUnderworldHeight;
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = 0f;
			g = 0f;
			b = 0f;
		}

		public override bool LavafallGlowmask()
		{
			return false;
		}
	}
}
