// Bu script Editor klasörüne konmalıdır (Assets/Editor)
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

#if UNITY_EDITOR
/// <summary>
/// MazeGenerator için özel editor penceresi.
/// Unity editor içinde labirentleri oluşturmak ve test etmek için
/// kullanıcı dostu bir arayüz sağlar.
/// </summary>
[CustomEditor(typeof(MazeGenerator))]
public class MazeGeneratorEditor : Editor
{
    // Hedef obje için property referansları
    private SerializedProperty widthProperty;
    private SerializedProperty heightProperty;
    private SerializedProperty cellSizeProperty;
    private SerializedProperty seedProperty;
    private SerializedProperty useRandomSeedProperty;
    
    // Prefab referansları
    private SerializedProperty wallPrefabProperty;
    private SerializedProperty floorPrefabProperty;
    private SerializedProperty cornerPrefabProperty;
    
    // Editor için özel değişkenler
    private MazeGenerator mazeGenerator;
    private int previewSeed;
    private bool showAdvancedOptions = false;
    
    // Editor başlangıcında bir kez çalışır
    private void OnEnable()
    {
        // Serialize edilmiş özellikleri bul
        widthProperty = serializedObject.FindProperty("width");
        heightProperty = serializedObject.FindProperty("height");
        cellSizeProperty = serializedObject.FindProperty("cellSize");
        seedProperty = serializedObject.FindProperty("seed");
        useRandomSeedProperty = serializedObject.FindProperty("useRandomSeed");
        
        // Prefab referanslarını bul
        wallPrefabProperty = serializedObject.FindProperty("wallPrefab");
        floorPrefabProperty = serializedObject.FindProperty("floorPrefab");
        cornerPrefabProperty = serializedObject.FindProperty("cornerPrefab");
        
        // Hedef objeyi al
        mazeGenerator = (MazeGenerator)target;
        
        // Editor değişkenlerini başlat
        previewSeed = mazeGenerator.GetCurrentSeed();
    }
    
