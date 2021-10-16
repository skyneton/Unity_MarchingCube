public static class Extensions
{
    public static bool IsLiquid(this BlockType m)
    {
        return m == BlockType.WATER || m == BlockType.LAVA;
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
    [BlockValue(0f, -1)]
    AIR,
    [BlockValue(1.2f, 0)]
    GRASS,
    [BlockValue(0.9f, 1)]
    DIRT,
    [BlockValue(2f, 2)]
    LOG,
    [BlockValue(0.4f, 3)]
    LEAVES,
    [BlockValue(0f, 4)]
    BEDROCK,
    [BlockValue(0f, 5)]
    WATER,
    [BlockValue(0f, 6)]
    LAVA,
    [BlockValue(11f, 7)]
    STONE,
    [BlockValue(11f, 9)]
    COAL_ORE,
    [BlockValue(13f, 8)]
    IRON_ORE,
    [BlockValue(9.6f, 10)]
    GOLD_ORE,
}

// public enum BlockFace
// {
//     TOP,
//     BOTTOM,
//     SIDE,
// }