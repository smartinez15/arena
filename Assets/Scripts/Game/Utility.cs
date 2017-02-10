using System.Collections;
using UnityEngine;

public static class Utility
{
    public static T[] ShuffleArray<T>(T[] array, int seed)
    {
        System.Random prng = new System.Random(seed);

        for (int i = 0; i < array.Length - 1; i++)
        {
            int randomIndex = prng.Next(i, array.Length);
            T temp = array[randomIndex];
            array[randomIndex] = array[i];
            array[i] = temp;
        }

        return array;
    }

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float maxNoise = float.MinValue;
        float minNoise = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1;
                float frecuency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth) / scale * frecuency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / scale * frecuency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frecuency *= lacunarity;
                }
                if (noiseHeight > maxNoise)
                {
                    maxNoise = noiseHeight;
                }
                else if (noiseHeight < minNoise)
                {
                    minNoise = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoise, maxNoise, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }

    public static float RandomGaussian(int seed, float mean, float stdDev)
    {
        System.Random r = new System.Random(seed);

        float u = 1;
        float v;
        float S = 1;

        while (S >= 1.0)
        {
            u = 2.0f * (float)r.NextDouble() - 1.0f;
            v = 2.0f * (float)r.NextDouble() - 1.0f;
            S = u * u + v * v;
        }

        float fac = Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);
        return ((u * fac) * stdDev) + mean;
    }
}
