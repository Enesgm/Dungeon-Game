using System.Linq;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    private MazeData[] allMazes;
    public int CurrentLevelIndex { get; private set; } = 1;

    void Awake()
    {
        // … Resources.LoadAll vb. kod …
        allMazes = Resources.LoadAll<MazeData>("MazeData")
                         .OrderBy(m => m.name)
                         .ToArray();
        Debug.Log($"[LevelManager] Yüklenen MazeData sayısı: {allMazes.Length}");

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Resources/MazeData içindeki tüm MazeData asset’lerini yükle
            allMazes = Resources
                .LoadAll<MazeData>("MazeData")
                .OrderBy(m => m.name)
                .ToArray();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public MazeData[] GetAllMazes() => allMazes;
    public void SetCurrentLevel(int levelNumber) =>
        CurrentLevelIndex = Mathf.Clamp(levelNumber, 1, allMazes.Length);
    public MazeData GetCurrentMazeData() => allMazes[CurrentLevelIndex - 1];
}
