using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public bool devMode;
    public Wave[] waves;
    public Enemy enemy;

    private LivingEntity playerEntity;
    private Transform playerT;

    private Wave currentWave;
    private int currentWaveNumber;

    private int enemiseRemainingToSpawn;//剩余的数量
    private int enemiseRemainingAlive;
    private float nextSpawnTime;

    [SerializeField] private MapGenerate map;

    private float timeBetweenCampingCheck = 2;
    private float campThresholdDistance = 1.5f;
    private float nextCampCheckTime;
    private Vector3 campPositionOld;
    private bool isComping;

    public event Action<int> OnNewWave; 

    private bool isDisable;
    private void Start()
    {
        playerEntity = FindObjectOfType<Player>();
        playerEntity.onDeath += OnPlayerDeath;
        playerT = playerEntity.transform;
        NextWave();
    }

    private void Update()
    {
        if (!isDisable)
        {
            if (Time.time > nextCampCheckTime)
            {
                nextCampCheckTime = Time.time + timeBetweenCampingCheck;

                isComping = (Vector3.Distance(playerT.position, campPositionOld) < campThresholdDistance);
                campPositionOld = playerT.position;
            }

            if ((enemiseRemainingToSpawn > 0 || currentWave.infinite) && Time.time > nextSpawnTime)
            {
                enemiseRemainingToSpawn--;
                nextSpawnTime = Time.time + currentWave.timeBetweenSpawn;

                StartCoroutine(nameof(SpawnEnemy));
            }
        }

        if (devMode)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StopCoroutine(nameof(SpawnEnemy));
                Enemy[] enemies = FindObjectsOfType<Enemy>();
                for (int i = 0; i < enemies.Length; i++)
                {
                    Destroy(enemies[i].gameObject);
                }
                NextWave();
            }
        }
    }

    IEnumerator SpawnEnemy()
    {
        float spawnDelay = 1;
        float tileFlashSpeed = 4;

        Transform randomTile = map.GetRandomOpenTile();
        if (isComping)
        {
            randomTile = map.GetTileFromPosition(playerT.position);
        }
        Material tileMat = randomTile.GetComponent<Renderer>().material;
        Color initialColor = Color.white;
        Color flashColor = Color.red;
        float spawnTimer = 0;

        while (spawnTimer < spawnDelay)
        {
            tileMat.color = Color.Lerp(initialColor, flashColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));
            spawnTimer += Time.deltaTime;
            yield return null;
        }
        
        Enemy enemy = Instantiate(this.enemy, randomTile.position + Vector3.up, Quaternion.identity);
        enemy.onDeath += OnDeath;

        enemy.SetCharacteristics(currentWave.moveSpeed, currentWave.hitsToKillPlayer, currentWave.enemyHealth, currentWave.skinColor);
    }

    void OnPlayerDeath()
    {
        isDisable = true;
    }

    void OnDeath()
    {
        enemiseRemainingAlive--;
        if (enemiseRemainingAlive <= 0 && !currentWave.infinite)
        {
            NextWave();
        }
    }
    void NextWave()
    {
        if (currentWaveNumber > 0)
        {
            AudioManager.instance.PlaySound2D("Level Complete");
        }
        currentWaveNumber++;
        if (currentWaveNumber - 1 < waves.Length)
        {
            currentWave = waves[currentWaveNumber - 1];
            enemiseRemainingAlive = currentWave.enemyCount;
            enemiseRemainingToSpawn = currentWave.enemyCount;
            
            OnNewWave?.Invoke(currentWaveNumber);
        }

        ResetPlayer();
    }

    void ResetPlayer()
    {
        playerT.position = map.GetTileFromPosition(Vector3.zero).position + Vector3.up * 1f;
    }

    [System.Serializable]
    public class Wave
    {
        public bool infinite;
        public int enemyCount;
        public float timeBetweenSpawn;

        public float moveSpeed;
        public int hitsToKillPlayer;
        public float enemyHealth;
        public Color skinColor;
    }
}
