namespace CustomCritters2.Framework.CritterData
{
    internal class LightModel
    {
        public int VanillaLightId = 3;
        public float Radius { get; set; } = 0.5f;
        public LightColorModel Color { get; set; } = new();
    }
}