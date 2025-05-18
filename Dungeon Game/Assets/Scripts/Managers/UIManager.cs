using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Paneller")]
    public GameObject victoryPanel;
    public GameObject gameOverPanel;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // GameState değiştiğinde çağrılacak
    public void ShowPanelFor(GameState state)
    {
        victoryPanel.SetActive(state == GameState.Victory);
        gameOverPanel.SetActive(state == GameState.GameOver);
    }
}
