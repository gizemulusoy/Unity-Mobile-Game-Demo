using UnityEngine;

public enum BoosterType
{
    RocketRow,
    RocketCol,
    Bomb,
    ColorBomb
}

public class BoosterManager : MonoBehaviour
{

    [ContextMenu("Fill Booster Stocks For Testing")]
    private void FillStocksForTesting()
    {
        foreach (BoosterType type in System.Enum.GetValues(typeof(BoosterType)))
        {
            PlayerPrefs.SetInt(GetStockKey(type), 99);
        }

        PlayerPrefs.Save();

        Debug.Log("All booster stocks set to 99.");
    }

    private const int InitialStock = 3;

    public GridManager gridManager;

    private BoosterType? selectedBooster;

    public bool IsSelecting => selectedBooster.HasValue;

    private string GetStockKey(BoosterType type)
    {
        return "BoosterStock_" + type;
    }

    public int GetStock(BoosterType type)
    {
        return Mathf.Max(
            0,
            PlayerPrefs.GetInt(GetStockKey(type), InitialStock)
        );
    }
    public bool TryConsume(BoosterType type)
    {
        int stock = GetStock(type);

        if (stock <= 0)
            return false;

        PlayerPrefs.SetInt(GetStockKey(type), stock - 1);
        PlayerPrefs.Save();

        return true;
    }

    public void SelectBooster(BoosterType type)
    {
        if (gridManager == null || gridManager.IsBusy)
            return;

        if (selectedBooster == type)
        {
            CancelSelection();
            return;
        }

        if (GetStock(type) <= 0)
            return;

        selectedBooster = type;
    }

    public void CancelSelection()
    {
        selectedBooster = null;
    }

    public void SelectRocketRow()
    {
        SelectBooster(BoosterType.RocketRow);
    }

    public void SelectRocketCol()
    {
        SelectBooster(BoosterType.RocketCol);
    }

    public void SelectBomb()
    {
        SelectBooster(BoosterType.Bomb);
    }

    public void SelectColorBomb()
    {
        SelectBooster(BoosterType.ColorBomb);
    }

    public bool TryUseOnTile(Tile tile)
    {
        if (!selectedBooster.HasValue || gridManager == null)
            return false;

        BoosterType type = selectedBooster.Value;

        if (GetStock(type) <= 0)
        {
            CancelSelection();
            return false;
        }

        if (!gridManager.TryPlaceBooster(tile, type))
            return false;

        TryConsume(type);
        CancelSelection();

        return true;
    }
}