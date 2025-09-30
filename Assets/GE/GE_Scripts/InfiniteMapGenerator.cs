using UnityEngine;
using System.Collections.Generic;

public class InfiniteMapGenerator : MonoBehaviour
{
    public GameObject chunkPrefab;
    public GameObject tilePrefab;
    public Transform player;
    public int chunkSize = 10;
    public int viewDistance = 2;

    private Dictionary<Vector2Int, MapChunk> spawnedChunks = new Dictionary<Vector2Int, MapChunk>();

    void Update()
    {
        Vector2 playerPos = player.position;
        Vector2Int playerChunkCoord = new Vector2Int(
            Mathf.FloorToInt(playerPos.x / chunkSize),
            Mathf.FloorToInt(playerPos.y / chunkSize)
        );

        for (int x = -viewDistance; x <= viewDistance; x++)
        {
            for (int y = -viewDistance; y <= viewDistance; y++)
            {
                Vector2Int chunkCoord = playerChunkCoord + new Vector2Int(x, y);
                if (!spawnedChunks.ContainsKey(chunkCoord))
                {
                    SpawnChunk(chunkCoord);
                }
            }
        }
    }

    void SpawnChunk(Vector2Int coord)
    {
        Vector3 worldPos = new Vector3(coord.x * chunkSize, coord.y * chunkSize, 0);
        GameObject chunkObj = Instantiate(chunkPrefab, worldPos, Quaternion.identity);
        MapChunk chunk = chunkObj.GetComponent<MapChunk>();
        chunk.width = chunkSize;
        chunk.height = chunkSize;
        chunk.tilePrefab = tilePrefab;
        int seed = coord.x * 73856093 ^ coord.y * 19349663; // hash para variar
        chunk.GenerateChunk(seed);

        spawnedChunks.Add(coord, chunk);
    }
}