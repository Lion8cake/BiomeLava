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
	public class ExampleLavaStyle : ModLavaStyle
	{
		public override int GetSplashDust()
		{
			return DustID.Asphalt;
		}

		public override int GetDropletGore()
		{
			return GoreID.HoneyDrip;
		}

		public override bool IsLavaActive()
		{
			return !Main.IsItDay();
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = 0.4f;
			g = 0.1f;
			b = 0.5f;
		}

		public override bool LavafallGlowmask()
		{
			return true;
		}
	}
}
