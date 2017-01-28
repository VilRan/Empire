using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Empire
{
    public class Icosphere
    {
        public List<Vector3> Vertices = new List<Vector3>();
        public List<IndexTriangle> Triangles = new List<IndexTriangle>();
        ConcurrentDictionary<long, int> middleIndexCache = new ConcurrentDictionary<long, int>();

        public Icosphere(int detailLevel)
        {
            Console.Write("Building icosphere... ");
            Stopwatch stopwatch = Stopwatch.StartNew();

            buildIcosahedron();
            for (int i = 0; i < detailLevel; i++)
                refine();

            stopwatch.Stop();
            Console.WriteLine("" + stopwatch.ElapsedMilliseconds + " ms");
        }

        public IEnumerable<int> GetIndices()
        {
            for (int i = 0; i < Triangles.Count; i++)
            {
                yield return Triangles[i].AIndex;
                yield return Triangles[i].BIndex;
                yield return Triangles[i].CIndex;
            }
        }

        public float GetVertexSeparation()
        {
            IndexTriangle triangle = Triangles[0];
            Vector3 vertex1 = Vertices[triangle.AIndex];
            Vector3 vertex2 = Vertices[triangle.BIndex];
            float vertexSeparation = (vertex1 - vertex2).Length();
            return vertexSeparation;
        }

        void buildIcosahedron()
        {
            float t = (float)(1.0 + Math.Sqrt(5.0) / 2.0);
            Vertices.AddRange( createVertices(
                -1, t, 0,
                1, t, 0,
                -1, -t, 0,
                1, -t, 0,

                0, -1, t,
                0, 1, t,
                0, -1, -t,
                0, 1, -t,

                t, 0, -1,
                t, 0, 1,
                -t, 0, -1,
                -t, 0, 1
            ));

            Triangles.AddRange( createTriangles(
                0, 11, 5,
                0, 5, 1,
                0, 1, 7,
                0, 7, 10,
                0, 10, 11,

                1, 5, 9,
                5, 11, 4,
                11, 10, 2,
                10, 7, 6,
                7, 1, 8,

                3, 9, 4,
                3, 4, 2,
                3, 2, 6,
                3, 6, 8,
                3, 8, 9,

                4, 9, 5,
                2, 4, 11,
                6, 2, 10,
                8, 6, 7,
                9, 8, 1
            ));
        }

        IEnumerable<Vector3> createVertices(params float[] coordinates)
        {
            for (int i = 0; i < coordinates.Length; i += 3)
                yield return Vector3.Normalize(new Vector3(coordinates[i], coordinates[i + 1], coordinates[i + 2]));
        }

        IEnumerable<IndexTriangle> createTriangles(params int[] indices)
        {
            for (int i = 0; i < indices.Length; i += 3)
            {
                int aIndex = indices[i];
                int bIndex = indices[i + 1];
                int cIndex = indices[i + 2];
                yield return new IndexTriangle(aIndex, bIndex, cIndex);
            }
        }

        void refine()
        {
            Stack<IndexTriangle> newTriangles = new Stack<IndexTriangle>();
            foreach (IndexTriangle triangle in Triangles)
            {
                int outerA = triangle.AIndex;
                int outerB = triangle.BIndex;
                int outerC = triangle.CIndex;
                int middleAB = addMiddleVertex(outerA, outerB);
                int middleBC = addMiddleVertex(outerB, outerC);
                int middleCA = addMiddleVertex(outerC, outerA);
                newTriangles.Push(new IndexTriangle(outerA, middleAB, middleCA));
                newTriangles.Push(new IndexTriangle(outerB, middleBC, middleAB));
                newTriangles.Push(new IndexTriangle(outerC, middleCA, middleBC));
                newTriangles.Push(new IndexTriangle(middleAB, middleBC, middleCA));
            };
            Triangles = newTriangles.ToList();
            middleIndexCache.Clear();
        }

        int addMiddleVertex(int index1, int index2)
        {
            bool firstIsSmaller = index1 < index2;
            long smallerIndex = firstIsSmaller ? index1 : index2;
            long greaterIndex = firstIsSmaller ? index2 : index1;
            long key = (smallerIndex << 32) + greaterIndex;

            int newIndex = Vertices.Count;
            int middleIndex = middleIndexCache.GetOrAdd(key, newIndex);
            if (middleIndex == newIndex)
            {
                Vector3 position1 = Vertices[index1];
                Vector3 position2 = Vertices[index2];
                Vector3 middlePosition = Vector3.Normalize((position1 + position2) / 2);
                Vertices.Add(middlePosition);
            }
            return middleIndex;
        }
    }
}
