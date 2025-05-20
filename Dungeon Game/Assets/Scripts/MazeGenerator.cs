using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [Header("Prefablar")]
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject cornerPrefab;
    public GameObject columnPrefab;

    [Header("Labirent Ölçüleri")]
    public int width = 15;
    public int height = 15;

    [Header("Trap Ayarları (Otomatik Yerleştirme)")]
    public GameObject[] trapPrefabs;            // Tuzak prefabları
    public int trapCount = 10;                  // Tuzak adeti

    private Transform mazeParent;               // Tüm instantiate'leri toplayacağımız parent
    private GameObject[,] verticalWalls;        // İç dikey duvar referansları
    private GameObject[,] horizontalWalls;      // İç yatay duvar referansları
    private bool[,] visited;                    // DFS için ziyaret matrisi

    void Start()
    {
        // LevelManager'dan seçili MazeData'yı al
        MazeData data = LevelManager.Instance.GetCurrentMazeData();

        width = data.width;
        height = data.height;

        if (data.useRandomSeed)
        {
            Random.InitState(System.DateTime.Now.Millisecond);
        }
        else
        {
            Random.InitState(data.seed);
        }


        // Parent objesini oluştur
        mazeParent = new GameObject("MazeParent").transform;
        mazeParent.parent = this.transform;

        // 1) Zemin
        GenerateFloor();
        // 2) Dış sınır (köşe + duvar + dolgu kolon)
        GenerateOuterWallsAndColumns();
        // 3) İç duvarlar (hücreler arası sınırlar)
        GenerateInnerWalls();
        // 4) DFS ile labirent carve et
        CarveMaze();
        // 5) Tuzakları rastgele hücrelere yerleştir
        PlaceTraps();
    }

    // 15x15 grid üzerinde zemin karoları yerleştir
    void GenerateFloor()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 pos = new Vector3(x, 0, z);
                Instantiate(floorPrefab, pos, Quaternion.identity, mazeParent);
            }
        }
    }

    // Labirentin etrafına duvar, köşe ve dolgu kolonlarını yerleştir
    void GenerateOuterWallsAndColumns()
    {
        float half = 0.5f;
        float leftX = -half;
        float rightX = width - half;
        float bottomZ = -half;
        float topZ = height - half;

        // - Köşeler -
        Instantiate(cornerPrefab, new Vector3(leftX, 1, bottomZ), Quaternion.identity, mazeParent);
        Instantiate(cornerPrefab, new Vector3(rightX, 1, bottomZ), Quaternion.identity, mazeParent);
        Instantiate(cornerPrefab, new Vector3(rightX, 1, topZ), Quaternion.identity, mazeParent);
        Instantiate(cornerPrefab, new Vector3(leftX, 1, topZ), Quaternion.identity, mazeParent);

        // - Alt ve üst kenarlar + Dolgu kolonlar -
        for (int x = 0; x < width; x++)
        {
            // Alt kenar duvarı
            Instantiate(wallPrefab, new Vector3(x, 1, bottomZ), Quaternion.identity, mazeParent);
            // Alt kenar dolgu kolonu
            Instantiate(columnPrefab, new Vector3(x + half, 1, bottomZ), Quaternion.identity, mazeParent);

            // Üst kenar duvarı
            Instantiate(wallPrefab, new Vector3(x, 1, topZ), Quaternion.identity, mazeParent);
            // Üst kenar dolgu kolonu
            Instantiate(columnPrefab, new Vector3(x + half, 1, topZ), Quaternion.identity, mazeParent);
        }

        // - Sol ve sağ kenarlar + Dolgu kolonları -
        for (int z = 0; z < height; z++)
        {
            // Sol kenar duvarı
            Instantiate(wallPrefab, new Vector3(leftX, 1, z), Quaternion.Euler(0, 90, 0), mazeParent);
            // Sol kenar dolgu kolonu
            Instantiate(columnPrefab, new Vector3(leftX, 1, z + half), Quaternion.identity, mazeParent);

            // Sağ kenar duvarı
            Instantiate(wallPrefab, new Vector3(rightX, 1, z), Quaternion.Euler(0, 90, 0), mazeParent);
            // Sağ kenar dolgu kolonu
            Instantiate(columnPrefab, new Vector3(rightX, 1, z + half), Quaternion.identity, mazeParent);
        }
    }

    // Hücreler arasındaki iç duvarları oluşturur ve duvarların kesişim noktalarına kolon ekler
    void GenerateInnerWalls()
    {
        // 1) Dizileri oluştur
        verticalWalls = new GameObject[width + 1, height]; // 16, 15
        horizontalWalls = new GameObject[width, height + 1]; // 15, 16

        // 2) Dikey iç duvarlar (x = 1... width - 1)
        for (int x = 1; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 pos = new Vector3(x - 0.5f, 1, z);
                verticalWalls[x, z] = Instantiate(wallPrefab, pos, Quaternion.Euler(0, 90, 0), mazeParent);
            }
        }

        // 3) Yatay iç duvarlar (z = 1... height - 1)
        for (int x = 0; x < width; x++)
        {
            for (int z = 1; z < height; z++)
            {
                Vector3 pos = new Vector3(x, 1, z - 0.5f);
                horizontalWalls[x, z] = Instantiate(wallPrefab, pos, Quaternion.identity, mazeParent);
            }
        }

        // 4) İç duvarlar arasındaki kolonlar
        // Her dikey+yatay duvarın kesişim noktasına, x = 1... width - 1, z = 1... height - 1 aralığına kolon yerleştir.
        for (int x = 1; x < width; x++)
        {
            for (int z = 1; z < height; z++)
            {
                Vector3 colPos = new Vector3(x - 0.5f, 1, z - 0.5f);
                Instantiate(columnPrefab, colPos, Quaternion.identity, mazeParent);
            }
        }
    }

    void CarveMaze()
    {
        visited = new bool[width, height];
        DFS(0, 0); // (0, 0) başlangıç hücresi
    }

    void DFS(int x, int z)
    {
        visited[x, z] = true;

        // 4 yönü rastgele sırala
        var dirs = new List<Vector2Int>
        {
            new Vector2Int(1, 0), // Doğu
            new Vector2Int(-1,0), // Batı
            new Vector2Int(0, 1), // Kuzey
            new Vector2Int(0,-1)  // Güney
        };

        for (int i = dirs.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var tmp = dirs[i];
            dirs[i] = dirs[j];
            dirs[j] = tmp;
        }

        // Her komşuyu hücreyi dene
        foreach (var d in dirs)
        {
            int nx = x + d.x;
            int nz = z + d.y;
            if (nx >= 0 && nx < width && nz >= 0 && nz < height && !visited[nx, nz])
            {
                // Aradaki duvarı Destroy ile kaldır
                if (d.x == 1) Destroy(verticalWalls[x + 1, z]);
                if (d.x == -1) Destroy(verticalWalls[x, z]);
                if (d.y == 1) Destroy(horizontalWalls[x, z + 1]);
                if (d.y == -1) Destroy(horizontalWalls[x, z]);

                DFS(nx, nz);
            }
        }
    }

    // Labirent içindeki rastgele hücrelere tuzak prefablarını yerleştir.
    void PlaceTraps()
    {
        // Aynı hücreye iki kez koymamızın önüne geçmek için bir set
        HashSet<Vector2Int> usedCells = new HashSet<Vector2Int>();

        int placed = 0;
        // İstedğimiz sayı kadar tuzak atana kadar dön
        while (placed < trapCount)
        {
            // Rastgele bir hücre seç(0..width-1, 0..height-1)
            int x = Random.Range(0, width);
            int z = Random.Range(0, height);
            Vector2Int cell = new Vector2Int(x, z);

            // Başlangıç ve bitiş hücrelerini atla
            if ((x == 0 && z == 0) || (x == width - 1 && z == height - 1))
            {
                continue;
            }
            // Aynı hücreye tekrar atlamamak için kontrol et
            if (usedCells.Contains(cell))
            {
                continue;
            }

            // Dizideki bir prefabı rastgele seç
            GameObject prefab = trapPrefabs[Random.Range(0, trapPrefabs.Length)];

            // Dünyadaki pozisyonunu hesapla (y = 0.5f biraz yukarıda)
            Vector3 pos = new Vector3(x, 0.05f, z);

            // Instantiate ile sahneye ekle, mazeParent altına koy
            Instantiate(prefab, pos, Quaternion.identity, mazeParent);

            // Kullanılan hücre olarak işaretle ve sayacı arttır
            usedCells.Add(cell);
            placed++;
        }
    }
}
