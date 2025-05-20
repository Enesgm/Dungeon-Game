using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    // Tek bir örnek (singleton) için static Instance
    public static HUDManager Instance { get; private set; }

    [Header("Health UI Elemanları")]
    public Slider healthBar;    // Can çubuğu Slider componenti
    public TextMeshProUGUI healthText;     // Sayısal can göstergesi

    void Awake()
    {
        // Singleton ayarı: Eğer Instance null ise bu örneği atar
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Sahne geçişinde yok etme
        }
        else
        {
            Destroy(gameObject);            // Fazladan olursa sil
        }
    }

    /// <summary>
    /// Can değiştiğinde UI'ı günceller.
    /// </summary>
    /// <param name="current">Mevcut can</param>
    /// <param name="max">Maksimum can</param>
    public void UpdateHealth(int current, int max)
    {
        // Slider'ın maksimum değerini ayarla
        healthBar.maxValue = max;
        // Slider'ın doluluk miktarını ayarla
        healthBar.value = current;
        // Sayısal göstergeyi güncelle
        healthText.text = current + " / " + max;
    }
}
