using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator2 : MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        ColorMap,
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

    public int seed;

    public Transform tilePrefab;
    public Transform groundPrefab;
    public Transform stairPrefab;

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

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.Draw(TextureGenerator.TextureFromNoiseMap(noiseMap));
        }
        else if (drawMode == DrawMode.ColorMap)
        {
            display.Draw(TextureGenerator.TextureFromColorMap(colorMap, width, height));
        }
        else if (drawMode == DrawMode.Map)
        {
            CreateMap();
        }
    }

    void CreateMap()
    {
        //creating map holder Object
        string holderName = "Generated Map";
        if (transform.FindChild(holderName))
        {
            DestroyImmediate(transform.FindChild(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;
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
                        if (point >= level[i].x && point <= level[i].y)
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
        float mean = Mathf.Lerp(0, 300, interpolation) * scaleMean.Evaluate(interpolation);
        float stdDev = scaleDev.Evaluate(interpolation) * 20 + 5;
        noiseScale = Mathf.Clamp(Utility.RandomGaussian(seed, mean, stdDev), 2, 300);

        //Octaves
        octaves = (int)(prng.NextDouble() * 3) + 3;

        //Persistance
        persistance = Mathf.Clamp(Utility.RandomGaussian(seed, 0.5f, 0.1f), 0, 1);

        //Lacunarity
        lacunarity = Mathf.Clamp(Utility.RandomGaussian(seed, 2, 0.25f), 1, 3);

        //Offset
        offset.x = (float)(prng.NextDouble() * 2000) - 1000;
        offset.y = (float)(prng.NextDouble() * 2000) - 1000;
    }
}
