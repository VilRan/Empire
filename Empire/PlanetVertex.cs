using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire
{
    public class PlanetVertex
    {
        public Vector3 Position;
        public Tile Tile;
        public List<PlanetVertex> Adjacencies = new List<PlanetVertex>(6);

        public Region Region { get { return Tile.Region; } }
        //public Color Color { get { if (Tile.Elevation > 0) return Region.Color; else return Color.Navy; } }
        //public Color Color { get { if (Tile.Temperature < 273.15f) return Color.Snow; else if (Tile.Elevation > 1) return Color.ForestGreen; else return Color.Navy; } }
        public bool IsOuter { get { return Adjacencies.Exists(adjacency => adjacency.Tile != Tile); } }
        public bool IsRegionOuter { get { return Adjacencies.Exists(adjacency => adjacency.Region != Region); } }
        
        public Color Color
        {
            get
            {
                Color color;
                if (Tile.Temperature < 273.15f)
                    color = Color.Snow;
                else if (Tile.Elevation > 0)
                {
                    if (Tile.Humidity < 0.1f)
                        color = Color.Lerp(Color.Olive, Color.YellowGreen, (Tile.Humidity) / 0.1f);
                    else if (Tile.Humidity < 0.5f)
                        color = Color.Lerp(Color.YellowGreen, Color.DarkGreen, (Tile.Humidity - 0.1f) / 0.4f);
                    else
                        color = Color.DarkGreen;
                }
                else
                    color = Color.Navy;
                return color;
            }
        }
        
        public PlanetVertex(Vector3 position)
        {
            Position = position;
        }

        public void AddAdjacency(PlanetVertex newAdjacency)
        {
            if (Adjacencies.Exists(n => n == newAdjacency) == false)
                Adjacencies.Add(newAdjacency);
        }

        public void AddMutualAdjacency(PlanetVertex newAdjacency)
        {
            AddAdjacency(newAdjacency);
            newAdjacency.AddAdjacency(this);
        }
    }
}
