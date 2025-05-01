using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Labirentteki özel bölgeleri (başlangıç, bitiş, kontrol noktaları, tuzaklar) yönetir.
/// Oyuncunun bu bölgelere girmesini algılar ve ilgili işlemleri gerçekleştirir.
/// </summary>
public class MazeTrigger : MonoBehaviour
{
    /// <summary>
    /// Tetikleyici türleri - her tür farklı bir işlevi temsil eder
    /// </summary>
    public enum TriggerType { Start, End, Checkpoint, Trap }
    
    public TriggerType type;              // Bu tetikleyicinin türü
    public GameManager gameManager;       // Oyun yöneticisi referansı
    public float triggerDelay = 0.5f;     // Tetiklenme gecikmesi (saniye)
    
    private bool triggered = false;       // Tetikleyicinin aktif edilip edilmediği
    
    /// <summary>
    /// Tetikleyici bölgesine bir nesne girdiğinde çağrılır.
    /// Oyuncunun girişini algılar ve türüne göre uygun işlemi gerçekleştirir.
    /// </summary>
    /// <param name="other">Tetikleyiciye giren nesnenin Collider bileşeni</param>
    void OnTriggerEnter(Collider other)
    {
        if (triggered) return; // Zaten tetiklendiyse işlem yapma
        
        // Tetikleyiciye giren nesnenin oyuncu olup olmadığını kontrol et
        if (other.CompareTag("Player"))
        {
            triggered = true;
            
            // Tetikleyici türüne göre farklı işlemler yap
            switch (type)
            {
                case TriggerType.Start:
                    // Başlangıç noktasında özel bir işlem yok
                    Debug.Log("Player entered the start position");
                    break;
                    
                case TriggerType.End:
                    // Bitiş noktasına ulaşıldı, oyunu kazanma durumuna geç
                    Debug.Log("Player reached the end of the maze!");
                    if (gameManager != null)
                    {
                        StartCoroutine(DelayedWin());
                    }
                    break;
                    
                case TriggerType.Checkpoint:
                    // Kontrol noktası işlevselliği buraya eklenebilir
                    Debug.Log("Player reached a checkpoint");
                    break;
                    
                case TriggerType.Trap:
                    // Tuzak işlevselliği
                    Debug.Log("Player triggered a trap!");
                    // Hasar, yavaşlatma, teleport gibi tuzak efektleri eklenebilir
                    break;
            }
        }
    }
    
    /// <summary>
    /// Kazanma durumunu gecikmeyle tetikler.
    /// Bu, oyuncunun bitiş noktasına ulaştığını anlamasına izin verir.
    /// </summary>
    IEnumerator DelayedWin()
    {
        // Kazanma durumunu tetiklemeden önce bekle
        yield return new WaitForSeconds(triggerDelay);
        gameManager.WinGame();
    }
}