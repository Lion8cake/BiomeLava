namespace BiomeLava.Droplets;

public class IceDroplet : BiomeLavaDroplet
{
    protected override string StyleName => "Ice";
    protected override Color LightColor => new(0.7f, 0.5f, 0.3f);
}
