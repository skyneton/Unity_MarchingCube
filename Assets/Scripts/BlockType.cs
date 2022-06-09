public static class Extensions
{
    public static bool IsLiquid(this BlockType m)
    {
        return m == BlockType.Water || m == BlockType.Lava;
    }

    public static float GetStrength(this BlockType m)
    {
        BlockValue[] blockValues =
            m.GetType().GetField(m.ToString()).GetCustomAttributes(typeof(BlockValue), false) as BlockValue[];
        if (blockValues.Length > 0) return blockValues[0].Strength;
        return 0;
    }

    public static int GetTextureID(this BlockType m)
    {
        BlockValue[] blockValues =
            m.GetType().GetField(m.ToString()).GetCustomAttributes(typeof(BlockValue), false) as BlockValue[];
        if (blockValues.Length == 0) return 0;

        return blockValues[0].faceTexture;
    }
}

[System.Serializable]
public class BlockValue : System.Attribute
{
    public float Strength { get; private set; }
    
    public int faceTexture;
    
    public BlockValue(float strength, int texture)
    {
        Strength = strength;

        faceTexture = texture;
    }
}
[System.Serializable]
public enum BlockType
{
    [BlockValue(-1, -1)]
    Air,
    [BlockValue(1.2f, 0)]
    Grass,
    [BlockValue(0.9f, 1)]
    Dirt,
    [BlockValue(2f, 2)]
    Log,
    [BlockValue(0.4f, 3)]
    Leaves,
    [BlockValue(-1, 4)]
    Bedrock,
    [BlockValue(-1, 5)]
    Water,
    [BlockValue(-1, 6)]
    Lava,
    [BlockValue(11f, 7)]
    Stone,
    [BlockValue(11f, 9)]
    CoalOre,
    [BlockValue(13f, 8)]
    IronOre,
    [BlockValue(9.6f, 10)]
    GoldOre,
}

// public enum BlockFace
// {
//     TOP,
//     BOTTOM,
//     SIDE,
// }