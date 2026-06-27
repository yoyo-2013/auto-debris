using StardewValley;
using StardewValley.Pathfinding;
using Microsoft.Xna.Framework;
using StardewValley.Tools;
using StardewModdingAPI;

namespace modding
{
    internal sealed class AutoDebris
    {
        private bool startUsingTool = false;
        private bool active = false;
        private IMonitor monitor;

        public AutoDebris(IMonitor monitor)
        {
            this.monitor = monitor;
        }
        public void Continue(Farmer player, GameLocation currentMap)
        {
            if (active && startUsingTool && !player.UsingTool)
            {
                startUsingTool = false;
                Start(player, currentMap, 0);
            }
        }
        public void Toggle(Farmer player, GameLocation currentMap)
        {
            if (active)
            {
                // need to stop the action.
                Stop(player);
                return;
            }
            if (!active)
            {
                Start(player, Game1.currentLocation, 0);
            }
        }

        private void Stop(Farmer player)
        {
            player.controller = null;
            active = false;
        }

        private Vector2? FindReachableAdjacentTile(Farmer player, GameLocation currentMap, Vector2 targetPosition)
        {
            var debrisAdjacentTiles = new Vector2[]
            {
                new(targetPosition.X, targetPosition.Y + 1),
                new(targetPosition.X, targetPosition.Y - 1),
                new(targetPosition.X + 1, targetPosition.Y),
                new(targetPosition.X - 1, targetPosition.Y),
            };

            var sortedTiles = debrisAdjacentTiles.OrderBy(v => Vector2.Distance(player.Tile, v));
            foreach (var adjacentTile in sortedTiles)
            {
                PathFindController controller = new PathFindController(
                                c: player,
                                location: currentMap,
                                endPoint: new Point((int)adjacentTile.X, (int)adjacentTile.Y),
                                finalFacingDirection: GetFacingDirection(targetPosition, adjacentTile));
                if (controller.pathToEndPoint != null && controller.pathToEndPoint.Count > 0)
                {
                    return adjacentTile;
                }
            }
            return null;
        }

        private void Start(Farmer player, GameLocation currentMap, int tileRank)
        {

            monitor.Log($"tileRank: {tileRank}", LogLevel.Debug);
            active = true;

            var (debris, vector, facingDirection) = this.FindNearestReachableDebrisAdjacentTile(player, currentMap);

            if (debris == null) return;

            player.controller = new PathFindController(
                            c: player,
                            location: currentMap,
                            endPoint: new Point((int)vector.X, (int)vector.Y),
                            finalFacingDirection: facingDirection,
                            endBehaviorFunction: (character, location) => RemoveDebris(player, debris));
        }

        private (StardewValley.Object? debris, Vector2 vector, int facingDirection) FindNearestReachableDebrisAdjacentTile(Farmer player, GameLocation currentMap)
        {
            var playerVector = player.Tile;
            var targetPair = currentMap.Objects.Pairs
                .Where(p => p.Value.IsBreakableStone() || p.Value.IsTwig() || p.Value.IsWeeds())
                .OrderBy(p => Vector2.Distance(player.Tile, p.Key))
                .SkipWhile(p => this.FindReachableAdjacentTile(player, currentMap, p.Key) == null)
                .ElementAt(0);
            var tile = this.FindReachableAdjacentTile(player, currentMap, targetPair.Key)!.Value;
            var facingDirection = GetFacingDirection(targetPair.Key, tile);
            return (targetPair.Value, tile, facingDirection);
        }

        private static int GetFacingDirection(Vector2 target, Vector2 standingPosition)
        {
            var xOffset = standingPosition.X - target.X;
            var yOffset = standingPosition.Y - target.Y;

            if ((int)Math.Round(xOffset) == 1 && (int)Math.Round(yOffset) == 0)
            {
                return 3;
            }
            if ((int)Math.Round(xOffset) == -1 && (int)Math.Round(yOffset) == 0)
            {
                return 1;
            }
            if ((int)Math.Round(xOffset) == 0 && (int)Math.Round(yOffset) == 1)
            {
                return 0;
            }
            if ((int)Math.Round(xOffset) == 0 && (int)Math.Round(yOffset) == -1)
            {
                return 2;
            }
            return 0;
        }

        private void RemoveDebris(Farmer player, StardewValley.Object debrisObject)
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
                this.startUsingTool = true;
            }

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
    }
}
