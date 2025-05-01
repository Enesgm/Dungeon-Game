using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Labirent seviyesini temsil eden sınıf.
/// Bir labirent seviyesinin temel özelliklerini saklar.
/// </summary>
[System.Serializable]
public class MazeLevel
{
    public int seed;          // Labirenti oluşturmak için kullanılan seed değeri
    public int width;         // Labirentin genişliği
    public int height;        // Labirentin yüksekliği
    public string levelName;  // Seviyenin kullanıcı dostu adı
    
    public MazeLevel(int seed, int width, int height, string levelName)
    {
        this.seed = seed;
        this.width = width;
        this.height = height;
        this.levelName = levelName;
    }
}

/// <summary>
/// Tüm labirent seviyelerini içeren koleksiyon sınıfı.
/// JSON serileştirme için kullanılır.
/// </summary>
[System.Serializable]
public class LevelCollection
{
    public List<MazeLevel> levels;
    
    public LevelCollection()
    {
        levels = new List<MazeLevel>();
    }
}

/// <summary>
/// Labirent seviyelerini yöneten script.
/// Seviyeleri kaydetme, yükleme ve UI arayüzü ile yönetme işlemlerini gerçekleştirir.
/// </summary>
public class LevelManager : MonoBehaviour
{
    public MazeGenerator mazeGenerator;  // Labirent oluşturucu referansı
    public GameObject levelSelectionPanel; // Seviye seçim paneli
    public GameObject levelButtonPrefab;   // Seviye butonları için prefab
    public Transform levelButtonContainer; // Butonların yerleştirileceği container
    public TMP_InputField levelNameInput;  // Seviye adı giriş alanı
    public TextMeshProUGUI currentSeedText; // Mevcut seed değeri text alanı
    
    // Public yapılması gerekiyor çünkü GameManager bu koleksiyona erişiyor
    [HideInInspector]
    public LevelCollection levelCollection; // Tüm seviyeleri içeren koleksiyon
    private string saveFilePath;  // Seviye kayıt dosyasının yolu
    
    /// <summary>
    /// Başlangıç işlevi. Seviye dosyasını yükler ve UI'ı günceller.
    /// </summary>
    void Start()
    {
        // Kayıt dosyası yolunu belirle
        saveFilePath = Path.Combine(Application.persistentDataPath, "mazelevels.json");
        Debug.Log("Seviye kayıt dosyası yolu: " + saveFilePath);
        
        // Var olan seviyeleri yükle veya yeni koleksiyon oluştur
        LoadLevels();
        
        // UI'ı güncelle
        UpdateCurrentSeedText();
    }
    
    /// <summary>
    /// Mevcut labirenti yeni bir seviye olarak kaydeder.
    /// </summary>
    public void SaveCurrentMazeAsLevel()
    {
        if (mazeGenerator == null)
        {
            Debug.LogError("MazeGenerator referansı bulunamadı!");
            return;
        }
        
        // Seviye adını al veya varsayılan ad oluştur
        string levelName = levelNameInput != null ? levelNameInput.text : "";
        if (string.IsNullOrEmpty(levelName))
        {
            levelName = "Level " + (levelCollection.levels.Count + 1);
        }
        
        // Mevcut labirent bilgilerinden yeni seviye oluştur
        MazeLevel newLevel = new MazeLevel(
            mazeGenerator.GetCurrentSeed(),
            mazeGenerator.width,
            mazeGenerator.height,
            levelName
        );
        
        // Koleksiyona ekle
        levelCollection.levels.Add(newLevel);
        
        // Dosyaya kaydet
        SaveLevels();
        
        // Seviye seçim panelini güncelle
        PopulateLevelSelectionPanel();
        
        Debug.Log("Yeni seviye kaydedildi: " + levelName + " seed: " + newLevel.seed);
        
        // Input alanını temizle
        if (levelNameInput != null)
        {
            levelNameInput.text = "";
        }
    }
    
    /// <summary>
    /// Rastgele yeni bir labirent oluşturur.
    /// </summary>
    public void GenerateRandomMaze()
    {
        if (mazeGenerator == null)
        {
            Debug.LogError("MazeGenerator referansı bulunamadı!");
            return;
        }
        
        mazeGenerator.useRandomSeed = true;
        mazeGenerator.GenerateMaze();
        UpdateCurrentSeedText();
    }
    
