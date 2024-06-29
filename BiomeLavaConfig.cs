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
		[Label("Lava style debuffs")]
		[Tooltip("Some lava styles will inflict different debuff/s apon contact")]
		[DefaultValue(true)]
		public bool LavaDebuffs;
	}
}
