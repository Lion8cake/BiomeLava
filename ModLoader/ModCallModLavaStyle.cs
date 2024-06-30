using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace BiomeLava.ModLoader
{
	[Autoload(false)]
	internal sealed class ModCallModLavaStyle : ModLavaStyle {
		public override string Name => NameCall ?? throw new Exception($"{nameof(NameCall)} is null for some reason.");
		public override string Texture => TextureCall ?? throw new Exception($"{nameof(TextureCall)} is null for some reason.");
		public override string BlockTexture => BlockTextureCall ?? base.BlockTexture;
		public override string SlopeTexture => SlopeTextureCall ?? base.SlopeTexture;
		public override string WaterfallTexture => WaterfallTextureCall ?? base.WaterfallTexture;

		internal string NameCall;
		internal string TextureCall;
		internal string BlockTextureCall;
		internal string SlopeTextureCall;
		internal string WaterfallTextureCall;

		internal Func<int> dustCall;
		internal Func<int> goreCall;
		internal Func<bool> IsActiveCall;
		internal Func<int, int, float, float, float, Vector3> LavaLightCall;
		internal Func<bool> LavafallGlowmaskCall;
		internal Func<Player, NPC, int, Action> buffCall;
		internal Func<bool> InflictsOnFireCall;

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			if (LavaLightCall == null) {
				base.ModifyLight(i, j, ref r, ref g, ref b);
				return;
			}

			var value = LavaLightCall.Invoke(i, j, r, g, b);
			r = value.X;
			g = value.Y;
			b = value.Z;
		}

		public override int GetSplashDust() {
			return dustCall?.Invoke() ?? base.GetSplashDust();
		}

		public override int GetDropletGore() {
			return goreCall?.Invoke() ?? base.GetDropletGore();
		}

		public override bool IsLavaActive() {
			return IsActiveCall?.Invoke() ?? base.IsLavaActive();
		}

		public override bool LavafallGlowmask() {
			return LavafallGlowmaskCall?.Invoke() ?? base.LavafallGlowmask();
		}

		public override bool InflictsOnFire() {
			return InflictsOnFireCall?.Invoke() ?? base.InflictsOnFire();
		}

		public override void InflictDebuff(Player player, NPC npc, int onfireDuration) {
			buffCall?.Invoke(player, npc, onfireDuration);
		}
	}
}