    /// <summary>
    /// Belirtilen indeksteki seviyeyi yükler.
    /// </summary>
    /// <param name="levelIndex">Yüklenecek seviye indeksi</param>
    public void LoadMazeLevel(int levelIndex)
    {
        if (levelCollection == null || levelIndex < 0 || levelIndex >= levelCollection.levels.Count)
        {
            Debug.LogError("Geçersiz seviye indeksi: " + levelIndex);
            return;
        }
        
        MazeLevel level = levelCollection.levels[levelIndex];
        
        if (mazeGenerator == null)
        {
            Debug.LogError("MazeGenerator referansı bulunamadı!");
            return;
        }
        
        // Labirent parametrelerini ayarla
        mazeGenerator.width = level.width;
        mazeGenerator.height = level.height;
        
        // Seviyenin seed'i ile labirent oluştur
        mazeGenerator.GenerateMazeWithSeed(level.seed);
        
        // UI'ı güncelle
        UpdateCurrentSeedText();
        
        // Eğer görünüyorsa seviye seçim panelini gizle
        if (levelSelectionPanel != null)
        {
            levelSelectionPanel.SetActive(false);
        }
        
        Debug.Log("Seviye yüklendi: " + level.levelName + " seed: " + level.seed);
    }
    
    /// <summary>
    /// Belirtilen indeksteki seviyeyi siler.
    /// </summary>
    /// <param name="levelIndex">Silinecek seviye indeksi</param>
    public void DeleteLevel(int levelIndex)
    {
        if (levelCollection == null || levelIndex < 0 || levelIndex >= levelCollection.levels.Count)
        {
            Debug.LogError("Geçersiz seviye indeksi: " + levelIndex);
            return;
        }
        
        string levelName = levelCollection.levels[levelIndex].levelName;
        levelCollection.levels.RemoveAt(levelIndex);
        
        // Değişiklikleri kaydet
        SaveLevels();
        
        // Seviye seçim panelini güncelle
        PopulateLevelSelectionPanel();
        
        Debug.Log("Seviye silindi: " + levelName);
    }
    
    /// <summary>
    /// Seviye seçim panelini gösterir.
    /// </summary>
    public void ShowLevelSelectionPanel()
    {
        if (levelSelectionPanel != null)
        {
            levelSelectionPanel.SetActive(true);
            PopulateLevelSelectionPanel();
        }
    }
    
