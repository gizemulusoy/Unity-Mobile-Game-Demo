using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    [SerializeField] private BoosterManager boosterManager;
    [SerializeField] private BoosterType boosterType;
    [SerializeField] private int price = 50;
    [SerializeField] private TMP_Text ownedText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button buyButton;

    private void OnEnable()
    {
        RefreshUI();
    }

    private void Update()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (boosterManager == null)
            return;

        int stock = boosterManager.GetStock(boosterType);
        int balance = PlayerPrefs.GetInt("CoinBalance", 0);

        string ownedLabel = "Owned: " + stock;
        string priceLabel = price.ToString();

        if (ownedText != null && ownedText.text != ownedLabel)
            ownedText.text = ownedLabel;

        if (priceText != null && priceText.text != priceLabel)
            priceText.text = priceLabel;

        if (buyButton != null)
        {
            buyButton.interactable =
                price > 0 && balance >= price && stock < int.MaxValue;
        }
    }

    public void Buy()
    {
        if (boosterManager == null)
            return;

        if (boosterManager.TryPurchase(boosterType, price))
            RefreshUI();
    }
}