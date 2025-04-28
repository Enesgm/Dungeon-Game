using UnityEngine;
using UnityEditor;
using System.IO;

public class MeshSaver : MonoBehaviour
{

    [MenuItem("Tools/Save Selected Mesh")]
    static void SaveSelectedMesh()
    {
        if (Selection.activeGameObject == null) return;

        MeshFilter meshFilter = Selection.activeGameObject.GetComponent<MeshFilter>();
        if (meshFilter == null) return;

        Mesh mesh = meshFilter.sharedMesh;
        if (mesh == null) return;

        // 1. Meshes klasörünü oluştur (varsa sorun yok)
        string meshesFolder = "Assets/Meshes";
        if (!AssetDatabase.IsValidFolder(meshesFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Meshes");
        }

        // 2. Mesh'i kaydet
        Mesh newMesh = Object.Instantiate(mesh);
        string meshName = Selection.activeGameObject.name; // Objeye göre isim
        string path = $"{meshesFolder}/{meshName}.asset";

        AssetDatabase.CreateAsset(newMesh, path);
        AssetDatabase.SaveAssets();

        Debug.Log("Mesh saved to " + path);
    }
}