    /// <summary>
    /// Seviye seçim panelini gizler.
    /// </summary>
    public void HideLevelSelectionPanel()
    {
        if (levelSelectionPanel != null)
        {
            levelSelectionPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Seviye seçim panelini kaydedilmiş seviyelerle doldurur.
    /// Her seviye için bir buton oluşturur.
    /// </summary>
    public void PopulateLevelSelectionPanel()
    {
        if (levelButtonContainer == null || levelButtonPrefab == null || levelCollection == null) 
        {
            Debug.LogWarning("Seviye butonları oluşturulamıyor. Referanslar eksik.");
            return;
        }
        
        // Mevcut butonları temizle
        foreach (Transform child in levelButtonContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Her seviye için buton oluştur
        for (int i = 0; i < levelCollection.levels.Count; i++)
        {
            MazeLevel level = levelCollection.levels[i];
            
            GameObject buttonObj = Instantiate(levelButtonPrefab, levelButtonContainer);
            Button button = buttonObj.GetComponent<Button>();
            
            // Buton metnini bul ve ayarla
            TextMeshProUGUI buttonText = null;
            
            // TextMeshProUGUI bileşenini butonun kendisinde veya çocuklarında ara
            buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            
            if (buttonText != null)
            {
                buttonText.text = level.levelName + " (" + level.width + "x" + level.height + ")";
            }
            else
            {
                // TMP bulunamazsa, eski UI.Text'i kontrol et
                Text legacyText = buttonObj.GetComponentInChildren<Text>();
                if (legacyText != null)
                {
                    legacyText.text = level.levelName + " (" + level.width + "x" + level.height + ")";
                }
                else
                {
                    Debug.LogWarning("Seviye butonunda metin bileşeni bulunamadı.");
                }
            }
            
            int levelIndex = i; // Lambda için i değerini yakala
            
            if (button != null)
            {
                // Önceki tüm listener'ları temizle
                button.onClick.RemoveAllListeners();
                
                // Yeni listener ekle
                button.onClick.AddListener(() => LoadMazeLevel(levelIndex));
            }
            
            // Eğer prefab'da varsa silme butonu ekle
            Transform deleteButtonTransform = buttonObj.transform.Find("DeleteButton");
            Button deleteButton = deleteButtonTransform != null ? deleteButtonTransform.GetComponent<Button>() : null;
            
            if (deleteButton != null)
            {
                // Önceki tüm listener'ları temizle
                deleteButton.onClick.RemoveAllListeners();
                
                // Yeni listener ekle
                deleteButton.onClick.AddListener(() => DeleteLevel(levelIndex));
            }
        }
        
        // Hiç seviye yoksa kullanıcıya bilgi ver
        if (levelCollection.levels.Count == 0)
        {
            // Bir bilgi metni oluştur
            GameObject infoTextObj = new GameObject("NoLevelsInfo");
            infoTextObj.transform.SetParent(levelButtonContainer, false);
            
            // Text veya TMP bileşenini mevcut duruma göre ekle
            if (levelButtonPrefab.GetComponentInChildren<TextMeshProUGUI>() != null)
            {
                TextMeshProUGUI infoText = infoTextObj.AddComponent<TextMeshProUGUI>();
                infoText.text = "Henüz kaydedilmiş seviye yok.\nÖnce bir labirent oluşturun ve kaydedin.";
                infoText.fontSize = 16;
                infoText.alignment = TextAlignmentOptions.Center;
            }
            else
            {
                Text infoText = infoTextObj.AddComponent<Text>();
                infoText.text = "Henüz kaydedilmiş seviye yok.\nÖnce bir labirent oluşturun ve kaydedin.";
                infoText.fontSize = 16;
                infoText.alignment = TextAnchor.MiddleCenter;
            }
        }
    }
    
    /// <summary>
    /// Kaydedilmiş seviyeleri dosyadan yükler.
    /// Dosya yoksa yeni bir seviye koleksiyonu oluşturur.
    /// </summary>
    private void LoadLevels()
    {
        try
        {
            if (File.Exists(saveFilePath))
            {
                string json = File.ReadAllText(saveFilePath);
                levelCollection = JsonUtility.FromJson<LevelCollection>(json);
                
                // Null kontrolü yap ve gerekirse yeni koleksiyon oluştur
                if (levelCollection == null)
                {
                    levelCollection = new LevelCollection();
                }
                
                Debug.Log(saveFilePath + " dosyasından " + levelCollection.levels.Count + " seviye yüklendi.");
            }
            else
            {
                levelCollection = new LevelCollection();
                Debug.Log("Kaydedilmiş seviye bulunamadı. Yeni koleksiyon oluşturuldu.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Seviye yükleme hatası: " + e.Message);
            levelCollection = new LevelCollection();
        }
    }
    
    /// <summary>
    /// Seviyeleri JSON formatında dosyaya kaydeder.
    /// </summary>
    private void SaveLevels()
    {
        try
        {
            // Kayıt dizininin var olduğundan emin ol
            string directory = Path.GetDirectoryName(saveFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            // Koleksiyonu JSON formatına dönüştür
            string json = JsonUtility.ToJson(levelCollection, true);
            
            // Dosyaya kaydet
            File.WriteAllText(saveFilePath, json);
            
            Debug.Log(saveFilePath + " dosyasına " + levelCollection.levels.Count + " seviye kaydedildi.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Seviye kaydetme hatası: " + e.Message);
        }
    }
    
    /// <summary>
    /// Mevcut seed bilgisini UI'da günceller.
    /// </summary>
    private void UpdateCurrentSeedText()
    {
        if (currentSeedText != null && mazeGenerator != null)
        {
            currentSeedText.text = "Mevcut Seed: " + mazeGenerator.GetCurrentSeed();
        }
    }
}