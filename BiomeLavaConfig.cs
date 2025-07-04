using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;

namespace BiomeLava
{
	public class BiomeLavaConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

		[Header("BiomeLavaServerConfig")]
		[DefaultValue(true)]
		[ReloadRequired]
		public bool LavaDebuffs;
	}
}
