using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public bool devMode;


    public Enemy enemy;
    public Wave[] waves;

    int enemyRemaining;
    int enemiesAlive;
    float nextSpawnTime;
    int currentWave = -1;
    float timeBetweenCampingChecks = 2;
    float campThresholdDistance = 1.5f;
    float nextCampCheck;
    Vector3 campPositionOld;
    bool isCamping;
    bool isDisabled;

    Entity player;
    Transform playerT;

    MapGenerator map;

    public event System.Action<int> OnNewWave;

    void Start()
    {
        player = FindObjectOfType<Player>();
        playerT = player.transform;
        player.OnDeath += OnPlayerDeath;

        nextCampCheck = timeBetweenCampingChecks + Time.time;
        campPositionOld = playerT.position;

        map = FindObjectOfType<MapGenerator>();
        NextWave();
    }

    void Update()
    {
        if (!isDisabled)
        {
            if (Time.time > nextCampCheck)
            {
                nextCampCheck = Time.time + timeBetweenCampingChecks;

                isCamping = (Vector3.Distance(playerT.position, campPositionOld) < campThresholdDistance);
                campPositionOld = playerT.position;
            }

            if ((enemyRemaining > 0 || waves[currentWave].infinite) && Time.time > nextSpawnTime)
            {
                enemyRemaining--;
                nextSpawnTime = Time.time + waves[currentWave].timeBetweenSpawns;

                StartCoroutine("SpawnEnemy");
            }
        }
        if (devMode)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                StopCoroutine("SpawnEnemy");
                foreach (Enemy e in FindObjectsOfType<Enemy>())
                {
                    Destroy(e.gameObject);
                }
                NextWave();
            }
        }
    }

    IEnumerator SpawnEnemy()
    {
        float spawnDelay = 1;
        float tileFlashSpeed = 4;

        Transform tile = map.GetRandomOpenTile();
        if (isCamping)
        {
            tile = map.GetTileFromPosition(playerT.position);
        }
        Material tileMat = tile.GetComponent<Renderer>().material;
        Color initialColor = Color.white;
        Color flashColor = Color.red;
        float spawnTimer = 0;

        while (spawnTimer < spawnDelay)
        {
            tileMat.color = Color.Lerp(initialColor, flashColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));
            spawnTimer += Time.deltaTime;
            yield return null;
        }
        tileMat.color = initialColor;
        Enemy spawnedEnemy = Instantiate(enemy, tile.position + Vector3.up, Quaternion.identity) as Enemy;
        spawnedEnemy.OnDeath += OnEnemyDeath;
        spawnedEnemy.SetDifficulty(waves[currentWave].moveSpeed, waves[currentWave].hitPoints, waves[currentWave].enemyHealth, waves[currentWave].skinColor);
    }

    void OnEnemyDeath()
    {
        enemiesAlive--;

        if (enemiesAlive == 0)
        {
            NextWave();
        }
    }

    void OnPlayerDeath()
    {
        Cursor.visible = true;
        isDisabled = true;
    }

    void NextWave()
    {
        if (currentWave >= 0)
        {
            AudioManager.instance.PlaySound2D("LevelComplete");
        }
        currentWave++;
        if (currentWave < waves.Length)
        {
            enemyRemaining = waves[currentWave].enemyCount;
            enemiesAlive = enemyRemaining;

            if (OnNewWave != null)
            {
                OnNewWave(currentWave);
                playerT.position = map.GetTileFromPosition(Vector2.zero).position + Vector3.up * 3;
            }
        }
    }

    [System.Serializable]
    public class Wave
    {
        public bool infinite;

        public int enemyCount;
        public float timeBetweenSpawns;

        public float moveSpeed;
        public int hitPoints;
        public int enemyHealth;
        public Color skinColor;
    }

}
