using System.Collections.Generic;
using UnityEngine;

public class MarchingCube : MonoBehaviour
{
    public static bool SmoothTerrain = true;
    public static void GenerateMeshData(float[,,] voxelData, BlockType[,,] blockTypes, List<Vector3> vertices,
        List<int> triangles, List<Vector2> uvs, float terrainSurface = 0.5f)
    {
        int posX = voxelData.GetLength(0);
        int posY = voxelData.GetLength(1);
        int posZ = voxelData.GetLength(2);

        for (int x = 0; x < posX - 1; x++) {
            for (int y = 0; y < posY - 1; y++)
            {
                for (int z = 0; z < posZ - 1; z++)
                {
                    if(y == 0)
                        MarchCubeWall(new Vector3Int(x, -1, z), voxelData, blockTypes[x, y, z], vertices, triangles, uvs, terrainSurface);
                    if(x == 0)
                        MarchCubeWall(new Vector3Int(-1, y, z), voxelData, blockTypes[x, y, z], vertices, triangles, uvs, terrainSurface);
                    if(z == 0)
                        MarchCubeWall(new Vector3Int(x, y, -1), voxelData, blockTypes[x, y, z], vertices, triangles, uvs, terrainSurface);
                    MarchCube(new Vector3Int(x, y, z), voxelData, blockTypes, blockTypes[x, y, z], vertices, triangles, uvs,
                        terrainSurface);
                }
            }
        }
    }

    private static void MarchCubeWall(Vector3Int pos, float[,,] voxelData, BlockType blockType, List<Vector3> vertices,
        List<int> triangles,
        List<Vector2> uvs, float terrainSurface)
    {
        if (blockType == BlockType.AIR) return;
        Vector3Int posInt = new Vector3Int((int) pos.x, (int) pos.y, (int) pos.z);
        
        int configIndex = GetCubeConfigurationWall(posInt, voxelData, terrainSurface);
        if (configIndex == 0 || configIndex == 255) return;
        
        int edgeIndex = 0;
        for (int i = 0; i < 5; i++)
        {
            for (int p = 0; p < 3; p++)
            {
                int indice = MarchingData.TriangleTable[configIndex, edgeIndex];

                if (indice == -1) return;

                Vector3Int vert1 = posInt + MarchingData.VertsTable[MarchingData.EdgeIndexes[indice, 0]];
                Vector3Int vert2 = posInt + MarchingData.VertsTable[MarchingData.EdgeIndexes[indice, 1]];
                
                Vector3 vertPosition;
                if (SmoothTerrain)
                {
                    float vert1Data = 1;
                    if(vert1.x >= 0 && vert1.y >= 0 && vert1.z >= 0)
                        vert1Data = voxelData[vert1.x, vert1.y, vert1.z];
                    
                    float vert2Data = 1;
                    if(vert2.x >= 0 && vert2.y >= 0 && vert2.z >= 0)
                        vert2Data = voxelData[vert2.x, vert2.y, vert2.z];
                    
                    float difference = -vert1Data / (vert2Data - vert1Data);

                    vertPosition = vert1 + (Vector3) (vert2 - vert1) * difference;
                }
                else
                {
                    vertPosition = (Vector3) (vert1 + vert2) / 2f;
                }
                
                vertPosition.y -= .5f;
                
                vertices.Add(vertPosition);
                triangles.Add(vertices.Count - 1);

                edgeIndex++;
            }
            
            // Chunk.AddTexture(uvs, blockType.GetTextureID());
        }
    }

