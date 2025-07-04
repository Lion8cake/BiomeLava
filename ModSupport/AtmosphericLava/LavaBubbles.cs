using Terraria.DataStructures;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent;
using Microsoft.Xna.Framework;
using BiomeLava.ModLoader;
using System;

namespace BiomeLava.ModSupport.AtmosphericLava
{
    public class LavaBubbles : ModGore
    {
		public override bool IsLoadingEnabled(Mod mod)
		{
			return Terraria.ModLoader.ModLoader.TryGetMod("AtmosphericLava", out _);
        }

        public override void SetStaticDefaults()
        {
            ChildSafety.SafeGore[Type] = true;
        }

        public override void OnSpawn(Gore gore, IEntitySource source)
        {
            gore.numFrames = 9;
            gore.behindTiles = true;
            gore.timeLeft = Gore.goreTime * 3;
            gore.scale = Main.rand.NextFloat(0.75f, 1.25f);
        }

        public override bool Update(Gore gore)
        {
            int frameDuration = 4;
            gore.frameCounter += 1;

            int tileX = (int)(gore.position.X / 16f);
            int tileY = (int)(gore.position.Y / 16f) + 1;
            if (Main.tile[tileX, tileY].LiquidType != 1)
            {
                gore.active = false;
            }
            if (gore.frame == 0 || gore.frame == 1 || gore.frame == 2)
            {
                frameDuration = 10 + Main.rand.Next(14);
            }
            if (gore.frame == 3)
            {
                frameDuration = 14 + Main.rand.Next(128);
            }
            if (gore.frame > 3)
            {
                frameDuration = 5 + Main.rand.Next(2);
            }


            if ((int)gore.frameCounter >= frameDuration)
            {
                if (gore.frame == 8)
                    gore.active = false;
                gore.frameCounter = 0;
                gore.frame += 1;
            }

            return false;
        }

		public override Color? GetAlpha(Gore gore, Color lightColor)
		{
			float num = BiomeLava.instance.lavaLightColor[BiomeLava.lavaStyle].X * 1.75f;
			float num2 = BiomeLava.instance.lavaLightColor[BiomeLava.lavaStyle].Y * 1.75f;
			float num3 = BiomeLava.instance.lavaLightColor[BiomeLava.lavaStyle].Z * 1.75f;
			LavaStylesLoader.ModifyLight((int)gore.position.X / 16, (int)gore.position.Y / 16, BiomeLava.lavaStyle, ref num, ref num2, ref num3);
			for (int j = 0; j < LavaStylesLoader.TotalCount; j++)
			{
				if (BiomeLava.lavaLiquidAlpha[j] > 0f && j != BiomeLava.lavaStyle)
				{
					float r = BiomeLava.instance.lavaLightColor[j].X;
					float g = BiomeLava.instance.lavaLightColor[j].Y;
					float b = BiomeLava.instance.lavaLightColor[j].Z;
					LavaStylesLoader.ModifyLight((int)gore.position.X / 16, (int)gore.position.Y / 16, j, ref r, ref g, ref b);
					float r2 = BiomeLava.instance.lavaLightColor[BiomeLava.lavaStyle].X;
					float g2 = BiomeLava.instance.lavaLightColor[BiomeLava.lavaStyle].Y;
					float b2 = BiomeLava.instance.lavaLightColor[BiomeLava.lavaStyle].Z;
					LavaStylesLoader.ModifyLight((int)gore.position.X / 16, (int)gore.position.Y / 16, BiomeLava.lavaStyle, ref r2, ref g2, ref b2);
					num = Single.Lerp(r, r2, BiomeLava.lavaLiquidAlpha[BiomeLava.lavaStyle]);
					num2 = Single.Lerp(g, g2, BiomeLava.lavaLiquidAlpha[BiomeLava.lavaStyle]);
					num3 = Single.Lerp(b, b2, BiomeLava.lavaLiquidAlpha[BiomeLava.lavaStyle]);
				}
			}
			return new Color(num, num2, num3);
		}
	}
}
