namespace BiomeLava.Droplets;

// This one is solely for this mod to use since it forces asset paths
public abstract class BiomeLavaDroplet : BaseDroplet
{
    protected abstract string StyleName { get; }

    public override string Texture => $"BiomeLava/Assets/{StyleName}/{StyleName}Lava_Droplet";
}

// This is just copied from vanilla
public abstract class BaseDroplet : ModGore
{
    protected abstract Color LightColor { get; }

    public override void OnSpawn(Gore gore, IEntitySource source)
    {
        gore.numFrames = 15;
        gore.behindTiles = true;
        gore.timeLeft = Gore.goreTime * 3;
    }

    public override Color? GetAlpha(Gore gore, Color lightColor)
    {
        return new Color(255, 255, 255, 200);
    }

    public override bool Update(Gore gore)
    {
        gore.alpha = gore.position.Y < Main.worldSurface * 16.0 + 8.0 ? 0 : 100;

        var frameDuration = 4;
        gore.frameCounter += 1;

        switch (gore.frame)
        {
            case <= 4:
            {
                var tileX = (int)(gore.position.X / 16f);
                var tileY = (int)(gore.position.Y / 16f) - 1;
                if (WorldGen.InWorld(tileX, tileY) && !Main.tile[tileX, tileY].HasTile)
                    gore.active = false;

                frameDuration = gore.frame switch
                {
                    0 or 1 or 2 => 24 + Main.rand.Next(256),
                    3 => 24 + Main.rand.Next(96),
                    _ => frameDuration
                };

                if (gore.frameCounter >= frameDuration)
                {
                    gore.frameCounter = 0;
                    gore.frame += 1;

                    if (gore.frame == 5)
                    {
                        var droplet = Gore.NewGoreDirect(new EntitySource_Misc(GetType().Name), gore.position, gore.velocity, gore.type);
                        droplet.frame = 9;
                        droplet.velocity *= 0f;
                    }
                }

                break;
            }
            case <= 6:
            {
                frameDuration = 8;

                if (gore.frameCounter >= frameDuration)
                {
                    gore.frameCounter = 0;
                    gore.frame += 1;

                    if (gore.frame == 7)
                        gore.active = false;
                }

                break;
            }
            case <= 9:
            {
                frameDuration = 6;
                gore.velocity.Y += 0.2f;

                if (gore.velocity.Y < 0.5f)
                    gore.velocity.Y = 0.5f;

                if (gore.velocity.Y > 12f)
                    gore.velocity.Y = 12f;

                if (gore.frameCounter >= frameDuration)
                {
                    gore.frameCounter = 0;
                    gore.frame += 1;
                }

                if (gore.frame > 9)
                    gore.frame = 7;

                break;
            }
            default:
            {
                gore.velocity.Y += 0.1f;

                if (gore.frameCounter >= frameDuration)
                {
                    gore.frameCounter = 0;
                    gore.frame += 1;
                }

                gore.velocity *= 0f;
                if (gore.frame > 14)
                    gore.active = false;

                break;
            }
        }

        var lightStrength = 0.6f;
        lightStrength *= gore.frame == 0 ? 0.1f :
            gore.frame == 1 ? 0.2f :
            gore.frame == 2 ? 0.3f :
            gore.frame == 3 ? 0.4f :
            gore.frame == 4 ? 0.5f :
            gore.frame == 5 ? 0.4f :
            gore.frame == 6 ? 0.2f :
            gore.frame <= 9 ? 0.5f :
            gore.frame == 10 ? 0.5f :
            gore.frame == 11 ? 0.4f :
            gore.frame == 12 ? 0.3f :
            gore.frame == 13 ? 0.2f :
            gore.frame != 14 ? 0f : 0.1f;

        Lighting.AddLight(gore.position + Vector2.One * 8, LightColor.ToVector3() * lightStrength);

        var oldVelocity = gore.velocity;
        gore.velocity = Collision.TileCollision(gore.position, gore.velocity, 16, 14);

        if (gore.velocity != oldVelocity)
        {
            if (gore.frame < 10)
            {
                gore.frame = 10;
                gore.frameCounter = 0;
            }
        }
        else if (Collision.WetCollision(gore.position + gore.velocity, 16, 14))
        {
            if (gore.frame < 10)
            {
                gore.frame = 10;
                gore.frameCounter = 0;
            }

            var tileX = (int)(gore.position.X + 8f) / 16;
            var tileY = (int)(gore.position.Y + 14f) / 16;
            var tile = Main.tile[tileX, tileY];

            if (tile.LiquidAmount > 0)
            {
                // ReSharper disable once PossibleLossOfFraction
                gore.position.Y = tileY * 16 - tile.LiquidAmount / 16;
                gore.velocity *= 0f;
            }
        }

        gore.position += gore.velocity;

        return false;
    }
}
