﻿using MorePetBreeds.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using StardewValley.Buildings;
using xTile.Dimensions;

namespace MorePetBreeds
{
    // TODO:
    // - Test multiplayer
    //   >>> handle skin id out of bounds
    // - Add In-game menu for removing pets
    // - Use better spawning location (temporarily fixed with supporting mod)
    // - Pet portrait update


    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        internal static IMonitor SMonitor;
        internal static IModHelper SHelper;
        internal static IManifest SModManifest;


        private static bool didPetsWarpHome = false;

        private static Pet newPet;

        private static Dictionary<string, Farmer> allFarmers = new Dictionary<string, Farmer>();
        private static Dictionary<string, Texture2D> catTextureMap = new Dictionary<string, Texture2D>();
        private static Dictionary<string, Texture2D> dogTextureMap = new Dictionary<string, Texture2D>();


        internal static bool IsEnabled = true;


        // Constants
        private readonly uint TextureUpdateRateWithSinglePlayer = 30;
        private readonly uint TextureUpdateRateWithMultiplePlayers = 3;
        internal static readonly string PlayerWarpedHomeMessageId = "PlayerHome";
        internal static readonly string PlayerAddedPetMessageId = "PlayerAddedPet";
        public static string MOD_DATA_SKIN_ID;
        public static string MOD_DATA_OWNER;

        // The minimum version the host must have for the mod to be enabled on a farmhand.
        private readonly string MinHostVersion = "1.0.0";


        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // Static variables
            SMonitor = Monitor;
            SHelper = helper;
            SModManifest = ModManifest;

            MOD_DATA_OWNER = $"{SModManifest.UniqueID}/owner";
            MOD_DATA_SKIN_ID = $"{SModManifest.UniqueID}/skinId";

            // Event handlers
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.Player.Warped += OnWarped;
            helper.Events.Multiplayer.ModMessageReceived += OnModMessageReceived;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Multiplayer.PeerConnected += OnPeerConnected;

