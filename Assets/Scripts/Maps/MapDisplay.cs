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

    public Shader tileShader;

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

    public void SculptMap(int[,] heightMap, int[,] stairsMap, Color[] colors, float outline)
    {
        textureRenderer.transform.localScale = Vector3.zero;

        //Create Materials
        Material[] mats = new Material[colors.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            mats[i] = new Material(tileShader);
            mats[i].color = colors[i];
        }

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
        //Creating Ground Blocks
        for (int i = 0; i < mats.Length; i++)
        {
            bool[,] added = new bool[mapWidth, mapHeight];
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
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
                                    added[xFi + 1, s] = true;
                                    changes.Add((xFi + 1) + ":" + s);
                                }
                                if (side)
                                {
                                    xFi++;
                                }
                                else
                                {
                                    for (int c = 0; c > changes.Count; c++)
                                    {
                                        string ch = changes[i];
                                        added[int.Parse(ch.Split(':')[0]), int.Parse(ch.Split(':')[1])] = false;
                                    }
                                    changes.Clear();
                                }
                            }
                            if (up)
                            {
                                up = (yFi + 1) < mapHeight;
                                for (int s = xIn; s <= xFi && up; s++)
                                {
                                    if (heightMap[s, yFi + 1] < i || added[s, yFi + 1])
                                    {
                                        up = false;
                                    }
                                    added[s, yFi + 1] = true;
                                    changes.Add(s + ":" + (yFi + 1));
                                }
                                if (up)
                                {
                                    yFi++;
                                }
                                else
                                {
                                    for (int c = 0; c > changes.Count; c++)
                                    {
                                        string ch = changes[i];
                                        added[int.Parse(ch.Split(':')[0]), int.Parse(ch.Split(':')[1])] = false;
                                    }
                                    changes.Clear();
                                }
                            }
                        }
                        //Put the block
                        Transform block = Instantiate(groundPrefab, CoordToPosition(xIn + (1 + xFi - xIn) / 2f, i, yIn + (1 + yFi - yIn) / 2f), Quaternion.identity);
                        block.localScale = new Vector3((1 + xFi - xIn), 1, (1 + yFi - yIn));
                        block.parent = mapHolder;
                    }
                }
            }
        }
        //Putting Tiles and Stairs
        //for (int y = 0; y < mapHeight; y++)
        //{
        //    for (int x = 0; x < mapWidth; x++)
        //    {
        //        if (heightMap[x, y] != -1)
        //        {
        //            //Create Block
        //            int altura = heightMap[x, y] + 1;
        //            /*Transform block = Instantiate(groundPrefab, CoordToPosition(x, altura, y), Quaternion.identity);
        //            block.localScale = new Vector3(1, altura, 1);
        //            block.parent = mapHolder;*/
        //            //Check Stairs
        //            if (stairsMap[x, y] > -20)
        //            {
        //                //Create Top Tile
        //                Transform tTile = Instantiate(tilePrefab);
        //                tTile.parent = mapHolder;
        //                tTile.localScale = Vector3.one * (1 - outline);
        //                tTile.localPosition = CoordToPosition(x, 0, y) + (Vector3.up * (altura - 1)) + (Vector3.up * 1.001f);
        //                tTile.localEulerAngles = new Vector3(90, 0, 0);
        //                tTile.GetComponent<Renderer>().material = mats[altura - 1];
        //            }
        //            else
        //            {
        //                //Create Stairs
        //                int stair = stairsMap[x, y];
        //                Transform stairT = null;
        //                if (stair < -40)
        //                {
        //                    stairT = Instantiate(outterStairPrefab);
        //                }
        //                else if (stair < -30)
        //                {
        //                    stairT = Instantiate(innerStairPrefab);
        //                }
        //                else
        //                {
        //                    stairT = Instantiate(stairPrefab);
        //                }
        //                Vector3 pos = CoordToPosition(x, altura + 1, y);
        //                stairT.position = new Vector3(pos.x, altura + 0.5f, pos.z);
        //                stairT.parent = mapHolder;
        //                int rot = ((-1) * stair) % 10;
        //                switch (rot)
        //                {
        //                    case 1:
        //                        stairT.eulerAngles = new Vector3(0f, 0f, 0f);
        //                        break;
        //                    case 2:
        //                        stairT.eulerAngles = new Vector3(0f, 90f, 0f);
        //                        break;
        //                    case 3:
        //                        stairT.eulerAngles = new Vector3(0f, 180f, 0f);
        //                        break;
        //                    case 4:
        //                        stairT.eulerAngles = new Vector3(0f, 270f, 0f);
        //                        break;
        //                }
        //            }
        //        }
        //    }
        //}
    }

    Vector3 CoordToPosition(float x, int i, float y)
    {
        return new Vector3(-mapWidth / 2f + 0.5f + x, i - 0.5f, -mapHeight / 2f + 0.5f + y);
    }
}
