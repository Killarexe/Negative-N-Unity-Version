using UnityEngine;

public static class Noise
{
    public static float get2DPerlin(Vector2 pos, float offset, float scale)
    {
        float x = ((pos.x + offset + 0.1f) * scale) / World.getChunkWidth;
        float y = ((pos.y + offset + 0.1f) * scale) / World.getChunkHeight;
        return Mathf.PerlinNoise((pos.x + 0.1f) / World.getChunkWidth * scale + offset, (pos.y + 0.1f) / World.getChunkWidth * scale + offset);
    }

    public static bool get3DPerlin(Vector3 pos, float offset, float scale, float threshold)
    {
        float x = (pos.x + offset + 0.1f) * scale;
        float y = (pos.y + offset + 0.1f) * scale;
        float z = (pos.z + offset + 0.1f) * scale;

        float AB = Mathf.PerlinNoise(x, y);
        float BC = Mathf.PerlinNoise(y, z);
        float AC = Mathf.PerlinNoise(x, z);
        float BA = Mathf.PerlinNoise(y, x);
        float CB = Mathf.PerlinNoise(z, y);
        float CA = Mathf.PerlinNoise(z, x);

        if ((AB + BC + AC + BA + CB + CA) / 6f > threshold)
            return true;
        else
            return false;
    }
}