            // SMAPI Commands
            helper.ConsoleCommands.Add("list_pets", "Lists the names of all pets on your farm.", CommandHandler.OnCommandReceived);
            helper.ConsoleCommands.Add("add_cat", "Adds a cat of given breed. Breed is a number between 0-2. This will give you an in-game naming prompt.", CommandHandler.OnCommandReceived);
            helper.ConsoleCommands.Add("add_dog", "Adds a dog of given breed. Breed is a number between 0-2. This will give you an in-game naming prompt.", CommandHandler.OnCommandReceived);
            helper.ConsoleCommands.Add("remove_pet", "Removes pet of given name from your farm.", CommandHandler.OnCommandReceived);
            helper.ConsoleCommands.Add("list_farmers", "Lists the names and Multiplayer ID of all farmers", CommandHandler.OnCommandReceived);
            helper.ConsoleCommands.Add("give_pet", "Specify pet name and farmer name that you want to give pet to", CommandHandler.OnCommandReceived);
        }


        /*****************
        ** Event Handlers
        ******************/

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {

            // check if mod should be enabled for the current player
            IsEnabled = Context.IsMainPlayer;
            if (!IsEnabled)
            {
                ISemanticVersion hostVersion = SHelper.Multiplayer.GetConnectedPlayer(Game1.MasterPlayer.UniqueMultiplayerID)?.GetMod(this.ModManifest.UniqueID)?.Version;
                if (hostVersion == null)
                {
                    IsEnabled = false;
                    SMonitor.Log("This mod is disabled because the host player doesn't have it installed.", LogLevel.Warn);
                }
                else if (hostVersion.IsOlderThan(this.MinHostVersion))
                {
                    IsEnabled = false;
                    SMonitor.Log($"This mod is disabled because the host player has {this.ModManifest.Name} {hostVersion}, but the minimum compatible version is {this.MinHostVersion}.", LogLevel.Warn);
                }
                else
                    IsEnabled = true;
            }

            LoadCatSprites();
            LoadDogSprites();
            SetPetSprites();
            GenerateAllFarmersDict();

            if (Context.IsMainPlayer)
            {
                // to handle version change
                foreach (Pet pet in GetAllPets())
                {
                    if (!pet.modData.ContainsKey(MOD_DATA_OWNER))
                    {
                        pet.modData[MOD_DATA_OWNER] = pet.loveInterest;
                    }
                }
            }
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (!Context.IsMainPlayer)
                return;

            didPetsWarpHome = false;
            if (Game1.isRaining)
            {
                didPetsWarpHome = true;
                foreach (Pet pet in GetAllPets())
                {
                    WarpToOwnerFarmHouse(pet);
                }
            }
            else
            {
                foreach (Pet pet in GetAllPets())
                {
                    if (pet.currentLocation is FarmHouse)
                    {
                        GameLocation farm = Game1.getLocationFromName("Farm");

                        // Get the position of the pet owner's home
                        Microsoft.Xna.Framework.Vector2 ownerHomePosition = GetPetOwnerHomePositionOrBowlPosition(pet);
                        if (ownerHomePosition == Microsoft.Xna.Framework.Vector2.Zero)
                        {
                            ModEntry.SMonitor.Log($"Could not find owner's home for pet {pet.Name}.", LogLevel.Warn);
                            continue;
                        }

                        // Find a suitable warp position near the owner's home
                        Microsoft.Xna.Framework.Vector2 warpPosition = FindSuitableWarpPosition(farm, ownerHomePosition, 15);

                        // Warp the pet to the found position
                        if (warpPosition != Microsoft.Xna.Framework.Vector2.Zero)
                        {
                            Game1.warpCharacter(pet, farm, warpPosition);
                            ModEntry.SMonitor.Log($"Pet {pet.Name} warped to farm near owner's home at tile: {warpPosition}", LogLevel.Info);
                        }
                        else
                        {
                            ModEntry.SMonitor.Log($"No suitable warp position found near owner's home for pet {pet.Name}.", LogLevel.Warn);
                        }
                    }
                }
            }
        }

        private Microsoft.Xna.Framework.Vector2 GetPetOwnerHomePositionOrBowlPosition(Pet pet)
        {
            if (pet.modData.ContainsKey(MOD_DATA_OWNER) && pet.modData[MOD_DATA_OWNER] != null)
            {
                if (allFarmers.ContainsKey(pet.modData[MOD_DATA_OWNER]))
                {
                    Farmer owner = allFarmers[pet.modData[MOD_DATA_OWNER]];
                    if (Utility.getHomeOfFarmer(owner) != null)
                    {
                        return Utility.getHomeOfFarmer(owner).getPorchStandingSpot().ToVector2();
                    }
                    else
                    {
                        ModEntry.SMonitor.Log($"Owner's home not found for pet {pet.Name}.", LogLevel.Warn);
                    }
                }
                else
                {
                    ModEntry.SMonitor.Log($"Owner not found in allFarmers for pet {pet.Name}.", LogLevel.Warn);
                }
            }
            else
            {
                ModEntry.SMonitor.Log($"MOD_DATA_OWNER not found for pet {pet.Name}.", LogLevel.Warn);
            }

            // Fallback to pet bowl position if owner's home is not found
            List<PetBowl> petBowls = GetPetBowls();
            if (petBowls.Count > 0)
            {
                PetBowl petBowl = petBowls[0];
                return new Microsoft.Xna.Framework.Vector2(petBowl.tileX.Value, petBowl.tileY.Value);
            }
            else
            {
                ModEntry.SMonitor.Log($"No pet bowls found for pet {pet.Name}.", LogLevel.Warn);
            }

            // If no pet bowls are found, return Vector2.Zero
            return Microsoft.Xna.Framework.Vector2.Zero;
        }

        private Microsoft.Xna.Framework.Vector2 FindSuitableWarpPosition(GameLocation farm, Microsoft.Xna.Framework.Vector2 centerPosition, int initialSearchRadius)
        {
            int searchRadius = initialSearchRadius;
            centerPosition.Y += searchRadius;

            // Create a list to store the random order of iterations
            List<Microsoft.Xna.Framework.Vector2> randomOrder = new List<Microsoft.Xna.Framework.Vector2>();

            while (searchRadius <= 100)
            {
                // Generate a random order of iterations within the search area
                for (int x = (int)centerPosition.X - searchRadius; x <= centerPosition.X + searchRadius; x++)
                {
                    for (int y = (int)centerPosition.Y - searchRadius; y <= centerPosition.Y + searchRadius; y++)
                    {
                        randomOrder.Add(new Microsoft.Xna.Framework.Vector2(x, y));
                    }
                }

                // Shuffle the list to randomize the order of iterations
                randomOrder = randomOrder.OrderBy(item => Game1.random.Next()).ToList();

                // Iterate through the randomized order
                foreach (var warpPosition in randomOrder)
                {
                    if (farm.isTileOnMap(warpPosition) && farm.isTilePassable(warpPosition) && !farm.IsTileOccupiedBy(warpPosition))
                    {
                        return warpPosition;
                    }
                }

                searchRadius++;
            }
            return Microsoft.Xna.Framework.Vector2.Zero;
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (!IsEnabled)
                return;

            if (didPetsWarpHome)
                return;

            if (Game1.timeOfDay < 2000)
                return;

            Farmer player = e.Player;

            if (!Context.IsMainPlayer)
            {
                // Send message to main player
                SHelper.Multiplayer.SendMessage(
                    message: new PlayerWarpedMessage(player.UniqueMultiplayerID),
                    messageType: PlayerWarpedHomeMessageId,
                    modIDs: new[] { SModManifest.UniqueID }
                    );
                return;
            }

            // Main Player ------------------------------------------
            HandleTeleportingPetsHomeAtNight(player);
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!IsEnabled)
                return;

            // multiplayer: override textures in the current location
            if (Context.IsWorldReady && Game1.currentLocation != null)
            {
                uint updateRate = Game1.currentLocation.farmers.Count > 1 ? TextureUpdateRateWithMultiplePlayers : TextureUpdateRateWithSinglePlayer;
                if (e.IsMultipleOf(updateRate))
                {
                    foreach (Pet pet in Game1.currentLocation.characters.OfType<Pet>())
                        SetPetSprite(pet);
                }
            }
        }

        private void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            // Handles farmhands warping home
            if (e.Type == PlayerWarpedHomeMessageId && Context.IsMainPlayer && e.FromModID == SModManifest.UniqueID)
            {
                PlayerWarpedMessage message = e.ReadAs<PlayerWarpedMessage>();
                HandleTeleportingPetsHomeAtNight(Game1.getFarmer(message.playerId));
                return;

            }

            // Handles farmhands creating new pets
            if (e.Type == PlayerAddedPetMessageId && Context.IsMainPlayer && e.FromModID == SModManifest.UniqueID)
            {
                PlayerAddedPetMessage message = e.ReadAs<PlayerAddedPetMessage>();

                if (message.petType == "cat")
                    InitializeCat("0");
                else
                    InitializeDog("0");

                newPet.Name = message.petName;
                newPet.displayName = message.petDisplayName;
                newPet.modData[MOD_DATA_OWNER] = message.petOwner;
                newPet.modData[MOD_DATA_SKIN_ID] = message.petSkinId;
                WarpToOwnerFarmHouse(newPet);
                SetPetSprite(newPet);

                newPet = null;
                return;

            }
        }

        public static List<PetBowl> GetPetBowls()
        {
            List<PetBowl> petBowls = new List<PetBowl>();
            foreach (Building building in Game1.getFarm().buildings)
            {
                if (building is PetBowl)
                {
                    PetBowl petBowl = building as PetBowl;
                    petBowls.Add(petBowl);
                }
            }
            return petBowls;
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (!IsEnabled)
                return;

            if (!Game1.player.currentLocation.IsFarm)
                return;

            if (Game1.activeClickableMenu != null)
                return;

            if (Game1.player.CurrentItem == null) 
                return;

            bool IsPlayerNearWaterBowl(Building petBowl)
            {
                var playerPosition = Game1.player.getStandingPosition();
                var playerTile = new Microsoft.Xna.Framework.Vector2(playerPosition.X, playerPosition.Y);
                var petBowlRect = new Microsoft.Xna.Framework.Rectangle(
                    petBowl.tileX.Value * 64, 
                    petBowl.tileY.Value * 64, 
                    petBowl.tilesWide.Value * 64, 
                    petBowl.tilesHigh.Value * 64
                );

                if (petBowlRect.Contains(playerPosition))
                    return true;
                return false;
            }


            // Right click near water bowl
            if (e.Button.IsActionButton())
            {
                // Player holding fish to adopt Cat or vegetable to adopt Dog
                if (Game1.player.CurrentItem != null && Game1.player.CurrentItem.getCategoryName() != null)
                {
                    if (Game1.player.CurrentItem.getCategoryName().Contains("Fish"))
                    {
                        foreach (PetBowl petBowl in GetPetBowls())
                        {
                            if (IsPlayerNearWaterBowl(petBowl))
                            {
                                Game1.player.reduceActiveItemByOne();
                                Helper.Input.Suppress(e.Button);
                                InitializeCat("0");
                                ShowAdoptPetDialog("cat");
                            }
                        }
                    }
                    else if (Game1.player.CurrentItem.getCategoryName().Contains("Vegetable"))
                    {
                        foreach (PetBowl petBowl in GetPetBowls())
                        {
                            if (IsPlayerNearWaterBowl(petBowl))
                            {
                                Game1.player.reduceActiveItemByOne();
                                Helper.Input.Suppress(e.Button);
                                InitializeDog("0");
                                ShowAdoptPetDialog("dog");
                            }
                        }
                    }
                }
                else
                {
                    return;
                }
            }
        }

        private void OnPeerConnected(object sender, PeerConnectedEventArgs e)
        {
            GenerateAllFarmersDict();
        }


        /*******************************************
        ** Internal methods - Handlers for Commands
        ********************************************/
        /// <summary>
        /// Dialog Box for Adopting a pet
        /// </summary>
        internal static void ShowAdoptPetDialog(string petType)
        {
            Game1.activeClickableMenu = new ConfirmationDialog($"Would you like to adopt a {petType}?", (who) =>
            {
                if (Game1.activeClickableMenu is ConfirmationDialog cd)
                    cd.cancel();

                if (Game1.activeClickableMenu == null)
                {
                    if (petType == "cat" && catTextureMap.Count < 1)
                    {
                        SMonitor.Log("The pet adoption texture selection menu is not available because no textures were found", LogLevel.Warn);
                        Game1.activeClickableMenu = new NamingMenu(AddPet, $"What will you name it?");
                        return;
                    }
                    else if (petType == "dog" && dogTextureMap.Count < 1)
                    {
                        SMonitor.Log("The pet adoption texture selection menu is not available because no textures were found", LogLevel.Warn);
                        Game1.activeClickableMenu = new NamingMenu(AddPet, $"What will you name it?");
                        return;
                    }

                    Game1.activeClickableMenu = new PetSkinSelectMenu(petType == "cat" ? catTextureMap : dogTextureMap);
                }

            });
        }

        /// <summary>
        /// Handles removing pet
        /// </summary>
        /// <param name="petName">Name of pet to be removed</param>
        internal static void RemovePet(string petName)
        {
            Pet petToRemove = null;

            GetAllPets().ForEach(delegate (Pet pet) {
                if (pet.displayName.TrimEnd() == petName)
                    petToRemove = pet;
            });

            if (petToRemove != null)
            {
                if (Game1.getFarm().characters.Contains(petToRemove))
                    RemoveOutdoorPet(petToRemove);
                else
                    RemoveIndoorPet(petToRemove);
            }
            else
            {
                Game1.drawObjectDialogue($"{petName} could not be removed");
                SMonitor.Log($"{petName} could not be removed", LogLevel.Error);
                return;
            }
            SMonitor.Log($"Pet list after Remove performed", LogLevel.Info);
            GetAllPets().ForEach(delegate (Pet pet) {
                SMonitor.Log($"- {pet.displayName}", LogLevel.Info);
            });
        }

        /// <summary>
        /// Gets all pets currently on Farm or in FarmHouses
        /// </summary>
        /// <returns>List of Pet objects</returns>
        internal static List<Pet> GetAllPets()
        {
            List<Pet> pets = new List<Pet>();
            foreach (NPC j in Game1.getFarm().characters)
            {
                if (j is Pet)
                {
                    pets.Add(j as Pet);
                }
            }
            foreach (Farmer farmer in Game1.getAllFarmers())
            {
                foreach (NPC i in Utility.getHomeOfFarmer(farmer).characters)
                {
                    if (i is Pet)
                    {
                        pets.Add(i as Pet);
                    }
                }
            }
            return pets;
        }

        /// <summary>
        /// Initializes newPet to be a cat
        /// </summary>
        /// <param name="breed">breed id for selecting pet texture</param>
        internal static void InitializeCat(string breed)
        {
            newPet = new Pet(0, 0, breed, "Cat")
            {
                Name = $"cat{breed}",
                displayName = $"cat{breed}",
                Position = new Microsoft.Xna.Framework.Vector2(0, 0),
                DefaultPosition = new Microsoft.Xna.Framework.Vector2(0, 0),
                Breather = false,
                willDestroyObjectsUnderfoot = false,
                HideShadow = true,
            };
            newPet.modData[MOD_DATA_OWNER] = Game1.player.displayName;
        }
        /// <summary>
        /// Initializes newPet to be a dog
        /// </summary>
        /// <param name="breed">breed id for selecting pet texture</param>
        internal static void InitializeDog(string breed)
        {
            newPet = new Pet(0, 0, breed, "Dog")
            {
                Name = $"dog{breed}",
                displayName = $"dog{breed}",
                Position = new Microsoft.Xna.Framework.Vector2(0, 0),
                DefaultPosition = new Microsoft.Xna.Framework.Vector2(0, 0),
                Breather = false,
                willDestroyObjectsUnderfoot = false,
                HideShadow = true,
            };
            newPet.modData[MOD_DATA_OWNER] = Game1.player.displayName;
        }

        /// <summary>
        /// For assigning an owner to a pet.
        /// </summary>
        /// <param name="petName">Name of pet to get new owner</param>
        /// <param name="farmerName">Name of farmer to assign as new owner</param>       
        /// <param name="farmerId">Multiplayer ID of farmer to assign as new owner</param>

        internal static void AssignPetOwner(string petName, string farmerName)
        {
            var petExists = false;
            var farmerExists = false;

            foreach (Farmer farmer in Game1.getAllFarmers())
            {
                if (farmer.displayName == farmerName)
                {
                    farmerExists = true;
                    break;
                }
            }

            if (farmerExists)
            {
                foreach (Pet pet in GetAllPets())
                {
                    if (pet.displayName.TrimEnd() == petName)
                    {
                        petExists = true;
                        pet.modData[MOD_DATA_OWNER] = farmerName;
                        SMonitor.Log($"{petName}'s new owner is {farmerName}.", LogLevel.Info);
                        break;
                    }
                }

                if (!petExists)
                {
                    SMonitor.Log($"Could not find pet with name {petName}.", LogLevel.Error);
                    return;
                }
            }
            else
            {
                SMonitor.Log($"Could not find farmer with name {farmerName}.", LogLevel.Error);
                return;
            }

        }




        /******************
        ** Private methods
        *******************/

        /// <summary>
        /// Adds the pet to the farm
        /// adds a space to the pet name to avoid conflict with villager names
        /// </summary>
        /// <param name="petName">User-provided name for the pet</param>
        public static void AddPet(string petName)
        {
            if (newPet == null)
            {
                SMonitor.Log($"Something went wrong adding the new pet \"{petName}\". No pet was added", LogLevel.Error);
                Game1.drawObjectDialogue($"{petName} could not be adopted");
                return;
            }
            newPet.Name = petName + " ";
            newPet.displayName = petName + " ";

            if (!Context.IsMainPlayer)
            {
                // Send message to main player
                SHelper.Multiplayer.SendMessage(
                    message: new PlayerAddedPetMessage(newPet.Name, newPet.displayName, newPet.modData[MOD_DATA_OWNER], newPet.modData[MOD_DATA_SKIN_ID], newPet.petType.Value == "Cat" ? "cat" : "dog"),
                    messageType: PlayerAddedPetMessageId,
                    modIDs: new[] { SModManifest.UniqueID }
                    );
                Game1.drawObjectDialogue($"{petName} has been adopted and is staying inside today");
                newPet = null;
                return;
            }
            else
            {
                WarpToOwnerFarmHouse(newPet);
                SetPetSprite(newPet);
            }
            Game1.drawObjectDialogue($"{petName} has been adopted and is staying inside today");
            newPet = null;
        }

        public static void SetPetSkin(int skinId)
        {
            newPet.modData[MOD_DATA_SKIN_ID] = skinId.ToString();
        }

        private void SetPetSprites()
        {
            foreach (Pet pet in GetAllPets())
            {
                SetPetSprite(pet);
            }
        }

        private static void SetPetSprite(Pet pet)
        {
            if (!pet.modData.ContainsKey(MOD_DATA_SKIN_ID))
                return;

            if (pet.petType.Value == "Cat" && catTextureMap.Count > 0 && catTextureMap.ContainsKey(pet.modData[MOD_DATA_SKIN_ID]))
            {
                pet.Sprite.spriteTexture = catTextureMap[pet.modData[MOD_DATA_SKIN_ID]];
            }
            else if (pet.petType.Value == "Dog" && dogTextureMap.Count > 0 && dogTextureMap.ContainsKey(pet.modData[MOD_DATA_SKIN_ID]))
            {
                pet.Sprite.spriteTexture = dogTextureMap[pet.modData[MOD_DATA_SKIN_ID]];
            }
        }

        /// <summary>
        /// Removes a pet from the game when the pet is indoors
        /// </summary>
        /// <param name="petToRemove">Pet object to remove from game</param>
        private static void RemoveIndoorPet(Pet petToRemove)
        {
            GetFarmHouses().ForEach(delegate (FarmHouse farmHouse)
            {
                if (farmHouse.characters.Contains(petToRemove))
                {
                    farmHouse.characters.Remove(petToRemove);
                    Game1.drawObjectDialogue($"{petToRemove.displayName} has been removed");
                    SMonitor.Log($"{petToRemove.displayName} has been removed", LogLevel.Info);
                }
            });
        }

        /// <summary>
        /// Removes a pet from the game when the pet is outside on the farm
        /// </summary>
        /// <param name="petToRemove">Pet object to remove from the game</param>
        private static void RemoveOutdoorPet(Pet petToRemove)
        {
            Game1.getFarm().characters.Remove(petToRemove);
            Game1.drawObjectDialogue($"{petToRemove.displayName} has been removed");
            SMonitor.Log($"{petToRemove.displayName} has been removed", LogLevel.Info);
        }

        private void LoadCatSprites()
        {
            string modPath = PathUtilities.NormalizePath(SHelper.DirectoryPath + "\\");
            string catPath = PathUtilities.NormalizePath($"{modPath}assets\\Cat");

            if (!Directory.Exists(catPath))
            {
                SMonitor.Log($"Cat asssets path could not be found. {catPath}", LogLevel.Warn);
                return;
            }
            var files = Directory.GetFiles(catPath, "*.png");
            for (var i = 0; i < files.Length; i++)
            {
                var relFileName = AbsoluteToRelativePath(files[i], modPath);
                catTextureMap[(i + 1).ToString()] = SHelper.ModContent.Load<Texture2D>(relFileName); // TODO: refactor to use filename for key
            }
        }

        private void LoadDogSprites()
        {
            string modPath = PathUtilities.NormalizePath(SHelper.DirectoryPath + "\\");
            string dogPath = PathUtilities.NormalizePath($"{modPath}assets\\Dog");

            if (!Directory.Exists(dogPath))
            {
                SMonitor.Log($"Dog asssets path could not be found. {dogPath}", LogLevel.Warn);
                return;
            }

            var files = Directory.GetFiles(dogPath, "*.png");
            for (var i = 0; i < files.Length; i++)
            {
                var relFileName = AbsoluteToRelativePath(files[i], modPath);
                dogTextureMap[(i + 1).ToString()] = SHelper.ModContent.Load<Texture2D>(relFileName); // TODO: refactor to use filename for key
            }
        }

        private string AbsoluteToRelativePath(string absolutePath, string modPath)
        {
            return absolutePath.Replace(modPath, "");
        }

        /// <summary>
        /// Gets all FarmHouses owned by current players. This is needed to find pets that are indoors
        /// </summary>
        /// <returns>List of FarmHouse objects</returns>
        private static List<FarmHouse> GetFarmHouses()
        {
            List<FarmHouse> farmHouses = new List<FarmHouse>();
            foreach (Farmer farmer in Game1.getAllFarmers())
            {
                farmHouses.Add(Utility.getHomeOfFarmer(farmer));
            }
            return farmHouses;
        }

        /// <summary>
        /// Generates Dictionary with all farmers. Farmer Name as key, farmer instance as value
        /// </summary>
        private static void GenerateAllFarmersDict()
        {
            allFarmers.Clear();
            foreach (Farmer farmer in Game1.getAllFarmers())
            {
                if (farmer != null && farmer.displayName != null && farmer.displayName != "")
                {
                    if (!allFarmers.ContainsKey(farmer.displayName))
                        allFarmers.Add(farmer.displayName, farmer);
                }
            }
        }


        /// <summary>
        /// Warps pet to the owner's farmhouse/cabin if owner is known. Otherwise, warps to host farmhouse
        /// </summary>
        /// <param name="pet">Instance of pet to warp</param>
        private static void WarpToOwnerFarmHouse(Pet pet)
        {
            if (pet.modData.ContainsKey(MOD_DATA_OWNER) && pet.modData[MOD_DATA_OWNER] != null)
            {
                if (allFarmers.ContainsKey(pet.modData[MOD_DATA_OWNER]))
                    pet.warpToFarmHouse(allFarmers[pet.modData[MOD_DATA_OWNER]]);
                else
                    pet.warpToFarmHouse(Game1.MasterPlayer);
            }
            else
            {
                pet.warpToFarmHouse(Game1.MasterPlayer);
            }
        }


        /// <summary>
        /// Helper function for getting a sprite's texture name
        /// </summary>
        /// <param name="pet">"cat" or "dog"</param>
        /// <param name="breed">breed id for selecting pet texture</param>
        /// <returns>string value for textureName</returns>
        private static string GetPetTextureName(string pet, string breed)
        {
            return $"Animals\\{pet}" + ((breed == "0") ? "" : string.Concat(breed));
        }

        private void HandleTeleportingPetsHomeAtNight(Farmer player)
        {
            if (didPetsWarpHome)
                return;

            if (!isPetOwner(player))
                return;

            bool isAPetOnBed = false;

            foreach (Pet pet in GetAllPets())
            {
                WarpToOwnerFarmHouse(pet);
                if (isAPetOnBed)
                {
                    pet.isSleepingOnFarmerBed.Value = false;
                    WarpPetAgain(pet);

                }
                if (pet.isSleepingOnFarmerBed.Value)
                    isAPetOnBed = true;
            }

            didPetsWarpHome = true;
        }

        private static List<string> GetAllPetOwnerNames()
        {
            List<string> ownerNames = new List<string>();
            foreach (Pet pet in GetAllPets())
            {
                if (pet.modData.ContainsKey(MOD_DATA_OWNER) && pet.modData[MOD_DATA_OWNER] != null && !ownerNames.Contains(pet.modData[MOD_DATA_OWNER]))
                {
                    ownerNames.Add(pet.modData[MOD_DATA_OWNER]);
                }
            }

            return ownerNames;
        }

        /// <summary>
        /// helper to determine if a given farmer is a pet owner
        /// </summary>
        /// <param name="farmer">farmer object to check</param>
        /// <returns>true if farmer is a pet owner</returns>
        private static bool isPetOwner(Farmer farmer)
        {
            return GetAllPetOwnerNames().Contains(farmer.displayName);
        }

        /// <summary>
        /// To warp pet to owner's farmhouse and ensure that only 1 pet will sleep on bed, we need to warp pets a second time if there is a pet on bed
        /// </summary>
        /// <param name="pet">pet to warp</param>
        /// <param name="owner">pet owner</param>
        private void WarpPetAgain(Pet pet)
        {
            Farmer owner = Game1.MasterPlayer;
            if (pet.modData[MOD_DATA_OWNER] != null)
                if (allFarmers.ContainsKey(pet.modData[MOD_DATA_OWNER]))
                    owner = allFarmers[pet.modData[MOD_DATA_OWNER]];

            pet.isSleepingOnFarmerBed.Value = false;
            FarmHouse farmHouse = Utility.getHomeOfFarmer(owner);
            Microsoft.Xna.Framework.Vector2 sleepTile = Microsoft.Xna.Framework.Vector2.Zero;
            int tries = 0;
            sleepTile = new Microsoft.Xna.Framework.Vector2(Game1.random.Next(2, farmHouse.map.Layers[0].LayerWidth - 3), Game1.random.Next(3, farmHouse.map.Layers[0].LayerHeight - 5));
            List<Furniture> rugs = new List<Furniture>();
            foreach (Furniture house_furniture in farmHouse.furniture)
            {
                if ((int)house_furniture.furniture_type.Value == 12)
                {
                    rugs.Add(house_furniture);
                }
            }

            if (Game1.random.NextDouble() <= 0.30000001192092896)
            {
                sleepTile = Utility.PointToVector2(farmHouse.getBedSpot()) + new Microsoft.Xna.Framework.Vector2(0f, 2f);
            }
            else if (Game1.random.NextDouble() <= 0.5)
            {
                if (rugs.Count > 0)
                {
                    int randomIndex = Game1.random.Next(0, rugs.Count);
                    Furniture rug = rugs[randomIndex];
                    sleepTile = new Microsoft.Xna.Framework.Vector2(rug.boundingBox.Left / 64, rug.boundingBox.Center.Y / 64);
                }
            }
            for (; tries < 50; tries++)
            {
                if (farmHouse.canPetWarpHere(sleepTile) && farmHouse.isTilePlaceable(sleepTile) && !farmHouse.IsTileOccupiedBy(sleepTile) && farmHouse.isTilePlaceable(sleepTile + new Microsoft.Xna.Framework.Vector2(1f, 0f)) && !farmHouse.isTileOnWall((int)sleepTile.X, (int)sleepTile.Y))
                {
                    break;
                }
                sleepTile = new Microsoft.Xna.Framework.Vector2(Game1.random.Next(2, farmHouse.map.Layers[0].LayerWidth - 3), Game1.random.Next(3, farmHouse.map.Layers[0].LayerHeight - 4));
            }
            if (tries < 50)
            {
                Game1.warpCharacter(pet, farmHouse, sleepTile);
                pet.CurrentBehavior = "Sleep";
            }
            else
            {
                pet.faceDirection(2);
                Game1.warpCharacter(pet, "Farm", (Game1.getLocationFromName("Farm") as Farm).GetStarterPetBowlLocation());
            }
            pet.UpdateSleepingOnBed();
            pet.Halt();
            pet.Sprite.CurrentAnimation = null;
            pet.OnNewBehavior();
            pet.Sprite.UpdateSourceRect();
        }
    }
}
