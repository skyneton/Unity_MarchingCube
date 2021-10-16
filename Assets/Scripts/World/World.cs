using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class World : MonoBehaviour
{
    public bool IsActive { get; private set; }
    public Material material;
    public static readonly List<World> worlds = new List<World>();
    
    public BiomeAttributes biome;
    public int seed = 100;

    public int RandomSeedRange = 99999;

    public int viewDistance = 5;
    
    public bool SmoothTerrain = true;

    [SerializeField]
    internal Player player = null;
    public Vector3 spawnPosition;

    public string TerrainNameTag = "Terrain";

    public Dictionary<ChunkCoord, Chunk> chunks { get; } = new Dictionary<ChunkCoord, Chunk>();
    public List<Chunk> activeChunks { get; } = new List<Chunk>();
    private List<ChunkCoord> needMeshUpdateChunks = new List<ChunkCoord>();

    private Thread chunkLoadThread;
    private ConcurrentQueue<ChunkCoord> loadingChunks = new ConcurrentQueue<ChunkCoord>();
    private ConcurrentQueue<Chunk> loadedChunks = new ConcurrentQueue<Chunk>();

    // Start is called before the first frame update
    void Start()
    {
        seed = new System.Random().Next(-RandomSeedRange, RandomSeedRange);
        IsActive = true;
        worlds.Add(this);
        spawnPosition = new Vector3(.5f, Mathf.FloorToInt((biome.terrainHeight - biome.minTerrainHeight) * GetNoise(0, 0) + biome.minTerrainHeight) + 1.5f, .5f);
        // new Chunk(new ChunkCoord(-40, -9), this).BuildMesh();
        ChunkLoad(new ChunkCoord(this, 0, 0));
        
        chunkLoadThread = new Thread(ChunkLoadWorker);
        chunkLoadThread.Start();
        
        player.transform.position = player.lastPosition = spawnPosition;
        player.PlayerChunkCoord = new ChunkCoord(this, 0, 0);
    }

    private void Update()
    {
        CheckViewDistance();
        CheckLoadedChunk();
    }

    private void OnDestroy()
    {
        IsActive = false;
        worlds.Remove(this);
        chunkLoadThread.Abort();
    }

    private void CheckLoadedChunk()
    {
        while (loadedChunks.Count > 0)
        {
            Chunk chunk;
            if (!loadedChunks.TryDequeue(out chunk))
            {
                print("Loaded Chunk is null");
                continue;
            }
            
            chunks.Add(chunk.coord, chunk);
            
            if (Mathf.Abs(chunk.coord.x - player.PlayerChunkCoord.x) > viewDistance ||
                Mathf.Abs(chunk.coord.z - player.PlayerChunkCoord.z) > viewDistance)
                needMeshUpdateChunks.Add(chunk.coord);

            else
            {
                chunk.BuildMesh();
                activeChunks.Add(chunk);
            }
        }
    }

    private void CheckViewDistance()
    {
        ChunkCoord coord = GetChunkPosition(player.transform.position);
        if (player.PlayerChunkCoord != coord)
        {
            List<Chunk> despawnChunks = new List<Chunk>();
            foreach (var chunk in activeChunks)
            {
                if (Mathf.Abs(chunk.coord.x - coord.x) > viewDistance ||
                    Mathf.Abs(chunk.coord.z - coord.z) > viewDistance)
                    despawnChunks.Add(chunk);
            }

            foreach (var despawnChunk in despawnChunks)
            {
                ChunkDespawn(despawnChunk);
            }

            player.PlayerChunkCoord = coord;

            ChunkLoad(coord);
        }
    }

    public bool ChunkActivateInPos(Vector3 pos)
    {
        ChunkCoord coord = GetChunkPosition(pos);
        if (chunks.ContainsKey(coord))
        {
            return activeChunks.Contains(chunks[coord]);
        }

        return false;
    }

    private void ChunkLoadWorker()
    {
        while (IsActive)
        {
            while (!loadingChunks.IsEmpty)
            {
                ChunkCoord coord;
                if (!loadingChunks.TryDequeue(out coord))
                {
                    print("ChunkCoord is null");
                    continue;
                }
                
                if (chunks.ContainsKey(coord)) continue;
                
                Chunk chunk = new Chunk(coord, this);
                loadedChunks.Enqueue(chunk);
            }
        }
    }

    private void ChunkLoad(ChunkCoord pos)
    {
        for (int x = pos.x - viewDistance; x <= pos.x + viewDistance; x++)
        for (int z = pos.z - viewDistance; z <= pos.z + viewDistance; z++)
        {
            ChunkCoord coord = new ChunkCoord(this, x, z);

            if (chunks.ContainsKey(coord))
                ChunkSpawn(chunks[coord]);
            else
            {
                if (!loadingChunks.Contains(coord))
                {
                    loadingChunks.Enqueue(coord);
                }
            }
        }
    }

    private void ChunkSpawn(Chunk chunk)
    {
        if(needMeshUpdateChunks.Contains(chunk.coord))
            chunk.BuildMesh();
        
        chunk.IsActive = true;
        
        activeChunks.Add(chunk);
    }

    private void ChunkDespawn(Chunk chunk)
    {
        activeChunks.Remove(chunk);
        chunk.IsActive = false;
    }

    public float GetNoise(Vector3 pos)
    {
        return GetNoise(pos.x, pos.z);
    }

    public float GetNoise(float x, float z)
    {
        return Noise.Get2DPerlin(x + seed, z + seed, 0, biome.terrainScale);
    }

    public ChunkCoord GetChunkPosition(Vector3 pos)
    {
        return GetChunkPosition(pos.x, pos.z);
    }

    public ChunkCoord GetChunkPosition(float x, float z)
    {
        return new ChunkCoord(this, Mathf.FloorToInt(x / Chunk.ChunkWidth), Mathf.FloorToInt(z / Chunk.ChunkWidth));
    }

    public static Vector3 WorldPos2ChunkPos(Vector3 pos)
    {
        return WorldPos2ChunkPos(pos.x, pos.y, pos.z);
    }

    public static Vector3 WorldPos2ChunkPos(float x, float y, float z)
    {
        x %= 16;
        z %= 16;
        if (x < 0) x += Chunk.ChunkWidth;
        if (z < 0) z += Chunk.ChunkWidth;

        return new Vector3(x, y, z);
    }

    public static Vector3Int Vector3ToVector3Int(Vector3 pos)
    {
        return new Vector3Int((int) pos.x, (int) pos.y, (int) pos.z);
    }

    public BlockType GetBlock(Vector3 pos)
    {
        return GetBlock(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
    }

    public BlockType GetBlock(int x, int y, int z)
    {
        if (y < 0 || y >= Chunk.ChunkHeight) return BlockType.AIR;
        
        ChunkCoord coord = GetChunkPosition(x, z);
        if (chunks.ContainsKey(coord))
        {
            Vector3Int chunkPos = Vector3ToVector3Int(WorldPos2ChunkPos(x, y, z));
            return chunks[coord].blockData[chunkPos.x, chunkPos.y, chunkPos.z];
        }
        
        if (y == 0) return BlockType.BEDROCK;
        
        float noise = GetNoise(x, z);

        int terrainHeight = Mathf.FloorToInt((biome.terrainHeight - biome.minTerrainHeight) * noise);
        terrainHeight += biome.minTerrainHeight;
        
        foreach (Lode lode in biome.lodes)
        {
            if ((y >= lode.minHeight || lode.minHeight == -1) && (y <= lode.maxHeight || lode.maxHeight == -1))
            {
                if (Noise.Get3DPerlin(x + seed, y + seed, z + seed, lode.noiseOffset, lode.scale) >= noise + lode.threshold)
                    return lode.block;
            }
        }

        if (y < terrainHeight - 5 || y < 20) return BlockType.STONE;
        if (y < terrainHeight) return BlockType.DIRT;
        if (y <= terrainHeight) return BlockType.GRASS;

        return BlockType.AIR;
    }
}
