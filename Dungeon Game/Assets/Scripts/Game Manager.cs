using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Oyun akışını yöneten ana script.
/// Menü geçişlerini, oyun durumlarını ve temel oyun mantığını kontrol eder.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("References")]
    public MazeGenerator mazeGenerator;   // Labirent oluşturucu referansı
    public LevelManager levelManager;     // Seviye yöneticisi referansı
    public PlayerController playerController; // Oyuncu kontrolcüsü referansı
    public Transform playerStartTransform;    // Oyuncunun başlangıç pozisyonu
    
    [Header("UI")]
    public GameObject mainMenuPanel;      // Ana menü paneli
    public GameObject gameplayPanel;      // Oyun içi panel
    public GameObject pausePanel;         // Duraklatma paneli
    public GameObject gameOverPanel;      // Oyun sonu paneli
    public GameObject winPanel;           // Kazanma paneli
    public TextMeshProUGUI timerText;     // Zamanlayıcı metin alanı
    public TextMeshProUGUI currentLevelText; // Mevcut seviye metin alanı
    
    [Header("Game Settings")]
    public bool useTimer = true;          // Zamanlayıcı kullanılsın mı
    public float timeLimit = 300f;        // Zaman sınırı (saniye)
    public GameObject endMarker;          // Bitiş noktası objesi
    
    [HideInInspector]
    public float currentTime;             // Mevcut zamanlayıcı değeri
    private bool isGamePaused = false;    // Oyun duraklatıldı mı
    private bool isGameActive = false;    // Oyun aktif mi
    [HideInInspector]
    public int currentLevelIndex = -1;    // Mevcut seviye indeksi (-1 = rastgele labirent)
    
    /// <summary>
    /// Başlangıç işlevi. UI'ı başlatır ve ana menüyü gösterir.
    /// </summary>
    void Start()
    {
        // UI'ı başlat
        ShowMainMenu();
        
        // Ana menüde imlecin görünür olmasını sağla
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    /// <summary>
    /// Her karede çağrılır. Zamanlayıcıyı günceller ve duraklatma girdisini kontrol eder.
    /// </summary>
    void Update()
    {
        if (isGameActive && !isGamePaused)
        {
            // Zamanlayıcıyı güncelle
            if (useTimer)
            {
                currentTime -= Time.deltaTime;
                UpdateTimerDisplay();
                
                // Süre doldu mu?
                if (currentTime <= 0)
                {
                    GameOver();
                }
            }
            
            // Duraklatma girdisini kontrol et
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }
    }
    
    #region Game Flow Methods
    
    /// <summary>
    /// Rastgele bir labirent ile oyunu başlatır.
    /// </summary>
    public void StartRandomMaze()
    {
        // Rastgele bir labirent oluştur
        mazeGenerator.useRandomSeed = true;
        mazeGenerator.GenerateMaze();
        
        StartGame(-1);
    }
    
    /// <summary>
    /// Belirtilen indeksteki seviyeyi yükler ve oyunu başlatır.
    /// </summary>
    /// <param name="levelIndex">Yüklenecek seviye indeksi</param>
    public void StartLevel(int levelIndex)
    {
        currentLevelIndex = levelIndex;
        levelManager.LoadMazeLevel(levelIndex);
        StartGame(levelIndex);
    }
    
    /// <summary>
    /// Oyunu başlatır, UI'ı ve oyuncu durumunu ayarlar.
    /// </summary>
    /// <param name="levelIndex">Başlatılan seviyenin indeksi</param>
    private void StartGame(int levelIndex)
    {
        // Mevcut seviye indeksini ayarla
        currentLevelIndex = levelIndex;
        
        // Ana menüyü gizle ve oyun içi UI'ı göster
        mainMenuPanel.SetActive(false);
        gameplayPanel.SetActive(true);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        winPanel.SetActive(false);
        
        // Oyuncuyu ayarla
        if (playerController != null && playerStartTransform != null)
        {
            // Oyuncuyu başlangıç pozisyonuna yerleştir
            playerController.transform.position = playerStartTransform.position;
            playerController.transform.rotation = playerStartTransform.rotation;
            playerController.ToggleCursorLock(true); // İmleci kilitle
        }
        
        // Zamanlayıcıyı sıfırla ve başlat
        if (useTimer)
        {
            currentTime = timeLimit;
            UpdateTimerDisplay();
        }
        
        // Seviye metnini güncelle
        if (currentLevelText != null)
        {
            currentLevelText.text = currentLevelIndex >= 0 ? 
                "Level: " + levelManager.levelCollection.levels[currentLevelIndex].levelName : 
                "Random Maze";
        }
        
        // Oyunu aktif olarak ayarla
        isGameActive = true;
        isGamePaused = false;
        
        // Zaman ölçeğinin 1 olduğundan emin ol (normal hız)
        Time.timeScale = 1f;
    }
    
    /// <summary>
    /// Oyun sonu durumunu aktifleştirir (kaybetme).
    /// </summary>
    public void GameOver()
    {
        isGameActive = false;
        
        // Oyun sonu panelini göster
        gameplayPanel.SetActive(false);
        gameOverPanel.SetActive(true);
        
        // İmleci göster
        if (playerController != null)
        {
            playerController.ToggleCursorLock(false);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        // Zamanı durdur
        Time.timeScale = 0f;
    }
    
    /// <summary>
    /// Oyunu kazanma durumunu aktifleştirir.
    /// Bitiş noktasına ulaşıldığında çağrılır.
    /// </summary>
    public void WinGame()
    {
        isGameActive = false;
        
        // Kazanma panelini göster
        gameplayPanel.SetActive(false);
        winPanel.SetActive(true);
        
        // İmleci göster
        if (playerController != null)
        {
            playerController.ToggleCursorLock(false);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        // Zamanı durdur
        Time.timeScale = 0f;
    }
    
    /// <summary>
    /// Oyunu duraklatır veya devam ettirir.
    /// </summary>
    public void TogglePause()
    {
        isGamePaused = !isGamePaused;
        
        if (isGamePaused)
        {
            // Oyunu duraklat
            Time.timeScale = 0f;
            pausePanel.SetActive(true);
            
            // İmleci göster
            if (playerController != null)
            {
                playerController.ToggleCursorLock(false);
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        else
        {
            // Oyuna devam et
            Time.timeScale = 1f;
            pausePanel.SetActive(false);
            
            // İmleci gizle
            if (playerController != null)
            {
                playerController.ToggleCursorLock(true);
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
    
    #endregion
    
    #region UI Methods
    
    /// <summary>
    /// Ana menüyü gösterir, diğer tüm panelleri gizler.
    /// </summary>
    public void ShowMainMenu()
    {
        // Oyun durumunu sıfırla
        isGameActive = false;
        isGamePaused = false;
        
        // Ana menüyü göster, diğerlerini gizle
        mainMenuPanel.SetActive(true);
        gameplayPanel.SetActive(false);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        winPanel.SetActive(false);
        
        // İmleci göster
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Zaman ölçeğinin 1 olduğundan emin ol
        Time.timeScale = 1f;
    }
    
    /// <summary>
    /// Zamanlayıcı görüntüsünü günceller.
    /// </summary>
    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            // Dakika ve saniyeyi hesapla
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            
            // Süre azaldığında rengi değiştir
            if (currentTime <= 30f)
            {
                timerText.color = Color.red;
            }
            else
            {
                timerText.color = Color.white;
            }
        }
    }
    
    #endregion
    
    #region Button Methods
    
    /// <summary>
    /// Duraklatma menüsündeki Devam Et butonuna tıklandığında çağrılır.
    /// </summary>
    public void OnResumeButtonClicked()
    {
        TogglePause();
    }
    
    /// <summary>
    /// Yeniden Başlat butonuna tıklandığında çağrılır.
    /// </summary>
    public void OnRestartButtonClicked()
    {
        if (currentLevelIndex >= 0)
        {
            StartLevel(currentLevelIndex);
        }
        else
        {
            StartRandomMaze();
        }
    }
    
    /// <summary>
    /// Ana Menü butonuna tıklandığında çağrılır.
    /// </summary>
    public void OnMainMenuButtonClicked()
    {
        ShowMainMenu();
    }
    
    /// <summary>
    /// Çıkış butonuna tıklandığında çağrılır.
    /// </summary>
    public void OnExitButtonClicked()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    /// <summary>
    /// Rastgele Labirent butonuna tıklandığında çağrılır.
    /// </summary>
    public void OnPlayRandomMazeButtonClicked()
    {
        StartRandomMaze();
    }
    
    /// <summary>
    /// Seviye Seç butonuna tıklandığında çağrılır.
    /// </summary>
    public void OnSelectLevelButtonClicked()
    {
        levelManager.ShowLevelSelectionPanel();
    }
    
    #endregion
}