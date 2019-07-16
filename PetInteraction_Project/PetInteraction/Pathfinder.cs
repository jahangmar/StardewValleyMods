//Copyright (c) 2019 Jahangmar

//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU Lesser General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//GNU Lesser General Public License for more details.

//You should have received a copy of the GNU Lesser General Public License
//along with this program. If not, see <https://www.gnu.org/licenses/>.

using StardewValley;

using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Node = Microsoft.Xna.Framework.Vector2;
using Path = System.Collections.Generic.Queue<Microsoft.Xna.Framework.Vector2>;
using CostMap = System.Collections.Generic.Dictionary<Microsoft.Xna.Framework.Vector2, double>;
using NodeMap = System.Collections.Generic.Dictionary<Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2>;
using StardewValley.TerrainFeatures;
using StardewValley.Buildings;
using StardewValley.Locations;

namespace PetInteraction
{
    public class PathFinder
    {
        public static Queue<Vector2> FindPath(Vector2 source, Vector2 destination)
        {
            return AStar(source, destination);
        }

        private static Queue<Vector2> ReconstructPath(NodeMap cameFrom, Node current)
        {
            List<Node> total_path = new List<Node>();
            total_path.Add(current);
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                total_path.Insert(0, current);
            }
            return new Queue<Node>(total_path);
        }

        private static Queue<Vector2> AStar(Node source, Node dest)
        {
            List<Node> openSet = new List<Node>();
            openSet.Add(source);

            NodeMap cameFrom = new NodeMap();

            CostMap gScore = new CostMap();
            gScore.Add(source, 0);

            CostMap fScore = new CostMap();
            fScore.Add(source, Heur(source, dest));

            while (openSet.Count > 0)
            {
                openSet.Sort((Node x, Node y) => (int)System.Math.Abs(GetCost(fScore, x)*100 - GetCost(fScore, x)*100));
                Node current = openSet[0];
                if (current == dest)
                    return ReconstructPath(cameFrom, current);

                openSet.RemoveAt(0);

                foreach (Node neighbor in GetPassableNeighbors(current))
                {
                    double tentative_gScore = GetCost(gScore, current) + 1;

                    if (tentative_gScore < GetCost(gScore, neighbor))
                    {
                        Add(cameFrom, neighbor, current);
                        Add(gScore, neighbor, tentative_gScore);
                        Add(fScore, neighbor, GetCost(gScore, neighbor) + Heur(neighbor, dest));
                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }
            throw new System.Exception("Failed to find path");
        }

        private static double GetCost(CostMap map, Node n) => map.ContainsKey(n) ? map[n] : double.PositiveInfinity;

        private static void Add<T>(Dictionary<Node, T> dic, Node key, T value)
        {
            if (dic.ContainsKey(key))
                dic[key] = value;
            else
                dic.Add(key, value);
        }

        private static double Heur(Node n, Node dest)
        {
            return StardewValley.Utility.distance(n.X, dest.X, n.Y, dest.Y);
        }

        private static List<Node> GetPassableNeighbors(Node n)
        {
            List<Node> adjacents = StardewValley.Utility.getAdjacentTileLocations(n);
            return adjacents.FindAll((Node node) => IsPassable(node));
        }

        private static bool IsPassable(Node tile)
        {
            GameLocation location = Game1.currentLocation;
            Object obj = location.getObjectAtTile((int)tile.X, (int)tile.Y);
            TerrainFeature tf;
            location.terrainFeatures.TryGetValue(tile, out tf);
            Building building = null;
            if (location is BuildableGameLocation bgl)
            {
                building = bgl.getBuildingAt(tile);
            }

            foreach (Character c in location.characters)
            {
                if (c.GetBoundingBox().Contains(tile.X * Game1.tileSize + Game1.tileSize / 2, tile.Y * Game1.tileSize + Game1.tileSize / 2))
                {
                    return false;
                }
            }

            return location.isTilePassable(new xTile.Dimensions.Location((int)tile.X, (int)tile.Y), Game1.viewport)
                && ((obj == null) || obj.isPassable())
                && ((tf == null) || (tf.isPassable()))
                && ((building == null) || (building.isTilePassable(tile)));
        }
    }
}
