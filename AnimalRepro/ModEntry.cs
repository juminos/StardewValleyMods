using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile;

namespace AnimalRepro
{
    public class ModEntry : Mod
    {
        internal static IMonitor SMonitor;
        internal static IModHelper SHelper;
        public override void Entry(IModHelper helper)
        {
            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.DayStarted += OnDayStarted;
        }

        private void OnDayStarted(object sender, EventArgs e)
        {
            foreach (GameLocation location in Game1.locations)
            {
                foreach (Building building in location.buildings)
                {
                    BuildingData buildingData = building.GetData();
                    if (building.AllowsAnimalPregnancy())
                    {
                        HandleAnimalPregnancy(building);
                    }
                    else if (buildingData.ValidOccupantTypes.Contains("Coop") || buildingData.ValidOccupantTypes.Contains("Barn"))
                    {
                        HandleEggFertilization(building);
                    }
                }
            }
        }

        private void HandleAnimalPregnancy(Building building)
        {
            // Iterate through animals in building
            foreach (FarmAnimal animal in building.indoors.Value.characters.OfType<FarmAnimal>())
            {
                // Check if the animal is not male
                if (!animal.isMale())
                {
                    // Disable reproduction for non-males
                    animal.allowReproduction.Value = false;
                }
                // Check if the animal is male
                if (animal.isMale())
                {
                    // Find females of the same type or counterpart
                    foreach (FarmAnimal female in building.indoors.Value.characters.OfType<FarmAnimal>()
                                                           .Where(f => !f.isMale() && f.isAdult()))
                    {
                        // Check for the same type or counterpart female types
                        if (female.type == animal.type ||
                            (animal.type.Contains("Bull") && female.type.Contains("Cow")) ||
                            (animal.type.Contains("Billy") && female.type.Contains("Goat")) ||
                            (animal.type.Contains("Ram") && female.type.Contains("Sheep")))
                        {
                            // Allow reproduction for relevant females
                            female.allowReproduction.Value = true;
                        }
                    }
                }
            }
        }

        private void HandleEggFertilization(Building building)
        {
            // Iterate through animals in building
            foreach (FarmAnimal animal in building.indoors.Value.characters.OfType<FarmAnimal>())
            {
                if (!animal.isMale() && animal.isAdult() && 
                    (animal.type.Contains("Duck") ||
                    animal.type.Contains("Chicken") ||
                    animal.type.Contains("Ostrich")))
                {
                    bool foundMale = building.indoors.Value.characters.OfType<FarmAnimal>().Any(m =>
                    m.isMale() && 
                    (m.type == animal.type ||
                    (animal.type.Contains("Ostrich") && m.type.Contains("Ostrich")) ||
                    (animal.type.Contains("Chicken") && m.type.Contains("Rooster")) || 
                    (animal.type.Contains("Duck") && m.type.Contains("Drake"))));

                    if (foundMale && new Random().NextDouble() < 0.2)
                    {
                        // Get a random position within the building to drop the object
                        Vector2 dropPosition = new Vector2(Game1.random.Next(building.tileX.Value, building.tileX.Value + building.tilesWide.Value),
                                                            Game1.random.Next(building.tileY.Value, building.tileY.Value + building.tilesHigh.Value));

                        string fertilizedEggId;

                        if (animal.type.Value == "Duck")
                        {
                            // Use fertilized duck egg ID
                            fertilizedEggId = "juminos.MoreFarmAnimals_FertilizedDuckEgg";
                        }
                        else if (animal.type.Contains("VoidDuck"))
                        {
                            // Use fertilized chicken egg ID
                            fertilizedEggId = "juminos.MoreFarmAnimals_FertilizedVoidDuckEgg";
                        }
                        else if (animal.type.Contains("Ostrich"))
                        {
                            // Use fertilized ostrich egg ID
                            fertilizedEggId = "juminos.MoreFarmAnimals_FertilizedOstrichEgg";
                        }
                        else if (animal.type.Contains("Brown Chicken"))
                        {
                            // Use fertilized chicken egg ID
                            fertilizedEggId = "juminos.MoreFarmAnimals_FertilizedBrownEgg";
                        }
                        else if (animal.type.Contains("White Chicken"))
                        {
                            // Use fertilized chicken egg ID
                            fertilizedEggId = "juminos.MoreFarmAnimals_FertilizedWhiteEgg";
                        }
                        else if (animal.type.Contains("Blue Chicken"))
                        {
                            // Use fertilized chicken egg ID
                            fertilizedEggId = "juminos.MoreFarmAnimals_FertilizedBlueEgg";
                        }
                        else if (animal.type.Contains("Void Chicken"))
                        {
                            // Use fertilized chicken egg ID
                            fertilizedEggId = "juminos.MoreFarmAnimals_FertilizedVoidEgg";
                        }
                        else
                            return;

                        // Create a fertilized egg object
                        StardewValley.Object fertilizedEgg = new StardewValley.Object(dropPosition, fertilizedEggId);

                        // Calculate the bounding box for the entire building
                        xTile.Dimensions.Rectangle buildingBounds = new xTile.Dimensions.Rectangle(building.tileX.Value, building.tileY.Value, building.tilesWide.Value, building.tilesHigh.Value);

                        // Drop the fertilized egg object into the building
                        Game1.getLocationFromName(building.indoors.Value.Name).dropObject(fertilizedEgg, dropPosition, buildingBounds, false, Game1.player);
                    }
                }
            }
        }
    }
}
