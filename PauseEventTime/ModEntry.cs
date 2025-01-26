using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace PauseEventTime
{
    public class ModEntry: Mod
    {
        public override void Entry(IModHelper helper)
        {
            // Subscribe to game loop events
            Helper.Events.GameLoop.TimeChanged += OnTimeChanged;
        }

        private void OnTimeChanged(object? sender, TimeChangedEventArgs e)
        {
            // Check if an event (cutscene) is active
            if (Game1.eventUp)
            {
                Game1.paused = true; // Freeze time during events
            }
            else
            {
                Game1.paused = false; // Unfreeze time when no event is active
            }
        }
    }
}
