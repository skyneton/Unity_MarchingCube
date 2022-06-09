using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Chunk
{
    public readonly World world;
    public readonly ChunkCoord coord;
    private GameObject chunkObject;
    
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    public MeshCollider meshCollider;

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();

    public const int ChunkWidth = 16, ChunkHeight = 255;
    
    internal BlockType[,,] blockData = new BlockType[ChunkWidth, ChunkHeight, ChunkWidth];
    

    public Vector3Int Position
    {
        get { return new Vector3Int(coord.x * ChunkWidth, 0, coord.z * ChunkWidth); }
    }

    public bool IsActive
    {
        get { return chunkObject != null && chunkObject.activeSelf;  }
        set { chunkObject?.SetActive(value); }
    }

    public Chunk(ChunkCoord _coord, World _world, Action<Chunk> loaded = null)
    {
        coord = _coord;
        world = _world;

        PopulateVoxelMap();
        GenerateMeshData();
    }

    private void PopulateVoxelMap()
    {
        for(int x = 0; x < ChunkWidth; x++)
            for (int y = 0; y < ChunkHeight; y++)
                for (int z = 0; z < ChunkWidth; z++)
                {
                    blockData[x, y, z] = world.GetBlock(x + Position.x, y, z + Position.z).Type;
                }
    }

    internal void GenerateMeshData()
    {
        ClearMeshData();
        for (int x = 0; x < ChunkWidth; x++)
        {
            for (int z = 0; z < ChunkWidth; z++)
            {
                MarchCubeFloor(new Vector3Int(x, -1, z), blockData[x, 0, z]);
                for (int y = 0; y < ChunkHeight; y++)
                {
                    MarchCube(new Vector3Int(x, y, z), blockData[x, y, z]);
                }
            }
        }
    }

    public void CreateChunkObject()
    {
        chunkObject = new GameObject();
        chunkObject.tag = world.TerrainNameTag;
        
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshCollider = chunkObject.AddComponent<MeshCollider>();

        meshRenderer.material = world.material;
        chunkObject.transform.position = Position;
        chunkObject.name = "Chunk (" + coord.x + ", " + coord.z + ")";
        chunkObject.transform.SetParent(world.transform);
    }

    public void BuildMesh()
    {
        if (chunkObject == null) CreateChunkObject();
        
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    private void ClearMeshData()
    {
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
    }

    private void MarchCube(Vector3Int pos, BlockType blockType)
    {
        if (blockType == BlockType.Air) return;

        int configIndex = GetCubeConfiguration(pos);
        AddMeshData(configIndex, pos, blockType);
        // if (configIndex == 8)
        // {
        //     pos.y += 1;
        //     AddMeshData(GetCubeConfiguration(pos), pos, blockType);
        //     pos.y -= 1;
        // }

        // Side Mesh Add
        for(int y = -1; y < 1; y++)
            for (int x = -1; x < 1; x++)
            for (int z = -1; z < 1; z++)
            {
                if (x == 0 && y == 0 && z == 0) continue;
                // if(pos.x + x < 0) continue;
                if(pos.y + y < 0) continue;
                // if(pos.z + z < 0) continue;
                if(world.GetBlock(ChunkPos2WorldPos(pos.x + x, pos.y + y, pos.z + z)) != BlockType.Air) continue;
        
                pos.x += x;
                pos.y += y;
                pos.z += z;
                configIndex = GetCubeConfiguration(pos);
                AddMeshData(configIndex, pos, blockType);
                // if (configIndex == 1 && pos.y > 0 && world.GetBlock(ChunkPos2WorldPos(pos.x, pos.y - 1, pos.z)) == BlockType.AIR)
                // {
                //     pos.y -= 1;
                //     AddMeshData(GetCubeConfiguration(pos), pos, blockType);
                //     pos.y += 1;
                // }
                pos.x -= x;
                pos.y -= y;
                pos.z -= z;
            }
    }

    private void MarchCubeFloor(Vector3Int pos, BlockType blockType)
    {
        if (blockType == BlockType.Air) return;
        AddMeshData(GetCubeConfiguration(pos), pos, blockType);
    }

    public void AddTexture(int textureId)
    {
        int posY = textureId / MarchingData.TextureAtlasSizeInBlocks;
        int posX = textureId % MarchingData.TextureAtlasSizeInBlocks;

        float x = posX * MarchingData.NormalizedBlockTextureSize;
        float y = posY * MarchingData.NormalizedBlockTextureSize;

        y = 1f - y - MarchingData.NormalizedBlockTextureSize;
        
        uvs.Add(new Vector2(x + 0.0018f, y + 0.0018f));
        uvs.Add(new Vector2(x + 0.0018f, y + MarchingData.NormalizedBlockTextureSize - 0.0018f));
        uvs.Add(new Vector2(x + MarchingData.NormalizedBlockTextureSize - 0.0018f, y + MarchingData.NormalizedBlockTextureSize - 0.0018f));
    }

    private Vector3 ChunkPos2WorldPos(Vector3 pos)
    {
        return pos + Position;
    }

    private Vector3 ChunkPos2WorldPos(int x, int y, int z)
    {
        return new Vector3Int(x, y, z) + Position;
    }

    private int GetCubeConfiguration(Vector3Int pos)
    {
        int configurationIndex = 0;
        for (int i = 0; i < 8; i++)
        {
            Vector3Int corner = pos + MarchingData.VertsTable[i];
            if (corner.y < 0 || corner.y >= ChunkHeight)
            {
                configurationIndex |= 1 << i;
                continue;
            }
            if (corner.x < 0 || corner.z < 0 || corner.x >= ChunkWidth || corner.z >= ChunkWidth)
            {
                // Chunk Out Of Range
                if(world.GetBlock(ChunkPos2WorldPos(corner)) == BlockType.Air)
                    configurationIndex |= 1 << i;
            }
            else if(blockData[corner.x, corner.y, corner.z] == BlockType.Air)
                configurationIndex |= 1 << i;
        }
        
        return configurationIndex;
    }
    

    private void AddMeshData(int configIndex, Vector3Int pos, BlockType blockType)
    {
        if (configIndex == 0 || configIndex >= 255) return;

        int edgeIndex = 0;
        for(int i = 0; i < 5; i++)
        {
            for (int p = 0; p < 3; p++)
            {
                int indice = MarchingData.TriangleTable[configIndex, edgeIndex];
                if(indice == -1) return;
                
                Vector3Int vert1 = pos + MarchingData.VertsTable[MarchingData.EdgeIndexes[indice, 0]];
                Vector3Int vert2 = pos + MarchingData.VertsTable[MarchingData.EdgeIndexes[indice, 1]];

                Vector3 vertPosition;
                if (world.SmoothTerrain)
                {
                    float vert1Data = world.GetBlock(ChunkPos2WorldPos(vert1)) == BlockType.Air
                        ? 1
                        : 0;

                    float vert2Data = world.GetBlock(ChunkPos2WorldPos(vert2)) == BlockType.Air
                        ? 1
                        : 0;

                    float difference = -vert1Data / (vert2Data - vert1Data);

                    vertPosition = (Vector3) vert1 + (Vector3) (vert2 - vert1) * difference;
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
            
            AddTexture(blockType.GetTextureID());
        }
    }
}

[System.Serializable]
public class ChunkCoord
{
    public readonly World world;
    public readonly int x;
    public readonly int z;

    public ChunkCoord(World world, int x, int z)
    {
        this.world = world;
        this.x = x;
        this.z = z;
    }

    public override int GetHashCode()
    {
        int hash = 3;
        hash = 19 * hash + (world != null ? world.GetHashCode() : 0);

        hash = 19 * hash + x | x << 6;
        hash = 19 * hash + z | z << 6;
        return hash;
    }

    public override bool Equals(object obj)
    {
        if (obj is null || obj is not ChunkCoord o) return false;
        return this == o;
    }

    public override string ToString()
    {
        return string.Format("ChunkCoord ({0}, {1})", x, z);
    }

    public static bool operator ==(ChunkCoord c1, ChunkCoord c2)
    {
        if (c1 is null || c2 is null) return c1 is null && c2 is null;

        return c1.x == c2.x && c1.z == c2.z && c1.world == c2.world;
    }

    public static bool operator !=(ChunkCoord c1, ChunkCoord c2) => !(c1 == c2);
}