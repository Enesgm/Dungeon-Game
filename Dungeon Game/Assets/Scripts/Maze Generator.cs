using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Bu script, DFS (Derinlik Öncelikli Arama) algoritması kullanarak 
/// 3 boyutlu bir labirent oluşturur ve görselleştirir.
/// Seed tabanlı rastgele labirent üretimi sağlar.
/// </summary>
public class MazeGenerator : MonoBehaviour
{
    [Header("Maze Settings")]
    public int width = 10;        // Labirentin genişliği (hücre sayısı)
    public int height = 10;       // Labirentin yüksekliği (hücre sayısı)
    public float cellSize = 2f;   // Her bir hücrenin boyutu (Unity birimleri)
    public int seed = 0;          // Labirent oluşturma için seed değeri
    public bool useRandomSeed = true; // Rastgele seed kullanılıp kullanılmayacağı
    
    [Header("Prefabs")]
    public GameObject wallPrefab;   // Duvar için kullanılacak prefab
    public GameObject floorPrefab;  // Zemin için kullanılacak prefab
    public GameObject cornerPrefab; // Köşe noktaları için kullanılacak prefab
    public GameObject startMarker;  // Başlangıç noktasını işaretlemek için prefab
    public GameObject endMarker;    // Bitiş noktasını işaretlemek için prefab
    
    [Header("Parent Objects")]
    public Transform wallsParent;   // Tüm duvarların parent objesi (hiyerarşi için)
    public Transform floorsParent;  // Tüm zeminlerin parent objesi (hiyerarşi için)
    public Transform cornersParent; // Tüm köşelerin parent objesi (hiyerarşi için)
    
    // Labirentdeki her bir hücreyi temsil eden iç sınıf
    private class Cell
    {
        public int x, y;  // Hücrenin grid koordinatları
        public bool[] walls = new bool[4] {true, true, true, true}; // Duvarlar: Kuzey, Doğu, Güney, Batı
        public bool visited = false; // DFS algoritması için ziyaret edilme durumu
        
        public Cell(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
    
    // Yön vektörleri: Kuzey, Doğu, Güney, Batı
    // Bu yönler, komşu hücreleri bulmak için kullanılır
    private Vector2Int[] directions = new Vector2Int[4] {
        new Vector2Int(0, 1),   // Kuzey (0)
        new Vector2Int(1, 0),   // Doğu (1)
        new Vector2Int(0, -1),  // Güney (2)
        new Vector2Int(-1, 0)   // Batı (3)
    };
    
    private Cell[,] maze;             // Labirent hücrelerinin 2D grid yapısı
    private System.Random rand;      // Seed tabanlı rastgele sayı üreteci
    private Vector2Int startPos;     // Başlangıç pozisyonu (genellikle 0,0)
    private Vector2Int endPos;       // Bitiş pozisyonu (genellikle width-1,height-1)
    
    // Unity başlangıç fonksiyonu - Oyun başladığında labirenti oluşturur
    void Start()
    {
        GenerateMaze();
    }
    
    /// <summary>
    /// Labirent oluşturma ana fonksiyonu.
    /// Tüm adımları sırasıyla çağırır.
    /// </summary>
    public void GenerateMaze()
    {
        ClearMaze();        // Eski labirenti temizle
        InitializeMaze();   // Yeni labirent veri yapısını hazırla
        RunDFSAlgorithm();  // DFS algoritmasını çalıştır
        BuildMaze();        // Görsel labirenti inşa et
    }
    
    /// <summary>
    /// Daha önce oluşturulmuş labirenti temizler.
    /// Tüm duvar, zemin ve köşe objelerini yok eder.
    /// </summary>
    private void ClearMaze()
    {
        // Eğer duvarlar parent objesi varsa, tüm çocuk objelerini yok et
        if (wallsParent != null)
        {
            foreach (Transform child in wallsParent)
            {
                Destroy(child.gameObject);
            }
        }
        
        // Eğer zeminler parent objesi varsa, tüm çocuk objelerini yok et
        if (floorsParent != null)
        {
            foreach (Transform child in floorsParent)
            {
                Destroy(child.gameObject);
            }
        }
        
        // Eğer köşeler parent objesi varsa, tüm çocuk objelerini yok et
        if (cornersParent != null)
        {
            foreach (Transform child in cornersParent)
            {
                Destroy(child.gameObject);
            }
        }
        
        // Başlangıç ve bitiş işaretçileri transform altında olduğu için temizle
        foreach (Transform child in transform)
        {
            if (child != wallsParent && child != floorsParent && child != cornersParent)
            {
                Destroy(child.gameObject);
            }
        }
    }
    
    /// <summary>
    /// Labirent veri yapısını oluşturur ve hazırlar.
    /// Seed değerini ayarlar, grid yapısını oluşturur ve başlangıç/bitiş konumlarını belirler.
    /// </summary>
    private void InitializeMaze()
    {
        // Seed ile başlat - rastgele seed isteniyorsa yeni bir seed oluştur
        if (useRandomSeed)
        {
            seed = UnityEngine.Random.Range(1, 100000);
        }
        // Seed ile rastgele sayı üretecini başlat
        rand = new System.Random(seed);
        Debug.Log("Using seed: " + seed);
        
        // Labirent grid yapısını oluştur
        maze = new Cell[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                maze[x, y] = new Cell(x, y);
            }
        }
        
        // Başlangıç ve bitiş pozisyonlarını ayarla
        // Genellikle başlangıç sol alt, bitiş sağ üst köşededir
        startPos = new Vector2Int(0, 0);
        endPos = new Vector2Int(width - 1, height - 1);
    }
    
