using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public int width = 15;
    public int height = 15;

    public GameObject floorPrefab;
    public GameObject straightWallPrefab;
    public GameObject windowWallPrefab;
    public GameObject tWallPrefab;
    public GameObject lWallPrefab;

    public float cellSize = 4f;
    public float windowWallChance = 0.3f; // %30 pencere duvar şansı
    public float tWallChance = 0.2f; // %20 t-duvar şansı

    private Dictionary<Vector3, GameObject> wallDictionary = new Dictionary<Vector3, GameObject>();

    private void Start()
    {
        GenerateFloor();
        GenerateOuterWalls();
        GenerateTConnections();
    }

    // Zemini oluşturur
    void GenerateFloor()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 position = new Vector3(x * cellSize, 0, z * cellSize);
                Instantiate(floorPrefab, position, Quaternion.identity, transform);
            }
        }
    }

    // Dış duvarları oluşturur
    void GenerateOuterWalls()
    {
        // Sol ve Sağ kenarlar
        for (int z = 0; z < height - 1; z++)
        {
            CreateWallPiece(new Vector3(-cellSize / 2f, 0, (z * cellSize) + (cellSize / 2f)), true);  // Sol
            CreateWallPiece(new Vector3(width * cellSize - cellSize / 2f, 0, (z * cellSize) + (cellSize / 2f)), true); // Sağ
        }

        // Alt ve Üst kenarlar
        for (int x = 0; x < width - 1; x++)
        {
            CreateWallPiece(new Vector3((x * cellSize) + (cellSize / 2f), 0, -cellSize / 2f), false); // Alt
            CreateWallPiece(new Vector3((x * cellSize) + (cellSize / 2f), 0, height * cellSize - cellSize / 2f), false); // Üst
        }

        // Köşelere L duvarları
        CreateCorner(new Vector3(-cellSize / 2f, 0, -cellSize / 2f), 0);
        CreateCorner(new Vector3(width * cellSize - cellSize / 2f, 0, -cellSize / 2f), -90);
        CreateCorner(new Vector3(-cellSize / 2f, 0, height * cellSize - cellSize / 2f), -270);
        CreateCorner(new Vector3(width * cellSize - cellSize / 2f, 0, height * cellSize - cellSize / 2f), 180);
    }

    // Duvar prefabı oluşturan ve kaydeden fonksiyon
    void CreateWallPiece(Vector3 position, bool rotate90)
    {
        GameObject prefab = Random.value < windowWallChance ? windowWallPrefab : straightWallPrefab;
        Quaternion rotation = rotate90 ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;
        GameObject wallPiece = Instantiate(prefab, position, rotation, transform);

        // Wall'ı konuma göre dictionary'ye ekle
        wallDictionary[position] = wallPiece;
    }

    // Köşe L duvar prefabı oluşturan fonksiyon
    void CreateCorner(Vector3 position, float rotationY)
    {
        Instantiate(lWallPrefab, position, Quaternion.Euler(0, rotationY, 0), transform);
    }

    // Dış kenarlara T duvarlar yerleştiren fonksiyon
    void GenerateTConnections()
    {
        // Sağ kenar
        for (int z = 1; z < height - 2; z++)
        {
            if (Random.value < tWallChance)
            {
                Vector3 pos = new Vector3(width * cellSize - cellSize / 2f, 0, (z * cellSize) + (cellSize / 2f));
                ReplaceWithTWall(pos, Quaternion.Euler(0, -90, 0));
            }
        }

        // Sol kenar
        for (int z = 1; z < height - 2; z++)
        {
            if (Random.value < tWallChance)
            {
                Vector3 pos = new Vector3(-cellSize / 2f, 0, (z * cellSize) + (cellSize / 2f));
                ReplaceWithTWall(pos, Quaternion.Euler(0, -270, 0));
            }
        }

        // Üst kenar
        for (int x = 1; x < width - 2; x++)
        {
            if (Random.value < tWallChance)
            {
                Vector3 pos = new Vector3((x * cellSize) + (cellSize / 2f), 0, height * cellSize - cellSize / 2f);
                ReplaceWithTWall(pos, Quaternion.Euler(0, -180, 0));
            }
        }

        // Alt kenar
        for (int x = 1; x < width - 2; x++)
        {
            if (Random.value < tWallChance)
            {
                Vector3 pos = new Vector3((x * cellSize) + (cellSize / 2f), 0, -cellSize / 2f);
                ReplaceWithTWall(pos, Quaternion.Euler(0, 0, 0));
            }
        }
    }

    // T duvar koyarken eski düz duvarı silip yerine t duvar koyar
    void ReplaceWithTWall(Vector3 position, Quaternion rotation)
    {
        // Önce oradaki düz duvar varsa yok et
        if (wallDictionary.ContainsKey(position))
        {
            Destroy(wallDictionary[position]);
            wallDictionary.Remove(position);
        }

        // Sonra T duvarı instantiate et
        Instantiate(tWallPrefab, position, rotation, transform);
    }
}
