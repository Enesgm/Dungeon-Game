using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
// Labirent boyutları ve seed
public int width = 15;
// Labirent genişliği (x ekseni)
public int height = 15;
// Labirent yüksekliği (z ekseni)
public float cellSize = 1.2f;
public int seed = 12345; // Aynı labirenti tekrar üretmek için seed (tanımlı olduğundan emin olun)
// Prefab'ler (Unity'de sahneye yerleştirilecek objeler)
public GameObject tWallPrefab; // T duvar
public GameObject lWallPrefab; // L duvar
public GameObject windowWallPrefab; // Pencereli duvar
public GameObject normalWallPrefab; // Normal duvar
public GameObject halfWallPrefab; // Yarım duvar (sonu olan)
public GameObject brokenWallPrefab; // Kırık duvar
public GameObject bookWallPrefab; // Kitaplı duvar
public GameObject midBrokenWallPrefab; // Ortası kırık duvar
public GameObject floorPrefab; // Zemin
public GameObject keyPrefab; // Anahtar
public GameObject doorWallPrefab; // Kapılı duvar (çıkış)
public GameObject[] decorations; // Süs eşyaları (9 adet)
public GameObject torchPrefab; // Meşale
public GameObject spikeTrapPrefab; // Yerden iğne tuzağı
public GameObject axeTrapPrefab; // Tavandan balta tuzağı
public GameObject bulletTrapPrefab; // Karşıdan mermi tuzağı

private int[,] maze; // Labirent haritası: 0 yol, 1 duvar
private List<Vector2Int> directions = new List<Vector2Int>
{
    new Vector2Int(0, 1), // sağ
    new Vector2Int(0, -1), // sol
    new Vector2Int(1, 0), // aşağı
    new Vector2Int(-1, 0) // yukarı
};
private int playerHealth = 100; // Oyuncunun canı (maks. 100)
private bool hasKey = false; // Anahtarı aldı mı?

// Oyun başlangıcında labirenti ve objeleri oluştur
void Start()
{
    // Seed ile rastgeleliği kontrol et (her oynayışta aynı labirent)
    Random.InitState(seed);

    // Labirenti oluştur ve objeleri yerleştir
    GenerateMaze();
    PlaceWallsAndFloor();
    PlaceDecorationsAndTraps();
    PlaceKeyAndDoor();
}

// Backtracking algoritması ile labirenti oluştur
void GenerateMaze()
{
    maze = new int[width, height];
    // Başlangıçta tüm hücreler duvar (1)
    for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
            maze[x, y] = 1;

    // Başlangıç noktası (0,0,0) yakınında
    Vector2Int start = new Vector2Int(1, 1);
    maze[start.x, start.y] = 0; // Başlangıç noktası yol
    RecursiveBacktrack(start);

    // Labirentin doğru şekilde oluşturulduğunu kontrol et (debug için)
    for (int x = 0; x < width; x++)
    {
        for (int y = 0; y < height; y++)
        {
            Debug.Log($"Hücre ({x},{y}): {(maze[x, y] == 1 ? "Duvar" : "Yol")}");
        }
    }
}

// Rekürsif backtracking: Rastgele yollar oluştur
void RecursiveBacktrack(Vector2Int pos)
{
    // Yönleri rastgele sırala
    List<Vector2Int> shuffledDirections = new List<Vector2Int>(directions);
    shuffledDirections.Sort((a, b) => Random.Range(-1, 2));

    foreach (Vector2Int dir in shuffledDirections)
    {
        Vector2Int next = pos + dir * 2; // 2 birim ileriye git (arada duvar var)
        if (IsInBounds(next) && maze[next.x, next.y] == 1)
        {
            maze[next.x, next.y] = 0; // Yeni hücreyi yol yap
            maze[pos.x + dir.x, pos.y + dir.y] = 0; // Aradaki duvarı kaldır
            RecursiveBacktrack(next);
        }
    }
}

