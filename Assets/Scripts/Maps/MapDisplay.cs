using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public Renderer textureRenderer;
    public Transform tilePrefab;
    public Transform groundPrefab;
    public Transform stairPrefab;
    public Transform innerStairPrefab;
    public Transform outterStairPrefab;
    public Transform stairColliderPrefab;
    public Transform innerStairColliderPrefab;
    public Transform outterStairColliderPrefab;

    public Material[] tileMats;

    private int mapHeight;
    private int mapWidth;

    public void Draw(Texture2D texture)
    {
        string holderName = "Generated Map";
        if (transform.FindChild(holderName))
        {
            DestroyImmediate(transform.FindChild(holderName).gameObject);
        }

        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width / 10f, 1, texture.height / 10f);
    }

    public void SculptMap(int[,] heightMap, int[,] stairsMap, Color[] colors, float outline, float stageSize)
    {
        textureRenderer.transform.localScale = Vector3.zero;

        //creating map holder Object
        string holderName = "Generated Map";
        if (transform.FindChild(holderName))
        {
            DestroyImmediate(transform.FindChild(holderName).gameObject);
        }
        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        mapWidth = heightMap.GetLength(0);
        mapHeight = heightMap.GetLength(1);

        //Creating tiles materials
        for (int i = 0; i < colors.Length && i < tileMats.Length; i++)
        {
            tileMats[i].color = colors[i];
        }

        //Creating Ground Blocks
        for (int i = 0; i < colors.Length; i++)
        {
            bool[,] added = new bool[mapWidth, mapHeight];
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    //Check Ground
                    if (heightMap[x, y] >= i && !added[x, y])
                    {
                        //Search for the maximun block that can be put
                        int xIn = x;
                        int yIn = y;
                        int xFi = xIn;
                        int yFi = yIn;
                        bool up = true;
                        bool side = true;
                        added[xIn, yIn] = true;
                        while (up || side)
                        {
                            List<string> changes = new List<string>();
                            if (side)
                            {
                                side = (xFi + 1) < mapWidth;
                                for (int s = yIn; s <= yFi && side; s++)
                                {
                                    if (heightMap[xFi + 1, s] < i || added[xFi + 1, s])
                                    {
                                        side = false;
                                    }
                                    else
                                    {
                                        changes.Add((xFi + 1) + ":" + s);
                                    }
                                }
                                if (side)
                                {
                                    xFi++;
                                    for (int c = 0; c < changes.Count; c++)
                                    {
                                        string ch = changes[c];
                                        added[int.Parse(ch.Split(':')[0]), int.Parse(ch.Split(':')[1])] = true;
                                    }
                                }
                            }
                            changes.Clear();
                            if (up)
                            {
                                up = (yFi + 1) < mapHeight;
                                for (int s = xIn; s <= xFi && up; s++)
                                {
                                    if (heightMap[s, yFi + 1] < i || added[s, yFi + 1])
                                    {
                                        up = false;
                                    }
                                    else
                                    {
                                        changes.Add(s + ":" + (yFi + 1));
                                    }
                                }
                                if (up)
                                {
                                    yFi++;
                                    for (int c = 0; c < changes.Count; c++)
                                    {
                                        string ch = changes[c];
                                        added[int.Parse(ch.Split(':')[0]), int.Parse(ch.Split(':')[1])] = true;
                                    }
                                }
                            }
                            changes.Clear();
                        }
                        //Put the block
                        Transform block = Instantiate(groundPrefab, CoordToPosition(xIn + (1 + xFi - xIn) / 2f, i, yIn + (1 + yFi - yIn) / 2f), Quaternion.identity);
                        block.localScale = new Vector3((1 + xFi - xIn), 1, (1 + yFi - yIn));
                        block.parent = mapHolder;
                    }

                    //Check Stairs
                    if (i == 0 && stairsMap[x, y] < -20)
                    {
                        int altura = heightMap[x, y] + 1;
                        int stair = stairsMap[x, y];
                        Transform stairT = null;
                        if (stair < -40)
                        {
                            stairT = Instantiate(outterStairPrefab);
                            Transform son = Instantiate(outterStairColliderPrefab);
                            son.parent = stairT;
                        }
                        else if (stair < -30)
                        {
                            stairT = Instantiate(innerStairPrefab);
                            Transform son = Instantiate(innerStairColliderPrefab);
                            son.parent = stairT;
                        }
                        else
                        {
                            stairT = Instantiate(stairPrefab);
                            Transform son = Instantiate(stairColliderPrefab);
                            son.parent = stairT;
                        }
                        stairT.position = CoordToPosition(x + 0.5f, altura, y + 0.5f);
                        stairT.parent = mapHolder;
                        int rot = ((-1) * stair) % 10;
                        switch (rot)
                        {
                            case 1:
                                stairT.eulerAngles = new Vector3(0f, 0f, 0f);
                                break;
                            case 2:
                                stairT.eulerAngles = new Vector3(0f, 90f, 0f);
                                break;
                            case 3:
                                stairT.eulerAngles = new Vector3(0f, 180f, 0f);
                                break;
                            case 4:
                                stairT.eulerAngles = new Vector3(0f, 270f, 0f);
                                break;
                        }
                    }
                    //Putting tiles
                    else if (i == 0 && heightMap[x, y] != -1)
                    {
                        int altura = heightMap[x, y];
                        Transform tile = Instantiate(tilePrefab);
                        tile.SetParent(mapHolder);
                        tile.localScale = tile.localScale * (1 - outline);
                        tile.position = CoordToPosition(x, altura, y) + new Vector3(0.5f, 0.51f, 0.5f);
                        tile.GetComponent<Renderer>().material = tileMats[Mathf.Clamp(altura, 0, 4)];
                    }
                }
            }
        }

        //Putting stage walls
        Transform wallFront = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
        wallFront.SetParent(mapHolder);
        wallFront.localScale = new Vector3(mapWidth, 20f, 1f);
        wallFront.localPosition = new Vector3(0.5f, 10f, mapHeight / 2f + 0.5f);

        Transform wallBack = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
        wallBack.SetParent(mapHolder);
        wallBack.localEulerAngles = Vector3.up * 180;
        wallBack.localScale = new Vector3(mapWidth, 20f, 1f);
        wallBack.localPosition = new Vector3(0.5f, 10f, -1 * (mapHeight / 2f - 0.5f));

        Transform wallRight = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
        wallRight.SetParent(mapHolder);
        wallRight.localEulerAngles = Vector3.up * 90;
        wallRight.localScale = new Vector3(mapHeight, 20f, 1f);
        wallRight.localPosition = new Vector3((mapWidth / 2f + 0.5f), 10f, 0.5f);

        Transform wallLeft = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
        wallLeft.SetParent(mapHolder);
        wallLeft.localEulerAngles = Vector3.up * -90;
        wallLeft.localScale = new Vector3(mapHeight, 20f, 1f);
        wallLeft.localPosition = new Vector3(-1 * (mapWidth / 2f - 0.5f), 10f, 0.5f);

        mapHolder.localScale = Vector3.one * stageSize;
    }

    Vector3 CoordToPosition(float x, int i, float y)
    {
        return new Vector3(-mapWidth / 2f + 0.5f + x, i - 0.5f, -mapHeight / 2f + 0.5f + y);
    }
}
