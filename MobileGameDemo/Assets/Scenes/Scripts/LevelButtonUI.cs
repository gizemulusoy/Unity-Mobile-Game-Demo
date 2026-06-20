using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButtonUI : MonoBehaviour
{
    public TMP_Text levelNumberText;
    public TMP_Text lockText;

    
    public void Setup(int levelIndex, bool unlocked)
    {
        levelNumberText.text = "Level " + (levelIndex + 1);

        lockText.text = unlocked ? "OPEN" : "LOCKED";
        lockText.gameObject.SetActive(true);

        GetComponent<Button>().interactable = unlocked;
    }
}