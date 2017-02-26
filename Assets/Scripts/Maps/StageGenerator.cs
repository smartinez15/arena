using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageGenerator : MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        ColorMap,
        ColorMapStairs,
        Map
    };
    public DrawMode drawMode;

    public int width;
    public int height;

    public AnimationCurve scaleMean;
    public AnimationCurve scaleDev;

    public int levels;
    [Range(0, 1)]
    public float voidPercent;

    public Color floor;
    public Color top;

    public float noiseScale;
    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;
    public Vector2 offset;

    [Range(0, 1)]
    public float outlinePercent;
    public int seed;

    void OnValidate()
    {
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }
        if (height < 1)
        {
            height = 1;
        }
        if (width < 1)
        {
            width = 1;
        }
    }

    public void GenerateMap()
    {
        GenerateVariables();

        float[,] noiseMap = Utility.GenerateNoiseMap(width, height, seed, noiseScale, octaves, persistance, lacunarity, offset);

        int[,] heightMap = CreateHeightMap(noiseMap);

        Color[] colorMap = CreateColorMap(heightMap);
        Color[] colorMapStairs = CreateColorMap(StairPlacement(heightMap));

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.Draw(TextureGenerator.TextureFromNoiseMap(noiseMap));
        }
        else if (drawMode == DrawMode.ColorMap)
        {
            display.Draw(TextureGenerator.TextureFromColorMap(colorMap, width, height));
        }
        else if (drawMode == DrawMode.ColorMapStairs)
        {
            display.Draw(TextureGenerator.TextureFromColorMap(colorMapStairs, width, height));
        }
        else if (drawMode == DrawMode.Map)
        {
            Color[] colors = CreateGradiant();
            display.SculptMap(heightMap, colors, outlinePercent);
        }
    }

    Color[] CreateGradiant()
    {
        Color[] colors = new Color[levels];
        for (int i = 0; i < levels; i++)
        {
            float interpolation = i / (float)(levels <= 1 ? 1 : (levels - 1));
            colors[i] = Color.Lerp(floor, top, interpolation);
        }
        return colors;
    }

    int[,] CreateHeightMap(float[,] noiseMap)
    {
        int[,] heightMap = new int[noiseMap.GetLength(0), noiseMap.GetLength(1)];
        float levelRange = (1f - voidPercent) / levels;
        Vector2[] level = new Vector2[levels];
        for (int i = 0; i < level.Length; i++)
        {
            level[i].x = voidPercent + (i * levelRange);
            level[i].y = level[i].x + levelRange;
        }
        level[levels - 1].y = 1;

        for (int y = 0; y < heightMap.GetLength(1); y++)
        {
            for (int x = 0; x < heightMap.GetLength(0); x++)
            {
                float point = noiseMap[x, y];
                if (point < voidPercent)
                {
                    heightMap[x, y] = -1;
                }
                else
                {
                    for (int i = 0; i < level.Length; i++)
                    {
                        if (point > level[i].x && point <= level[i].y)
                        {
                            heightMap[x, y] = i;
                            break;
                        }
                    }
                }
            }
        }
        return heightMap;
    }

    int[,] StairPlacement(int[,] heightMap)
    {
        int[,] stairPlaced = new int[heightMap.GetLength(0), heightMap.GetLength(1)];

        for (int y = 0; y < heightMap.GetLength(1); y++)
        {
            for (int x = 0; x < heightMap.GetLength(0); x++)
            {
                stairPlaced[x, y] = heightMap[x, y];
                if ((x > 0) && heightMap[x, y] < heightMap[x - 1, y] && heightMap[x, y] != -1 && heightMap[x - 1, y] != -1)
                {
                    stairPlaced[x, y] = -2;
                }
            }
        }
        for (int x = 0; x < heightMap.GetLength(0); x++)
        {
            for (int y = 0; y < heightMap.GetLength(1); y++)
            {
                if ((y > 0) && heightMap[x, y] < heightMap[x, y - 1] && heightMap[x, y] != -1 && heightMap[x, y - 1] != -1)
                {
                    stairPlaced[x, y] = -2;
                }
            }
        }
        for (int y = heightMap.GetLength(1) - 1; y >= 0; y--)
        {
            for (int x = heightMap.GetLength(0) - 1; x >= 0; x--)
            {
                if ((x < heightMap.GetLength(0) - 1) && heightMap[x, y] < heightMap[x + 1, y] && heightMap[x, y] != -1 && heightMap[x + 1, y] != -1)
                {
                    stairPlaced[x, y] = -2;
                }
            }
        }
        for (int x = heightMap.GetLength(0) - 1; x >= 0; x--)
        {
            for (int y = heightMap.GetLength(1) - 1; y >= 0; y--)
            {
                if ((y < heightMap.GetLength(1) - 1) && heightMap[x, y] < heightMap[x, y + 1] && heightMap[x, y] != -1 && heightMap[x, y + 1] != -1)
                {
                    stairPlaced[x, y] = -2;
                }
            }
        }
        return stairPlaced;
    }

    Color[] CreateColorMap(int[,] heightMap)
    {
        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (heightMap[x, y] == -1)
                {
                    colorMap[y * width + x] = Color.black;
                }
                else if (heightMap[x, y] == -2)
                {
                    colorMap[y * width + x] = Color.white;
                }
                else if (heightMap[x, y] == -5)
                {
                    colorMap[y * width + x] = Color.yellow;
                }
                else
                {
                    float interpolation = heightMap[x, y] / (float)(levels <= 1 ? 1 : (levels - 1));
                    colorMap[y * width + x] = Color.Lerp(floor, top, interpolation);
                }
            }
        }
        return colorMap;
    }

    void GenerateVariables()
    {
        System.Random prng = new System.Random(seed);
        //Noise Scale
        float interpolation = ((width * height) - 25) / (float)(40000 - 25);
        float mean = Mathf.Lerp(5, 300, interpolation) * scaleMean.Evaluate(interpolation);
        float stdDev = scaleDev.Evaluate(interpolation) * 20 + 5;
        //noiseScale = Mathf.Clamp(Utility.RandomGaussian(seed, mean, stdDev), 2, 300);
        noiseScale = 40;

        //Octaves
        octaves = (int)(prng.NextDouble() * 3) + 3;

        //Persistance
        persistance = Mathf.Clamp(Utility.RandomGaussian(seed, 0.5f, 0.15f), 0, 1);

        //Lacunarity
        //lacunarity = Mathf.Clamp(Utility.RandomGaussian(seed, 2, 0.25f), 1, 3);
        lacunarity = 1;

        //Offset
        offset.x = (float)(prng.NextDouble() * 2000) - 1000;
        offset.y = (float)(prng.NextDouble() * 2000) - 1000;
    }
}
