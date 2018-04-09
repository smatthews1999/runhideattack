using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GetNavmeshEdges : MonoBehaviour {

    // These functions will get a list of vectors on the edges of the Navmesh
  
    public List<Vector3> BorderVectors;

    void Awake () {
        this.BorderVectors = FindNavMeshBorders(NavMesh.CalculateTriangulation());
    }

    // This code was adapted from ... 
    //http://stagpoint.com/forums/threads/finding-the-borders-of-a-navmesh.10/
    // (thank you for this excellent recource)
    private List<Vector3> FindNavMeshBorders(NavMeshTriangulation mesh)
    {

        Vector3[] wverts = null;
        int[] triangles = null;

        WeldVertices(mesh, 0.01f, 2f, out wverts, out triangles);

        var map = new Dictionary<uint, int>();

        Action<ushort, ushort> processEdge = (a, b) =>
        {

            if (a > b)
            {
                var temp = b;
                b = a;
                a = temp;
            }

            uint key = ((uint)a << 16) | (uint)b;

            if (!map.ContainsKey(key))
                map[key] = 1;
            else
                map[key] += 1;

        };

        for (int i = 0; i < triangles.Length; i += 3)
        {

            var a = (ushort)triangles[i + 0];
            var b = (ushort)triangles[i + 1];
            var c = (ushort)triangles[i + 2];

            processEdge(a, b);
            processEdge(b, c);
            processEdge(c, a);

        }

        var verts = new List<Vector3>();

        foreach (var key in map.Keys)
        {

            var count = map[key];
            if (count != 1)
                continue;

            var a = (key >> 16);

            verts.Add(wverts[a]);

        }

        return verts;

    }


    private void WeldVertices(NavMeshTriangulation mesh, float threshold, float bucketStep, out Vector3[] vertices, out int[] indices)
    {

        // This code was adapted from http://answers.unity3d.com/questions/228841/dynamically-combine-verticies-that-share-the-same.html

        Vector3[] oldVertices = mesh.vertices;
        Vector3[] newVertices = new Vector3[oldVertices.Length];
        int[] old2new = new int[oldVertices.Length];
        int newSize = 0;

        // Find AABB
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        for (int i = 0; i < oldVertices.Length; i++)
        {
            if (oldVertices[i].x < min.x)
                min.x = oldVertices[i].x;
            if (oldVertices[i].y < min.y)
                min.y = oldVertices[i].y;
            if (oldVertices[i].z < min.z)
                min.z = oldVertices[i].z;
            if (oldVertices[i].x > max.x)
                max.x = oldVertices[i].x;
            if (oldVertices[i].y > max.y)
                max.y = oldVertices[i].y;
            if (oldVertices[i].z > max.z)
                max.z = oldVertices[i].z;
        }

        // Make cubic buckets, each with dimensions "bucketStep"
        int bucketSizeX = Mathf.FloorToInt((max.x - min.x) / bucketStep) + 1;
        int bucketSizeY = Mathf.FloorToInt((max.y - min.y) / bucketStep) + 1;
        int bucketSizeZ = Mathf.FloorToInt((max.z - min.z) / bucketStep) + 1;
        List<int>[,,] buckets = new List<int>[bucketSizeX, bucketSizeY, bucketSizeZ];

        // Make new vertices
        for (int i = 0; i < oldVertices.Length; i++)
        {

            // Determine which bucket it belongs to
            int x = Mathf.FloorToInt((oldVertices[i].x - min.x) / bucketStep);
            int y = Mathf.FloorToInt((oldVertices[i].y - min.y) / bucketStep);
            int z = Mathf.FloorToInt((oldVertices[i].z - min.z) / bucketStep);

            // Check to see if it's already been added
            if (buckets[x, y, z] == null)
                buckets[x, y, z] = new List<int>(); // Make buckets lazily

            for (int j = 0; j < buckets[x, y, z].Count; j++)
            {
                Vector3 to = newVertices[buckets[x, y, z][j]] - oldVertices[i];
                if (Vector3.SqrMagnitude(to) < threshold)
                {
                    old2new[i] = buckets[x, y, z][j];
                    goto skip; // Skip to next old vertex if this one is already there
                }
            }

            // Add new vertex
            newVertices[newSize] = oldVertices[i];
            buckets[x, y, z].Add(newSize);
            old2new[i] = newSize;
            newSize++;

            skip:
            ;

        }

        // Make new triangles
        int[] oldTris = mesh.indices;
        indices = new int[oldTris.Length];
        for (int i = 0; i < oldTris.Length; i++)
        {
            indices[i] = old2new[oldTris[i]];
        }

        vertices = new Vector3[newSize];
        for (int i = 0; i < newSize; i++)
        {
            vertices[i] = newVertices[i];
        }

    }

}
