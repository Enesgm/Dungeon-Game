using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    // Singleton: Her sahnede tek bir örnek olacak, diğerleri silinecek
    public static GameStateManager Instance { get; private set; }

    // Şu anki state, default Menu
    public GameState State { get; private set; } = GameState.Menu;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;                // Bu örneği global Instance yap
            DontDestroyOnLoad(gameObject);  // Sahne geçişlerinde yok etme
        }

        else
        {
            Destroy(gameObject);            // Zaten varsa yenisini sil
        }
    }

    // Başka sınıflar bu fonksiyonu çağırarak state'i değiştirecek
    public void SetState(GameState newState)
    {
        State = newState;                   // State'i güncelle
        Debug.Log($"[GameStateManager] New State -> {State}");
        OnStateChanged();                   // State değişince tetikle
    }

    // State değiştikçe UI/Ses/diğerler çalışsın diye bunları kullanacağız
    void OnStateChanged()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.ShowPanelFor(State);
    }

    void Update()
    {
        // - DEBUG TETİKLEYİCİLER -
        if (Input.GetKeyDown(KeyCode.P))
            SetState(GameState.Playing);

        if (Input.GetKeyDown(KeyCode.V))
            SetState(GameState.Victory);

        if (Input.GetKeyDown(KeyCode.G))
            SetState(GameState.GameOver);
            
        if (Input.GetKeyDown(KeyCode.Escape))
            SetState(GameState.Paused);

    }
}
