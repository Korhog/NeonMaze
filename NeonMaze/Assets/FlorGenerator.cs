using System.Linq;

using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]

class Quad
{
    public List<Vector3> Vertices { get; private set; }
    public List<Vector3> Normals { get; private set; }

    public Quad(int col, int row)
    {
        // Quad include 2 triangles
        Vertices = new List<Vector3>
        {
            new Vector3(-0.5f + col,  0.0f, -0.5f + row),
            new Vector3(-0.5f + col,  0.0f,  0.5f + row),
            new Vector3( 0.5f + col,  0.0f,  0.5f + row),

            new Vector3(-0.5f + col,  0.0f, -0.5f + row),
            new Vector3( 0.5f + col,  0.0f,  0.5f + row),
            new Vector3( 0.5f + col,  0.0f, -0.5f + row),
        };

        Normals = new List<Vector3>
        {
            Vector3.up,
            Vector3.up,
            Vector3.up,
            Vector3.up,
            Vector3.up,
            Vector3.up
        };
    }
}

public class FlorGenerator : MonoBehaviour
{
    MeshFilter filter;

    void Start()
    {
        filter = GetComponent<MeshFilter>();
        filter.mesh = GenetareFlor();
    }

    Mesh GenetareFlor()
    {
        Mesh mesh = new Mesh();

        var quads = new List<Quad>()
        {
            new Quad(0, 0),
            new Quad(0, 1),
            new Quad(1, 1),
            new Quad(2, 1)
        };

        var vertices = quads.SelectMany(x => x.Vertices).ToList();
        var normals = quads.SelectMany(x => x.Normals).ToList();
        var indices = vertices.Select(x => vertices.IndexOf(x)).ToList();

        mesh.SetVertices(vertices);
        mesh.SetTriangles(indices, 0);
        mesh.SetNormals(normals);

        return mesh;
    }
}
