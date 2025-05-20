using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Can Değerleri")]
    public int maxHealth = 100;     // Başlangıçtaki maksimum cam
    private int currentHealth;      // Anlık can değeri

    void Start()
    {
        // Oyun başladığında canı full yap ve HUD'I güncelle
        currentHealth = maxHealth;
        HUDManager.Instance.UpdateHealth(currentHealth, maxHealth);
    }

    /// <summary>
    /// Bu metod hasar aldığında çağrılacak.
    /// <summary>
    /// <param name="amount">Alınan hasar miktarı</param>
    public void TakeDamage(int amount)
    {
        // Canı azalt, 0 altına düşürme
        currentHealth = Mathf.Max(0, currentHealth - amount);

        // HUD'ı güncelle
        HUDManager.Instance.UpdateHealth(currentHealth, maxHealth);

        // Eğer can bitti ise GameOver state'ini geç
        if (currentHealth == 0)
        {
            GameStateManager.Instance.SetState(GameState.GameOver);
        }
    }
}
