using UnityEngine;
using UnityEngine.UI;

public class LevelSelectUI : MonoBehaviour
{
    [Header("References")]
    public LevelManager levelManager;
    public LevelDatabase database;
    public Transform content;
    public GameObject levelButtonPrefab;

    private void Start()
    {
        GenerateButtons();
    }
    
    private void OnEnable()
    {
        GenerateButtons();
    }

    private void GenerateButtons()
    {
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
        
        for (int i = 0; i < database.levels.Length; i++)
        {
            int levelIndex = i;

            GameObject obj = Instantiate(levelButtonPrefab, content);

            LevelButtonUI buttonUI = obj.GetComponent<LevelButtonUI>();

            bool unlocked = levelManager.IsLevelUnlocked(levelIndex);

            buttonUI.Setup(levelIndex, unlocked);

            Button button = obj.GetComponent<Button>();

            button.onClick.AddListener(() =>
            {
                levelManager.OnPressSelectLevel(levelIndex);
            });
        }
    }
}