// Sınır kontrolü
bool IsInBounds(Vector2Int pos)
{
    return pos.x > 0 && pos.x < width - 1 && pos.y > 0 && pos.y < height - 1;
}

// Duvarları ve zemini yerleştir (hücre boyutu dikkate alınarak)
void PlaceWallsAndFloor()
{
    for (int x = 0; x < width; x++)
    {
        for (int y = 0; y < height; y++)
        {
            // Hücre boyutuna göre konum hesapla
            Vector3 position = new Vector3(x * cellSize, 0, y * cellSize);
            if (maze[x, y] == 1) // Duvar
            {
                // Debug çizgisi ekle (konumları kontrol için)
                Debug.DrawLine(position, position + Vector3.up * 3, Color.red, 100f);

                // Dış duvarlar
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    // Köşelerde L duvar
                    if ((x == 0 && y == 0) || (x == 0 && y == height - 1) ||
                        (x == width - 1 && y == 0) || (x == width - 1 && y == height - 1))
                    {
                        GameObject wall = Instantiate(lWallPrefab, position, Quaternion.identity);
                        // Köşelere göre yönlendirme
                        if (x == 0 && y == 0) wall.transform.Rotate(0, 180, 0);
                        else if (x == 0 && y == height - 1) wall.transform.Rotate(0, 90, 0);
                        else if (x == width - 1 && y == 0) wall.transform.Rotate(0, 270, 0);
                    }
                    // Diğer dış duvarlar: Pencereli ve normal duvarlar rastgele
                    else
                    {
                        int rand = Random.Range(0, 2);
                        GameObject wallPrefab = rand == 0 ? windowWallPrefab : normalWallPrefab;
                        Instantiate(wallPrefab, position, Quaternion.identity);
                    }
                }
                else // İç duvarlar
                {
                    // Yarım duvar kontrolü: Sonrası boş bırakılacak
                    if (Random.value < 0.2f) // %20 ihtimalle yarım duvar
                    {
                        Instantiate(halfWallPrefab, position, Quaternion.identity);
                        // Sonrasına duvar koymamak için çevresini yol yap
                        foreach (Vector2Int dir in directions)
                        {
                            Vector2Int next = new Vector2Int(x, y) + dir;
                            if (IsInBounds(next) && maze[next.x, next.y] == 1)
                                maze[next.x, next.y] = 0;
                        }
                    }
                    else
                    {
                        // Diğer iç duvar türleri
                        int rand = Random.Range(0, 6);
                        GameObject wallPrefab = normalWallPrefab;
                        if (rand == 0) wallPrefab = tWallPrefab;
                        else if (rand == 1) wallPrefab = lWallPrefab;
                        else if (rand == 2) wallPrefab = brokenWallPrefab;
                        else if (rand == 3) wallPrefab = bookWallPrefab;
                        else if (rand == 4) wallPrefab = midBrokenWallPrefab;

                        GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity);
                        // T ve L duvarları devam edecek şekilde yönlendir
                        if (wallPrefab == tWallPrefab || wallPrefab == lWallPrefab)
                        {
                            wall.transform.Rotate(0, Random.Range(0, 4) * 90, 0);
                        }
                    }
                }
            }
            else // Zemin
            {
                Instantiate(floorPrefab, position, Quaternion.identity);
            }
        }
    }
}

