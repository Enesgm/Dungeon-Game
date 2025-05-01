using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Kullanıcı arayüzünü yöneten script.
/// Panelleri, butonları ve göstergeleri kontrol eder.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject levelSelectionPanel;
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject settingsPanel;
    
    [Header("Main Menu")]
    [SerializeField] private Button playRandomButton;
    [SerializeField] private Button selectLevelButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;
    
    [Header("Level Selection")]
    [SerializeField] private Button backFromLevelSelectionButton;
    [SerializeField] private Button createNewLevelButton;
    
    [Header("Game UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private TextMeshProUGUI seedInfoText;
    
    [Header("Pause Menu")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuFromPauseButton;
    
    [Header("Game Over")]
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private Button tryAgainButton;
    [SerializeField] private Button mainMenuFromGameOverButton;
    
    [Header("Win Screen")]
    [SerializeField] private TextMeshProUGUI winTimeText;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button mainMenuFromWinButton;
    
    [Header("Settings")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Dropdown resolutionDropdown;
    [SerializeField] private Button applySettingsButton;
    [SerializeField] private Button backFromSettingsButton;
    
    [Header("Dependencies")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private LevelManager levelManager;
    
    // Çözünürlük ayarları için
    private Resolution[] resolutions;
    
    private void Awake()
    {
        FindReferences();
    }
    
    private void Start()
    {
        SetupResolutions();
        AttachButtonEvents();
        LoadSettings();
        ShowMainMenu();
    }
    
    /// <summary>
    /// Eksik referansları otomatik bulur
    /// </summary>
    private void FindReferences()
    {
        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
            if (gameManager != null)
            {
                Debug.Log("GameManager referansı otomatik bulundu");
            }
        }
        
        if (levelManager == null)
        {
            levelManager = FindAnyObjectByType<LevelManager>();
            if (levelManager != null)
            {
                Debug.Log("LevelManager referansı otomatik bulundu");
            }
        }
    }
    
    /// <summary>
    /// Çözünürlük seçeneklerini hazırlar
    /// </summary>
    private void SetupResolutions()
    {
        if (resolutionDropdown == null) return;
        
        // Çözünürlükleri al
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = $"{resolutions[i].width} x {resolutions[i].height}";
            options.Add(option);
            
            if (resolutions[i].width == Screen.currentResolution.width && 
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        
        // Tüm çözünürlükleri ekle
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }
    
    /// <summary>
    /// Buton olaylarını ekler
    /// </summary>
    private void AttachButtonEvents()
    {
        // Ana Menü
        AddButtonListener(playRandomButton, OnPlayRandomClicked);
        AddButtonListener(selectLevelButton, OnSelectLevelClicked);
        AddButtonListener(settingsButton, OnSettingsClicked);
        AddButtonListener(exitButton, OnExitClicked);
        
        // Seviye Seçimi
        AddButtonListener(backFromLevelSelectionButton, OnBackFromLevelSelectionClicked);
        AddButtonListener(createNewLevelButton, OnCreateNewLevelClicked);
        
        // Duraklatma
        AddButtonListener(resumeButton, OnResumeClicked);
        AddButtonListener(restartButton, OnRestartClicked);
        AddButtonListener(mainMenuFromPauseButton, OnMainMenuClicked);
        
        // Oyun Sonu
        AddButtonListener(tryAgainButton, OnTryAgainClicked);
        AddButtonListener(mainMenuFromGameOverButton, OnMainMenuClicked);
        
        // Kazanma
        AddButtonListener(nextLevelButton, OnNextLevelClicked);
        AddButtonListener(mainMenuFromWinButton, OnMainMenuClicked);
        
        // Ayarlar
        AddButtonListener(applySettingsButton, OnApplySettingsClicked);
        AddButtonListener(backFromSettingsButton, OnBackFromSettingsClicked);
    }
    
    /// <summary>
    /// Butona olay ekler (null kontrolü ile)
    /// </summary>
    private void AddButtonListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }
    }
    
    #region Panel Management
    
    /// <summary>
    /// Ana menü panelini gösterir
    /// </summary>
    public void ShowMainMenu()
    {
        HideAllPanels();
        ShowPanel(mainMenuPanel);
    }
    
    /// <summary>
    /// Seviye seçim panelini gösterir
    /// </summary>
    public void ShowLevelSelection()
    {
        HideAllPanels();
        ShowPanel(levelSelectionPanel);
        
        if (levelManager != null)
        {
            levelManager.PopulateLevelSelectionPanel();
        }
    }
    
    /// <summary>
    /// Oyun içi paneli gösterir
    /// </summary>
    public void ShowGameplay()
    {
        HideAllPanels();
        ShowPanel(gameplayPanel);
    }
    
    /// <summary>
    /// Duraklatma panelini gösterir
    /// </summary>
    public void ShowPauseMenu()
    {
        ShowPanel(pausePanel);
    }
    
    /// <summary>
    /// Duraklatma panelini gizler
    /// </summary>
    public void HidePauseMenu()
    {
        HidePanel(pausePanel);
    }
    
    /// <summary>
    /// Oyun sonu panelini gösterir
    /// </summary>
    public void ShowGameOver()
    {
        HideAllPanels();
        ShowPanel(gameOverPanel);
    }
    
    /// <summary>
    /// Kazanma panelini gösterir
    /// </summary>
    public void ShowWin()
    {
        HideAllPanels();
        ShowPanel(winPanel);
        
        if (winTimeText != null && gameManager != null && gameManager.useTimer)
        {
            float timeUsed = gameManager.timeLimit - gameManager.currentTime;
            int minutes = Mathf.FloorToInt(timeUsed / 60f);
            int seconds = Mathf.FloorToInt(timeUsed % 60f);
            winTimeText.text = $"Süre: {minutes:00}:{seconds:00}";
        }
    }
    
    /// <summary>
    /// Ayarlar panelini gösterir
    /// </summary>
    public void ShowSettings()
    {
        HideAllPanels();
        ShowPanel(settingsPanel);
        UpdateSettingsUI();
    }
    
    /// <summary>
    /// Tüm panelleri gizler
    /// </summary>
    private void HideAllPanels()
    {
        HidePanel(mainMenuPanel);
        HidePanel(levelSelectionPanel);
        HidePanel(gameplayPanel);
        HidePanel(pausePanel);
        HidePanel(gameOverPanel);
        HidePanel(winPanel);
        HidePanel(settingsPanel);
    }
    
    /// <summary>
    /// Belirli bir paneli gösterir
    /// </summary>
    private void ShowPanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }
    }
    
    /// <summary>
    /// Belirli bir paneli gizler
    /// </summary>
    private void HidePanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }
    
    #endregion
    
    #region Button Event Handlers
    
    // Ana Menü
    private void OnPlayRandomClicked()
    {
        if (gameManager != null)
        {
            gameManager.StartRandomMaze();
        }
    }
    
    private void OnSelectLevelClicked()
    {
        ShowLevelSelection();
    }
    
    private void OnSettingsClicked()
    {
        ShowSettings();
    }
    
    private void OnExitClicked()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    // Seviye Seçimi
    private void OnBackFromLevelSelectionClicked()
    {
        ShowMainMenu();
    }
    
    private void OnCreateNewLevelClicked()
    {
        if (levelManager != null)
        {
            levelManager.SaveCurrentMazeAsLevel();
        }
    }
    
    // Duraklatma
    private void OnResumeClicked()
    {
        if (gameManager != null)
        {
            gameManager.TogglePause();
        }
        else
        {
            HidePauseMenu();
        }
    }
    
    private void OnRestartClicked()
    {
        if (gameManager != null)
        {
            gameManager.OnRestartButtonClicked();
        }
    }
    
    private void OnMainMenuClicked()
    {
        if (gameManager != null)
        {
            gameManager.ShowMainMenu();
        }
        else
        {
            ShowMainMenu();
        }
    }
    
    // Oyun Sonu
    private void OnTryAgainClicked()
    {
        if (gameManager != null)
        {
            gameManager.OnRestartButtonClicked();
        }
    }
    
    // Kazanma
    private void OnNextLevelClicked()
    {
        if (gameManager != null && levelManager != null)
        {
            int currentIndex = gameManager.currentLevelIndex;
            
            if (currentIndex >= 0 && currentIndex < levelManager.levelCollection.levels.Count - 1)
            {
                gameManager.StartLevel(currentIndex + 1);
            }
            else
            {
                ShowLevelSelection();
            }
        }
        else
        {
            ShowLevelSelection();
        }
    }
    
    // Ayarlar
    private void OnApplySettingsClicked()
    {
        ApplySettings();
    }
    
    private void OnBackFromSettingsClicked()
    {
        ShowMainMenu();
    }
    
    #endregion
    
    #region Settings Management
    
    /// <summary>
    /// Kaydedilmiş ayarları yükler
    /// </summary>
    private void LoadSettings()
    {
        // Ses ayarları
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        }
        
        // Tam ekran ayarı
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
            Screen.fullScreen = fullscreenToggle.isOn;
        }
        
        // Çözünürlük ayarı
        if (resolutionDropdown != null && resolutions != null && resolutions.Length > 0)
        {
            int resIndex = PlayerPrefs.GetInt("ResolutionIndex", -1);
            
            if (resIndex >= 0 && resIndex < resolutionDropdown.options.Count)
            {
                resolutionDropdown.value = resIndex;
                resolutionDropdown.RefreshShownValue();
                
                if (resIndex < resolutions.Length)
                {
                    Screen.SetResolution(resolutions[resIndex].width, 
                                         resolutions[resIndex].height, 
                                         Screen.fullScreen);
                }
            }
        }
    }
    
    /// <summary>
    /// Ayarları uygular ve kaydeder
    /// </summary>
    private void ApplySettings()
    {
        // Ses ayarları
        if (musicVolumeSlider != null)
        {
            PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
            // TODO: Ses sistemine uygula
        }
        
        if (sfxVolumeSlider != null)
        {
            PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
            // TODO: Ses sistemine uygula
        }
        
        // Tam ekran ayarı
        if (fullscreenToggle != null)
        {
            PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);
            Screen.fullScreen = fullscreenToggle.isOn;
        }
        
        // Çözünürlük ayarı
        if (resolutionDropdown != null && resolutions != null && resolutions.Length > 0)
        {
            int resIndex = resolutionDropdown.value;
            PlayerPrefs.SetInt("ResolutionIndex", resIndex);
            
            if (resIndex < resolutions.Length)
            {
                Screen.SetResolution(resolutions[resIndex].width, 
                                     resolutions[resIndex].height, 
                                     Screen.fullScreen);
            }
        }
        
        // Ayarları kaydet
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Ayarlar UI'ını günceller
    /// </summary>
    private void UpdateSettingsUI()
    {
        // Ses ayarları
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        }
        
        // Tam ekran ayarı
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        }
        
        // Çözünürlük ayarı
        if (resolutionDropdown != null)
        {
            int resIndex = PlayerPrefs.GetInt("ResolutionIndex", -1);
            
            if (resIndex >= 0 && resIndex < resolutionDropdown.options.Count)
            {
                resolutionDropdown.value = resIndex;
                resolutionDropdown.RefreshShownValue();
            }
        }
    }
    
    #endregion
    
    #region UI Updates
    
    /// <summary>
    /// Zamanlayıcıyı günceller
    /// </summary>
    public void UpdateTimer(float currentTime)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
            
            timerText.color = (currentTime <= 30f) ? Color.red : Color.white;
        }
    }
    
    /// <summary>
    /// Seviye adını günceller
    /// </summary>
    public void UpdateLevelName(string levelName)
    {
        if (levelNameText != null)
        {
            levelNameText.text = levelName;
        }
    }
    
    /// <summary>
    /// Seed bilgisini günceller
    /// </summary>
    public void UpdateSeedInfo(int seed)
    {
        if (seedInfoText != null)
        {
            seedInfoText.text = $"Seed: {seed}";
        }
    }
    
    #endregion
}