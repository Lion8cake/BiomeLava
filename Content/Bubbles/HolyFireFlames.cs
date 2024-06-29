using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System;
using Terraria;
using Terraria.ModLoader;

namespace BiomeLava.Content.Bubbles
{
	public class HolyFireFlames : ModDust
	{
		public override string Texture => "BiomeLava/Assets/Hallow/HolyFireFlames";

        public override void OnSpawn(Dust dust)
        {
			dust.velocity.Y = (float)Main.rand.Next(-10, 6) * 0.1f;
			dust.velocity.X *= 0.3f;
			dust.scale *= 0.7f;
		}

		public override bool Update(Dust dust)
		{
			if (dust.scale > 10f)
			{
				dust.active = false;
			}
			Dust.lavaBubbles++;
			dust.position += dust.velocity;

			if (!dust.noGravity)
			{
				dust.velocity.Y += 0.05f;
			}
			if (!dust.noLight && !dust.noLightEmittence)
			{
				if (dust.frame == new Rectangle(0, 0, 10, 10))
				{
					Lighting.AddLight((int)(dust.position.X / 16f), (int)(dust.position.Y / 16f), 2.27f, 1.7f, 2.3f);
				}
				else if (dust.frame == new Rectangle(0, 0, 10, 20))
				{
					Lighting.AddLight((int)(dust.position.X / 16f), (int)(dust.position.Y / 16f), 2.5f, 2.32f, 1.36f);
				}
				else
				{
					Lighting.AddLight((int)(dust.position.X / 16f), (int)(dust.position.Y / 16f), 1.84f, 2f, 2.41f);
				}
			}

			dust.rotation += dust.velocity.X * 0.5f;
			if (dust.fadeIn > 0f && dust.fadeIn < 100f)
			{
				dust.scale += 0.03f;
				if (dust.scale > dust.fadeIn)
				{
					dust.fadeIn = 0f;
				}
			}
			dust.scale -= 0.01f;
			if (dust.noGravity)
			{
				dust.velocity *= 0.92f;
				if (dust.fadeIn == 0f)
				{
					dust.scale -= 0.04f;
				}
			}
			if (dust.position.Y > Main.screenPosition.Y + (float)Main.screenHeight)
			{
				dust.active = false;
			}
			float num17 = 0.1f;
			if ((double)Dust.dCount == 0.5)
			{
				dust.scale -= 0.001f;
			}
			if ((double)Dust.dCount == 0.6)
			{
				dust.scale -= 0.0025f;
			}
			if ((double)Dust.dCount == 0.7)
			{
				dust.scale -= 0.005f;
			}
			if ((double)Dust.dCount == 0.8)
			{
				dust.scale -= 0.01f;
			}
			if ((double)Dust.dCount == 0.9)
			{
				dust.scale -= 0.02f;
			}
			if ((double)Dust.dCount == 0.5)
			{
				num17 = 0.11f;
			}
			if ((double)Dust.dCount == 0.6)
			{
				num17 = 0.13f;
			}
			if ((double)Dust.dCount == 0.7)
			{
				num17 = 0.16f;
			}
			if ((double)Dust.dCount == 0.8)
			{
				num17 = 0.22f;
			}
			if ((double)Dust.dCount == 0.9)
			{
				num17 = 0.25f;
			}
			if (dust.scale < num17)
			{
				dust.active = false;
			}
			return false;
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return new Color((int)lightColor.R, (int)lightColor.G, (int)lightColor.B, 25);
		}
	}
}