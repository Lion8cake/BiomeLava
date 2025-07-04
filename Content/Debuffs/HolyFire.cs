using BiomeLava.Content.Bubbles;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace BiomeLava.Content.Debuffs
{
	public class HolyFire : ModBuff
	{
		public override bool IsLoadingEnabled(Mod mod)
		{
			return ModContent.GetInstance<BiomeLavaConfig>().LavaDebuffs;
		}

		public override string Texture => "BiomeLava/Assets/Hallow/HolyFire";

		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			Main.buffNoSave[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex) {
			player.GetModPlayer<HolyFirePlayer>().holyFire = true;
		}
		
		public override void Update(NPC npc, ref int buffIndex)
		{
			npc.GetGlobalNPC<HolyFireNPC>().holyFire = true;
		}
	}

	public class HolyFirePlayer : ModPlayer
	{
		public bool holyFire;

		public override void ResetEffects()
		{
			holyFire = false;
		}

		public override void UpdateBadLifeRegen()
		{
			if (holyFire)
			{
				if (Player.lifeRegen > 0)
				{
					Player.lifeRegen = 0;
				}
				Player.lifeRegenTime = 0f;
				Player.lifeRegen -= 24;
			}
		}

		public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
		{
			if (holyFire)
			{
				if (Main.rand.NextBool(4) && drawInfo.shadow == 0f)
				{
					Dust dust = Dust.NewDustDirect(new Vector2(drawInfo.Position.X - 2f, drawInfo.Position.Y - 2f), drawInfo.drawPlayer.width + 4, drawInfo.drawPlayer.height + 4, ModContent.DustType<HolyFireFlames>(), drawInfo.drawPlayer.velocity.X * 0.4f, drawInfo.drawPlayer.velocity.Y * 0.4f, 100, default(Color), 3f);
					dust.noGravity = true;
					dust.velocity *= 1.8f;
					dust.velocity.Y -= 0.5f;
				}
			}
		}
	}

	public class HolyFireNPC : GlobalNPC
	{
		public override bool IsLoadingEnabled(Mod mod)
		{
			return ModContent.GetInstance<BiomeLavaConfig>().LavaDebuffs;
		}

		public override bool InstancePerEntity => true;

		public bool holyFire;

		public override void ResetEffects(NPC npc)
		{
			holyFire = false;
		}

		public override void UpdateLifeRegen(NPC npc, ref int damage)
		{
			if (holyFire)
			{
				if (npc.lifeRegen > 0)
				{
					npc.lifeRegen = 0;
				}
				npc.lifeRegen -= 24;
			}
		}

		public override void DrawEffects(NPC npc, ref Color drawColor)
		{
			if (holyFire)
			{
				if (Main.rand.NextBool(4))
				{
					Dust dust = Dust.NewDustDirect(new Vector2(npc.position.X - 2f, npc.position.Y - 2f), npc.width + 4, npc.height + 4, ModContent.DustType<HolyFireFlames>(), npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 100, default(Color), 3f);
					dust.noGravity = true;
					dust.velocity *= 1.8f;
					dust.velocity.Y -= 0.5f;
				}
			}
		}
	}
}
