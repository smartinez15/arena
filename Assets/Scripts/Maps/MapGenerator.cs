using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Transform tilePrefab;
    public Transform obstacleprefab;
    public Transform navMeshFloor;
    public Transform bound;

    [Range(0, 1)]
    public float outlinePercent;
    public float tileSize = 1;

    public Map[] maps;
    public int mapIndex;


    int maxWidth = 25;
    int maxHeight = 25;

    List<Coord> allTilesCoord;
    Queue<Coord> shuffleTile;
    Queue<Coord> shuffleOpenTile;
    Transform[,] tileMap;

    Map currentMap;

    void Start()
    {
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
    }

    void OnNewWave(int waveNumber)
    {
        mapIndex = waveNumber;
        GenerateMap();
    }

    public void GenerateMap()
    {
        currentMap = maps[mapIndex];
        tileMap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y];
        System.Random prng = new System.Random(currentMap.seed);
        GetComponent<BoxCollider>().size = new Vector3(currentMap.mapSize.x * tileSize, 0.05f, currentMap.mapSize.y * tileSize);

        //Generating Coords
        allTilesCoord = new List<Coord>();
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                allTilesCoord.Add(new Coord(x, y));
            }
        }
        shuffleTile = new Queue<Coord>(Utility.ShuffleArray(allTilesCoord.ToArray(), currentMap.seed));

        //creating map holder Object
        string holderName = "Generated Map";
        if (transform.FindChild(holderName))
        {
            DestroyImmediate(transform.FindChild(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        //Spawning tiles
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                Vector3 tilePosition = CoordToPosition(x, y);
                Transform tile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
                tile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                tile.parent = mapHolder;
                tileMap[x, y] = tile;
            }
        }

        //Spawning obstacles
        bool[,] obstacleMap = new bool[currentMap.mapSize.x, currentMap.mapSize.y];
        int currentObstacle = 0;
        int obstacleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent);
        List<Coord> openCoords = new List<Coord>(allTilesCoord);
        for (int i = 0; i < obstacleCount; i++)
        {
            Coord randomCoord = GetRandomCoord();
            obstacleMap[randomCoord.x, randomCoord.y] = true;
            currentObstacle++;
            if (randomCoord != currentMap.mapCenter && MapIsFullyAccesible(obstacleMap, currentObstacle))
            {
                float obstacleheight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float)prng.NextDouble());
                Vector3 obstaclePos = CoordToPosition(randomCoord.x, randomCoord.y);
                Transform newObstacle = Instantiate(obstacleprefab, obstaclePos + Vector3.up * obstacleheight / 2, Quaternion.identity);
                newObstacle.parent = mapHolder;
                newObstacle.localScale = new Vector3((1 - outlinePercent) * tileSize, obstacleheight, (1 - outlinePercent) * tileSize);

                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
                float colorpercent = randomCoord.y / (float)currentMap.mapSize.y;
                obstacleMaterial.color = Color.Lerp(currentMap.foreground, currentMap.background, colorpercent);
                obstacleRenderer.sharedMaterial = obstacleMaterial;

                openCoords.Remove(randomCoord);
            }
            else
            {
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacle--;
            }
        }

        shuffleOpenTile = new Queue<Coord>(Utility.ShuffleArray(openCoords.ToArray(), currentMap.seed));

        //Creating NavMesh mask
        Transform leftbound = Instantiate(bound, Vector3.left * (currentMap.mapSize.x + maxWidth) / 4f * tileSize, Quaternion.identity) as Transform;
        leftbound.parent = mapHolder;
        leftbound.localScale = new Vector3((maxWidth - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;
        Transform rightbound = Instantiate(bound, Vector3.right * (currentMap.mapSize.x + maxWidth) / 4f * tileSize, Quaternion.identity) as Transform;
        rightbound.parent = mapHolder;
        rightbound.localScale = new Vector3((maxWidth - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;
        Transform topbound = Instantiate(bound, Vector3.forward * (currentMap.mapSize.y + maxHeight) / 4f * tileSize, Quaternion.identity) as Transform;
        topbound.parent = mapHolder;
        topbound.localScale = new Vector3(maxWidth, 1, (maxHeight - currentMap.mapSize.y) / 2f) * tileSize;
        Transform bottombound = Instantiate(bound, Vector3.back * (currentMap.mapSize.y + maxHeight) / 4f * tileSize, Quaternion.identity) as Transform;
        bottombound.parent = mapHolder;
        bottombound.localScale = new Vector3(maxWidth, 1, (maxHeight - currentMap.mapSize.y) / 2f) * tileSize;

        navMeshFloor.localScale = new Vector3(maxWidth, maxHeight) * tileSize;
    }

    bool MapIsFullyAccesible(bool[,] obstacleMap, int obstacleCount)
    {
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(currentMap.mapCenter);
        mapFlags[currentMap.mapCenter.x, currentMap.mapCenter.y] = true;

        int accesible = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();

            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    int neighbourX = tile.x + x;
                    int neighbourY = tile.y + y;
                    if (x == 0 || y == 0)
                    {
                        if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) && neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1))
                        {
                            if (!mapFlags[neighbourX, neighbourY] && !obstacleMap[neighbourX, neighbourY])
                            {
                                mapFlags[neighbourX, neighbourY] = true;
                                queue.Enqueue(new Coord(neighbourX, neighbourY));
                                accesible++;
                            }
                        }
                    }
                }
            }
        }

        int targetAccessible = (currentMap.mapSize.x * currentMap.mapSize.y - obstacleCount);
        return targetAccessible == accesible;
    }

    Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3(-currentMap.mapSize.x / 2f + 0.5f + x, 0f, -currentMap.mapSize.y / 2f + 0.5f + y) * tileSize;
    }

    public Transform GetTileFromPosition(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / tileSize + (currentMap.mapSize.x - 1) / 2f);
        int y = Mathf.RoundToInt(position.z / tileSize + (currentMap.mapSize.y - 1) / 2f);
        x = Mathf.Clamp(x, 0, tileMap.GetLength(0) - 1);
        y = Mathf.Clamp(y, 0, tileMap.GetLength(1) - 1);
        return tileMap[x, y];
    }

    public Coord GetRandomCoord()
    {
        Coord random = shuffleTile.Dequeue();
        shuffleTile.Enqueue(random);
        return random;
    }

    public Transform GetRandomOpenTile()
    {
        Coord random = shuffleOpenTile.Dequeue();
        shuffleOpenTile.Enqueue(random);
        return tileMap[random.x, random.y];
    }

    [System.Serializable]
    public struct Coord
    {
        public int x;
        public int y;

        public Coord(int nX, int nY)
        {
            x = nX;
            y = nY;
        }

        public static bool operator ==(Coord a, Coord b)
        {
            return (a.x == b.x && a.y == b.y);
        }

        public static bool operator !=(Coord a, Coord b)
        {
            return !(a == b);
        }
    }

    [System.Serializable]
    public class Map
    {
        public Coord mapSize;
        [Range(0, 1)]
        public float obstaclePercent;
        public float minObstacleHeight;
        public float maxObstacleHeight;
        public int seed;
        public Color foreground;
        public Color background;

        public Coord mapCenter
        {
            get
            {
                return new Coord(mapSize.x / 2, mapSize.y / 2);
            }
        }
    }
}
