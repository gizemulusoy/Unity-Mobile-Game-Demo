using UnityEngine;
using TMPro;

public class MovesUI : MonoBehaviour
{
    [Header("TMP Reference")]
    public TextMeshProUGUI movesText;

    private int moves;

    public void SetMoves(int value)
    {
        moves = Mathf.Max(0, value);
        movesText.text = $"Moves: {moves}";
    }
}