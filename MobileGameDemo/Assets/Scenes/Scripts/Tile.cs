using UnityEngine;
using System.Collections;

public enum TileColor
{
    Red, Green, Blue, Yellow, Purple, Orange
}

[RequireComponent(typeof(SpriteRenderer))]
public class Tile : MonoBehaviour
{
    public Vector2Int GridPos { get; private set; }
    public TileColor ColorType { get; private set; }

    private SpriteRenderer sr;

    public void Init(Vector2Int gridPos, TileColor colorType, Sprite sprite, float size)
    {
        GridPos = gridPos;
        ColorType = colorType;

        sr = GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = ToUnityColor(colorType);
        
        transform.localScale = Vector3.one * size;
        name = $"Tile_{gridPos.x}_{gridPos.y}_{colorType}";
    }

    public void SetGridPos(Vector2Int newPos)
    {
        GridPos = newPos;
    }

    public void SetColor(TileColor newColor)
    {
        ColorType = newColor;
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        sr.color = ToUnityColor(newColor);
    }

    public static Color ToUnityColor(TileColor t)
    {
        return t switch
        {
            TileColor.Red => new Color(0.95f, 0.25f, 0.25f),
            TileColor.Green => new Color(0.25f, 0.9f, 0.35f),
            TileColor.Blue => new Color(0.25f, 0.5f, 0.95f),
            TileColor.Yellow => new Color(0.95f, 0.9f, 0.25f),
            TileColor.Purple => new Color(0.7f, 0.35f, 0.95f),
            TileColor.Orange => new Color(0.98f, 0.55f, 0.2f),
            _ => Color.white
        };
    }
    
    public IEnumerator FlashWhite(float duration = 0.08f)
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();

        Color original = sr.color;
        sr.color = Color.white;

        yield return new WaitForSeconds(duration);

        sr.color = original;
    }

}