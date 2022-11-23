using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class PxLineDrawer : MonoBehaviour {
    [SerializeField] private int pixelPerUnit = 16;
    [SerializeField] private Color _color = Color.white;

    public Color color { get; private set; }

    private List<Vector3> allVertices = new();
    private List<int> allTriangles = new();
    private List<Vector2> allUvs = new();
    private List<Color> allColors = new();
    private List<PxLine> lines = new();

    private MeshFilter meshFilter;
    private MeshRenderer rd;

    private void Awake() {
        meshFilter = GetComponent<MeshFilter>();
        rd = GetComponent<MeshRenderer>();
        SetColor(_color);
    }

    public void SetColor(Color color) {
        this.color = color;
    }

    public void LineTo(Vector2 to) {
        var toPos = new Vector2Int(Mathf.RoundToInt(to.x * pixelPerUnit), Mathf.RoundToInt(to.y * pixelPerUnit));
        var flipY = false;

        var offset = Vector2.zero;
        if (toPos.x < 0) {
            offset = to;
            toPos = -toPos;
        }

        if (toPos.y < 0) {
            flipY = true;
            toPos.y *= -1;
        }

        Bresenham(toPos.x, toPos.y);
        Display(offset, flipY);
    }
    
    private void Bresenham(int toX,  int toY) {
        lines.Clear();
        var x = 0;
        var y = 0;
        var counter = 0;

        var node = new PxLine {
            ready = true
        };

        if (toX >= toY) {
            for (var i = 0; i < toX; i++) {
                x++;
                counter += toY;
                if (counter >= toX) {
                    y++;
                    counter -= toX;
                }

                node.ex = x;
                node.ey = y;
                if (!node.ready) {
                    node.ready = true;
                }

                if (node.size == 8) {
                    lines.Add(node);
                    node = new PxLine();
                    node.sx = x;
                    node.sy = y;
                }                
            }
        } else {
            for (var i = 0; i < toY; i++) {
                y++;
                counter += toX;
                if (counter >= toY) {
                    x++;
                    counter -= toY;
                }

                node.ex = x;
                node.ey = y;
                if (!node.ready) {
                    node.ready = true;
                }

                if (node.size == 8) {
                    lines.Add(node);
                    node = new PxLine();
                    node.sx = x;
                    node.sy = y;
                }
            }
        }
        
        if (node.ready) { 
            lines.Add(node);
        }
    }

    private void Display(Vector3 offset, bool yFlip) {
        allVertices.Clear();
        allTriangles.Clear();
        allUvs.Clear();
        allColors.Clear();
        
        var scale = 1f / pixelPerUnit;
        var width = 8;
        
        var index = 0;
        var scaleX = scale;
        var scaleY = scale;
        if (yFlip) {
            scaleY *= -1;
        }
        
        lines.ForEach(line => {
            if (line.size == 0) {
                return;
            }

            var at = index * 4;

            allColors.Add(color);
            allColors.Add(color);
            allColors.Add(color);
            allColors.Add(color);

            allVertices.Add(new Vector3(line.sx * scaleX, line.sy * scaleY, 0) + offset);
            allVertices.Add(new Vector3(line.sx * scaleX, (line.sy + width) * scaleY, 0) + offset);
            allVertices.Add(new Vector3((line.sx + width) * scaleX, (line.sy + width) * scaleY, 0) + offset);
            allVertices.Add(new Vector3((line.sx + width) * scaleX, line.sy * scaleY, 0) + offset);

            allTriangles.Add(at);
            allTriangles.Add(at + 1);
            allTriangles.Add(at + 2);
            allTriangles.Add(at);
            allTriangles.Add(at + 2);
            allTriangles.Add(at + 3);

            Calculate(line);
            index++;
        });

        var mesh = meshFilter.mesh;
        if (mesh == null) {
            mesh = new Mesh();
            meshFilter.mesh = mesh;
        } 
    
        mesh.Clear();
        mesh.vertices = allVertices.ToArray();
        mesh.triangles = allTriangles.ToArray();
        mesh.uv = allUvs.ToArray();
        mesh.colors = allColors.ToArray();
    }

    private void Calculate(PxLine line) {
        var width = 153f;
        var cell = 8f;
        var padding = 1f;
        var row = line.size;
        var col = 0;
        if (line.height <= line.width) {
            col = line.height;
        } else {
            col = line.height + (line.size - line.width);
        }
        
        var ps = new Vector2(col * (cell + padding) / width, row * (cell + padding) / width);
        var pe = new Vector2(ps.x + cell / width, ps.y + cell / width);

        allUvs.Add(new Vector2(ps.x, ps.y));
        allUvs.Add(new Vector2(ps.x, pe.y));
        allUvs.Add(new Vector2(pe.x, pe.y));
        allUvs.Add(new Vector2(pe.x, ps.y));
    }
}