    private static void MarchCube(Vector3Int pos, float[,,] voxelData, BlockType[,,] blockTypes, BlockType blockType,
        List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, float terrainSurface)
    {
        if (blockType == BlockType.AIR) return;
        
        AddMeshData(GetCubeConfiguration(pos, voxelData, terrainSurface), pos, voxelData, blockType, vertices, triangles, uvs);
        
        // Side Mesh Add
        if (pos.x > 0 && blockTypes[pos.x - 1, pos.y, pos.z] == BlockType.AIR)
        {
            pos.x -= 1;
            AddMeshData(GetCubeConfiguration(pos, voxelData, terrainSurface), pos, voxelData, blockType, vertices, triangles, uvs);
            pos.x += 1;
        }
        
        if (pos.y > 0 && blockTypes[pos.x, pos.y - 1, pos.z] == BlockType.AIR)
        {
            pos.y -= 1;
            AddMeshData(GetCubeConfiguration(pos, voxelData, terrainSurface), pos, voxelData, blockType, vertices, triangles, uvs);
            pos.y += 1;
        }
        
        if (pos.z > 0 && blockTypes[pos.x, pos.y, pos.z - 1] == BlockType.AIR)
        {
            pos.z -= 1;
            AddMeshData(GetCubeConfiguration(pos, voxelData, terrainSurface), pos, voxelData, blockType, vertices, triangles, uvs);
            pos.z += 1;
        }
        
        if (pos.x > 0 && pos.z > 0 && blockTypes[pos.x - 1, pos.y, pos.z - 1] == BlockType.AIR)
        {
            pos.x -= 1;
            pos.z -= 1;
            AddMeshData(GetCubeConfiguration(pos, voxelData, terrainSurface), pos, voxelData, blockType, vertices, triangles, uvs);
            pos.x += 1;
            pos.z += 1;
        }
        
        if (pos.x > 0 && pos.z > 0 && blockTypes[pos.x - 1, pos.y, pos.z - 1] == BlockType.AIR)
        {
            pos.x -= 1;
            pos.z -= 1;
            AddMeshData(GetCubeConfiguration(pos, voxelData, terrainSurface), pos, voxelData, blockType, vertices, triangles, uvs);
            pos.x += 1;
            pos.z += 1;
        }
    }

    private static void AddMeshData(int configIndex, Vector3Int pos, float[,,] voxelData, BlockType blockType, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        if (configIndex == 0 || configIndex == 255) return;
        
        int edgeIndex = 0;
        for (int i = 0; i < 5; i++)
        {
            for (int p = 0; p < 3; p++)
            {
                int indice = MarchingData.TriangleTable[configIndex, edgeIndex];

                if (indice == -1) return;

                Vector3Int vert1 = pos + MarchingData.VertsTable[MarchingData.EdgeIndexes[indice, 0]];
                Vector3Int vert2 = pos + MarchingData.VertsTable[MarchingData.EdgeIndexes[indice, 1]];
                
                Vector3 vertPosition;
                if (SmoothTerrain)
                {
                    float vert1Data = voxelData[vert1.x, vert1.y, vert1.z];
                    float vert2Data = voxelData[vert2.x, vert2.y, vert2.z];
                    
                    float difference = -vert1Data / (vert2Data - vert1Data);

                    vertPosition = vert1 + (Vector3) (vert2 - vert1) * difference;
                }
                else
                {
                    vertPosition = (Vector3) (vert1 + vert2) / 2f;
                }

                vertPosition.y -= .5f;
                
                vertices.Add(vertPosition);
                triangles.Add(vertices.Count - 1);

                edgeIndex++;
            }
            
            // Chunk.AddTexture(uvs, blockType.GetTextureID());
        }
    }

    private static int GetCubeConfiguration(Vector3Int pos, float[,,] voxelData, float terrainSurface)
    {
        int configurationIndex = 0;
        for (int i = 0; i < 8; i++)
        {
            Vector3Int corner = pos + MarchingData.VertsTable[i];
            if(voxelData[corner.x, corner.y, corner.z] > terrainSurface)
                configurationIndex |= 1 << i;
        }
        
        return configurationIndex;
    }

    private static int GetCubeConfigurationWall(Vector3Int pos, float[,,] voxelData, float terrainSurface)
    {
        int configurationIndex = 0;
        for (int i = 0; i < 8; i++)
        {
            Vector3Int corner = pos + MarchingData.VertsTable[i];
            if((corner.x < 0 || corner.y < 0 || corner.z < 0) // 바깥쪽에서 보이게
               || (voxelData[corner.x, corner.y, corner.z] > terrainSurface))
                configurationIndex |= 1 << i;
        }
        
        return configurationIndex;
    }
}