using UnityEngine;

public class Block
{
    public BlockType Type { get; }
    public Vector3Int Location { get; }

    public float Strength => Type.GetStrength();

    public Block(BlockType type, Vector3Int location)
    {
        Type = type;
        Location = location;
    }
    public Block(BlockType type, int x, int y, int z) : this(type, new Vector3Int(x, y, z)) { }

    public static bool operator ==(Block block, BlockType type) => block.Type == type;
    public static bool operator !=(Block block, BlockType type) => !(block == type);
    public static bool operator ==(Block block, Block block2) => block2 is not null && block.Type == block2.Type && block.Location == block2.Location;
    public static bool operator !=(Block block, Block block2) => !(block == block2);

    public override string ToString()
    {
        return $"Block({Type}, {Location})";
    }
}
