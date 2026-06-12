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
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;

        }

        bool startUsingTool = false;

        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            var player = Game1.player;

            if(player.UsingTool)
            {
                this.startUsingTool = true;    
            }
            if (startUsingTool && !player.UsingTool)
            {


                this.Monitor.Log($"tick: stopped using tool", LogLevel.Debug);
                startUsingTool = false;


                var currentMap = Game1.currentLocation;
                var (debris, vector) = FindNearestDebris(player, currentMap);
                if (debris == null) return;

                player.controller = new PathFindController(
                    c: player,
                    location: currentMap,
                    endPoint: new Point((int)vector.X + 1, (int)vector.Y),
                    finalFacingDirection: 3,
                    endBehaviorFunction: (character, location) => RemoveDebris(player, debris));

            }
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            var player = Game1.player;

            if (player.currentLocation.Name != "Farm")
                return;

            var currentMap = Game1.currentLocation;

            if (e.Button == SButton.I)
            {
                var (debris, vector) = FindNearestDebris(player, currentMap);
                if (debris == null) return;

                player.controller = new PathFindController(
                    c: player,
                    location: currentMap,
                    endPoint: new Point((int)vector.X + 1, (int)vector.Y),
                    finalFacingDirection: 3,
                    endBehaviorFunction: (character, location) => RemoveDebris(player, debris));

            }
        }

        private static (StardewValley.Object? debris, Vector2 vector) FindNearestDebris(Farmer player, GameLocation currentMap)
        {
            var playerVector = player.Tile;
            var smallestDistance = float.MaxValue;
            var nearestDebrisVector = Vector2.Zero;
            StardewValley.Object? nearestDebrisObject = null;
            foreach (var p in currentMap.Objects.Pairs)
            {

                var obj = p.Value;
                if (obj.IsBreakableStone() || obj.IsTwig() || obj.IsWeeds())
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
            return (nearestDebrisObject, nearestDebrisVector);

        }

        private static Tool? SelectTool<T>(Farmer player) where T : Tool
        {
            foreach (var item in player.Items)
            {
                if (item is T playerTool)
                {
                    return playerTool;
                }
            }
            return null;

        }

        private static void RemoveDebris(Farmer player, StardewValley.Object debrisObject)
        {
            Tool? targetTool = null;
            if (debrisObject.IsBreakableStone())
            {
                targetTool = SelectTool<Pickaxe>(player);

            }
            if (debrisObject.IsTwig())
            {
                targetTool = SelectTool<Axe>(player);
            }
            if (debrisObject.IsWeeds())
            {
                foreach (var item in player.Items)
                {
                    if (item is MeleeWeapon mw)
                    {
                        if (mw.isScythe())
                        {
                            targetTool = mw;
                        }
                    }
                }

            }


            if (targetTool != null)
            {
                player.CurrentToolIndex = player.getIndexOfInventoryItem(targetTool);
                player.CurrentTool.beginUsing(
                    player.currentLocation,
                    0,
                    0,
                    player
                );
                player.UsingTool = true;
                
            }

        }
    }
}