// Eşyaları, meşaleleri ve tuzakları yerleştir
void PlaceDecorationsAndTraps()
{
    // 9 süs eşyası: Yoğun olmayacak şekilde
    for (int i = 0; i < 9; i++)
    {
        Vector2Int pos = GetRandomPathPosition();
        Instantiate(decorations[Random.Range(0, decorations.Length)], new Vector3(pos.x * cellSize, 0, pos.y * cellSize), Quaternion.identity);
    }

    // Meşaleler: Duvar kenarlarına, karanlık olmayacak şekilde
    for (int i = 0; i < 3; i++) // 3 meşale yeterli
    {
        Vector2Int torchPos = GetRandomPathPositionNearWall();
        Instantiate(torchPrefab, new Vector3(torchPos.x * cellSize, 1, torchPos.y * cellSize), Quaternion.identity);
    }

    // Tuzaklar: 1 iğne, 1 balta, 1 mermi
    // İğne: Zeminden
    Vector2Int spikePos = GetRandomPathPosition();
    Instantiate(spikeTrapPrefab, new Vector3(spikePos.x * cellSize, 0, spikePos.y * cellSize), Quaternion.identity);

    // Balta: Tavandan
    Vector2Int axePos = GetRandomPathPosition();
    Instantiate(axeTrapPrefab, new Vector3(axePos.x * cellSize, 2, axePos.y * cellSize), Quaternion.identity); // Yükseklik 2

    // Mermi: Karşıdan gelecek
    Vector2Int bulletPos = GetRandomPathPositionNearWall();
    GameObject bullet = Instantiate(bulletTrapPrefab, new Vector3(bulletPos.x * cellSize, 1, bulletPos.y * cellSize), Quaternion.identity);
    // Mermi yönü rastgele
    bullet.transform.Rotate(0, Random.Range(0, 4) * 90, 0);
}

// Anahtar ve kapıyı yerleştir
void PlaceKeyAndDoor()
{
    // Anahtar: Çok kolay bir yere olmasın (başlangıçtan uzak)
    Vector2Int keyPos;
    do
    {
        keyPos = GetRandomPathPosition();
    } while (Vector2Int.Distance(keyPos, new Vector2Int(1, 1)) < 5); // Başlangıçtan en az 5 birim uzak
    Instantiate(keyPrefab, new Vector3(keyPos.x * cellSize, 0, keyPos.y * cellSize), Quaternion.identity);

    // Kapı: (15,0,15) - Sağ alt köşe
    Vector3 doorPos = new Vector3((width - 1) * cellSize, 0, (height - 1) * cellSize);
    Instantiate(doorWallPrefab, doorPos, Quaternion.identity);
}

// Rastgele bir yol pozisyonu bul
Vector2Int GetRandomPathPosition()
{
    Vector2Int pos;
    int attempts = 0;
    do
    {
        pos = new Vector2Int(Random.Range(1, width - 1), Random.Range(1, height - 1));
        attempts++;
    } while (maze[pos.x, pos.y] != 0 && attempts < 100);
    return pos;
}

// Duvar kenarında rastgele bir yol pozisyonu bul
Vector2Int GetRandomPathPositionNearWall()
{
    Vector2Int pos;
    int attempts = 0;
    do
    {
        pos = new Vector2Int(Random.Range(1, width - 1), Random.Range(1, height - 1));
        attempts++;
    } while (maze[pos.x, pos.y] != 0 || !IsNearWall(pos) && attempts < 100);
    return pos;
}

// Duvar kenarında mı kontrol et
bool IsNearWall(Vector2Int pos)
{
    return maze[pos.x + 1, pos.y] == 1 || maze[pos.x - 1, pos.y] == 1 ||
           maze[pos.x, pos.y + 1] == 1 || maze[pos.x, pos.y - 1] == 1;
}

// Tuzakla çarpışma kontrolü
void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("SpikeTrap") || other.CompareTag("AxeTrap") || other.CompareTag("BulletTrap"))
    {
        playerHealth -= 20; // Her tuzak 20 can götürür
        Debug.Log("Can: " + playerHealth);
        if (playerHealth <= 0)
        {
            Debug.Log("Oyun Bitti!");
            // Oyun sonu mantığı burada
        }
    }
    else if (other.CompareTag("Key"))
    {
        hasKey = true;
        Destroy(other.gameObject); // Anahtarı al
        Debug.Log("Anahtar alındı!");
    }
    else if (other.CompareTag("Door"))
    {
        if (hasKey)
        {
            Debug.Log("Kapı açıldı, tebrikler!");
            // Kapıyı aç ve oyunu bitir
        }
        else
        {
            Debug.Log("Anahtar gerekiyor!");
        }
    }
}
}
