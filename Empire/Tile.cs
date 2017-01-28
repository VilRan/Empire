using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire
{
    public class Tile
    {
        public PlanetVertex Origin;
        public Color Color;
        public Region Region;
        public List<PlanetVertex> Vertices = new List<PlanetVertex>();
        public List<PlanetVertex> OuterVertices = new List<PlanetVertex>();
        public List<Tile> Adjacencies = new List<Tile>();
        public float Elevation;
        public float Temperature;
        public float Humidity;
        
        public Planet Planet { get { return Region.Planet; } }
        public Vector3 Position { get { return Origin.Position; } }
        public bool IsOuter { get { return Adjacencies.Exists(adjacency => adjacency.Region != this.Region); } }

        EmpireGame game { get { return Planet.Game; } }
        Session session { get { return Planet.Session; } }

        public Tile(PlanetVertex origin)
        {
            Origin = origin;
        }

        public void AddVertex(PlanetVertex vertex)
        {
            vertex.Tile = this;
            Vertices.Add(vertex);
        }

        public void AddOuterVertex(PlanetVertex vertex)
        {
            AddVertex(vertex);
            OuterVertices.Add(vertex);
        }

        public void Expand(Random random)
        {
            if (OuterVertices.Count == 0)
                return;

            PlanetVertex vertex = OuterVertices[random.Next(OuterVertices.Count)];
            foreach (PlanetVertex adjacentVertex in vertex.Adjacencies)
            {
                if (adjacentVertex.Tile == null)
                    AddOuterVertex(adjacentVertex);
                else if (adjacentVertex.Tile != this)
                    AddMutualAdjacency(adjacentVertex.Tile);
            }
            OuterVertices.Remove(vertex);
        }

        public void AddAdjacency(Tile newAdjacency)
        {
            if (isAdjacentTo(newAdjacency) == false)
                Adjacencies.Add(newAdjacency);
        }

        public void AddMutualAdjacency(Tile newAdjacency)
        {
            AddAdjacency(newAdjacency);
            newAdjacency.AddAdjacency(this);
        }

        public float GetDistanceToRegionEdge()
        {
            if (IsOuter)
                return 0f;
            return Region.OuterTiles
                .Where(tile => tile.IsOuter)
                .Min(tile => Vector3.Distance(tile.Position, this.Position));
        }

        public void SimulateClimate()
        {
            double sunAngle = Planet.AxialTilt * Math.Cos(2 * Math.PI * session.Day / Planet.YearLength);
            double latitude = Math.Asin(Position.Y);
            double difference = latitude - sunAngle;
            if (difference > Math.PI / 2)
                difference = Math.PI / 2;
            else if (difference < -Math.PI / 2)
                difference = -Math.PI / 2;

            float multiplier = (float)Math.Sin(difference);
            multiplier *= multiplier;

            Temperature = 310f - 70f * multiplier;
            Temperature -= 0.0065f * Elevation;

            if (Elevation < 0)
                Humidity += 1f;

            float totalHumidityExchanged = 0f;
            foreach (Tile adjacency in Adjacencies)
            {
                float humidityExchanged = (Humidity - adjacency.Humidity) / (Adjacencies.Count + 1);
                adjacency.Humidity += humidityExchanged;
                totalHumidityExchanged += humidityExchanged;
            }
            Humidity += totalHumidityExchanged;

            Humidity /= 2;
        }

        bool isAdjacentTo(Tile otherTile)
        {
            return Adjacencies.Contains(otherTile);
        }
    }
}
