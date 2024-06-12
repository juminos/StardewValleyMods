using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace DinoForm
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public string BuffedFoods { get; set; } = "Pepper Poppers";
        public KeybindList TransformKey { get; set; } = new KeybindList();
        public KeybindList FireKey { get; set; } = new KeybindList(SButton.MouseLeft);
        public string TransformSound { get; set; } = "cowboy_explosion";
        public string FireSound { get; set; } = "furnace";
        public int FireDamage { get; set; } = 10;
        public int FireDistance { get; set; } = 256;
        public int Duration { get; set; } = 15;
        public int MoveSpeed { get; set; } = -3;
        public int Defense { get; set; } = 3;
        public int StaminaUse { get; set; } = 0;
    }
}
