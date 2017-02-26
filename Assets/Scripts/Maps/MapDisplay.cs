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

    public void SculptMap(int[,] heightMap, Color[] colors, float outline)
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
                    //Create Tiles
                    //Front
                    Transform fTile = Instantiate(tilePrefab);
                    fTile.localScale = Vector3.one * (1 - outline);
                    fTile.localPosition = new Vector3(0, altura - 0.5f, 0.501f);
                    fTile.localEulerAngles = new Vector3(180, 0, 0);
                    fTile.SetParent(block, true);
                    fTile.localPosition = new Vector3(0, fTile.localPosition.y, 0.501f);
                    fTile.GetComponent<Renderer>().material = mats[altura - 1];
                    //Back
                    Transform bTile = Instantiate(tilePrefab);
                    bTile.localScale = Vector3.one * (1 - outline);
                    bTile.localPosition = new Vector3(0, altura - 0.5f, -0.501f);
                    bTile.localEulerAngles = new Vector3(0, 0, 0);
                    bTile.SetParent(block, true);
                    bTile.localPosition = new Vector3(0, bTile.localPosition.y, -0.501f);
                    bTile.GetComponent<Renderer>().material = mats[altura - 1];
                    //Right
                    Transform rTile = Instantiate(tilePrefab);
                    rTile.localScale = Vector3.one * (1 - outline);
                    rTile.localPosition = new Vector3(0.501f, altura - 0.5f, 0);
                    rTile.localEulerAngles = new Vector3(0, -90, 0);
                    rTile.SetParent(block, true);
                    rTile.localPosition = new Vector3(0.501f, rTile.localPosition.y, 0);
                    rTile.GetComponent<Renderer>().material = mats[altura - 1];
                    //Left
                    Transform lTile = Instantiate(tilePrefab);
                    lTile.localScale = Vector3.one * (1 - outline);
                    lTile.localPosition = new Vector3(-0.501f, altura - 0.5f, 0);
                    lTile.localEulerAngles = new Vector3(0, 90, 0);
                    lTile.SetParent(block, true);
                    lTile.localPosition = new Vector3(-0.501f, lTile.localPosition.y, 0);
                    lTile.GetComponent<Renderer>().material = mats[altura - 1];
                    //Top
                    Transform tTile = Instantiate(tilePrefab);
                    tTile.parent = block;
                    tTile.localScale = Vector3.one * (1 - outline);
                    tTile.localPosition = new Vector3(0, 0.501f, 0);
                    tTile.localEulerAngles = new Vector3(90, 0, 0);
                    tTile.GetComponent<Renderer>().material = mats[altura - 1];
                }
            }
        }
    }

    Vector3 CoordToPosition(int x, int i, int y)
    {
        return new Vector3(-mapWidth / 2f + 0.5f + x, i / 2f, -mapHeight / 2f + 0.5f + y);
    }
}
