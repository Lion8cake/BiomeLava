using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Microsoft.Xna.Framework;

namespace BiomeLava.Content.Fountains
{
	public class ObsidianFountains : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = false;
			Main.tileWaterDeath[Type] = false;
			TileID.Sets.HasOutlines[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.Height = 4;
			TileObjectData.newTile.Origin = new Point16(1, 3);
			TileObjectData.newTile.CoordinateHeights = new int[4] { 16, 16, 16, 16 };
			TileObjectData.newTile.DrawYOffset = 2;
			TileObjectData.addTile(Type);
			AnimationFrameHeight = 72;
			AddMapEntry(new Color(69, 69, 69));
			DustType = DustID.Asphalt;
		}

		public override void NearbyEffects(int i, int j, bool closer)
		{
			if (closer)
			{
				Tile tile2 = Main.tile[i, j];
				if (tile2.TileFrameY >= 72)
				{
					int ActiveFountainColor = -1;
					if (tile2.TileFrameX / 36 >= 0 && tile2.TileFrameX / 36 <= 6)
					{
						ActiveFountainColor = tile2.TileFrameX / 36;
					}
					BiomeLava.instance.ActiveLavaFountainColor = ActiveFountainColor;
				}
			}
		}

		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
		{
			return true;
		}

		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
		{
			offsetY = 2;
		}

		public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
		{
			if (Main.tile[i, j].TileFrameY >= 72)
			{
				frameYOffset = Main.tileFrame[type];
				int num43 = i;
				if (Main.tile[i, j].TileFrameX % 36 != 0)
				{
					num43--;
				}
				frameYOffset += num43 % 6;
				if (frameYOffset >= 6)
				{
					frameYOffset -= 6;
				}
				frameYOffset *= 72;
			}
			else
			{
				frameYOffset = 0;
			}
		}

		public override void AnimateTile(ref int frame, ref int frameCounter)
		{
			frameCounter++;
			if (frameCounter > 4)
			{
				frameCounter = 0;
				frame++;
				if (frame >= 6)
				{
					frame = 0;
				}
			}
		}

		public override void HitWire(int i, int j)
		{
			SwitchLavaFountain(i, j);
		}

		public static void SwitchLavaFountain(int i, int j)
		{
			int num = i;
			int num2 = j;
			Tile tile = Main.tile[i, j];
			int num3;
			for (num3 = tile.TileFrameX / 18; num3 >= 2; num3 -= 2)
			{
			}
			tile = Main.tile[i, j];
			int num4 = tile.TileFrameY / 18;
			if (num4 >= 4)
			{
				num4 -= 4;
			}
			num = i - num3;
			num2 = j - num4;
			for (int k = num; k < num + 2; k++)
			{
				for (int l = num2; l < num2 + 4; l++)
				{
					tile = Main.tile[k, l];
					if (!tile.HasTile)
					{
						continue;
					}
					if (tile.TileFrameY < 72)
					{
						tile.TileFrameY += 72;
					}
					else
					{
						tile.TileFrameY -= 72;
					}
				}
			}
			if (Wiring.running)
			{
				Wiring.SkipWire(num, num2);
				Wiring.SkipWire(num, num2 + 1);
				Wiring.SkipWire(num, num2 + 2);
				Wiring.SkipWire(num, num2 + 3);
				Wiring.SkipWire(num + 1, num2);
				Wiring.SkipWire(num + 1, num2 + 1);
				Wiring.SkipWire(num + 1, num2 + 2);
				Wiring.SkipWire(num + 1, num2 + 3);
			}
			NetMessage.SendTileSquare(-1, num, num2, 2, 4);
		}


		public override bool RightClick(int i, int j)
		{
			SwitchLavaFountain(i, j);
			SoundEngine.PlaySound(SoundID.Mech, (Vector2?)new Vector2((float)(i * 16), (float)(j * 16)));
			return true;
		}

		public override IEnumerable<Item> GetItemDrops(int i, int j)
		{
			int itemID = ModContent.ItemType<PurityObsidianFountain>();
			switch (Main.tile[i, j].TileFrameX / 36)
			{
				case 0:
					itemID = ModContent.ItemType<PurityObsidianFountain>();
					break;
				case 1:
					itemID = ModContent.ItemType<CorruptObsidianFountain>();
					break;
				case 2:
					itemID = ModContent.ItemType<CrimsonObsidianFountain>();
					break;
				case 3:
					itemID = ModContent.ItemType<HallowObsidianFountain>();
					break;
				case 4:
					itemID = ModContent.ItemType<JungleObsidianFountain>();
					break;
				case 5:
					itemID = ModContent.ItemType<IceObsidianFountain>();
					break;
				case 6:
					itemID = ModContent.ItemType<DesertObsidianFountain>();
					break;
			}
			yield return new Item(itemID);
		}

		public override void MouseOver(int i, int j)
		{
			Player player = Main.LocalPlayer;
			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
			switch (Main.tile[i, j].TileFrameX / 36)
			{
				case 0:
					player.cursorItemIconID = ModContent.ItemType<PurityObsidianFountain>();
					break;
				case 1:
					player.cursorItemIconID = ModContent.ItemType<CorruptObsidianFountain>();
					break;
				case 2:
					player.cursorItemIconID = ModContent.ItemType<CrimsonObsidianFountain>();
					break;
				case 3:
					player.cursorItemIconID = ModContent.ItemType<HallowObsidianFountain>();
					break;
				case 4:
					player.cursorItemIconID = ModContent.ItemType<JungleObsidianFountain>();
					break;
				case 5:
					player.cursorItemIconID = ModContent.ItemType<IceObsidianFountain>();
					break;
				case 6:
					player.cursorItemIconID = ModContent.ItemType<DesertObsidianFountain>();
					break;
			}
		}
	}
}
