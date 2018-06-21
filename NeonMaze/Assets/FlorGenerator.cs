using System.Linq;

using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using System.Xml;
using System;
using System.Xml.Serialization;

public class UV
{
    [XmlAttribute("u")]
    public float U { get; private set; }
    [XmlAttribute("v")]
    public float V { get; private set; }
}

[XmlType("UV")]
public class UVDesc
{
    public UV A { get; private set; } 
    public UV B { get; private set; }
    public UV C { get; private set; } 
    public UV D { get; private set; } 
}

[XmlType("entries")]
public class EntriesDesc
{
    [XmlAttribute("left")]
    public bool Left { get; private set; }

    [XmlAttribute("right")]
    public bool Right { get; private set; }

    [XmlAttribute("top")]
    public bool Top { get; private set; }

    [XmlAttribute("bottom")]
    public bool Bottom { get; private set; }
}

[XmlRoot("flor"), XmlType("flor")]
public class FlorDesc    
{
    [XmlAttribute("col")]
    public int Col { get; private set; }

    [XmlAttribute("row")]
    public int Row { get; private set; }

    [XmlElement("entries")]
    public EntriesDesc Entries { get; private set; }

    [XmlElement("UV")]
    public UVDesc UV { get; private set; }
}

class Quad
{
    public List<Vector3> Vertices { get; private set; }
    public List<Vector3> Normals { get; private set; }
    public List<Vector2> UVs { get; private set; }

    public Quad(int col, int row)
    {

    }

    public Quad(XmlElement node)
    {
        /* XML struct
        <flor col="28" row="6">
            <entries left="false" right="true" top="false" bottom="false"/>
            <UV>
                <A u="0" v="1"/>
                <B u="0" v="0.75"/>
                <C u="0.125" v="0.75"/>
                <D u="0.125" v="1"/>
            </UV>
        </flor>
        */

        FlorDesc flor;
        var formatter = new XmlSerializer(typeof(FlorDesc));
        using (var sr = new StringReader(node.OuterXml))
        {
            flor = (FlorDesc)formatter.Deserialize(sr);
        }


        var col = flor.Col;
        var row = flor.Row;

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

        UVs = new List<Vector2>
        {
            // B C
            // A D

            new Vector2(flor.UV.A.U, 1 - flor.UV.A.V),
            new Vector2(flor.UV.B.U, 1 - flor.UV.B.V),
            new Vector2(flor.UV.C.U, 1 - flor.UV.C.V),

            new Vector2(flor.UV.A.U, 1 - flor.UV.A.V),
            new Vector2(flor.UV.C.U, 1 - flor.UV.C.V),
            new Vector2(flor.UV.D.U, 1 - flor.UV.D.V),
        };
    }
}


[RequireComponent(typeof(MeshFilter))]
public class FlorGenerator : MonoBehaviour
{

    public GameObject player;
    MeshFilter filter;

    void Start()
    {
        filter = GetComponent<MeshFilter>();
        filter.mesh = GenetareFlor();
    }

    Mesh GenetareFlor()
    {
        TextAsset xml = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/Levels/level_s0_e14.xml");
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xml.text);

        var root = xmlDoc.GetElementById("grid");
        var geometry = xmlDoc.GetElementsByTagName("geometry").Item(0);

        var playerNode = xmlDoc.GetElementsByTagName("object").Item(0);
        var pCols = playerNode.Attributes.GetNamedItem("col").Value;
        int pCol = 0;
        int.TryParse(pCols, out pCol);

        var pRows = playerNode.Attributes.GetNamedItem("row").Value;
        int pRow = 0;
        int.TryParse(pRows, out pRow);

        player.transform.position = new Vector3(pCol, 1, pRow);


        Mesh mesh = new Mesh();
        var quads = new List<Quad>();

        foreach (var child in geometry.ChildNodes)
        {
            var item = child as XmlElement;
            if (item == null)
                break;

            quads.Add(new Quad(item));
        } 

        var vertices = quads.SelectMany(x => x.Vertices).ToList();
        var normals = quads.SelectMany(x => x.Normals).ToList();


        var indices = new List<int>();
        for(int i = 0; i < quads.Count; i++)
        {
            indices.AddRange(new int[] { 0, 1, 2, 3, 4, 5 }.Select(x => x + i * 6).ToArray());
        }

        vertices.Select(x => vertices.IndexOf(x)).ToList();
        var uvs = quads.SelectMany(x => x.UVs).ToList();

        mesh.SetVertices(vertices);
        mesh.SetTriangles(indices, 0);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);

        return mesh;
    }
}
