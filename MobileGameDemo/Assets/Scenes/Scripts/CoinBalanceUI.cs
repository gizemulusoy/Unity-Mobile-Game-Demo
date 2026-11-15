using TMPro;
using UnityEngine;

public class CoinBalanceUI : MonoBehaviour
{
    [SerializeField] private TMP_Text balanceText;

    private void OnEnable()
    {
        if (balanceText == null)
            return;

        int balance = PlayerPrefs.GetInt("CoinBalance", 0);
        balanceText.text = balance.ToString();
    }
}