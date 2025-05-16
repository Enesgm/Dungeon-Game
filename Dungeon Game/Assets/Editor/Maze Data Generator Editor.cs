using UnityEngine;
using UnityEditor;

public class MazeDataGeneratorEditor : EditorWindow
{
    // Oluşturulacak level sayısı
    private int levelCount = 10;
    // Oluşturulacak MazeData'ların saklanacağı klasör
    private string targetFolder = "Assets/MazeData";

    [MenuItem("Tools/Maze enerator")]
    public static void ShowWindow()
    {
        GetWindow<MazeDataGeneratorEditor>("MazeData Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Otomatik MazeData Oluşturucu", EditorStyles.boldLabel);

        levelCount = EditorGUILayout.IntField("Level Sayısı", levelCount);
        targetFolder = EditorGUILayout.TextField("Hedef Klasör", targetFolder);

        if (GUILayout.Button("MazeData Oluştur"))
        {
            GenerateMazeDataAssets();
        }
    }

    void GenerateMazeDataAssets()
    {
        // Klasörün varlığına bak, yoksa oluştur
        if (!AssetDatabase.IsValidFolder(targetFolder))
        {
            AssetDatabase.CreateFolder("Assets", "MazeData");
        }

        for (int i = 1; i <= levelCount; i++)
        {
            // Yeni ScriptableObject örneği
            var data = ScriptableObject.CreateInstance<MazeData>();
            data.width = 15;
            data.height = 15;
            data.useRandomSeed = false;
            data.seed = i;

            // Dosya adı: Level1.asset, Level2.asset, ...
            string assetPath = $"{targetFolder}/Level{i}.asset";
            AssetDatabase.CreateAsset(data, assetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"{levelCount} adet MazeData asset'i oluşturuldu: {targetFolder}");
    }
}
