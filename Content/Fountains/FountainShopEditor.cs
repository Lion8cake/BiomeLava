using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace BiomeLava.Content.Fountains
{
	public class FountainShopEditor : GlobalNPC
	{
		public override void ModifyShop(NPCShop shop)
		{
			if (shop.NpcType == NPCID.WitchDoctor)
			{
				shop.InsertAfter(ItemID.OasisFountain, ModContent.ItemType<PurityObsidianFountain>());
				shop.InsertAfter(ModContent.ItemType<PurityObsidianFountain>(), ModContent.ItemType<CorruptObsidianFountain>());
				shop.InsertAfter(ModContent.ItemType<CorruptObsidianFountain>(), ModContent.ItemType<CrimsonObsidianFountain>());
				shop.InsertAfter(ModContent.ItemType<CrimsonObsidianFountain>(), ModContent.ItemType<HallowObsidianFountain>());
				shop.InsertAfter(ModContent.ItemType<HallowObsidianFountain>(), ModContent.ItemType<JungleObsidianFountain>()); 
				shop.InsertAfter(ModContent.ItemType<JungleObsidianFountain>(), ModContent.ItemType<IceObsidianFountain>());
				shop.InsertAfter(ModContent.ItemType<IceObsidianFountain>(), ModContent.ItemType<DesertObsidianFountain>());
			}
		}
	}
}
