using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace BiomeLava.Content.Fountains
{
	public class HallowObsidianFountain : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.useStyle = 1;
			Item.useTurn = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.autoReuse = true;
			Item.maxStack = Item.CommonMaxStack;
			Item.consumable = true;
			Item.createTile = ModContent.TileType<ObsidianFountains>();
			Item.placeStyle = 3;
			Item.width = 26;
			Item.height = 36;
			Item.value = Item.buyPrice(0, 4);
			Item.rare = ItemRarityID.Orange;
		}
	}
}
