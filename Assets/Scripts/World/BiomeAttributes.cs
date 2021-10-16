using UnityEngine;

[CreateAssetMenu(fileName="BiomeAttributes", menuName="Biome Attribute")]
public class BiomeAttributes : ScriptableObject
{
    public string biomeName;

    public int terrainHeight;
    public int minTerrainHeight;
    public float terrainScale;

    public Lode[] lodes;
}

[System.Serializable]
public class Lode
{
    public string nodeName;
    public BlockType block;
    public int minHeight;
    public int maxHeight;
    public float scale;
    public float threshold;
    public float noiseOffset;
}