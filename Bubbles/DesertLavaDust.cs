namespace BiomeLava.Bubbles;

public class DesertLavaDust : BiomeLavaDust
{
    protected override Color LightColor => new(0.7f, 0.5f, 0.1f);
    protected override string StyleName => "Desert";
}
