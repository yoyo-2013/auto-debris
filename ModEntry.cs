using System;
using System.Runtime.Serialization;
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
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }
        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            var player = Game1.player;

            if (player.currentLocation.Name != "Farm")
                return;

            this.Monitor.Log($"You pressed {e.Button}.", LogLevel.Debug);

            var currentMap = Game1.currentLocation;

            var playerPosition = player.TilePoint;

            var playerVector = player.Tile;

            if (e.Button == SButton.I)
            {
                var smallestDistance = float.MaxValue;
                var nearestDebrisVector = Vector2.Zero;
                StardewValley.Object? nearestDebrisObject = null;
                foreach (var p in currentMap.Objects.Pairs)
                {

                    var obj = p.Value;
                    if (obj.IsBreakableStone())
                    {
                        var distance = Vector2.Distance(playerVector, p.Key);
                        if (distance < smallestDistance)
                        {
                            smallestDistance = distance;
                            nearestDebrisVector = p.Key;
                            nearestDebrisObject = obj;
                        }
                    }
                }

                player.controller = new PathFindController(
                    c: player,
                    location: currentMap,
                    endPoint: new Point((int)nearestDebrisVector.X + 1, (int)nearestDebrisVector.Y),
                    finalFacingDirection: 3,
                    endBehaviorFunction: (character, location) =>
                    {

                        if (nearestDebrisObject != null && nearestDebrisObject.IsBreakableStone())
                        {
                            Tool? targetTool = null;
                            foreach (var item in player.Items)
                            {
                                if (item is Pickaxe playerPickaxe)
                                {
                                    targetTool = playerPickaxe;
                                    break;
                                }
                            }
                            if (targetTool != null)
                            {
                                player.CurrentToolIndex = player.getIndexOfInventoryItem(targetTool);
                                // player.faceGeneralDirection(nearestDebrisVector * 64f);
                                
                                
                                player.CurrentTool.beginUsing(
                                    player.currentLocation,
                                    0,
                                    0,
                                    player
                                );
                                player.UsingTool = true;
                                // player.canReleaseTool = true;

                            }

                        }

                    }
                );


            }
        }
    }
}