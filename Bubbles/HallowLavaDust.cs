using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System;
using Terraria;
using Terraria.ModLoader;

namespace BiomeLava.Bubbles
{
	public class HallowLavaDust : ModDust
	{
		public override string Texture => "BiomeLava/Assets/Hallow/HallowLavaBubble";

		public override void SetStaticDefaults()
        {
            //UpdateType = 35;
        }

        public override void OnSpawn(Dust dust)
        {
			dust.velocity *= 0.1f;
			dust.velocity.Y = -0.5f;
		}

		public override bool Update(Dust dust)
		{
			Dust.lavaBubbles++;
			if (dust.velocity.Y > 0f)
			{
				dust.velocity.Y -= 0.2f;
			}
			if (dust.noGravity)
			{
				float num109 = dust.scale * 0.6f;
				if (num109 > 1f)
				{
					num109 = 1f;
				}
				Lighting.AddLight((int)(dust.position.X / 16f), (int)(dust.position.Y / 16f + 1f), num109 * 0.2f, num109 * 0.4f, num109 * 0.5f);
			}
			float num3 = dust.scale * 0.3f + 0.4f;
			if (num3 > 1f)
			{
				num3 = 1f;
			}
			Lighting.AddLight((int)(dust.position.X / 16f), (int)(dust.position.Y / 16f), num3 * 0.2f, num3 * 0.4f, num3 * 0.5f);
			return true;
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			float num = (float)(255 - dust.alpha) / 255f;
			int num4;
			int num3;
			int num2;
			num = (num + 3f) / 4f;
			num4 = (int)((int)lightColor.R * num);
			num3 = (int)((int)lightColor.G * num);
			num2 = (int)((int)lightColor.B * num);
			int num6 = lightColor.A - dust.alpha;
			if (num6 < 0)
			{
				num6 = 0;
			}
			if (num6 > 255)
			{
				num6 = 255;
			}
			return new Color(num4, num3, num2, num6);
		}
	}
}