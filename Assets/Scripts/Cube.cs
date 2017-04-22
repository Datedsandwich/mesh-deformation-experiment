using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Cube : MonoBehaviour {

    public int xSize, ySize, zSize;

    private Mesh mesh;
    private Vector3[] vertices;

    private void Awake() {
        Generate();
    }

    private void Generate() {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Cube";
        CreateVertices();
        CreateTriangles();
    }

    private void CreateVertices() {
        int cornerVertices = 8;
        int edgeVertices = (xSize + ySize + zSize - 3) * 4;
        int faceVertices = (
            (xSize - 1) * (ySize - 1) +
            (xSize - 1) * (zSize - 1) +
            (ySize - 1) * (zSize - 1)) * 2;
        vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];

        int vertex = 0;
        // Generate a "ring" and then extrude it upwards
        for (int y = 0; y <= ySize; y++) {
            for (int x = 0; x <= xSize; x++) {
                vertices[vertex++] = new Vector3(x, y, 0);
            }
            for (int z = 1; z <= zSize; z++) {
                vertices[vertex++] = new Vector3(xSize, y, z);
            }
            for (int x = xSize - 1; x >= 0; x--) {
                vertices[vertex++] = new Vector3(x, y, zSize);
            }
            for (int z = zSize - 1; z > 0; z--) {
                vertices[vertex++] = new Vector3(0, y, z);
            }
        }

        // Fill in the top and bottom of the cube
        for (int z = 1; z < zSize; z++) {
            for (int x = 1; x < xSize; x++) {
                vertices[vertex++] = new Vector3(x, ySize, z);
            }
        }

        for (int z = 1; z < zSize; z++) {
            for (int x = 1; x < xSize; x++) {
                vertices[vertex++] = new Vector3(x, 0, z);
            }
        }

        mesh.vertices = vertices;
    }

    private void CreateTriangles() {
        int quads = (xSize * ySize + xSize * zSize + ySize * zSize) * 2;
        int[] triangles = new int[quads * 6];
        int ring = (xSize + zSize) * 2;
        int triangle = 0, vertex = 0;

        for(int y = 0; y < ySize; y++, vertex++) {
            // Loop through the triangles and create the faces
            for (int i = 0; i < ring - 1; i++, vertex++) {
                triangle = CreateQuad(triangles, triangle, vertex, vertex + 1, vertex + ring, vertex + ring + 1);
            }
            // For the last quad, its second and fourth vertex need to rewind to the start of the ring.
            triangle = CreateQuad(triangles, triangle, vertex, vertex - ring + 1, vertex + ring, vertex + 1);
        }

        triangle = CreateTopFace(triangles, triangle, ring);
        triangle = CreateBottomFace(triangles, triangle, ring);

        mesh.triangles = triangles;
    }

    private int CreateQuad(int[] triangles, int i, int bottomLeft, int bottomRight, int topLeft, int topRight) {
        triangles[i] = bottomLeft;
        triangles[i + 1] = triangles[i + 4] = topLeft;
        triangles[i + 2] = triangles[i + 3] = bottomRight;
        triangles[i + 5] = topRight;
        return i + 6;
    }

    private int CreateTopFace(int[] triangles, int triangle, int ring) {
        int vertex = ring * ySize;
        for (int x = 0; x < xSize - 1; x++, vertex++) {
            triangle = CreateQuad(triangles, triangle, vertex, vertex + 1, vertex + ring - 1, vertex + ring);
        }
        triangle = CreateQuad(triangles, triangle, vertex, vertex + 1, vertex + ring - 1, vertex + 2);

        int vMin = ring * (ySize + 1) - 1;
        int vMid = vMin + 1;
        int vMax = vertex + 2;

        for (int z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++) {
            triangle = CreateQuad(triangles, triangle, vMin, vMid, vMin - 1, vMid + xSize - 1);
            for (int x = 1; x < xSize - 1; x++, vMid++) {
                triangle = CreateQuad(
                    triangles, triangle,
                    vMid, vMid + 1, vMid + xSize - 1, vMid + xSize);
            }
            triangle = CreateQuad(triangles, triangle, vMid, vMax, vMid + xSize - 1, vMax + 1);
        }

        int vTop = vMin - 2;
        triangle = CreateQuad(triangles, triangle, vMin, vMid, vTop + 1, vTop);
        for (int x = 1; x < xSize - 1; x++, vTop--, vMid++) {
            triangle = CreateQuad(triangles, triangle, vMid, vMid + 1, vTop, vTop - 1);
        }
        triangle = CreateQuad(triangles, triangle, vMid, vTop - 2, vTop, vTop - 1);

        return triangle;
    }

    private int CreateBottomFace(int[] triangles, int triangle, int ring) {
        int v = 1;
        int vMid = vertices.Length - (xSize - 1) * (zSize - 1);
        triangle = CreateQuad(triangles, triangle, ring - 1, vMid, 0, 1);
        for (int x = 1; x < xSize - 1; x++, v++, vMid++) {
            triangle = CreateQuad(triangles, triangle, vMid, vMid + 1, v, v + 1);
        }
        triangle = CreateQuad(triangles, triangle, vMid, v + 2, v, v + 1);

        int vMin = ring - 2;
        vMid -= xSize - 2;
        int vMax = v + 2;

        for (int z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++) {
            triangle = CreateQuad(triangles, triangle, vMin, vMid + xSize - 1, vMin + 1, vMid);
            for (int x = 1; x < xSize - 1; x++, vMid++) {
                triangle = CreateQuad(
                    triangles, triangle,
                    vMid + xSize - 1, vMid + xSize, vMid, vMid + 1);
            }
            triangle = CreateQuad(triangles, triangle, vMid + xSize - 1, vMax + 1, vMid, vMax);
        }

        int vTop = vMin - 1;
        triangle = CreateQuad(triangles, triangle, vTop + 1, vTop, vTop + 2, vMid);
        for (int x = 1; x < xSize - 1; x++, vTop--, vMid++) {
            triangle = CreateQuad(triangles, triangle, vTop, vTop - 1, vMid, vMid + 1);
        }
        triangle = CreateQuad(triangles, triangle, vTop, vTop - 1, vMid, vTop - 2);

        return triangle;
    }

    private void OnDrawGizmos() {
        if (vertices == null) {
            return;
        }
        Gizmos.color = Color.black;
        for (int i = 0; i < vertices.Length; i++) {
            Gizmos.DrawSphere(vertices[i], 0.1f);
        }
    }
}