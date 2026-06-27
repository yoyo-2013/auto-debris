using System;
using System.Runtime.Serialization;
using System.Security.AccessControl;
using System.Xml.Schema;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Pathfinding;
using StardewValley.Tools;

namespace modding
{
    internal sealed class ModEntry : Mod
    {

        private AutoDebris autoDebris;
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            autoDebris = new AutoDebris(this.Monitor);
        }

        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            autoDebris.Continue(Game1.player, Game1.currentLocation);
        }


        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (Context.IsWorldReady)
            {
                if (e.Button == SButton.I)
                {
                    autoDebris.Toggle(Game1.player, Game1.currentLocation);
                }
            }
        }
    }
}