    // Inspector'da çizim yapar
    public override void OnInspectorGUI()
    {
        // Stil tanımları
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 14;
        headerStyle.alignment = TextAnchor.MiddleCenter;
        headerStyle.margin = new RectOffset(0, 0, 10, 10);
        
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.padding = new RectOffset(10, 10, 10, 10);
        boxStyle.margin = new RectOffset(0, 0, 5, 5);
        
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.margin = new RectOffset(2, 2, 2, 2);
        buttonStyle.padding = new RectOffset(8, 8, 4, 4);
        
        // Property değişikliklerini takip et
        serializedObject.Update();
        
        // Başlık
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("LABIRENT OLUŞTURUCU", headerStyle);
        EditorGUILayout.Space(5);
        
        // Temel Özellikler
        EditorGUILayout.BeginVertical(boxStyle);
        EditorGUILayout.LabelField("Labirent Boyutları", EditorStyles.boldLabel);
        
        EditorGUILayout.PropertyField(widthProperty, new GUIContent("Genişlik"));
        EditorGUILayout.PropertyField(heightProperty, new GUIContent("Yükseklik"));
        EditorGUILayout.PropertyField(cellSizeProperty, new GUIContent("Hücre Boyutu"));
        
        EditorGUILayout.EndVertical();
        
        // Seed Ayarları
        EditorGUILayout.BeginVertical(boxStyle);
        EditorGUILayout.LabelField("Seed Ayarları", EditorStyles.boldLabel);
        
        EditorGUILayout.PropertyField(useRandomSeedProperty, new GUIContent("Rastgele Seed Kullan"));
        
        // Eğer rastgele seed kullanılmıyorsa seed değeri göster
        if (!useRandomSeedProperty.boolValue)
        {
            EditorGUILayout.PropertyField(seedProperty, new GUIContent("Seed Değeri"));
            
            // Seed değiştiği zaman ön izleme seed'ini güncelle
            if (seedProperty.intValue != previewSeed)
            {
                previewSeed = seedProperty.intValue;
            }
        }
        
        // Mevcut seed değerini göster
        EditorGUILayout.HelpBox("Mevcut Seed: " + mazeGenerator.GetCurrentSeed(), MessageType.Info);
        
        // Seed kopyalama butonu
        if (GUILayout.Button("Seed'i Panoya Kopyala", buttonStyle))
        {
            EditorGUIUtility.systemCopyBuffer = mazeGenerator.GetCurrentSeed().ToString();
            Debug.Log("Seed kopyalandı: " + mazeGenerator.GetCurrentSeed());
        }
        
        EditorGUILayout.EndVertical();
        
        // Prefab Referansları (katlanabilir)
        showAdvancedOptions = EditorGUILayout.Foldout(showAdvancedOptions, "Gelişmiş Ayarlar", true);
        if (showAdvancedOptions)
        {
            EditorGUILayout.BeginVertical(boxStyle);
            
            EditorGUILayout.LabelField("Prefab Referansları", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(wallPrefabProperty, new GUIContent("Duvar Prefab"));
            EditorGUILayout.PropertyField(floorPrefabProperty, new GUIContent("Zemin Prefab"));
            EditorGUILayout.PropertyField(cornerPrefabProperty, new GUIContent("Köşe Prefab"));
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Diğer Özellikler", EditorStyles.boldLabel);
            DrawPropertiesExcluding(serializedObject, new string[] { 
                "m_Script", "width", "height", "cellSize", "seed", "useRandomSeed", 
                "wallPrefab", "floorPrefab", "cornerPrefab" 
            });
            
            EditorGUILayout.EndVertical();
        }
        
        // Değişiklikleri kaydet
        serializedObject.ApplyModifiedProperties();
        
        EditorGUILayout.Space(10);
        
        // Labirent Oluşturma Butonları
        EditorGUILayout.BeginVertical(boxStyle);
        EditorGUILayout.LabelField("Labirent Oluştur", EditorStyles.boldLabel);
        
        // Rastgele Labirent Oluştur
        if (GUILayout.Button("Rastgele Labirent Oluştur", buttonStyle, GUILayout.Height(30)))
        {
            GenerateRandomMaze();
        }
        
        // Seed ile Labirent Oluştur
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Mevcut Seed ile Oluştur", buttonStyle))
        {
            GenerateMazeWithCurrentSeed();
        }
        
        if (GUILayout.Button("Seed Değerini Sıfırla", buttonStyle))
        {
            seedProperty.intValue = 0;
            serializedObject.ApplyModifiedProperties();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
        
        // Editör bilgi notu
        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox(
            "Editor modunda labirent oluştururken değişiklikleri kaydetmeyi unutmayın.\n" +
            "Sahneyi kaydetmek için Ctrl+S (Windows) veya Cmd+S (Mac) kullanın.",
            MessageType.Info
        );
    }
    
    /// <summary>
    /// Rastgele bir labirent oluşturur
    /// </summary>
    private void GenerateRandomMaze()
    {
        Undo.RecordObject(mazeGenerator, "Generate Random Maze");
        
        // Rastgele seed kullanımını aç
        mazeGenerator.useRandomSeed = true;
        
        try
        {
            // Labirenti oluştur
            mazeGenerator.GenerateMaze();
            
            // Değişiklikleri kaydet
            EditorUtility.SetDirty(mazeGenerator);
            MarkSceneDirty();
            
            // Seed değerini güncelle
            previewSeed = mazeGenerator.GetCurrentSeed();
            Debug.Log("Rastgele labirent oluşturuldu. Seed: " + previewSeed);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Labirent oluşturma hatası: " + e.Message);
        }
    }
    
    /// <summary>
    /// Mevcut seed ile labirent oluşturur
    /// </summary>
    private void GenerateMazeWithCurrentSeed()
    {
        Undo.RecordObject(mazeGenerator, "Generate Maze With Seed");
        
        // Rastgele seed kullanımını kapat
        mazeGenerator.useRandomSeed = false;
        
        // Seed değerini al
        mazeGenerator.seed = seedProperty.intValue;
        
        try
        {
            // Labirenti oluştur
            mazeGenerator.GenerateMaze();
            
            // Değişiklikleri kaydet
            EditorUtility.SetDirty(mazeGenerator);
            MarkSceneDirty();
            
            Debug.Log("Labirent oluşturuldu. Seed: " + mazeGenerator.GetCurrentSeed());
        }
        catch (System.Exception e)
        {
            Debug.LogError("Labirent oluşturma hatası: " + e.Message);
        }
    }
    
    /// <summary>
    /// Sahneyi kirli olarak işaretler (değişiklikler var)
    /// </summary>
    private void MarkSceneDirty()
    {
        if (!Application.isPlaying)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
}
#endif