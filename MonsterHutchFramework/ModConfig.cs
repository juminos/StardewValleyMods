using StardewModdingAPI;

namespace MonsterHutchFramework
{
    public interface IGenericModConfigMenuApi
    {
        void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false);

        void AddSectionTitle(IManifest mod, Func<string> text, Func<string> tooltip = null);

        void AddNumberOption(IManifest mod, Func<int> getValue, Action<int> setValue, Func<string> name, Func<string> tooltip = null, int? min = null, int? max = null, int? interval = null, Func<int, string> formatValue = null, string fieldId = null);

        void AddNumberOption(IManifest mod, Func<float> getValue, Action<float> setValue, Func<string> name, Func<string> tooltip = null, float? min = null, float? max = null, float? interval = null, Func<float, string> formatValue = null, string fieldId = null);

        void AddBoolOption(IManifest mod, Func<bool> getValue, Action<bool> setValue, Func<string> name, Func<string> tooltip = null, string fieldId = null);

        void AddTextOption(IManifest mod, Func<string> getValue, Action<string> setValue, Func<string> name, Func<string> tooltip = null, string[] allowedValues = null, Func<string, string> formatAllowedValue = null, string fieldId = null);
    }
    public class ModConfig
    {
        public bool HutchExpansion { get; set; } = false;
        public bool RandomizeMonsterPositions { get; set; } = true;
        public bool SkipRandomizeSlimePositions { get; set; } = false;
        public int HutchSlimeCapacity { get; set; } = 20;
        public int HutchMonsterCapacity { get; set; } = 40;
        public bool DoubleNodeDrops { get; set; } = true;
        public bool IncubatorIsAffectedByCoopmaster { get; set; } = true;
        public bool IncubatorWobblesWhileIncubating { get; set; } = false;
        internal const string DefaultIncubatorAdditionalRequiredItemID = "(O)769";
        public string IncubatorAdditionalRequiredItemID { get; set; } = DefaultIncubatorAdditionalRequiredItemID;
        public int IncubatorAdditionalRequiredItemCount { get; set; } = 10;
        internal const string DefaultIncubatorRecipe = "337 2 769 50";
        public string IncubatorRecipe { get; set; } = DefaultIncubatorRecipe;
        internal const string DefaultIncubatorRecipeUnlock = "f Wizard 6";
        public string IncubatorRecipeUnlock { get; set; } = DefaultIncubatorRecipeUnlock;
        public bool LethalRings { get; set; } = false;
        public bool VanillaSlimeRing { get; set; } = false;

        public static void VerifyConfigValues(ModConfig config, ModEntry mod)
        {
            bool invalidConfig = false;
            bool minorInvalidConfig = false;

            if (config.IncubatorAdditionalRequiredItemCount < 0)
            {
                config.IncubatorAdditionalRequiredItemCount = 0;
                minorInvalidConfig = true;
            }
            if (minorInvalidConfig || invalidConfig)
            {
                if (invalidConfig)
                {
                    mod.Monitor.Log("At least one config value was out of range and was reset.", LogLevel.Debug);
                }

                mod.Helper.WriteConfig(config);
            }
        }

        public static void InvalidateCache(ModEntry mod)
        {
            try
            {
                mod.Helper.GameContent.InvalidateCacheAndLocalized("Data/CraftingRecipes");
                mod.Helper.GameContent.InvalidateCacheAndLocalized("Data/BigCraftables");
                mod.Helper.GameContent.InvalidateCacheAndLocalized("Data/Machines");
            }
            catch (Exception e)
            {
                mod.Monitor.Log($"Exception when trying to invalidate cache after config change: {e}", LogLevel.Debug);
            }
        }
        public static void SetUpModConfigMenu(ModConfig config, ModEntry mod)
        {
            IGenericModConfigMenuApi api = mod.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

            if (api == null)
            {
                return;
            }

            var manifest = mod.ModManifest;

            api.Register(
                mod: manifest,
                reset: () => { config = new ModConfig(); InvalidateCache(mod); },
                save: () => { mod.Helper.WriteConfig(config); VerifyConfigValues(config, mod); InvalidateCache(mod); }
            );

            api.AddSectionTitle(manifest, GetConfigName(mod, "HutchSection"));

            api.AddBoolOption(manifest, () => config.HutchExpansion, (val) => config.HutchExpansion = val,
                GetConfigName(mod, "HutchExpansion"), GetConfigDescription(mod, "HutchExpansion"));
            api.AddBoolOption(manifest, () => config.RandomizeMonsterPositions, (val) => config.RandomizeMonsterPositions = val,
                GetConfigName(mod, "RandomizeMonsterPositions"), GetConfigDescription(mod, "RandomizeMonsterPositions"));
            api.AddBoolOption(manifest, () => config.SkipRandomizeSlimePositions, (val) => config.SkipRandomizeSlimePositions = val,
                GetConfigName(mod, "SkipRandomizeSlimePositions"), GetConfigDescription(mod, "SkipRandomizeSlimePositions"));
            api.AddNumberOption(manifest, () => config.HutchSlimeCapacity, (val) => config.HutchSlimeCapacity = val,
                GetConfigName(mod, "HutchSlimeCapacity"), GetConfigDescription(mod, "HutchSlimeCapacity"), 0);
            api.AddNumberOption(manifest, () => config.HutchMonsterCapacity, (val) => config.HutchMonsterCapacity = val,
                GetConfigName(mod, "HutchMonsterCapacity"), GetConfigDescription(mod, "HutchMonsterCapacity"), 0);
            api.AddBoolOption(manifest, () => config.DoubleNodeDrops, (val) => config.DoubleNodeDrops = val,
                GetConfigName(mod, "DoubleNodeDrops"), GetConfigDescription(mod, "DoubleNodeDrops"));

            api.AddSectionTitle(manifest, GetConfigName(mod, "IncubatorSection"));

            api.AddBoolOption(manifest, () => config.IncubatorIsAffectedByCoopmaster, (val) => config.IncubatorIsAffectedByCoopmaster = val,
                GetConfigName(mod, "IncubatorIsAffectedByCoopmaster"), GetConfigDescription(mod, "IncubatorIsAffectedByCoopmaster"));
            api.AddBoolOption(manifest, () => config.IncubatorWobblesWhileIncubating, (val) => config.IncubatorWobblesWhileIncubating = val,
                GetConfigName(mod, "IncubatorWobblesWhileIncubating"));
            api.AddTextOption(manifest, () => config.IncubatorAdditionalRequiredItemID, (val) => config.IncubatorAdditionalRequiredItemID = val,
                GetConfigName(mod, "IncubatorAdditionalRequiredItemID"), GetConfigDescription(mod, "IncubatorAdditionalRequiredItemID"));
            api.AddNumberOption(manifest, () => config.IncubatorAdditionalRequiredItemCount, (val) => config.IncubatorAdditionalRequiredItemCount = val,
                GetConfigName(mod, "IncubatorAdditionalRequiredItemCount"), GetConfigDescription(mod, "IncubatorAdditionalRequiredItemCount"), 0);
            api.AddTextOption(manifest, () => config.IncubatorRecipe, (val) => config.IncubatorRecipe = val,
                GetConfigName(mod, "IncubatorRecipe"), GetConfigDescription(mod, "IncubatorRecipe"));
            api.AddTextOption(manifest, () => config.IncubatorRecipeUnlock, (val) => config.IncubatorRecipeUnlock = val,
                GetConfigName(mod, "IncubatorRecipeUnlock"), GetConfigDescription(mod, "IncubatorRecipeUnlock"));

            api.AddSectionTitle(manifest, GetConfigName(mod, "RingSection"));

            api.AddBoolOption(manifest, () => config.LethalRings, (val) => config.LethalRings = val,
                GetConfigName(mod, "LethalRings"), GetConfigDescription(mod, "LethalRings"));
            api.AddBoolOption(manifest, () => config.VanillaSlimeRing, (val) => config.VanillaSlimeRing = val,
                GetConfigName(mod, "VanillaSlimeRing"), GetConfigDescription(mod, "VanillaSlimeRing"));
        }

        private static Func<string> GetConfigName(ModEntry mod, string key)
        {
            return () => mod.Helper.Translation.Get($"Config{key}");
        }

        private static Func<string> GetConfigDescription(ModEntry mod, string key)
        {
            return () => mod.Helper.Translation.Get($"Config{key}Description");
        }
    }
}
