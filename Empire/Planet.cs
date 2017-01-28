using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire
{
    public class Planet
    {
        public Session Session;
        public float AxialTilt = MathHelper.ToRadians(22f);
        public int YearLength = 365;

        List<Region> regions;
        List<Tile> tiles;
        List<PlanetVertex> vertices;
        List<int> indices;
        VertexBuffer vertexBuffer;
        IndexBuffer indexBuffer;
        VertexPositionNormalColor[] vertexArray;

        public EmpireGame Game { get { return Session.Game; } }
        Random random { get { return Game.Random; } }
        GraphicsDevice graphicsDevice { get { return Game.GraphicsDevice; } }

        public Planet(Session session)
        {
            Session = session;

            Console.WriteLine("\nGenerating planet...");
            Stopwatch stopwatch = Stopwatch.StartNew();

            Icosphere icosphere = new Icosphere(7);
            initializeVertices(icosphere);
            initializeAdjacencies(icosphere.Triangles);
            createTiles();
            expandTiles();
            resetOuterVertices();
            createRegions();
            expandRegions();
            resetOuterTiles();
            randomizeElevation();
            simulateClimate();
            indices = icosphere.GetIndices().ToList();
            buildVertexBuffer();
            buildIndexBuffer();

            stopwatch.Stop();
            Console.WriteLine("Total: " + stopwatch.ElapsedMilliseconds + " ms");
        }

        public void Update()
        {
            Console.Write("Simulating Climate... ");
            Stopwatch stopwatch = Stopwatch.StartNew();
            foreach (Tile tile in tiles)
            //Parallel.ForEach(tiles, (tile) =>
            {
                tile.SimulateClimate();
            };
            stopwatch.Stop();
            Console.WriteLine("" + stopwatch.ElapsedMilliseconds + " ms");
            buildVertexBuffer();
        }

        public void Draw()
        {
            graphicsDevice.SetVertexBuffer(vertexBuffer);
            graphicsDevice.Indices = indexBuffer;
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indexBuffer.IndexCount / 3);
        }

        void initializeVertices(Icosphere icosphere)
        {
            Console.Write("Initializing vertices... ");
            Stopwatch stopwatch = Stopwatch.StartNew();

            vertices = new List<PlanetVertex>(icosphere.Vertices.Count);
            float vertexSeparation = icosphere.GetVertexSeparation();
            for (int i = 0; i < icosphere.Vertices.Count; i++)
            {
                Vector3 position = icosphere.Vertices[i];
                float xOffset = (-0.5f + random.NextFloat()) * vertexSeparation / 2;
                float yOffset = (-0.5f + random.NextFloat()) * vertexSeparation / 2;
                float zOffset = (-0.5f + random.NextFloat()) * vertexSeparation / 2;
                position += new Vector3(xOffset, yOffset, zOffset);
                position.Normalize();

                PlanetVertex vertex = new PlanetVertex(position);
                vertices.Add(vertex);
            }

            stopwatch.Stop();
            Console.WriteLine("" + stopwatch.ElapsedMilliseconds + " ms");
        }

        void initializeAdjacencies(IEnumerable<IndexTriangle> triangles)
        {
            Console.Write("Initializing adjacencies... ");
            Stopwatch stopwatch = Stopwatch.StartNew();

            foreach (IndexTriangle triangle in triangles)
            {
                PlanetVertex aVertex = vertices[triangle.AIndex];
                PlanetVertex bVertex = vertices[triangle.BIndex];
                PlanetVertex cVertex = vertices[triangle.CIndex];
                aVertex.AddMutualAdjacency(bVertex);
                aVertex.AddMutualAdjacency(cVertex);
                bVertex.AddMutualAdjacency(cVertex);
            }

            stopwatch.Stop();
            Console.WriteLine("" + stopwatch.ElapsedMilliseconds + " ms");

            //List<PlanetVertex> adjacencyless = vertices.Where(vertex => vertex.Adjacencies.Count == 0).ToList();
        }

        void createTiles()
        {
            Console.Write("Creating tiles... ");
            Stopwatch stopwatch = Stopwatch.StartNew();

            int numberOfTiles = random.Next(5000, 6000);
            tiles = new List<Tile>(numberOfTiles);
            for (int i = 0; i < numberOfTiles; i++)
            {
                PlanetVertex origin;
                do origin = vertices[random.Next(vertices.Count)];
                //while (origin.Tile != null);
                while (origin.Tile != null || origin.Adjacencies.Count(adjacency => adjacency.Tile != null) > 0);
                Tile tile = new Tile(origin);
                tile.Color = new Color(random.Next(256), random.Next(256), random.Next(256));
                tile.AddOuterVertex(origin);
                tile.Expand(random);
                tile.Expand(random);
                tiles.Add(tile);
            }
            stopwatch.Stop();
            Console.WriteLine("" + stopwatch.ElapsedMilliseconds + " ms");
        }

        void expandTiles()
        {
            Console.Write("Expanding tiles... ");
            Stopwatch stopwatch = Stopwatch.StartNew();

            List<Tile> expandableTiles = new List<Tile>();
            expandableTiles.AddRange(tiles);
            while (expandableTiles.Count > 0)
            {
                Tile tile = expandableTiles[random.Next(expandableTiles.Count)];
                tile.Expand(random);
                if (tile.OuterVertices.Count == 0)
                    expandableTiles.Remove(tile);
            }

            stopwatch.Stop();
            Console.WriteLine("" + stopwatch.ElapsedMilliseconds + " ms");
        }

        void resetOuterVertices()
        {
            Console.Write("Resetting tiles' outer vertices... ");
            Stopwatch stopwatch = Stopwatch.StartNew();

            foreach (Tile tile in tiles)
            {
                tile.OuterVertices.AddRange(tile.Vertices.
                    Where(vertex => vertex.Adjacencies.
                    Exists(adjacency => adjacency.Tile != tile)));
            }

            stopwatch.Stop();
            Console.WriteLine("" + stopwatch.ElapsedMilliseconds + " ms");
        }

        void createRegions()
        {
            Console.Write("Creating regions... ");
            Stopwatch stopwatch = Stopwatch.StartNew();

            int numberOfRegions = random.Next(60, 72);
            regions = new List<Region>(numberOfRegions);
            for (int i = 0; i < numberOfRegions; i++)
            {
                Tile origin;
                do origin = tiles[random.Next(tiles.Count)];
                while (origin.Region != null);
                Region region = new Region(this, origin);
                region.Color = new Color(random.Next(256), random.Next(256), random.Next(256));
                region.AddOuterTile(origin);
                region.Expand(random);
                regions.Add(region);
            }

            stopwatch.Stop();
            Console.WriteLine("" + stopwatch.ElapsedMilliseconds + " ms");
        }

        void expandRegions()
        {
            Console.Write("Expanding regions... ");
            Stopwatch stopwatch = Stopwatch.StartNew();

            List<Region> expandableRegions = new List<Region>();
            expandableRegions.AddRange(regions);
            while (expandableRegions.Count > 0)
            {
                Region region = expandableRegions[random.Next(expandableRegions.Count)];
                region.Expand(random);
                if (region.OuterTiles.Count == 0)
                    expandableRegions.Remove(region);
            }

            stopwatch.Stop();
            Console.WriteLine("" + stopwatch.ElapsedMilliseconds + " ms");
        }

        void resetOuterTiles()
        {
            Console.Write("Resetting regions' outer tiles... ");
            Stopwatch stopwatch = Stopwatch.StartNew();

            foreach (Region region in regions)
            {
                region.OuterTiles.AddRange(region.Tiles.
                    Where(tile => tile.Adjacencies.
                    Exists(adjacency => adjacency.Region != region)));
            }

            stopwatch.Stop();
            Console.WriteLine("" + stopwatch.ElapsedMilliseconds + " ms");
        }
        
        void randomizeElevation()
        {
            Console.Write("Randomizing elevations... ");
            Stopwatch stopwatch = Stopwatch.StartNew();

            foreach (Region region in regions)
            {
                float maxElevation;
                float maxDistanceToEdge = region.Tiles.Max(tile => tile.GetDistanceToRegionEdge());
                double oceanChance = 0.7;//Math.Sqrt(0.1 * continent.Adjacencies.Count);
                if (random.NextDouble() < oceanChance)
                    maxElevation = -8000f * random.NextFloat();
                else
                    maxElevation = 8000f * random.NextFloat();

                foreach (Tile tile in region.Tiles)
                {
                    float distanceToEdge = tile.GetDistanceToRegionEdge();
                    tile.Elevation = maxElevation * (0.1f + 0.9f * distanceToEdge / maxDistanceToEdge);
                }
            };

            stopwatch.Stop();
            Console.WriteLine("" + stopwatch.ElapsedMilliseconds + " ms");
        }

        void simulateClimate()
        {
            foreach (Tile tile in tiles)
                tile.SimulateClimate();
        }

        void buildVertexBuffer()
        {
            Console.Write("Building vertex buffers... ");
            Stopwatch stopwatch = Stopwatch.StartNew();

            if (vertexArray == null || vertexArray.Length != vertices.Count)
                vertexArray = new VertexPositionNormalColor[vertices.Count];

            //for (int i = 0; i < vertexArray.Length; i++)
            Parallel.For(0, vertexArray.Length, (i) =>
            {
                Vector3 position = vertices[i].Position;
                Vector3 normal = vertices[i].Position;
                Color color = vertices[i].Color;
                vertexArray[i] = new VertexPositionNormalColor(position, normal, color);
            });

            if (vertexBuffer == null || vertexBuffer.VertexCount != vertexArray.Length)
                vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionNormalColor), vertexArray.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertexArray);
            
            stopwatch.Stop();
            Console.WriteLine("" + stopwatch.ElapsedMilliseconds + " ms");
        }

        void buildIndexBuffer()
        {
            if (indexBuffer == null)
                indexBuffer = new IndexBuffer(graphicsDevice, typeof(int), indices.Count, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices.ToArray());
        }
    }
}
