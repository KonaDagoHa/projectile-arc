using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class ArcMesh : MonoBehaviour
{
    
    public ShootSource shootSource;
    [Range(1, 100)]
    public int resolution = 10;
    float g; // Force of gravity on the y-axis
    float radianAngle;
    float velocity, angle;

    Mesh mesh;
    public float meshWidth;

    void Awake()
    {
        velocity = shootSource.shootVelocity;
        angle = shootSource.shootAngle;
        mesh = GetComponent<MeshFilter>().mesh;
        g = Mathf.Abs(Physics2D.gravity.y);
    }

    void OnValidate()
    {
        if (mesh != null && Application.isPlaying)
        {
            MakeArcMesh(CalculateArcArray());
        }
    }

    void Start()
    {
        MakeArcMesh(CalculateArcArray());
    }

    
    private void Update()
    {
        velocity = shootSource.shootVelocity;
        angle = shootSource.shootAngle;
        MakeArcMesh(CalculateArcArray());
    }
    

    void MakeArcMesh(Vector3[] arcVerts)
    {
        mesh.Clear();
        Vector3[] vertices = new Vector3[(resolution + 1) * 2];
        int[] triangles = new int[resolution * 6 * 2];

        for (int i = 0; i < resolution + 1; i++)
        {
            // Set vertices
            vertices[i * 2] = new Vector3(meshWidth * 0.5f, arcVerts[i].y, arcVerts[i].x); // Even numbers
            vertices[i * 2 + 1] = new Vector3(meshWidth * -0.5f, arcVerts[i].y, arcVerts[i].x); // Odd numbers
            // Set triangles
            if (i != resolution)
            {
                // Top triangles
                triangles[i * 12] = i * 2;
                triangles[i * 12 + 1] = triangles[i * 12 + 4] = i * 2 + 1;
                triangles[i * 12 + 2] = triangles[i * 12 + 3] = (i + 1) * 2;
                triangles[i * 12 + 5] = (i + 1) * 2 + 1;
                // Bottom triangles
                triangles[i * 12 + 6] = i * 2;
                triangles[i * 12 + 7] = triangles[i * 12 + 10] = (i + 1) * 2;
                triangles[i * 12 + 8] = triangles[i * 12 + 9] = i * 2 + 1;
                triangles[i * 12 + 11] = (i + 1) * 2 + 1;
            }

            
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
    }

    // Create array of Vector3 positions for arc
    Vector3[] CalculateArcArray()
    {
        Vector3[] arcArray = new Vector3[resolution + 500];
        radianAngle = Mathf.Deg2Rad * angle;
        float maxDistance = velocity * velocity * Mathf.Sin(2 * radianAngle) / g;

        for (int i = 0; i < resolution + 500; i++)
        {
            float t = (float)i / resolution; // Progress along the array
            arcArray[i] = CalculateArcPoint(t, maxDistance);
        }
        return arcArray;
    }

    // Calculate height and distance of each vertex in arc array
    Vector3 CalculateArcPoint(float t, float maxDistance)
    {
        float x = t * maxDistance;
        float y = x * Mathf.Tan(radianAngle) - (g * x * x / (2 * velocity * velocity * Mathf.Cos(radianAngle) * Mathf.Cos(radianAngle)));
        return new Vector3(x, y);
    }
}
