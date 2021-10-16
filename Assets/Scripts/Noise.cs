using UnityEngine;

public class Noise
{
    public static float Get2DPerlin(Vector2 pos, float offset, float scale)
    {
        return Mathf.PerlinNoise(pos.x * scale + offset, pos.y * scale + offset);
    }
    public static float Get2DPerlin(float x, float y, float offset, float scale)
    {
        return Mathf.PerlinNoise(x * scale + offset,y * scale + offset);
    }

    // public static float Get3DPerlin(Vector3 pos, float offset, float scale)
    // {
    //     float x = (pos.x + 0.1f) * scale + offset;
    //     float y = (pos.y + 0.1f) * scale + offset;
    //     float z = (pos.z + 0.1f) * scale + offset;
    //
    //     float AB = Mathf.PerlinNoise(x, y);
    //     float BC = Mathf.PerlinNoise(y, z);
    //     float AC = Mathf.PerlinNoise(x, z);
    //     float BA = Mathf.PerlinNoise(y, x);
    //     float CB = Mathf.PerlinNoise(z, y);
    //     float CA = Mathf.PerlinNoise(z, x);
    //
    //     return (AB + BC + AC + BA + CB + CA) / 6f;
    // }

    public static float Get3DPerlin(float x, float y, float z, float offset, float scale)
    {
        x = (x + 0.1f) * scale + offset;
        y = (y + 0.1f) * scale + offset;
        z = (z + 0.1f) * scale + offset;

        float AB = Mathf.PerlinNoise(x, y);
        float BC = Mathf.PerlinNoise(y, z);
        // float AC = Mathf.PerlinNoise(x, z);
        //
        // float BA = Mathf.PerlinNoise(y, x);
        // float CB = Mathf.PerlinNoise(z, y);
        float CA = Mathf.PerlinNoise(z, x);

        return (AB + BC + CA) / 3f;
    }
}