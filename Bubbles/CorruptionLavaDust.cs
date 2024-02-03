namespace BiomeLava.Bubbles;

public class CorruptionLavaDust : BiomeLavaDust
{
    protected override Color LightColor => new(0.3f, 1f, 0.1f);
    protected override string StyleName => "Corruption";
}
