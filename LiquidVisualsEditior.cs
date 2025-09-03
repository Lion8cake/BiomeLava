using BiomeLava.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ModLiquidLib.ModLoader;
using ModLiquidLib.Utils.Structs;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace BiomeLava
{
	public class LiquidVisualsEditior : GlobalLiquid
	{
		public override bool OnPlayerSplash(Player player, int type, bool isEnter)
		{
			if (type == LiquidID.Lava)
			{
				if (isEnter)
				{
					for (int num107 = 0; num107 < 20; num107++)
					{
						int num108 = Dust.NewDust(new Vector2(player.position.X - 6f, player.position.Y + (float)(player.height / 2) - 8f), player.width + 12, 24, BiomeLava.instance.lavaBubbleDust[BiomeLava.lavaStyle]);
						Main.dust[num108].velocity.Y -= 1.5f;
						Main.dust[num108].velocity.X *= 2.5f;
						Main.dust[num108].scale = 1.3f;
						Main.dust[num108].alpha = 100;
						Main.dust[num108].noGravity = true;
					}
					SoundEngine.PlaySound(SoundID.SplashWeak, player.position);
				}
				else
				{
					for (int num15 = 0; num15 < 20; num15++)
					{
						int num16 = Dust.NewDust(new Vector2(player.position.X - 6f, player.position.Y + (float)(player.height / 2) - 8f), player.width + 12, 24, BiomeLava.instance.lavaBubbleDust[BiomeLava.lavaStyle]);
						Main.dust[num16].velocity.Y -= 1.5f;
						Main.dust[num16].velocity.X *= 2.5f;
						Main.dust[num16].scale = 1.3f;
						Main.dust[num16].alpha = 100;
						Main.dust[num16].noGravity = true;
					}
					SoundEngine.PlaySound(SoundID.SplashWeak, player.position);
				}
				return false;
			}
			return true;
		}

		public override bool OnNPCSplash(NPC npc, int type, bool isEnter)
		{
			if (type == LiquidID.Lava)
			{
				if (isEnter)
				{
					for (int m = 0; m < 10; m++)
					{
						int num6 = Dust.NewDust(new Vector2(npc.position.X - 6f, npc.position.Y + (float)(npc.height / 2) - 8f), npc.width + 12, 24, BiomeLava.instance.lavaBubbleDust[BiomeLava.lavaStyle]);
						Main.dust[num6].velocity.Y -= 1.5f;
						Main.dust[num6].velocity.X *= 2.5f;
						Main.dust[num6].scale = 1.3f;
						Main.dust[num6].alpha = 100;
						Main.dust[num6].noGravity = true;
					}
					if (npc.aiStyle != 1 && type != 1 && type != 16 && type != 147 && type != 59 && type != 300 && npc.aiStyle != 39 && !npc.noGravity)
					{
						SoundEngine.PlaySound(SoundID.SplashWeak, npc.position);
					}
				}
				else
				{
					for (int num2 = 0; num2 < 10; num2++)
					{
						int num3 = Dust.NewDust(new Vector2(npc.position.X - 6f, npc.position.Y + (float)(npc.height / 2) - 8f), npc.width + 12, 24, BiomeLava.instance.lavaBubbleDust[BiomeLava.lavaStyle]);
						Main.dust[num3].velocity.Y -= 1.5f;
						Main.dust[num3].velocity.X *= 2.5f;
						Main.dust[num3].scale = 1.3f;
						Main.dust[num3].alpha = 100;
						Main.dust[num3].noGravity = true;
					}
					if (npc.aiStyle != 1 && type != 1 && type != 16 && type != 59 && type != 300 && npc.aiStyle != 39 && !npc.noGravity)
					{
						SoundEngine.PlaySound(SoundID.SplashWeak, npc.position);
					}
				}
				return false;
			}
			return true;
		}

		public override bool OnProjectileSplash(Projectile proj, int type, bool isEnter)
		{
			if (type == LiquidID.Lava)
			{
				if (isEnter)
				{
					for (int num31 = 0; num31 < 10; num31++)
					{
						int num32 = Dust.NewDust(new Vector2(proj.position.X - 6f, proj.position.Y + (float)(proj.height / 2) - 8f), proj.width + 12, 24, BiomeLava.instance.lavaBubbleDust[BiomeLava.lavaStyle]);
						Main.dust[num32].velocity.Y -= 1.5f;
						Main.dust[num32].velocity.X *= 2.5f;
						Main.dust[num32].scale = 1.3f;
						Main.dust[num32].alpha = 100;
						Main.dust[num32].noGravity = true;
					}
					SoundEngine.PlaySound(SoundID.SplashWeak, proj.position);
				}
				else
				{
					for (int num7 = 0; num7 < 10; num7++)
					{
						int num8 = Dust.NewDust(new Vector2(proj.position.X - 6f, proj.position.Y + (float)(proj.height / 2) - 8f), proj.width + 12, 24, BiomeLava.instance.lavaBubbleDust[BiomeLava.lavaStyle]);
						Main.dust[num8].velocity.Y -= 1.5f;
						Main.dust[num8].velocity.X *= 2.5f;
						Main.dust[num8].scale = 1.3f;
						Main.dust[num8].alpha = 100;
						Main.dust[num8].noGravity = true;
					}
					SoundEngine.PlaySound(SoundID.SplashWeak, proj.position);
				}
				return false;
			}
			return true;
		}

		public override bool OnItemSplash(Item item, int type, bool isEnter)
		{
			if (type == LiquidID.Lava)
			{
				if (isEnter)
				{
					for (int n = 0; n < 5; n++)
					{
						int num14 = Dust.NewDust(new Vector2(item.position.X - 6f, item.position.Y + (float)(item.height / 2) - 8f), item.width + 12, 24, BiomeLava.instance.lavaBubbleDust[BiomeLava.lavaStyle]);
						Main.dust[num14].velocity.Y -= 1.5f;
						Main.dust[num14].velocity.X *= 2.5f;
						Main.dust[num14].scale = 1.3f;
						Main.dust[num14].alpha = 100;
						Main.dust[num14].noGravity = true;
					}
					SoundEngine.PlaySound(SoundID.SplashWeak, item.position);
				}
				else
				{
					for (int num7 = 0; num7 < 5; num7++)
					{
						int num8 = Dust.NewDust(new Vector2(item.position.X - 6f, item.position.Y + (float)(item.height / 2) - 8f), item.width + 12, 24, BiomeLava.instance.lavaBubbleDust[BiomeLava.lavaStyle]);
						Main.dust[num8].velocity.Y -= 1.5f;
						Main.dust[num8].velocity.X *= 2.5f;
						Main.dust[num8].scale = 1.3f;
						Main.dust[num8].alpha = 100;
						Main.dust[num8].noGravity = true;
					}
					SoundEngine.PlaySound(SoundID.SplashWeak, item.position);
				}
				return false;
			}
			return true;
		}

		public override bool PreDraw(int i, int j, int type, LiquidDrawCache liquidDrawCache, Vector2 drawOffset, bool isBackgroundDraw, int waterStyle, float waterAlpha)
		{
			if (type == LiquidID.Lava)
			{
				return false;
			}
			return true;
		}

		public override bool PreRetroDraw(int i, int j, int type, SpriteBatch spriteBatch)
		{
			if (type == LiquidID.Lava)
			{
				return false;
			}
			return true;
		}

		public override bool EmitEffects(int i, int j, int type, LiquidCache liquidCache)
		{
			if (liquidCache.VisibleType == 1 && liquidCache.HasVisibleLiquid && Dust.lavaBubbles < 200)
			{
				if (Main.rand.Next(700) == 0)
				{
					Dust.NewDust(new Vector2((float)(i * 16), (float)(j * 16)), 16, 16, BiomeLava.instance.lavaBubbleDust[BiomeLava.lavaStyle], 0f, 0f, 0, Color.White);
				}
				if (Main.rand.Next(350) == 0)
				{
					int num20 = Dust.NewDust(new Vector2((float)(i * 16), (float)(j * 16)), 16, 8, BiomeLava.instance.lavaBubbleDust[BiomeLava.lavaStyle], 0f, 0f, 50, Color.White, 1.5f);
					Dust obj = Main.dust[num20];
					obj.velocity *= 0.8f;
					Main.dust[num20].velocity.X *= 2f;
					Main.dust[num20].velocity.Y -= (float)Main.rand.Next(1, 7) * 0.1f;
					if (Main.rand.Next(10) == 0)
					{
						Main.dust[num20].velocity.Y *= Main.rand.Next(2, 5);
					}
					Main.dust[num20].noGravity = true;
				}
			}
			return false;
		}

		public override void ModifyLight(int i, int j, int type, ref float r, ref float g, ref float b)
		{
			Tile tile = Main.tile[i, j];
			if (tile.LiquidAmount <= 0)
			{
				return;
			}
			if (tile.LiquidType == LiquidID.Lava)
			{
				float num = BiomeLava.instance.lavaLightColor[BiomeLava.lavaStyle].X;
				float num2 = BiomeLava.instance.lavaLightColor[BiomeLava.lavaStyle].Y;
				float num3 = BiomeLava.instance.lavaLightColor[BiomeLava.lavaStyle].Z;
				LavaStylesLoader.ModifyLight(tile.X(), tile.Y(), BiomeLava.lavaStyle, ref num, ref num2, ref num3);
				for (int l = 0; l < LavaStylesLoader.TotalCount; l++)
				{
					if (BiomeLava.lavaLiquidAlpha[l] > 0f && l != BiomeLava.lavaStyle)
					{
						float r1 = BiomeLava.instance.lavaLightColor[l].X;
						float g1 = BiomeLava.instance.lavaLightColor[l].Y;
						float b1 = BiomeLava.instance.lavaLightColor[l].Z;
						LavaStylesLoader.ModifyLight(tile.X(), tile.Y(), l, ref r1, ref g1, ref b1);
						float r2 = BiomeLava.instance.lavaLightColor[BiomeLava.lavaStyle].X;
						float g2 = BiomeLava.instance.lavaLightColor[BiomeLava.lavaStyle].Y;
						float b2 = BiomeLava.instance.lavaLightColor[BiomeLava.lavaStyle].Z;
						LavaStylesLoader.ModifyLight(tile.X(), tile.Y(), BiomeLava.lavaStyle, ref r2, ref g2, ref b2);
						num = Single.Lerp(r1, r2, BiomeLava.lavaLiquidAlpha[BiomeLava.lavaStyle]);
						num2 = Single.Lerp(g1, g2, BiomeLava.lavaLiquidAlpha[BiomeLava.lavaStyle]);
						num3 = Single.Lerp(b1, b2, BiomeLava.lavaLiquidAlpha[BiomeLava.lavaStyle]);
					}
				}
				if (!(num == 0 && num2 == 0 && num3 == 0))
				{
					float colorManipulator = (float)(270 - Main.mouseTextColor) / 900f;
					num += colorManipulator;
					num2 += colorManipulator;
					num3 += colorManipulator;
				}
				r = num; 
				g = num2; 
				b = num3;
			}
		}
	}
}
