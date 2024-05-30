using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace DinoForm
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public KeybindList FireKey { get; set; } = new KeybindList(SButton.MouseLeft);
        public string TransformSound { get; set; } = "cowboy_explosion";
        public string FireSound { get; set; } = "furnace";
        public int FireDamage { get; set; } = 10;
        public int FireDistance { get; set; } = 256;
    }
}
