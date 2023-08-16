using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class MapGenerate : MonoBehaviour
{
    public Map[] maps;
    public int mapIndex;
    
    public Transform tilePrefab;//地面预制体
    public Transform obstaclePrefab;//障碍物预制体
    public Transform navMeshFloor;
    public Transform navmeshPrefab;
    public Transform mapFloor;
    public Vector2 maxMapSize;

    [Range(0,1)]
    public float outlinePercent;
    public float tileSize;
    
    private List<Coord> allTileCoords;
    private Queue<Coord> shuffledTileCoords;//重新排列后的地面位置
    private Queue<Coord> shuffledOpenTileCoords;
    private Transform[,] tileMap;

    private Map currentMap;
    private void Awake()
    {
        GenerateMap();
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
    }

    void OnNewWave(int waveNumber)
    {
        mapIndex = waveNumber - 1;
        GenerateMap();
    }

    public void GenerateMap()
    {
        currentMap = maps[mapIndex];
        tileMap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y];
        BoxCollider collider = GetComponent<BoxCollider>();
        
        
        allTileCoords = new List<Coord>();
        for (int i = 0; i < currentMap.mapSize.x; i++)
        {
            for (int j = 0; j < currentMap.mapSize.y; j++)
            {
                allTileCoords.Add(new Coord(i,j));
            }
        }
        shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTileCoords.ToArray(), currentMap.seed));

        string holderName = "Generate Map";
        Transform holder = transform.Find(holderName); 
        if (holder != null)
        {
            DestroyImmediate(holder.gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;
        
        for (int x = 0; x < currentMap.mapSize.x; x++)//生成地面
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                Vector3 tilePosition = CoordToPosition(x, y);//得到相应位置
                Transform newTile =
                    Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
                newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                newTile.parent = mapHolder;
                tileMap[x, y] = newTile;
            }
        }

        SpawnObstacle(mapHolder);
        
        
        
        SpawnNavMesh(mapHolder);
    }
    private void SpawnObstacle(Transform mapHolder)
    {
        Random prng = new Random(currentMap.seed);
        bool[,] obstacleMap = new bool[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y]; //障碍物map
        int obstacleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent); //障碍物数量
        int currentObstacleCount = 0; //当前生成的障碍物数量
        List<Coord> allOpenCoords = new List<Coord>(allTileCoords);

        for (int i = 0; i < obstacleCount; i++)
        {
            Coord randomCoord = GetRandomCoord();
            obstacleMap[randomCoord.x, randomCoord.y] = true; //标记该位置存在障碍物
            currentObstacleCount++; //障碍物数量加1

            if (randomCoord != currentMap.mapCenter && MapIsFullyAccessible(obstacleMap, currentObstacleCount))
            {
                float obstacleRandomHeight = Mathf.Lerp(currentMap.minobstacleHeight, currentMap.maxobstacleHeight,
                    (float)prng.NextDouble());
                Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y); //得到相应的位置
                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * obstacleRandomHeight / 2,
                    Quaternion.identity);
                newObstacle.localScale = new Vector3((1 - outlinePercent) * tileSize,obstacleRandomHeight ,(1 - outlinePercent) * tileSize);
                newObstacle.parent = mapHolder;

                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
                float colorPercent = randomCoord.y / (float)currentMap.mapSize.y;
                obstacleMaterial.color =
                    Color.Lerp(currentMap.foregroundColor, currentMap.backgroundColor, colorPercent);
                obstacleRenderer.sharedMaterial = obstacleMaterial;

                allOpenCoords.Remove(randomCoord);
            }
            else
            {
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount--;
            }
        }
        shuffledOpenTileCoords = new Queue<Coord>(Utility.ShuffleArray(allOpenCoords.ToArray(), currentMap.seed));
    }

    public Transform GetRandomOpenTile()
    {
        Coord randomCoord = shuffledOpenTileCoords.Dequeue();
        shuffledOpenTileCoords.Enqueue(randomCoord);
        return tileMap[randomCoord.x, randomCoord.y];
    }

    public Transform GetTileFromPosition(Vector3 pos)
    {
        int x = Mathf.RoundToInt(pos.x / tileSize + (currentMap.mapSize.x - 1) / 2);
        int y = Mathf.RoundToInt(pos.z / tileSize + (currentMap.mapSize.y - 1) / 2);
        x = Mathf.Clamp(x, 0, tileMap.GetLength(0) - 1);
        y = Mathf.Clamp(y, 0, tileMap.GetLength(1) - 1);
        return tileMap[x, y];
    }
    void SpawnNavMesh(Transform mapHolder)
    {
        Transform maskLeft = Instantiate(navmeshPrefab, (Vector3.left * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize) - Vector3.forward * currentMap.mapSize.y / 2f,
            Quaternion.identity);
        maskLeft.parent = mapHolder;
        maskLeft.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;
        
        Transform maskRight = Instantiate(navmeshPrefab, Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize - Vector3.forward * currentMap.mapSize.y / 2f,
            Quaternion.identity);
        maskRight.parent = mapHolder;
        maskRight.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;
        
        Transform maskTop = Instantiate(navmeshPrefab, Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize - Vector3.forward * currentMap.mapSize.y / 2f,
            Quaternion.identity);
        maskTop.parent = mapHolder;
        maskTop.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;
        
        Transform maskBottom = Instantiate(navmeshPrefab, Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize - Vector3.forward * currentMap.mapSize.y / 2f,
            Quaternion.identity);
        maskBottom.parent = mapHolder;
        maskBottom.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;
        
        navMeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;
        mapFloor.localPosition =
            new Vector3(mapFloor.localPosition.x, mapFloor.localPosition.y, -currentMap.mapSize.y / 2.0f);
        mapFloor.localScale = new Vector3(currentMap.mapSize.x, currentMap.mapSize.y,
            1);
    }
    bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
    {
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(currentMap.mapCenter);
        mapFlags[currentMap.mapCenter.x, currentMap.mapCenter.y] = true;

        int accessibleTileCount = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            
            // 一个3x3 的数组 
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1 ; y++)
                {
                    int neighbourX = tile.x + x;
                    int neighbourY = tile.y + y;
                    if (x == 0 || y == 0)//当 x为0 或者 y为0 或者 x和y都为0时
                    {
                        //当临近的 x 
                        if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) && neighbourY >= 0 &&
                            neighbourY < obstacleMap.GetLength(1))
                        {
                            if (!mapFlags[neighbourX, neighbourY] && !obstacleMap[neighbourX, neighbourY])
                            {
                                mapFlags[neighbourX, neighbourY] = true;
                                queue.Enqueue(new Coord(neighbourX, neighbourY));
                                accessibleTileCount++;
                            }
                        }
                    }
                }
            }
        }
        int targetAccessibleTileCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount);
        return targetAccessibleTileCount == accessibleTileCount;
    }
    Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3(-currentMap.mapSize.x / 2 + 0.5f + x, 0, -currentMap.mapSize.y + 0.5f + y) * tileSize;
    }
    Coord GetRandomCoord()
    {
        Coord randomCoord = shuffledTileCoords.Dequeue();//取出头元素
        shuffledTileCoords.Enqueue(randomCoord);//添加到队尾
        return randomCoord;
    }
    
    [Serializable]
    public struct Coord
    {
        public int x;
        public int y;

        public Coord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        
        public static bool operator ==(Coord c1, Coord c2)
        {
            return c1.x == c2.x && c1.y == c2.y;
        }
        public static bool operator !=(Coord c1, Coord c2)
        {
            return !(c1 == c2);
        }
    }

    [Serializable]
    public class Map
    {
        public Coord mapSize;
        [Range(0,1)]
        public float obstaclePercent;
        public int seed;
        public float minobstacleHeight;
        public float maxobstacleHeight;
        public Color foregroundColor;
        public Color backgroundColor;

        public Coord mapCenter
        {
            get { return new Coord(mapSize.x / 2, mapSize.y / 2); }
        }
    }
}
