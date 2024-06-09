﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BugNet2.Framework;
using StardewModdingAPI;
using StardewValley.BellsAndWhistles;

namespace BugNet2.Framework
{
    /// <summary>The BugNet2 API which other mods can access.</summary>
    public class BugNetApi
    {
        /*********
        ** Fields
        *********/
        /// <summary>Add a new critter which can be caught.</summary>
        private readonly Action<string, Texture2D, string, Rectangle, string, Dictionary<string, string>, Func<int, int, Critter>, Func<Critter, bool>> RegisterCritterImpl;

        /// <summary>The monitor with which to log critter changes through the API.</summary>
        private readonly IMonitor Monitor;

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="registerCritter">Add a new critter which can be caught.</param>
        /// <param name="monitor">The monitor with which to log critter changes through the API.</param>
        internal BugNetApi(Action<string, Texture2D, string, Rectangle, string, Dictionary<string, string>, Func<int, int, Critter>, Func<Critter, bool>> registerCritter, IMonitor monitor)
        {
            this.RegisterCritterImpl = registerCritter;
            this.Monitor = monitor;
        }

        /// <inheritdoc />
        public void RegisterCritter(IManifest manifest, string critterId, Texture2D texture, string textureName, Rectangle textureArea, string defaultCritterName, Dictionary<string, string> translatedCritterNames, Func<int, int, Critter> makeCritter, Func<Critter, bool> isThisCritter)
        {
            // validate
            if (manifest is null)
                throw new ArgumentNullException(nameof(manifest));
            if (texture is null)
                throw new ArgumentNullException(nameof(texture));
            if (texture.IsDisposed)
                throw new ObjectDisposedException(nameof(texture));
            if (textureArea == Rectangle.Empty)
                throw new InvalidOperationException("You must provide a non-empty texture pixel area.");
            if (string.IsNullOrWhiteSpace(defaultCritterName))
                throw new ArgumentNullException(nameof(defaultCritterName));
            if (makeCritter == null)
                throw new ArgumentNullException(nameof(makeCritter));
            if (isThisCritter == null)
                throw new ArgumentNullException(nameof(isThisCritter));

            // register critter
            try
            {
                this.RegisterCritterImpl(critterId, texture, textureName, textureArea, defaultCritterName, translatedCritterNames, makeCritter, isThisCritter);
                this.Monitor.Log($"'{manifest.Name}' registered critter ID '{critterId}'.");
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Failed registering critter ID '{critterId}' added by '{manifest.Name}'. Technical details:\n{ex}", LogLevel.Error);
            }
        }
    }
}