    /// <summary>
    /// Derinlik Öncelikli Arama (DFS) algoritmasını çalıştırır.
    /// Bu algoritma labirent oluşturmak için duvarları kaldırarak yollar oluşturur.
    /// </summary>
    private void RunDFSAlgorithm()
    {
        // Hücreleri takip etmek için yığın (stack) veri yapısı kullan
        Stack<Cell> cellStack = new Stack<Cell>();
        
        // Başlangıç hücresinden başla
        Cell currentCell = maze[startPos.x, startPos.y];
        currentCell.visited = true;
        
        // DFS algoritması
        while (true)
        {
            // Mevcut hücrenin ziyaret edilmemiş komşularını bul
            List<Cell> unvisitedNeighbors = GetUnvisitedNeighbors(currentCell);
            
            if (unvisitedNeighbors.Count > 0)
            {
                // Rastgele bir ziyaret edilmemiş komşu seç
                Cell nextCell = unvisitedNeighbors[rand.Next(unvisitedNeighbors.Count)];
                
                // Mevcut hücreyi yığına ekle (geri dönüş için)
                cellStack.Push(currentCell);
                
                // Mevcut hücre ile seçilen komşu arasındaki duvarları kaldır
                RemoveWalls(currentCell, nextCell);
                
                // Seçilen komşuyu mevcut hücre yap ve ziyaret edildi olarak işaretle
                currentCell = nextCell;
                currentCell.visited = true;
            }
            else if (cellStack.Count > 0)
            {
                // Geri izleme - Ziyaret edilmemiş komşu kalmadığında, yığından önceki hücreye dön
                currentCell = cellStack.Pop();
            }
            else
            {
                // Tüm hücreler ziyaret edildi, labirent tamamlandı
                break;
            }
        }
    }
    
    /// <summary>
    /// Belirtilen hücrenin ziyaret edilmemiş komşularını bulur.
    /// DFS algoritması için gereklidir.
    /// </summary>
    /// <param name="cell">Komşuları bulunacak mevcut hücre</param>
    /// <returns>Ziyaret edilmemiş komşu hücrelerin listesi</returns>
    private List<Cell> GetUnvisitedNeighbors(Cell cell)
    {
        List<Cell> neighbors = new List<Cell>();
        
        // Dört yönü de kontrol et (Kuzey, Doğu, Güney, Batı)
        for (int i = 0; i < 4; i++)
        {
            // Komşu hücrenin koordinatlarını hesapla
            int nx = cell.x + directions[i].x;
            int ny = cell.y + directions[i].y;
            
            // Komşu hücrenin sınırlar içinde olup olmadığını kontrol et
            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
            {
                // Komşu hücrenin ziyaret edilmemiş olduğunu kontrol et
                if (!maze[nx, ny].visited)
                {
                    neighbors.Add(maze[nx, ny]);
                }
            }
        }
        
        return neighbors;
    }
    
