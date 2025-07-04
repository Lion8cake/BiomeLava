using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace BiomeLava.Content.Debuffs
{
	public class Tarred : ModBuff
	{
		public override bool IsLoadingEnabled(Mod mod)
		{
			return ModContent.GetInstance<BiomeLavaConfig>().LavaDebuffs;
		}

		public override string Texture => "BiomeLava/Assets/Jungle/Tarred";

		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			Main.buffNoSave[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex) {
			player.GetModPlayer<TarredPlayer>().tarred = true;
			player.moveSpeed /= 4f;
			if (player.velocity.Y == 0f && Math.Abs(player.velocity.X) > 1f)
			{
				player.velocity.X /= 3f;
			}
		}
		
		public override void Update(NPC npc, ref int buffIndex)
		{
			npc.GetGlobalNPC<TarredNPC>().tarred = true;
			npc.velocity /= 4f;
			if (npc.velocity.Y == 0f && Math.Abs(npc.velocity.X) > 1f)
			{
				npc.velocity.X /= 3f;
			}
		}
	}

	public class TarredPlayer : ModPlayer
	{
		public bool tarred;

		public override void ResetEffects()
		{
			tarred = false;
		}

		public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
		{
			if (tarred)
			{
				r = 0.65f;
				g = 0.65f;
				b = 0.65f;

				if (Main.rand.NextBool(7) && drawInfo.shadow == 0f)
				{
					Dust dust = Main.dust[Dust.NewDust(drawInfo.Position, Player.width, Player.height, DustID.TintableDust, 0f, 0f, 100)];
					dust.scale = 0.6f + Main.rand.NextFloat() * 0.6f;
					dust.velocity *= 0.01f;
					dust.color = new Color(120, 120, 120, 120);
				}
			}
		}
	}

	public class TarredNPC : GlobalNPC
	{
		public override bool IsLoadingEnabled(Mod mod)
		{
			return ModContent.GetInstance<BiomeLavaConfig>().LavaDebuffs;
		}

		public override bool InstancePerEntity => true;

		public bool tarred;

		public override void ResetEffects(NPC npc)
		{
			tarred = false;
		}

		public override void DrawEffects(NPC npc, ref Color drawColor)
		{
			if (tarred)
			{
				Color newColor = drawColor;
				newColor.R = 120;
				newColor.G = 120;
				newColor.B = 120;
				drawColor = newColor;

				if (Main.rand.NextBool(7))
				{
					Dust dust = Main.dust[Dust.NewDust(npc.position, npc.width, npc.height, DustID.TintableDust, 0f, 0f, 100)];
					dust.scale = 0.6f + Main.rand.NextFloat() * 0.6f;
					dust.velocity *= 0.01f;
					dust.color = new Color(120, 120, 120, 120);
				}
			}
		}
	}
}
