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

    public int levels;
    [Range(0, 1)]
    public float voidPercent;

    public Color floor;
    public Color top;

    private float noiseScale;
    private int octaves;
    [Range(0, 1)]
    private float persistance;
    private float lacunarity;
    private Vector2 offset;

    [Range(0, 1)]
    public float outlinePercent;
    public float stageSize;
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
        if (stageSize < 1)
        {
            stageSize = 1;
        }
    }

    public void GenerateMap()
    {
        GenerateVariables();

        float[,] noiseMap = Utility.GenerateNoiseMap(width, height, seed, noiseScale, octaves, persistance, lacunarity, offset);

        int[,] heightMap = CreateHeightMap(noiseMap);
        int[,] stairsMap = StairPlacement(ref heightMap);

        Color[] colorMap = CreateColorMap(heightMap);
        Color[] colorMapStairs = CreateColorMap(stairsMap);

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
            display.SculptMap(heightMap, stairsMap, colors, outlinePercent, stageSize);
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

    int[,] StairPlacement(ref int[,] heightMap)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        //Identify Stairs and Inner Stairs
        //-21 - pos X   |   -31 - pos X neg Z
        //-22 - neg Z   |   -32 - neg X neg Z 
        //-23 - neg X   |   -33 - neg X pos Z  
        //-24 - pos Z   |   -34 - pos X pos Z 
        int[,] stairsMap = new int[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (stairsMap[x, y] > -20)
                {
                    stairsMap[x, y] = heightMap[x, y];
                }
                if (heightMap[x, y] != -1)
                {
                    int n = 0;
                    bool posX, negX, posZ, negZ;
                    posX = negX = posZ = negZ = false;
                    if ((x < width - 1) && heightMap[x + 1, y] > heightMap[x, y])
                    {
                        n++;
                        posX = true;
                    }
                    if ((x > 0) && heightMap[x - 1, y] > heightMap[x, y])
                    {
                        n++;
                        negX = true;
                    }
                    if ((y < height - 1) && heightMap[x, y + 1] > heightMap[x, y])
                    {
                        n++;
                        posZ = true;
                    }
                    if ((y > 0) && heightMap[x, y - 1] > heightMap[x, y])
                    {
                        n++;
                        negZ = true;
                    }
                    bool correctAliasing = false;
                    if (n == 1)
                    {
                        if (posX)
                        {
                            correctAliasing = !(x > 0);
                            stairsMap[x, y] = -21;
                        }
                        else if (negX)
                        {
                            correctAliasing = !(x < width - 1);
                            stairsMap[x, y] = -23;
                        }
                        else if (posZ)
                        {
                            correctAliasing = !(y > 0);
                            stairsMap[x, y] = -24;
                        }
                        else if (negZ)
                        {
                            correctAliasing = !(y < height - 1);
                            stairsMap[x, y] = -22;
                        }
                    }
                    else if (n == 2)
                    {
                        if (posX && posZ)
                        {
                            correctAliasing = !(x > 0 && y > 0);
                            stairsMap[x, y] = -34;
                        }
                        else if (negX && posZ)
                        {
                            correctAliasing = !(x < width - 1 && y > 0);
                            stairsMap[x, y] = -33;
                        }
                        else if (posX && negZ)
                        {
                            correctAliasing = !(x > 0 && y < height - 1);
                            stairsMap[x, y] = -31;
                        }
                        else if (negX && negZ)
                        {
                            correctAliasing = !(x < width - 1 && y < height - 1);
                            stairsMap[x, y] = -32;
                        }
                    }
                    else if (n == 3)
                    {
                        correctAliasing = true;
                        if (!posX && (x < width - 1))
                        {
                            stairsMap[x + 1, y] = -23;
                        }
                        else if (!negX && x > 0)
                        {
                            stairsMap[x - 1, y] = -21;
                        }
                        else if (!posZ && (y < height - 1))
                        {
                            stairsMap[x, y + 1] = -22;
                        }
                        else if (!negZ && y > 0)
                        {
                            stairsMap[x, y - 1] = -24;
                        }
                    }

                    //Correct aliasing problems
                    if (correctAliasing)
                    {
                        heightMap[x, y]++;
                        stairsMap[x, y] = heightMap[x, y];
                    }
                }
            }
        }

        //Identify Outter Stairs
        //-41 - pos X neg Z
        //-42 - neg X neg Z
        //-43 - neg X pos Z
        //-44 - pos X pos Z
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool one = false;
                bool two = false;
                if (stairsMap[x, y] < -20 && stairsMap[x, y] > -40)
                {
                    if ((x < width - 1) && (y < height - 1) && stairsMap[x + 1, y + 1] < -20 && stairsMap[x + 1, y + 1] > -40)
                    {
                        //pos X pos Z
                        one = (heightMap[x + 1, y] == heightMap[x, y]);

                        two = (heightMap[x, y + 1] == heightMap[x, y]);

                        if (one && !two)
                        {
                            stairsMap[x + 1, y] = -41;
                        }
                        else if (!one && two)
                        {
                            stairsMap[x, y + 1] = -43;
                        }
                    }
                    if ((x < width - 1) && (y > 0) && stairsMap[x + 1, y - 1] < -20 && stairsMap[x + 1, y - 1] > -40)
                    {
                        //pos X neg Z
                        one = (heightMap[x + 1, y] == heightMap[x, y]);

                        two = (heightMap[x, y - 1] == heightMap[x, y]);

                        if (one && !two)
                        {
                            stairsMap[x + 1, y] = -42;
                        }
                        else if (!one && two)
                        {
                            stairsMap[x, y - 1] = -44;
                        }
                    }
                    if ((x > 0) && (y < height - 1) && stairsMap[x - 1, y + 1] < -20 && stairsMap[x - 1, y + 1] > -40)
                    {
                        //neg X pos Z
                        one = (heightMap[x - 1, y] == heightMap[x, y]);

                        two = (heightMap[x, y + 1] == heightMap[x, y]);

                        if (one && !two)
                        {
                            stairsMap[x - 1, y] = -44;
                        }
                        else if (!one && two)
                        {
                            stairsMap[x, y + 1] = -42;
                        }
                    }
                    if ((x > 0) && (y > 0) && stairsMap[x - 1, y - 1] < -20 && stairsMap[x - 1, y - 1] > -40)
                    {
                        //neg X neg Z
                        one = (heightMap[x - 1, y] == heightMap[x, y]);

                        two = (heightMap[x, y - 1] == heightMap[x, y]);

                        if (one && !two)
                        {
                            stairsMap[x - 1, y] = -41;
                        }
                        else if (!one && two)
                        {
                            stairsMap[x, y - 1] = -43;
                        }
                    }
                }
            }
        }
        return stairsMap;
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
                else if (heightMap[x, y] < -20 && heightMap[x, y] != 50)
                {
                    if (heightMap[x, y] < -40)
                    {
                        colorMap[y * width + x] = Color.red;
                    }
                    else if (heightMap[x, y] < -30)
                    {
                        colorMap[y * width + x] = Color.gray;
                    }
                    else
                    {
                        colorMap[y * width + x] = Color.white;
                    }
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
        noiseScale = Utility.RandomGaussian(seed, 40f, 5f);

        //Octaves
        octaves = (int)(prng.NextDouble() * 3) + 3;

        //Persistance
        persistance = Mathf.Clamp(Utility.RandomGaussian(seed, 0.5f, 0.15f), 0, 1);

        //Lacunarity
        lacunarity = 1;

        //Offset
        offset.x = (float)(prng.NextDouble() * 2000) - 1000;
        offset.y = (float)(prng.NextDouble() * 2000) - 1000;
    }
}