    /// <summary>
    /// İki komşu hücre arasındaki duvarları kaldırır.
    /// Bu, labirentte yollar oluşturur.
    /// </summary>
    /// <param name="current">Mevcut hücre</param>
    /// <param name="next">Komşu hücre</param>
    private void RemoveWalls(Cell current, Cell next)
    {
        // İki hücre arasındaki farkı hesapla
        int dx = next.x - current.x;
        int dy = next.y - current.y;
        
        // Eğer komşu hücre mevcut hücrenin kuzeyinde ise
        if (dx == 0 && dy == 1)
        {
            current.walls[0] = false; // Mevcut hücrenin kuzey duvarını kaldır
            next.walls[2] = false;    // Komşu hücrenin güney duvarını kaldır
        }
        // Eğer komşu hücre mevcut hücrenin doğusunda ise
        else if (dx == 1 && dy == 0)
        {
            current.walls[1] = false; // Mevcut hücrenin doğu duvarını kaldır
            next.walls[3] = false;    // Komşu hücrenin batı duvarını kaldır
        }
        // Eğer komşu hücre mevcut hücrenin güneyinde ise
        else if (dx == 0 && dy == -1)
        {
            current.walls[2] = false; // Mevcut hücrenin güney duvarını kaldır
            next.walls[0] = false;    // Komşu hücrenin kuzey duvarını kaldır
        }
        // Eğer komşu hücre mevcut hücrenin batısında ise
        else if (dx == -1 && dy == 0)
        {
            current.walls[3] = false; // Mevcut hücrenin batı duvarını kaldır
            next.walls[1] = false;    // Komşu hücrenin doğu duvarını kaldır
        }
    }
    
    /// <summary>
    /// Labirentin fiziksel görünümünü oluşturur.
    /// Zemin, duvarlar, köşe kolonları ve işaretçileri yerleştirir.
    /// </summary>
    private void BuildMaze()
    {
        // Eğer parent objeler yoksa oluştur
        if (wallsParent == null)
        {
            GameObject wallsObj = new GameObject("Walls");
            wallsParent = wallsObj.transform;
            wallsParent.SetParent(transform); // transform.parent yerine SetParent kullan
        }
        
        if (floorsParent == null)
        {
            GameObject floorsObj = new GameObject("Floors");
            floorsParent = floorsObj.transform;
            floorsParent.SetParent(transform);
        }
        
        if (cornersParent == null)
        {
            GameObject cornersObj = new GameObject("Corners");
            cornersParent = cornersObj.transform;
            cornersParent.SetParent(transform);
        }
        
        // Her hücre için zemin yerleştir
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                PlaceFloor(x, y);
            }
        }
        
