using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField, Min(0.01f)]
    float voxelSize = 1f;
    [SerializeField, Min(1)]
    int renderDistance;

    // Start is called before the first frame update
    void Start()
    {
        GenerateTriangles();
    }

    void GenerateTriangles()
    {
        List<RayTracingMaster.Triangle> triangles = new List<RayTracingMaster.Triangle>();
        Vector3Int currentVoxel = WorldToGrid(Camera.main.transform.position);
        for(int x = -renderDistance; x <= renderDistance; x++)
        {
            for(int y = -renderDistance; y <= renderDistance; y++)
            {
                for (int z = -renderDistance; z <= renderDistance; z++)
                {
                    Vector3Int offset = new Vector3Int(x, y, z);
                    Vector3Int lookingAtVoxel = currentVoxel + offset;
                    GenerateVoxel(lookingAtVoxel, triangles);
                }
            }
        }
        RayTracingMaster.Instance.AddTriangles(triangles);
    }

    void GenerateVoxel(Vector3Int voxel, List<RayTracingMaster.Triangle> triangles)
    {
        triangles.Add(new RayTracingMaster.Triangle
        {
            v1 = Vector3.zero,
            v2 = Vector3.one,
            v3 = Vector3.one * 2,
            albedo = new Vector3(1, 1, 1),
            specular = new Vector3(0, 0, 0),
            smoothness = 0,
            emission = new Vector3(1,1,1),
        });
    }

    Vector3Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector3Int(Mathf.FloorToInt(worldPos.x / voxelSize),
                            Mathf.FloorToInt(worldPos.y / voxelSize),
                            Mathf.FloorToInt(worldPos.z / voxelSize));
    }
}
