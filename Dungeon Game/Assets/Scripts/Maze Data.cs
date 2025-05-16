using UnityEngine;

[CreateAssetMenu(menuName = "Maze/MazeData")]
public class MazeData : ScriptableObject
{
    [Header("Labirent Ölçüleri")]
    public int width = 15;
    public int height = 15;

    [Header("Random Seed (opsiyonel)")]
    public int seed = 0;
    public bool useRandomSeed = true;
}
