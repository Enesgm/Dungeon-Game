using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
// TextMeshPro kullanıyorsanız:
using TMPro;

public class LevelSelectUI : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform contentParent;
    public string mazeSceneName = "MazeScene";

    void Start()
    {
        // … önceki null‐check’ler …

        var allMazes = LevelManager.Instance.GetAllMazes();
        for (int i = 0; i < allMazes.Length; i++)
        {
            int levelNumber = i + 1;
            GameObject btnObj = Instantiate(buttonPrefab, contentParent);

            // Önce legacy UI.Text dene
            Text uiText = btnObj.GetComponentInChildren<Text>();
            if (uiText != null)
            {
                uiText.text = levelNumber.ToString();
            }
            else
            {
                // TextMeshProUGUI varsa onu güncelle
                TextMeshProUGUI tmp = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = levelNumber.ToString();
                }
                else
                {
                    Debug.LogError("Buton prefab’ında ne Text ne de TextMeshProUGUI bulundu!");
                }
            }

            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => OnLevelButtonClicked(levelNumber));
            else
                Debug.LogError("Button component bulunamadı!");
        }
    }

    void OnLevelButtonClicked(int levelNumber)
    {
        LevelManager.Instance.SetCurrentLevel(levelNumber);
        SceneManager.LoadScene(mazeSceneName);
    }
}
