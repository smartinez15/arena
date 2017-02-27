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

    int mapHeight;
    int mapWidth;

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
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (heightMap[x, y] != -1)
                {
                    //Create Block
                    int altura = heightMap[x, y] + 1;
                    Transform block = Instantiate(groundPrefab, CoordToPosition(x, altura, y), Quaternion.identity);
                    block.localScale = new Vector3(1, altura, 1);
                    block.parent = mapHolder;
                    //Check Stairs
                    if (stairsMap[x, y] > -20)
                    {
                        //Create Top Tile
                        Transform tTile = Instantiate(tilePrefab);
                        tTile.parent = block;
                        tTile.localScale = Vector3.one * (1 - outline);
                        tTile.localPosition = new Vector3(0, 0.501f, 0);
                        tTile.localEulerAngles = new Vector3(90, 0, 0);
                        tTile.GetComponent<Renderer>().material = mats[altura - 1];
                    }
                    else
                    {
                        //Create Stairs
                        int stair = stairsMap[x, y];
                        Transform stairT = null;
                        if (stair < -40)
                        {
                            stairT = Instantiate(outterStairPrefab);
                        }
                        else if (stair < -30)
                        {
                            stairT = Instantiate(innerStairPrefab);
                        }
                        else
                        {
                            stairT = Instantiate(stairPrefab);
                        }
                        Vector3 pos = CoordToPosition(x, altura + 1, y);
                        stairT.position = new Vector3(pos.x, altura + 0.5f, pos.z);
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
                }
            }
        }
    }

    Vector3 CoordToPosition(int x, int i, int y)
    {
        return new Vector3(-mapWidth / 2f + 0.5f + x, i / 2f, -mapHeight / 2f + 0.5f + y);
    }
}
