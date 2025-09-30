using UnityEngine;

public class MapChunk : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public GameObject tilePrefab;

    public void GenerateChunk(int seed)
    {
        System.Random rand = new System.Random(seed);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(transform.position.x + x, transform.position.y + y, 0);
                GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                // Exemplo: Muda cor aleatoriamente
                tile.GetComponent<SpriteRenderer>().color = rand.NextDouble() > 0.2 ? Color.green : Color.gray;
            }
        }
    }
}