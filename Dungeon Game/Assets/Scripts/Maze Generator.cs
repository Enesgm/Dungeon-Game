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
    public GameObject startMarkerPrefab; // Başlangıç noktası işaretleyici
    public GameObject endMarkerPrefab;   // Bitiş noktası işaretleyici

    public float cellSize = 4f;
    public float windowWallChance = 0.3f; // %30 pencere duvar şansı
    public float crackedWallChance = 0.2f; // %20 çatlak duvar şansı

    // Labirent verilerini saklamak için veri yapıları
    private Dictionary<Vector2Int, Cell> maze = new Dictionary<Vector2Int, Cell>();
    private Dictionary<Vector2Int, bool> wallPositions = new Dictionary<Vector2Int, bool>();
    
    // Başlangıç ve bitiş noktası koordinatları (grid koordinatları)
    private Vector2Int startPoint;
    private Vector2Int endPoint;

    // Hücre verilerini depolamak için yardımcı sınıf
    private class Cell
    {
        public bool visited = false;
        public bool[] walls = new bool[4] { true, true, true, true }; // Kuzey, Doğu, Güney, Batı
    }

    private void Start()
    {
        InitializeMaze();
        GenerateMaze();
        BuildMaze();
    }

    // Labirent veri yapısını başlatır
    void InitializeMaze()
    {
        // Tüm hücreleri duvarlarla çevrili olarak oluştur
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                maze[new Vector2Int(x, z)] = new Cell();
            }
        }

        // Başlangıç noktası: sağ alt köşe (grid koordinatları)
        startPoint = new Vector2Int(width - 1, 0);
        
        // Bitiş noktası: sol üst köşe (grid koordinatları)
        endPoint = new Vector2Int(0, height - 1);
        
        // Duvar konumlarını izlemek için dictionary'yi temizle
        wallPositions.Clear();
        
        Debug.Log("Labirent boyutu: " + width + "x" + height + " hücre");
        Debug.Log("Hücre boyutu: " + cellSize + " birim");
    }

    // Derinlik öncelikli arama ile labirent oluştur
    void GenerateMaze()
    {
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        Vector2Int current = startPoint;
        maze[current].visited = true;

        // Derinlik öncelikli arama (DFS) ile labirent oluştur
        do
        {
            List<Vector2Int> unvisitedNeighbors = GetUnvisitedNeighbors(current);

            if (unvisitedNeighbors.Count > 0)
            {
                // Rastgele komşu seç
                Vector2Int next = unvisitedNeighbors[Random.Range(0, unvisitedNeighbors.Count)];
                stack.Push(current);

                // Seçilen komşuya duvarı yıkarak geçiş aç
                RemoveWallBetween(current, next);

                // Sonraki hücreye ilerle
                current = next;
                maze[current].visited = true;
            }
            else if (stack.Count > 0)
            {
                // Çıkmaz sokağa gelindiyse, geri dön
                current = stack.Pop();
            }
        } while (stack.Count > 0);

        // Başlangıç ve bitiş noktaları arasında yol olduğundan emin ol
        EnsurePathFromStartToEnd();
    }

    // Belirtilen hücrenin ziyaret edilmemiş komşularını döndürür
    List<Vector2Int> GetUnvisitedNeighbors(Vector2Int cell)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),  // Kuzey
            new Vector2Int(1, 0),  // Doğu
            new Vector2Int(0, -1), // Güney
            new Vector2Int(-1, 0)  // Batı
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighbor = cell + dir;
            // Labirent sınırları içindeki ziyaret edilmemiş komşuları ekle
            if (neighbor.x >= 0 && neighbor.x < width && 
                neighbor.y >= 0 && neighbor.y < height && 
                !maze[neighbor].visited)
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    // İki hücre arasındaki duvarı kaldırır
    void RemoveWallBetween(Vector2Int a, Vector2Int b)
    {
        int dx = b.x - a.x;
        int dy = b.y - a.y;

        if (dx == 1)
        {
            maze[a].walls[1] = false; // A'nın doğu duvarını kaldır
            maze[b].walls[3] = false; // B'nin batı duvarını kaldır
        }
        else if (dx == -1)
        {
            maze[a].walls[3] = false; // A'nın batı duvarını kaldır
            maze[b].walls[1] = false; // B'nin doğu duvarını kaldır
        }
        else if (dy == 1)
        {
            maze[a].walls[0] = false; // A'nın kuzey duvarını kaldır
            maze[b].walls[2] = false; // B'nin güney duvarını kaldır
        }
        else if (dy == -1)
        {
            maze[a].walls[2] = false; // A'nın güney duvarını kaldır
            maze[b].walls[0] = false; // B'nin kuzey duvarını kaldır
        }
    }

    // Başlangıç ve bitiş noktaları arasında ulaşılabilir bir yol oluştur
    void EnsurePathFromStartToEnd()
    {
        // Başlangıç ve bitiş noktalarının dış duvarlarla bağlantısını aç

        // Başlangıç noktası dış duvarı (sağ alt köşe - sağ duvarı)
        maze[startPoint].walls[1] = false;

        // Bitiş noktası dış duvarı (sol üst köşe - sol duvarı)
        maze[endPoint].walls[3] = false;

        // Acil durum: Özel yol oluştur
        // Başlangıç ve bitiş noktaları arasında belirgin bir yol garantile
        if (!IsPathExistsBetween(startPoint, endPoint))
        {
            CreateDirectPath(startPoint, endPoint);
        }
        
        // Başlangıç ve bitiş noktalarını konsola yazdır (Debug amaçlı)
        Debug.Log("Başlangıç noktası: " + startPoint + " konumunda");
        Debug.Log("Bitiş noktası: " + endPoint + " konumunda");
    }

    // Başlangıç ve bitiş noktaları arasında doğrudan bir yol oluştur
    void CreateDirectPath(Vector2Int start, Vector2Int end)
    {
        Vector2Int current = start;
        
        // Önce x ekseninde ilerle
        while (current.x != end.x)
        {
            Vector2Int next = current;
            if (current.x > end.x)
            {
                next.x--;
                // Sol duvarı aç
                maze[current].walls[3] = false;
                // Sağ duvarı aç
                maze[next].walls[1] = false;
            }
            else
            {
                next.x++;
                // Sağ duvarı aç
                maze[current].walls[1] = false;
                // Sol duvarı aç
                maze[next].walls[3] = false;
            }
            current = next;
        }
        
        // Sonra y ekseninde ilerle
        while (current.y != end.y)
        {
            Vector2Int next = current;
            if (current.y > end.y)
            {
                next.y--;
                // Alt duvarı aç
                maze[current].walls[2] = false;
                // Üst duvarı aç
                maze[next].walls[0] = false;
            }
            else
            {
                next.y++;
                // Üst duvarı aç
                maze[current].walls[0] = false;
                // Alt duvarı aç
                maze[next].walls[2] = false;
            }
            current = next;
        }
    }

    // BFS algoritması ile iki nokta arasında yol olup olmadığını kontrol et
    bool IsPathExistsBetween(Vector2Int start, Vector2Int end)
    {
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        
        queue.Enqueue(start);
        visited.Add(start);
        
        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            
            if (current.Equals(end))
                return true;
                
            // Her dört yönü kontrol et
            CheckDirection(current, 0, new Vector2Int(0, 1), visited, queue);  // Kuzey
            CheckDirection(current, 1, new Vector2Int(1, 0), visited, queue);  // Doğu
            CheckDirection(current, 2, new Vector2Int(0, -1), visited, queue); // Güney
            CheckDirection(current, 3, new Vector2Int(-1, 0), visited, queue); // Batı
        }
        
        return false;
    }
    
    // Belirtilen yönde geçiş olup olmadığını kontrol et
    void CheckDirection(Vector2Int current, int direction, Vector2Int offset, 
                        HashSet<Vector2Int> visited, Queue<Vector2Int> queue)
    {
        // Bu yönde duvar yoksa
        if (!maze[current].walls[direction])
        {
            Vector2Int neighbor = current + offset;
            
            // Sınırlar içindeyse ve daha önce ziyaret edilmediyse
            if (neighbor.x >= 0 && neighbor.x < width && 
                neighbor.y >= 0 && neighbor.y < height && 
                !visited.Contains(neighbor))
            {
                visited.Add(neighbor);
                queue.Enqueue(neighbor);
            }
        }
    }

    // Oluşturulan labirent verilerine göre fiziksel duvarları inşa et
    void BuildMaze()
    {
        // Önce zemini oluştur
        GenerateFloor();
        
        // Önce dış duvarları oluştur - bunu ayrı bir adım olarak yapalım
        CreateOuterWalls();

        // İç labirent duvarlarını oluştur
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector2Int pos = new Vector2Int(x, z);
                Cell cell = maze[pos];

                // Kuzey duvarı (iç duvarlar için)
                if (cell.walls[0] && z < height - 1)
                {
                    Vector3 wallPos = new Vector3(x * cellSize + cellSize / 2, 0, (z + 1) * cellSize - cellSize / 2);
                    Vector2Int wallKey = new Vector2Int(x, z + 1);
                    if (!wallPositions.ContainsKey(wallKey))
                    {
                        CreateWallPiece(wallPos, false);
                        wallPositions[wallKey] = true;
                    }
                }

                // Doğu duvarı (iç duvarlar için)
                if (cell.walls[1] && x < width - 1)
                {
                    Vector3 wallPos = new Vector3((x + 1) * cellSize - cellSize / 2, 0, z * cellSize + cellSize / 2);
                    Vector2Int wallKey = new Vector2Int(x + 1, z);
                    if (!wallPositions.ContainsKey(wallKey))
                    {
                        CreateWallPiece(wallPos, true);
                        wallPositions[wallKey] = true;
                    }
                }

                // Not: Güney ve Batı duvarları komşu hücreler tarafından zaten kapsanıyor
                // Bu nedenle burada özel olarak ele almıyoruz
            }
        }

        // L-duvarları köşelerde oluştur
        PlaceCornerWalls();
        
        // T-duvarları ve L-duvarları uygun yerlere yerleştir
        PlaceSpecialWalls();
        
        // Başlangıç ve bitiş işaretleyicilerini yerleştir
        PlaceStartAndEndMarkers();
    }
    
    // Dış duvarları oluşturan fonksiyon
    void CreateOuterWalls()
    {
        // Alt duvar (z = 0)
        for (int x = 0; x < width; x++)
        {
            Vector3 wallPos = new Vector3(x * cellSize + cellSize / 2, 0, 0);
            Vector2Int wallKey = new Vector2Int(x, -1);
            if (!wallPositions.ContainsKey(wallKey))
            {
                CreateWallPiece(wallPos, false);
                wallPositions[wallKey] = true;
            }
        }
        
        // Üst duvar (z = height)
        for (int x = 0; x < width; x++)
        {
            Vector3 wallPos = new Vector3(x * cellSize + cellSize / 2, 0, height * cellSize);
            Vector2Int wallKey = new Vector2Int(x, height);
            if (!wallPositions.ContainsKey(wallKey))
            {
                CreateWallPiece(wallPos, false);
                wallPositions[wallKey] = true;
            }
        }
        
        // Sol duvar (x = 0)
        for (int z = 0; z < height; z++)
        {
            Vector3 wallPos = new Vector3(0, 0, z * cellSize + cellSize / 2);
            Vector2Int wallKey = new Vector2Int(-1, z);
            if (!wallPositions.ContainsKey(wallKey))
            {
                CreateWallPiece(wallPos, true);
                wallPositions[wallKey] = true;
            }
        }
        
        // Sağ duvar (x = width)
        for (int z = 0; z < height; z++)
        {
            Vector3 wallPos = new Vector3(width * cellSize, 0, z * cellSize + cellSize / 2);
            Vector2Int wallKey = new Vector2Int(width, z);
            if (!wallPositions.ContainsKey(wallKey))
            {
                CreateWallPiece(wallPos, true);
                wallPositions[wallKey] = true;
            }
        }
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

    // Duvar prefabı oluşturan ve kaydeden fonksiyon
    void CreateWallPiece(Vector3 position, bool rotate90)
    {
        // Rastgele duvar tipi seç (normal, pencereli veya çatlak)
        float randomValue = Random.value;
        GameObject prefab = straightWallPrefab; // Varsayılan düz duvar
        
        if (randomValue < windowWallChance)
        {
            prefab = windowWallPrefab;
        }
        else if (randomValue < windowWallChance + crackedWallChance)
        {
            prefab = straightWallPrefab; // Çatlak duvar prefabınız varsa burada kullanabilirsiniz
        }
        
        Quaternion rotation = rotate90 ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;
        Instantiate(prefab, position, rotation, transform);
    }

    // Dış köşelere L duvarları yerleştiren fonksiyon
    void PlaceCornerWalls()
    {
        // Dış köşeler
        CreateCorner(new Vector3(0, 0, 0), -90); // Sol-Alt köşe
        CreateCorner(new Vector3(width * cellSize, 0, 0), 0);  // Sağ-Alt köşe
        CreateCorner(new Vector3(0, 0, height * cellSize), 180); // Sol-Üst köşe
        CreateCorner(new Vector3(width * cellSize, 0, height * cellSize), 90); // Sağ-Üst köşe
        
        // İç köşeleri kontrol et ve L duvarları yerleştir
        for (int x = 0; x < width-1; x++)
        {
            for (int z = 0; z < height-1; z++)
            {
                Vector2Int pos = new Vector2Int(x, z);
                CheckAndPlaceLWall(pos);
            }
        }
    }
    
    // Belirtilen konumda L-duvar gerekliliğini kontrol et ve yerleştir
    void CheckAndPlaceLWall(Vector2Int pos)
    {
        int x = pos.x;
        int z = pos.y;
        
        Cell current = maze[pos];
        Cell right = maze.ContainsKey(new Vector2Int(x+1, z)) ? maze[new Vector2Int(x+1, z)] : null;
        Cell up = maze.ContainsKey(new Vector2Int(x, z+1)) ? maze[new Vector2Int(x, z+1)] : null;
        Cell upRight = maze.ContainsKey(new Vector2Int(x+1, z+1)) ? maze[new Vector2Int(x+1, z+1)] : null;
        
        // Duvarların L şeklini oluşturup oluşturmadığını kontrol et
        if (current != null && right != null && up != null && upRight != null)
        {
            // Sağ üst köşede L duvar olmalı mı?
            if (!current.walls[0] && !current.walls[1] && up.walls[1] && right.walls[0])
            {
                Vector3 cornerPos = new Vector3((x+1) * cellSize, 0, (z+1) * cellSize);
                Vector2Int cornerKey = new Vector2Int(x+1, z+1);
                
                if (!wallPositions.ContainsKey(cornerKey))
                {
                    CreateCorner(cornerPos, 90);
                    wallPositions[cornerKey] = true;
                }
            }
            
            // Sol üst köşede L duvar olmalı mı?
            if (!current.walls[0] && !current.walls[3] && current.walls[1] && up.walls[3])
            {
                Vector3 cornerPos = new Vector3(x * cellSize, 0, (z+1) * cellSize);
                Vector2Int cornerKey = new Vector2Int(x, z+1);
                
                if (!wallPositions.ContainsKey(cornerKey))
                {
                    CreateCorner(cornerPos, 180);
                    wallPositions[cornerKey] = true;
                }
            }
            
            // Sağ alt köşede L duvar olmalı mı?
            if (!current.walls[1] && !current.walls[2] && current.walls[0] && right.walls[2])
            {
                Vector3 cornerPos = new Vector3((x+1) * cellSize, 0, z * cellSize);
                Vector2Int cornerKey = new Vector2Int(x+1, z);
                
                if (!wallPositions.ContainsKey(cornerKey))
                {
                    CreateCorner(cornerPos, 0);
                    wallPositions[cornerKey] = true;
                }
            }
            
            // Sol alt köşede L duvar olmalı mı?
            if (!current.walls[2] && !current.walls[3] && current.walls[0] && current.walls[1])
            {
                Vector3 cornerPos = new Vector3(x * cellSize, 0, z * cellSize);
                Vector2Int cornerKey = new Vector2Int(x, z);
                
                if (!wallPositions.ContainsKey(cornerKey))
                {
                    CreateCorner(cornerPos, -90);
                    wallPositions[cornerKey] = true;
                }
            }
        }
    }

    // Köşe L duvar prefabı oluşturan fonksiyon
    void CreateCorner(Vector3 position, float rotationY)
    {
        Instantiate(lWallPrefab, position, Quaternion.Euler(0, rotationY, 0), transform);
    }

    // T-duvarları yerleştiren fonksiyon
    void PlaceSpecialWalls()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector2Int pos = new Vector2Int(x, z);
                Cell cell = maze[pos];
                
                // T-duvar gereken yerleri tespit et
                CheckAndPlaceTWall(pos);
            }
        }
    }
    
    // Belirtilen konumda T duvar gerekliliğini kontrol et ve yerleştir
    void CheckAndPlaceTWall(Vector2Int pos)
    {
        int x = pos.x;
        int z = pos.y;
        Cell cell = maze[pos];
        
        // T duvar durumu: 3 duvar, 1 açık yol
        int wallCount = 0;
        int openDirection = -1;
        
        for (int i = 0; i < 4; i++)
        {
            if (cell.walls[i])
                wallCount++;
            else
                openDirection = i;
        }
        
        if (wallCount == 3 && openDirection != -1)
        {
            // Duvar yerleştirme koordinatlarını ve döndürme açısını hesapla
            Vector3 tWallPos = new Vector3((x + 0.5f) * cellSize, 0, (z + 0.5f) * cellSize);
            float rotation = 0;
            
            switch (openDirection)
            {
                case 0: rotation = 0; break;   // Kuzey açık
                case 1: rotation = 90; break;  // Doğu açık
                case 2: rotation = 180; break; // Güney açık
                case 3: rotation = 270; break; // Batı açık
            }
            
            Vector2Int tWallKey = new Vector2Int(x, z);
            
            // Daha önce bu pozisyona duvar yerleştirilmemişse T duvar ekle
            if (!wallPositions.ContainsKey(tWallKey))
            {
                Instantiate(tWallPrefab, tWallPos, Quaternion.Euler(0, rotation, 0), transform);
                wallPositions[tWallKey] = true;
            }
        }
    }
    
    // Başlangıç ve bitiş işaretleyicilerini yerleştiren fonksiyon
    void PlaceStartAndEndMarkers()
    {
        // Başlangıç noktasının dünya koordinatları (sağ alt köşe)
        Vector3 startPos = new Vector3(startPoint.x * cellSize + cellSize / 2, 0.1f, startPoint.y * cellSize + cellSize / 2);
        
        // Başlangıç işaretleyicisini yerleştir
        GameObject startMarker = Instantiate(startMarkerPrefab, startPos, Quaternion.identity, transform);
        startMarker.name = "BaşlangıçNoktası";
        Debug.Log("Başlangıç noktası yerleştirildi: " + startPos);
        
        // Bitiş noktasının dünya koordinatları (sol üst köşe)
        Vector3 endPos = new Vector3(endPoint.x * cellSize + cellSize / 2, 0.1f, endPoint.y * cellSize + cellSize / 2);
        
        // Bitiş işaretleyicisini yerleştir
        GameObject endMarker = Instantiate(endMarkerPrefab, endPos, Quaternion.identity, transform);
        endMarker.name = "BitişNoktası";
        Debug.Log("Bitiş noktası yerleştirildi: " + endPos);
    }
}
