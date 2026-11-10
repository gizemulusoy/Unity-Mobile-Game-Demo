using TMPro;
using UnityEngine;

public class BoosterButtonUI : MonoBehaviour
{
    public BoosterManager boosterManager;
    public BoosterType boosterType;
    public TMP_Text stockText;

    private int lastStock = -1;

    private void OnEnable()
    {
        lastStock = -1;
        RefreshStock();
    }

    private void Update()
    {
        RefreshStock();
    }

    private void RefreshStock()
    {
        if (boosterManager == null || stockText == null)
            return;

        int stock = boosterManager.GetStock(boosterType);

        if (stock == lastStock)
            return;

        stockText.text = stock.ToString();
        lastStock = stock;
    }
}