        // Her hücre için duvarları yerleştir
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                PlaceWalls(x, y);
            }
        }
        
        // Köşe kolonlarını yerleştir
        PlaceCorners();
        
        // Başlangıç işaretçisini yerleştir
        if (startMarker != null)
        {
            GameObject start = Instantiate(startMarker, CellToWorldPosition(startPos.x, startPos.y, 0.1f), Quaternion.identity);
            start.transform.SetParent(transform);
            start.name = "StartMarker";
            
            // MazeTrigger bileşeni ekle (yoksa)
            MazeTrigger trigger = start.GetComponent<MazeTrigger>();
            if (trigger == null)
            {
                trigger = start.AddComponent<MazeTrigger>();
                trigger.type = MazeTrigger.TriggerType.Start;
                
                // GameManager referansını ayarla
                GameManager gameManager = FindAnyObjectByType<GameManager>();
                if (gameManager != null)
                {
                    trigger.gameManager = gameManager;
                }
            }
        }
        
        // Bitiş işaretçisini yerleştir
        if (endMarker != null)
        {
            GameObject end = Instantiate(endMarker, CellToWorldPosition(endPos.x, endPos.y, 0.1f), Quaternion.identity);
            end.transform.SetParent(transform);
            end.name = "EndMarker";
            
            // MazeTrigger bileşeni ekle (yoksa)
            MazeTrigger trigger = end.GetComponent<MazeTrigger>();
            if (trigger == null)
            {
                trigger = end.AddComponent<MazeTrigger>();
                trigger.type = MazeTrigger.TriggerType.End;
                
                // GameManager referansını ayarla
                GameManager gameManager = FindAnyObjectByType<GameManager>();
                if (gameManager != null)
                {
                    trigger.gameManager = gameManager;
                }
            }
        }
    }
    
    /// <summary>
    /// Belirtilen hücre konumunda zemin yerleştirir.
    /// </summary>
    /// <param name="x">Hücrenin x koordinatı</param>
    /// <param name="y">Hücrenin y koordinatı</param>
    private void PlaceFloor(int x, int y)
    {
        if (floorPrefab != null)
        {
            // Zemin prefabını dünya konumuna yerleştir
            GameObject floor = Instantiate(floorPrefab, CellToWorldPosition(x, y, 0), Quaternion.identity);
            floor.transform.SetParent(floorsParent); // parent yerine SetParent kullan
            floor.name = "Floor_" + x + "_" + y; // Tanımlayıcı isim ver
        }
    }
    
    /// <summary>
    /// Belirtilen hücrenin duvarlarını yerleştirir.
    /// Duvarlar, hücre veri yapısındaki wall[] dizisine göre yerleştirilir.
    /// </summary>
    /// <param name="x">Hücrenin x koordinatı</param>
    /// <param name="y">Hücrenin y koordinatı</param>
    private void PlaceWalls(int x, int y)
    {
        if (wallPrefab == null) return;
        
        // Null reference hatasını önlemek için kontrol et
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            Cell cell = maze[x, y];
            
            // Hücrenin mevcut duvarlarını yerleştir
            if (cell.walls[0]) // Kuzey duvarı
            {
                GameObject wall = Instantiate(wallPrefab, CellToWorldPosition(x, y, 0) + new Vector3(0, 0, cellSize/2), Quaternion.identity);
                wall.transform.SetParent(wallsParent);
                wall.name = "Wall_N_" + x + "_" + y;
            }
            
            if (cell.walls[1]) // Doğu duvarı
            {
                GameObject wall = Instantiate(wallPrefab, CellToWorldPosition(x, y, 0) + new Vector3(cellSize/2, 0, 0), Quaternion.Euler(0, 90, 0));
                wall.transform.SetParent(wallsParent);
                wall.name = "Wall_E_" + x + "_" + y;
            }
            
            if (cell.walls[2]) // Güney duvarı
            {
                GameObject wall = Instantiate(wallPrefab, CellToWorldPosition(x, y, 0) + new Vector3(0, 0, -cellSize/2), Quaternion.identity);
                wall.transform.SetParent(wallsParent);
                wall.name = "Wall_S_" + x + "_" + y;
            }
            
            if (cell.walls[3]) // Batı duvarı
            {
                GameObject wall = Instantiate(wallPrefab, CellToWorldPosition(x, y, 0) + new Vector3(-cellSize/2, 0, 0), Quaternion.Euler(0, 90, 0));
                wall.transform.SetParent(wallsParent);
                wall.name = "Wall_W_" + x + "_" + y;
            }
        }
    }
    
    /// <summary>
    /// Labirentteki köşe kolonlarını yerleştirir.
    /// Duvar kesişim noktalarında veya labirent sınırlarında köşeler eklenir.
    /// </summary>
    private void PlaceCorners()
    {
        if (cornerPrefab == null) return;
        
        // Köşe noktalarını duvar kesişimlerini kontrol ederek oluştur
        for (int x = 0; x <= width; x++)
        {
            for (int y = 0; y <= height; y++)
            {
                // Köşe pozisyonunu hesapla (-0.5f ofset ile hücre köşelerine yerleştir)
                Vector3 cornerPos = CellToWorldPosition(x - 0.5f, y - 0.5f, 0);
                
                // Bu noktaya köşe kolonu yerleştirilmeli mi kontrol et
                bool placeCorner = false;
                
                try
                {
                    placeCorner = ShouldPlaceCorner(x, y);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Error in ShouldPlaceCorner: " + e.Message + " at x=" + x + ", y=" + y);
                    continue; // Hata durumunda bu köşeyi atla
                }
                
                if (placeCorner)
                {
                    GameObject corner = Instantiate(cornerPrefab, cornerPos, Quaternion.identity);
                    corner.transform.SetParent(cornersParent);
                    corner.name = "Corner_" + x + "_" + y;
                }
            }
        }
    }
    
    /// <summary>
    /// Belirtilen pozisyona köşe kolonu yerleştirilmeli mi kontrol eder.
    /// Duvar kesişimleri ve labirent sınırlarını dikkate alır.
    /// </summary>
    /// <param name="x">Köşe noktasının x koordinatı</param>
    /// <param name="y">Köşe noktasının y koordinatı</param>
    /// <returns>Köşe kolonu yerleştirilmeli mi</returns>
    private bool ShouldPlaceCorner(int x, int y)
    {
        // Köşe noktasının labirent sınırında olup olmadığını kontrol et
        bool isAtBoundary = (x == 0 || x == width || y == 0 || y == height);
        
        if (isAtBoundary)
        {
            // Sınırlardaki özel durumları ele al
            bool isCorner = (x == 0 && y == 0) || (x == width && y == 0) || 
                           (x == 0 && y == height) || (x == width && y == height);
            
            if (isCorner)
            {
                return true; // Labirentin dört köşesine her zaman kolon yerleştir
            }
            
            // Sınır boyunca duvar olup olmadığını kontrol et - İndeks kontrolü ekle
            if (x == 0 && y > 0 && y < height)
            {
                return (y - 1 >= 0 && maze[0, y-1].walls[3]) || (y < height && maze[0, y].walls[3]);
            }
            if (x == width && y > 0 && y < height)
            {
                return (x > 0 && y - 1 >= 0 && x - 1 < width && maze[x-1, y-1].walls[1]) || 
                       (x > 0 && y < height && x - 1 < width && maze[x-1, y].walls[1]);
            }
            if (y == 0 && x > 0 && x < width)
            {
                return (x - 1 >= 0 && maze[x-1, 0].walls[2]) || (x < width && maze[x, 0].walls[2]);
            }
            if (y == height && x > 0 && x < width)
            {
                return (y > 0 && x - 1 >= 0 && y - 1 < height && maze[x-1, y-1].walls[0]) || 
                       (x < width && y > 0 && y - 1 < height && maze[x, y-1].walls[0]);
            }
            
            return false;
        }
        else
        {
            // İç köşeler - komşu hücreleri kontrol et - İndeks kontrolü ekle
            bool hasNorthEastCorner = (x > 0 && y > 0 && x - 1 < width && y - 1 < height && maze[x-1, y-1].walls[1] && maze[x-1, y-1].walls[0]);
            bool hasNorthWestCorner = (x < width && y > 0 && y - 1 < height && maze[x, y-1].walls[3] && maze[x, y-1].walls[0]);
            bool hasSouthEastCorner = (x > 0 && y < height && x - 1 < width && maze[x-1, y].walls[1] && maze[x-1, y].walls[2]);
            bool hasSouthWestCorner = (x < width && y < height && maze[x, y].walls[3] && maze[x, y].walls[2]);
            
            return hasNorthEastCorner || hasNorthWestCorner || hasSouthEastCorner || hasSouthWestCorner;
        }
    }
    
    /// <summary>
    /// Hücre koordinatlarını dünya koordinatlarına çevirir.
    /// </summary>
    /// <param name="x">Hücrenin x koordinatı</param>
    /// <param name="y">Hücrenin y koordinatı</param>
    /// <param name="heightOffset">Yükseklik ofseti</param>
    /// <returns>Dünya koordinatı (Vector3)</returns>
    private Vector3 CellToWorldPosition(float x, float y, float heightOffset)
    {
        // Hücre koordinatlarını dünya pozisyonuna çevir
        return new Vector3(x * cellSize, heightOffset, y * cellSize);
    }
    
    /// <summary>
    /// Belirli bir seed ile labirent oluşturur.
    /// Dışarıdan erişilebilen bir fonksiyondur.
    /// </summary>
    /// <param name="newSeed">Kullanılacak seed değeri</param>
    public void GenerateMazeWithSeed(int newSeed)
    {
        seed = newSeed;
        useRandomSeed = false; // Rastgele seed kullanımını devre dışı bırak
        GenerateMaze();
    }
    
    /// <summary>
    /// Mevcut seed değerini döndürür.
    /// </summary>
    /// <returns>Aktif seed değeri</returns>
    public int GetCurrentSeed()
    {
        return seed;
    }
    
    /// <summary>
    /// Labirent hücrelerine dışarıdan erişim için yardımcı metod
    /// </summary>
    /// <returns>Labirent grid'i</returns>
    public bool IsCellWall(int x, int y, int direction)
    {
        if (x < 0 || x >= width || y < 0 || y >= height || direction < 0 || direction > 3)
            return true; // Sınırların dışı
            
        return maze[x, y].walls[direction];
    }
}