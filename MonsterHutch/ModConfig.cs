﻿using StardewModdingAPI;
using System;
using System.Diagnostics.CodeAnalysis;

namespace MonsterHutch
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
        public bool RandomizeMonsterPositions { get; set; } = true;

        public bool RandomizeOnlyModMonsterPositions { get; set; } = true;

        public int IncubatorDuration { get; set; } = 3;

        public bool IncubatorDurationIsInDaysInsteadOfMinutes { get; set; } = true;

        public bool IncubatorIsAffectedByCoopmaster { get; set; } = true;

        public bool IncubatorWobblesWhileIncubating { get; set; } = true;

        internal const string DefaultIncubatorAdditionalRequiredItemID = "(O)769";
        public string IncubatorAdditionalRequiredItemID { get; set; } = DefaultIncubatorAdditionalRequiredItemID;

        public int IncubatorAdditionalRequiredItemCount { get; set; } = 10;

        internal const string DefaultIncubatorRecipe = "337 2 769 50";

        public string IncubatorRecipe { get; set; } = DefaultIncubatorRecipe;

        internal const string DefaultIncubatorRecipeUnlock = "Combat 8";

        public string IncubatorRecipeUnlock { get; set; } = DefaultIncubatorRecipeUnlock;

        internal static string[] IncubatorDurationIsInChoices { get; set; } = new string[] { "Minutes", "Days" };

        public static void VerifyConfigValues(ModConfig config, ModEntry mod)
        {
            bool invalidConfig = false;
            bool minorInvalidConfig = false;

            if (config.IncubatorDuration < 1)
            {
                config.IncubatorDuration = 1;
                invalidConfig = true;
            }

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

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1107:CodeMustNotContainMultipleStatementsOnOneLine", Justification = "Reviewed.")]
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "Necessary.")]
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

            api.AddBoolOption(manifest, () => config.RandomizeMonsterPositions, (bool val) => config.RandomizeMonsterPositions = val,
                GetConfigName(mod, "RandomizeMonsterPositions"), GetConfigDescription(mod, "RandomizeMonsterPositions"));
            api.AddBoolOption(manifest, () => config.RandomizeOnlyModMonsterPositions, (bool val) => config.RandomizeOnlyModMonsterPositions = val,
                GetConfigName(mod, "RandomizeOnlyModMonsterPositions"), null);

            api.AddSectionTitle(manifest, GetConfigName(mod, "IncubatorSection"));

            api.AddNumberOption(manifest, () => config.IncubatorDuration, (int val) => config.IncubatorDuration = val,
                GetConfigName(mod, "IncubatorDuration"), GetConfigDescription(mod, "IncubatorDuration"), 1);
            api.AddTextOption(manifest, () => GetElementFromConfig(IncubatorDurationIsInChoices, config.IncubatorDurationIsInDaysInsteadOfMinutes ? 1 : 0), (string val) => config.IncubatorDurationIsInDaysInsteadOfMinutes = GetIndexFromArrayElement(IncubatorDurationIsInChoices, val) == 1,
                GetConfigName(mod, "IncubatorDurationIsIn"), null, IncubatorDurationIsInChoices, GetConfigDropdownChoice(mod, "IncubatorDurationIsIn"));

            api.AddBoolOption(manifest, () => config.IncubatorIsAffectedByCoopmaster, (bool val) => config.IncubatorIsAffectedByCoopmaster = val,
                GetConfigName(mod, "IncubatorIsAffectedByCoopmaster"), GetConfigDescription(mod, "IncubatorIsAffectedByCoopmaster"));

            api.AddBoolOption(manifest, () => config.IncubatorWobblesWhileIncubating, (bool val) => config.IncubatorWobblesWhileIncubating = val,
                GetConfigName(mod, "IncubatorWobblesWhileIncubating"));

            api.AddTextOption(manifest, () => config.IncubatorAdditionalRequiredItemID, (string val) => config.IncubatorAdditionalRequiredItemID = val,
                GetConfigName(mod, "IncubatorAdditionalRequiredItemID"), GetConfigDescription(mod, "IncubatorAdditionalRequiredItemID"));
            api.AddNumberOption(manifest, () => config.IncubatorAdditionalRequiredItemCount, (int val) => config.IncubatorAdditionalRequiredItemCount = val,
                GetConfigName(mod, "IncubatorAdditionalRequiredItemCount"), GetConfigDescription(mod, "IncubatorAdditionalRequiredItemCount"), 0);

            api.AddTextOption(manifest, () => config.IncubatorRecipe, (string val) => config.IncubatorRecipe = val,
                GetConfigName(mod, "IncubatorRecipe"), GetConfigDescription(mod, "IncubatorRecipe"));
            api.AddTextOption(manifest, () => config.IncubatorRecipeUnlock, (string val) => config.IncubatorRecipeUnlock = val,
                GetConfigName(mod, "IncubatorRecipeUnlock"), GetConfigDescription(mod, "IncubatorRecipeUnlock"));
        }

        private static Func<string, string> GetConfigDropdownChoice(ModEntry mod, string key)
        {
            return (s) => mod.Helper.Translation.Get($"Config{key}{s}");
        }

        private static Func<string> GetConfigName(ModEntry mod, string key)
        {
            return () => mod.Helper.Translation.Get($"Config{key}");
        }

        private static Func<string> GetConfigDescription(ModEntry mod, string key)
        {
            return () => mod.Helper.Translation.Get($"Config{key}Description");
        }

        private static string GetElementFromConfig(string[] options, int config)
        {
            if (config >= 0 && config < options.Length)
            {
                return options[config];
            }
            else
            {
                return options[0];
            }
        }

        private static int GetIndexFromArrayElement(string[] options, string element)
        {
            var index = Array.IndexOf(options, element);

            return index == -1 ? 0 : index;
        }

    }
}
