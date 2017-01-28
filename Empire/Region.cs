using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire
{
    public class Region
    {
        public readonly Planet Planet;
        public Color Color;
        public List<Tile> Tiles = new List<Tile>();
        public List<Tile> OuterTiles = new List<Tile>();
        public List<Region> Adjacencies = new List<Region>();
        public Tile Origin;

        public Vector3 Position { get { return Origin.Position; } }

        public Region(Planet planet, Tile origin)
        {
            Planet = planet;
            Origin = origin;
        }

        public void AddTile(Tile tile)
        {
            tile.Region = this;
            Tiles.Add(tile);
        }

        public void AddOuterTile(Tile tile)
        {
            AddTile(tile);
            OuterTiles.Add(tile);
        }

        public void Expand(Random random)
        {
            if (OuterTiles.Count == 0)
                return;

            Tile tile = OuterTiles[random.Next(OuterTiles.Count)];
            foreach (Tile adjacentTile in tile.Adjacencies)
            {
                if (adjacentTile.Region == null)
                    AddOuterTile(adjacentTile);
                else if (adjacentTile.Region != this)
                    AddMutualAdjacency(adjacentTile.Region);
            }
            OuterTiles.Remove(tile);
        }

        public void AddAdjacency(Region newAdjacency)
        {
            if (isAdjacentTo(newAdjacency) == false)
                Adjacencies.Add(newAdjacency);
        }

        public void AddMutualAdjacency(Region newAdjacency)
        {
            AddAdjacency(newAdjacency);
            newAdjacency.AddAdjacency(this);
        }

        IEnumerable<Tile> getTilesBordering(Region otherRegion)
        {
            return OuterTiles.
                Where(region => region.Adjacencies.
                Exists(adjacency => adjacency.Region == otherRegion));
        }

        bool isAdjacentTo(Region otherRegion)
        {
            return Adjacencies.Contains(otherRegion);
        }
    }
}
